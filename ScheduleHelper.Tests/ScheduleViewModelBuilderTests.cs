using ScheduleHelper.Models;
using ScheduleHelper.Services;

namespace ScheduleHelper.Tests;

public class ScheduleViewModelBuilderTests
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
                        ? new List<ScheduleEntry>
                        {
                            new() { MedicationId = "med1", MedicationName = "Med1", Dose = "10mg", ConflictFootnotes = [1] }
                        }
                        : new List<ScheduleEntry>())
            },
            new()
            {
                Date = new DateOnly(2026, 1, 2),
                DayNumber = 2,
                Entries = Enum.GetValues<TimeBlock>().ToDictionary(
                    tb => tb,
                    tb => new List<ScheduleEntry>())
            }
        };

        var footnotes = new List<ConflictFootnote>
        {
            new() { Number = 1, Note = "Take 2 hours apart", MedicationIds = ["med1", "med2"] }
        };

        var meds = new List<MedicationRule>
        {
            new() { Id = "med1", Name = "Med1", Dose = "10mg", Notes = "Take with food" },
            new() { Id = "med2", Name = "Med2", Dose = "20mg" }
        };

        return (days, footnotes, meds);
    }

    [Fact]
    public void Build_SetsTitle()
    {
        var (days, footnotes, meds) = MakeTestData();
        var vm = ScheduleViewModelBuilder.Build(days, footnotes, meds);
        Assert.Equal("Medication Schedule Worksheet", vm.Title);
    }

    [Fact]
    public void Build_SetsDateRange()
    {
        var (days, footnotes, meds) = MakeTestData();
        var vm = ScheduleViewModelBuilder.Build(days, footnotes, meds);
        Assert.Equal("January 1, 2026 - January 2, 2026", vm.DateRange);
    }

    [Fact]
    public void Build_HasFiveTimeBlockLabels()
    {
        var (days, footnotes, meds) = MakeTestData();
        var vm = ScheduleViewModelBuilder.Build(days, footnotes, meds);
        Assert.Equal(5, vm.TimeBlockLabels.Count);
        Assert.Equal("Morning\n(7-9 AM)", vm.TimeBlockLabels[0]);
        Assert.Equal("Bedtime\n(9-10 PM)", vm.TimeBlockLabels[4]);
    }

    [Fact]
    public void Build_SetsDayHeaders()
    {
        var (days, footnotes, meds) = MakeTestData();
        var vm = ScheduleViewModelBuilder.Build(days, footnotes, meds);
        Assert.Equal(2, vm.Days.Count);
        Assert.Equal("Day 1\nThu 1/1", vm.Days[0].Header);
        Assert.Equal("Day 2\nFri 1/2", vm.Days[1].Header);
    }

    [Fact]
    public void Build_MapsEntryLabelsAndFootnotes()
    {
        var (days, footnotes, meds) = MakeTestData();
        var vm = ScheduleViewModelBuilder.Build(days, footnotes, meds);

        // Morning block (index 0) of day 1 should have one entry
        var morningEntries = vm.Days[0].TimeBlockEntries[0];
        Assert.Single(morningEntries);
        Assert.Equal("Med1 10mg", morningEntries[0].Label);
        Assert.Equal([1], morningEntries[0].FootnoteNumbers);
    }

    [Fact]
    public void Build_MapsConflictWarnings()
    {
        var (days, footnotes, meds) = MakeTestData();
        var vm = ScheduleViewModelBuilder.Build(days, footnotes, meds);
        Assert.Single(vm.ConflictWarnings);
        Assert.Equal(1, vm.ConflictWarnings[0].Number);
        Assert.Equal("Take 2 hours apart", vm.ConflictWarnings[0].Note);
    }

    [Fact]
    public void Build_FiltersMedicationNotes()
    {
        var (days, footnotes, meds) = MakeTestData();
        var vm = ScheduleViewModelBuilder.Build(days, footnotes, meds);

        // Only med1 has notes
        Assert.Single(vm.MedicationNotes);
        Assert.Equal("Med1", vm.MedicationNotes[0].MedicationName);
        Assert.Equal("Take with food", vm.MedicationNotes[0].Note);
    }

    [Fact]
    public void Build_EmptyEntriesProduceEmptyLists()
    {
        var (days, footnotes, meds) = MakeTestData();
        var vm = ScheduleViewModelBuilder.Build(days, footnotes, meds);

        // Day 2 has no entries in any block
        foreach (var blockEntries in vm.Days[1].TimeBlockEntries)
        {
            Assert.Empty(blockEntries);
        }
    }
}
