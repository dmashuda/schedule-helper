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
            col.Item().Text(viewModel.Title).Style(TitleStyle);
            col.Item().Text(viewModel.DateRange).Style(DateRangeStyle);
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
                            col.Item().Text("").Style(EntryTextStyle);
                        }
                        else
                        {
                            foreach (var entry in entries)
                            {
                                col.Item().Row(row =>
                                {
                                    row.ConstantItem(12).Element(CheckboxStyle);

                                    row.RelativeItem().PaddingLeft(3).Text(text =>
                                    {
                                        text.Span(entry.Label).Style(EntryTextStyle);
                                        foreach (var fn in entry.FootnoteNumbers)
                                        {
                                            text.Span($" [{fn}]").Style(FootnoteMarkerStyle);
                                        }
                                    });
                                });
                            }

                            col.Item().PaddingTop(3).Text(viewModel.TimeEntryPlaceholder).Style(SmallTextStyle).Italic();
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
                col.Item().PaddingBottom(4).Text(viewModel.ConflictWarningsHeader).Style(SectionHeaderStyle);
                foreach (var fn in viewModel.ConflictWarnings)
                {
                    col.Item().Text(text =>
                    {
                        text.Span($"[{fn.Number}] ").Style(FootnoteMarkerStyle);
                        text.Span(fn.Note);
                    });
                }
            }

            if (viewModel.MedicationNotes.Count > 0)
            {
                col.Item().PaddingTop(6).PaddingBottom(4).Text(viewModel.NotesHeader).Style(SectionHeaderStyle);
                foreach (var med in viewModel.MedicationNotes)
                {
                    col.Item().Text($"- {med.MedicationName}: {med.Note}").Style(FooterTextStyle);
                }
            }
        });
    }

    // Container styles
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

    private static IContainer CheckboxStyle(IContainer c) =>
        c.Height(10).Width(10).Border(0.75f).BorderColor(Colors.Black);

    // Text styles
    private static TextStyle TitleStyle => TextStyle.Default.FontSize(14).Bold();
    private static TextStyle DateRangeStyle => TextStyle.Default.FontSize(10);
    private static TextStyle EntryTextStyle => TextStyle.Default.FontSize(7);
    private static TextStyle SmallTextStyle => TextStyle.Default.FontSize(6);
    private static TextStyle FootnoteMarkerStyle => TextStyle.Default.FontSize(6).Bold().FontColor(Colors.Red.Medium);
    private static TextStyle SectionHeaderStyle => TextStyle.Default.FontSize(8).Bold();
    private static TextStyle FooterTextStyle => TextStyle.Default.FontSize(8);
}
