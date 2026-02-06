using ScheduleHelper.Models;
using ScheduleHelper.Services;

namespace ScheduleHelper.Tests;

public class ConflictCheckerTests
{
    private static ScheduleDay MakeDay(Dictionary<TimeBlock, List<ScheduleEntry>> entries) =>
        new() { Date = new DateOnly(2026, 1, 1), DayNumber = 1, Entries = entries };

    private static Dictionary<TimeBlock, List<ScheduleEntry>> AllBlockEntries(
        params (TimeBlock block, string medId)[] assignments)
    {
        var entries = Enum.GetValues<TimeBlock>().ToDictionary(tb => tb, _ => new List<ScheduleEntry>());
        foreach (var (block, medId) in assignments)
            entries[block].Add(new ScheduleEntry { MedicationId = medId, MedicationName = medId, Dose = "10mg" });
        return entries;
    }

    [Fact]
    public void Check_SameBlockConflict_DetectsConflict()
    {
        var day = MakeDay(AllBlockEntries(
            (TimeBlock.Morning, "med_a"),
            (TimeBlock.Morning, "med_b")));

        var rules = new List<ConflictRule>
        {
            new() { Medications = ["med_a", "med_b"], MinHoursApart = 2, Note = "Conflict!" }
        };

        var footnotes = ConflictChecker.Check([day], rules);

        Assert.Single(footnotes);
        Assert.Equal("Conflict!", footnotes[0].Note);
        // Both entries in Morning should have the footnote
        Assert.Contains(1, day.Entries[TimeBlock.Morning][0].ConflictFootnotes);
        Assert.Contains(1, day.Entries[TimeBlock.Morning][1].ConflictFootnotes);
    }

    [Fact]
    public void Check_AdjacentBlockConflict_DetectsConflict()
    {
        // Morning -> Midday is 3 hours apart
        var day = MakeDay(AllBlockEntries(
            (TimeBlock.Morning, "med_a"),
            (TimeBlock.Midday, "med_b")));

        var rules = new List<ConflictRule>
        {
            new() { Medications = ["med_a", "med_b"], MinHoursApart = 4, Note = "Too close" }
        };

        var footnotes = ConflictChecker.Check([day], rules);

        Assert.Single(footnotes);
        Assert.Contains(1, day.Entries[TimeBlock.Morning][0].ConflictFootnotes);
        Assert.Contains(1, day.Entries[TimeBlock.Midday][0].ConflictFootnotes);
    }

    [Fact]
    public void Check_FarApartBlocks_NoConflict()
    {
        // Morning -> Evening is 3+2+3 = 8 hours apart
        var day = MakeDay(AllBlockEntries(
            (TimeBlock.Morning, "med_a"),
            (TimeBlock.Evening, "med_b")));

        var rules = new List<ConflictRule>
        {
            new() { Medications = ["med_a", "med_b"], MinHoursApart = 4, Note = "Far enough" }
        };

        var footnotes = ConflictChecker.Check([day], rules);

        Assert.Empty(footnotes);
    }

    [Fact]
    public void Check_NoConflictRules_ReturnsEmptyList()
    {
        var day = MakeDay(AllBlockEntries(
            (TimeBlock.Morning, "med_a")));

        var footnotes = ConflictChecker.Check([day], []);

        Assert.Empty(footnotes);
    }

    [Fact]
    public void Check_MultipleRules_FootnoteNumbersAreSequential()
    {
        var day = MakeDay(AllBlockEntries(
            (TimeBlock.Morning, "med_a"),
            (TimeBlock.Morning, "med_b"),
            (TimeBlock.Morning, "med_c")));

        var rules = new List<ConflictRule>
        {
            new() { Medications = ["med_a", "med_b"], MinHoursApart = 2, Note = "Conflict 1" },
            new() { Medications = ["med_b", "med_c"], MinHoursApart = 2, Note = "Conflict 2" }
        };

        var footnotes = ConflictChecker.Check([day], rules);

        Assert.Equal(2, footnotes.Count);
        Assert.Equal(1, footnotes[0].Number);
        Assert.Equal(2, footnotes[1].Number);
    }

    [Fact]
    public void Check_ConflictFootnotesAttachedToCorrectEntries()
    {
        var day = MakeDay(AllBlockEntries(
            (TimeBlock.Morning, "med_a"),
            (TimeBlock.Morning, "med_b"),
            (TimeBlock.Morning, "med_c")));

        var rules = new List<ConflictRule>
        {
            new() { Medications = ["med_a", "med_b"], MinHoursApart = 2, Note = "AB conflict" }
        };

        var footnotes = ConflictChecker.Check([day], rules);

        // med_a and med_b should have footnote 1
        var morningEntries = day.Entries[TimeBlock.Morning];
        Assert.Contains(1, morningEntries.First(e => e.MedicationId == "med_a").ConflictFootnotes);
        Assert.Contains(1, morningEntries.First(e => e.MedicationId == "med_b").ConflictFootnotes);
        // med_c should NOT have footnote 1
        Assert.DoesNotContain(1, morningEntries.First(e => e.MedicationId == "med_c").ConflictFootnotes);
    }
}
