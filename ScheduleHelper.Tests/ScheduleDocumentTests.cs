using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using ScheduleHelper.Models;
using ScheduleHelper.Pdf;

namespace ScheduleHelper.Tests;

public class ScheduleDocumentTests
{
    private static (List<ScheduleDay> days, List<ConflictFootnote> footnotes, List<MedicationRule> meds) MakeTestData()
    {
        var days = new List<ScheduleDay>
        {
            new()
            {
                Date = new DateOnly(2026, 1, 1),
                DayNumber = 1,
                Entries = Enum.GetValues<TimeBlock>().ToDictionary(
                    tb => tb,
                    tb => tb == TimeBlock.Morning
                        ? new List<ScheduleEntry> { new() { MedicationId = "med1", MedicationName = "Med1", Dose = "10mg" } }
                        : new List<ScheduleEntry>())
            }
        };

        var footnotes = new List<ConflictFootnote>();
        var meds = new List<MedicationRule>
        {
            new() { Id = "med1", Name = "Med1", Dose = "10mg", Notes = "A note" }
        };

        return (days, footnotes, meds);
    }

    [Fact]
    public void Constructor_DoesNotThrow()
    {
        var (days, footnotes, meds) = MakeTestData();
        var doc = new ScheduleDocument(days, footnotes, meds);
        Assert.NotNull(doc);
    }

    [Fact]
    public void GetMetadata_ReturnsNonNull()
    {
        var (days, footnotes, meds) = MakeTestData();
        var doc = new ScheduleDocument(days, footnotes, meds);
        Assert.NotNull(doc.GetMetadata());
    }

    [Fact]
    public void GetSettings_ReturnsLeftToRight()
    {
        var (days, footnotes, meds) = MakeTestData();
        var doc = new ScheduleDocument(days, footnotes, meds);
        var settings = doc.GetSettings();
        Assert.Equal(ContentDirection.LeftToRight, settings.ContentDirection);
    }

    [Fact]
    public void GeneratePdf_DoesNotThrow()
    {
        QuestPDF.Settings.License = LicenseType.Community;
        var (days, footnotes, meds) = MakeTestData();
        var doc = new ScheduleDocument(days, footnotes, meds);
        // GeneratePdf to byte array should not throw
        var pdfBytes = doc.GeneratePdf();
        Assert.NotEmpty(pdfBytes);
    }
}
