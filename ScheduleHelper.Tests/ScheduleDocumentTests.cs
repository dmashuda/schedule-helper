using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using ScheduleHelper.Models;
using ScheduleHelper.Pdf;
using ScheduleHelper.Services;

namespace ScheduleHelper.Tests;

public class ScheduleDocumentTests
{
    private static ScheduleHelper.ViewModels.ScheduleViewModel MakeTestViewModel()
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

        return ScheduleViewModelBuilder.Build(days, footnotes, meds);
    }

    [Fact]
    public void Constructor_DoesNotThrow()
    {
        var vm = MakeTestViewModel();
        var doc = new ScheduleDocument(vm);
        Assert.NotNull(doc);
    }

    [Fact]
    public void GetMetadata_ReturnsNonNull()
    {
        var vm = MakeTestViewModel();
        var doc = new ScheduleDocument(vm);
        Assert.NotNull(doc.GetMetadata());
    }

    [Fact]
    public void GetSettings_ReturnsLeftToRight()
    {
        var vm = MakeTestViewModel();
        var doc = new ScheduleDocument(vm);
        var settings = doc.GetSettings();
        Assert.Equal(ContentDirection.LeftToRight, settings.ContentDirection);
    }

    [Fact]
    public void GeneratePdf_DoesNotThrow()
    {
        QuestPDF.Settings.License = LicenseType.Community;
        var vm = MakeTestViewModel();
        var doc = new ScheduleDocument(vm);
        var pdfBytes = doc.GeneratePdf();
        Assert.NotEmpty(pdfBytes);
    }
}
