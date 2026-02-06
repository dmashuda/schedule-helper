using ScheduleHelper.Models;
using ScheduleHelper.Services;

namespace ScheduleHelper.Tests;

public class ScheduleBuilderTests
{
    private static readonly DateOnly StartDate = new(2026, 1, 1);

    [Fact]
    public void Build_ReturnsCorrectNumberOfDays()
    {
        var input = new InputFile { Schedule = new ScheduleConfig { Days = 5 } };
        var days = ScheduleBuilder.Build(input, StartDate);
        Assert.Equal(5, days.Count);
    }

    [Fact]
    public void Build_DaysHaveCorrectDatesAndNumbers()
    {
        var input = new InputFile { Schedule = new ScheduleConfig { Days = 3 } };
        var days = ScheduleBuilder.Build(input, StartDate);

        Assert.Equal(new DateOnly(2026, 1, 1), days[0].Date);
        Assert.Equal(1, days[0].DayNumber);
        Assert.Equal(new DateOnly(2026, 1, 3), days[2].Date);
        Assert.Equal(3, days[2].DayNumber);
    }

    [Fact]
    public void Build_MedicationsAssignedToCorrectTimeBlocks()
    {
        var input = new InputFile
        {
            Schedule = new ScheduleConfig { Days = 1 },
            Medications =
            [
                new MedicationRule
                {
                    Id = "med1", Name = "Med1", Dose = "10mg",
                    DurationDays = 1, TimeBlocks = ["Morning", "Evening"]
                }
            ]
        };

        var days = ScheduleBuilder.Build(input, StartDate);

        Assert.Single(days[0].Entries[TimeBlock.Morning]);
        Assert.Equal("med1", days[0].Entries[TimeBlock.Morning][0].MedicationId);
        Assert.Single(days[0].Entries[TimeBlock.Evening]);
        Assert.Empty(days[0].Entries[TimeBlock.Midday]);
        Assert.Empty(days[0].Entries[TimeBlock.Afternoon]);
        Assert.Empty(days[0].Entries[TimeBlock.Bedtime]);
    }

    [Fact]
    public void Build_DurationFiltering_MedicationDisappearsAfterDuration()
    {
        var input = new InputFile
        {
            Schedule = new ScheduleConfig { Days = 5 },
            Medications =
            [
                new MedicationRule
                {
                    Id = "short", Name = "Short", Dose = "5mg",
                    DurationDays = 3, TimeBlocks = ["Morning"]
                }
            ]
        };

        var days = ScheduleBuilder.Build(input, StartDate);

        // Days 1-3 should have the medication
        Assert.Single(days[0].Entries[TimeBlock.Morning]);
        Assert.Single(days[1].Entries[TimeBlock.Morning]);
        Assert.Single(days[2].Entries[TimeBlock.Morning]);
        // Days 4-5 should not
        Assert.Empty(days[3].Entries[TimeBlock.Morning]);
        Assert.Empty(days[4].Entries[TimeBlock.Morning]);
    }

    [Fact]
    public void Build_EmptyMedications_ProducesEmptyEntries()
    {
        var input = new InputFile { Schedule = new ScheduleConfig { Days = 2 } };
        var days = ScheduleBuilder.Build(input, StartDate);

        Assert.Equal(2, days.Count);
        foreach (var day in days)
        {
            foreach (var (_, entries) in day.Entries)
                Assert.Empty(entries);
        }
    }

    [Fact]
    public void Build_UnknownTimeBlockName_IsSkipped()
    {
        var input = new InputFile
        {
            Schedule = new ScheduleConfig { Days = 1 },
            Medications =
            [
                new MedicationRule
                {
                    Id = "med1", Name = "Med1", Dose = "10mg",
                    DurationDays = 1, TimeBlocks = ["Morning", "InvalidBlock"]
                }
            ]
        };

        var days = ScheduleBuilder.Build(input, StartDate);

        // Morning still works, invalid block silently skipped
        Assert.Single(days[0].Entries[TimeBlock.Morning]);
    }
}
