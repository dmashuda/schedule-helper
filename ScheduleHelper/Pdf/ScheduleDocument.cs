using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ScheduleHelper.Models;

namespace ScheduleHelper.Pdf;

public class ScheduleDocument : IDocument
{
    private readonly List<ScheduleDay> _days;
    private readonly List<ConflictFootnote> _footnotes;
    private readonly List<MedicationRule> _medications;

    private static readonly TimeBlock[] AllBlocks =
        [TimeBlock.Morning, TimeBlock.Midday, TimeBlock.Afternoon, TimeBlock.Evening, TimeBlock.Bedtime];

    public ScheduleDocument(List<ScheduleDay> days, List<ConflictFootnote> footnotes, List<MedicationRule> medications)
    {
        _days = days;
        _footnotes = footnotes;
        _medications = medications;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public DocumentSettings GetSettings() => new()
    {
        ContentDirection = ContentDirection.LeftToRight
    };

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4.Landscape());
            page.MarginHorizontal(1.5f, Unit.Centimetre);
            page.MarginVertical(1.5f, Unit.Centimetre);
            page.DefaultTextStyle(x => x.FontSize(8));

            page.Header().Element(ComposeHeader);
            page.Content().Element(ComposeContent);
            page.Footer().Element(ComposeFooter);
        });
    }

    private void ComposeHeader(IContainer container)
    {
        var startDate = _days.First().Date;
        var endDate = _days.Last().Date;

        container.PaddingBottom(8).Column(col =>
        {
            col.Item().Text("Medication Schedule Worksheet").Bold().FontSize(14);
            col.Item().Text($"{startDate:MMMM d, yyyy} - {endDate:MMMM d, yyyy}").FontSize(10);
        });
    }

    private void ComposeContent(IContainer container)
    {
        container.Table(table =>
        {
            // Define columns: label + one per day
            table.ColumnsDefinition(cols =>
            {
                cols.ConstantColumn(80); // time block label
                foreach (var _ in _days)
                    cols.RelativeColumn();
            });

            // Header row with day labels
            table.Header(header =>
            {
                header.Cell().Element(HeaderCellStyle).Text("Time Block");
                foreach (var day in _days)
                {
                    header.Cell().Element(HeaderCellStyle)
                        .Text($"Day {day.DayNumber}\n{day.Date:ddd M/d}");
                }
            });

            // One row per time block
            foreach (var block in AllBlocks)
            {
                table.Cell().Element(LabelCellStyle).Text(BlockLabel(block));

                foreach (var day in _days)
                {
                    table.Cell().Element(DataCellStyle).Column(col =>
                    {
                        var entries = day.Entries[block];
                        if (entries.Count == 0)
                        {
                            col.Item().Text("").FontSize(7);
                        }
                        else
                        {
                            foreach (var entry in entries)
                            {
                                col.Item().Row(row =>
                                {
                                    // Checkbox
                                    row.ConstantItem(12).Height(10).Width(10)
                                        .Border(0.75f).BorderColor(Colors.Black);

                                    row.RelativeItem().PaddingLeft(3).Text(text =>
                                    {
                                        text.Span($"{entry.MedicationName} {entry.Dose}").FontSize(7);
                                        foreach (var fn in entry.ConflictFootnotes)
                                        {
                                            text.Span($" [{fn}]").FontSize(6).Bold().FontColor(Colors.Red.Medium);
                                        }
                                    });
                                });
                            }

                            col.Item().PaddingTop(3).Text("Time: ____").FontSize(6).Italic();
                        }
                    });
                }
            }
        });
    }

    private void ComposeFooter(IContainer container)
    {
        container.PaddingTop(10).Column(col =>
        {
            if (_footnotes.Count > 0)
            {
                col.Item().PaddingBottom(4).Text("Conflict Warnings:").Bold().FontSize(8);
                foreach (var fn in _footnotes)
                {
                    col.Item().Text(text =>
                    {
                        text.Span($"[{fn.Number}] ").Bold().FontColor(Colors.Red.Medium);
                        text.Span(fn.Note);
                    });
                }
            }

            var medsWithNotes = _medications.Where(m => !string.IsNullOrWhiteSpace(m.Notes)).ToList();
            if (medsWithNotes.Count > 0)
            {
                col.Item().PaddingTop(6).PaddingBottom(4).Text("Notes:").Bold().FontSize(8);
                foreach (var med in medsWithNotes)
                {
                    col.Item().Text($"- {med.Name}: {med.Notes}").FontSize(8);
                }
            }
        });
    }

    private static string BlockLabel(TimeBlock block) => block switch
    {
        TimeBlock.Morning => "Morning\n(7-9 AM)",
        TimeBlock.Midday => "Midday\n(11 AM-1 PM)",
        TimeBlock.Afternoon => "Afternoon\n(2-4 PM)",
        TimeBlock.Evening => "Evening\n(5-7 PM)",
        TimeBlock.Bedtime => "Bedtime\n(9-10 PM)",
        _ => block.ToString()
    };

    private static IContainer HeaderCellStyle(IContainer c) =>
        c.Border(0.5f).BorderColor(Colors.Grey.Medium)
         .Background(Colors.Grey.Lighten3)
         .Padding(4).AlignCenter().AlignMiddle();

    private static IContainer LabelCellStyle(IContainer c) =>
        c.Border(0.5f).BorderColor(Colors.Grey.Medium)
         .Background(Colors.Grey.Lighten4)
         .Padding(4).AlignCenter().AlignMiddle();

    private static IContainer DataCellStyle(IContainer c) =>
        c.Border(0.5f).BorderColor(Colors.Grey.Medium)
         .Padding(3);
}
