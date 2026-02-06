using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ScheduleHelper.ViewModels;

namespace ScheduleHelper.Pdf;

public class ScheduleDocument(ScheduleViewModel viewModel) : IDocument
{
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
        container.PaddingBottom(8).Column(col =>
        {
            col.Item().Text(viewModel.Title).Bold().FontSize(14);
            col.Item().Text(viewModel.DateRange).FontSize(10);
        });
    }

    private void ComposeContent(IContainer container)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(cols =>
            {
                cols.ConstantColumn(80);
                foreach (var _ in viewModel.Days)
                    cols.RelativeColumn();
            });

            table.Header(header =>
            {
                header.Cell().Element(HeaderCellStyle).Text(viewModel.TimeBlockColumnHeader);
                foreach (var day in viewModel.Days)
                {
                    header.Cell().Element(HeaderCellStyle).Text(day.Header);
                }
            });

            for (var blockIndex = 0; blockIndex < viewModel.TimeBlockLabels.Count; blockIndex++)
            {
                table.Cell().Element(LabelCellStyle).Text(viewModel.TimeBlockLabels[blockIndex]);

                foreach (var day in viewModel.Days)
                {
                    var entries = day.TimeBlockEntries[blockIndex];
                    table.Cell().Element(DataCellStyle).Column(col =>
                    {
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
                                    row.ConstantItem(12).Height(10).Width(10)
                                        .Border(0.75f).BorderColor(Colors.Black);

                                    row.RelativeItem().PaddingLeft(3).Text(text =>
                                    {
                                        text.Span(entry.Label).FontSize(7);
                                        foreach (var fn in entry.FootnoteNumbers)
                                        {
                                            text.Span($" [{fn}]").FontSize(6).Bold().FontColor(Colors.Red.Medium);
                                        }
                                    });
                                });
                            }

                            col.Item().PaddingTop(3).Text(viewModel.TimeEntryPlaceholder).FontSize(6).Italic();
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
            if (viewModel.ConflictWarnings.Count > 0)
            {
                col.Item().PaddingBottom(4).Text(viewModel.ConflictWarningsHeader).Bold().FontSize(8);
                foreach (var fn in viewModel.ConflictWarnings)
                {
                    col.Item().Text(text =>
                    {
                        text.Span($"[{fn.Number}] ").Bold().FontColor(Colors.Red.Medium);
                        text.Span(fn.Note);
                    });
                }
            }

            if (viewModel.MedicationNotes.Count > 0)
            {
                col.Item().PaddingTop(6).PaddingBottom(4).Text(viewModel.NotesHeader).Bold().FontSize(8);
                foreach (var med in viewModel.MedicationNotes)
                {
                    col.Item().Text($"- {med.MedicationName}: {med.Note}").FontSize(8);
                }
            }
        });
    }

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
