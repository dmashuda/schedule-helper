using ScheduleHelper.Models;
using ScheduleHelper.ViewModels;

namespace ScheduleHelper.Services;

public static class ScheduleViewModelBuilder
{
    private static readonly TimeBlock[] AllBlocks =
        [TimeBlock.Morning, TimeBlock.Midday, TimeBlock.Afternoon, TimeBlock.Evening, TimeBlock.Bedtime];

    public static ScheduleViewModel Build(
        List<ScheduleDay> days,
        List<ConflictFootnote> footnotes,
        List<MedicationRule> medications)
    {
        var startDate = days.First().Date;
        var endDate = days.Last().Date;

        return new ScheduleViewModel
        {
            Title = "Medication Schedule Worksheet",
            DateRange = $"{startDate:MMMM d, yyyy} - {endDate:MMMM d, yyyy}",
            TimeBlockLabels = AllBlocks.Select(BlockLabel).ToList(),
            Days = days.Select(BuildDayColumn).ToList(),
            ConflictWarnings = footnotes.Select(fn => new FootnoteViewModel
            {
                Number = fn.Number,
                Note = fn.Note
            }).ToList(),
            MedicationNotes = medications
                .Where(m => !string.IsNullOrWhiteSpace(m.Notes))
                .Select(m => new MedicationNoteViewModel
                {
                    MedicationName = m.Name,
                    Note = m.Notes!
                }).ToList()
        };
    }

    private static DayColumnViewModel BuildDayColumn(ScheduleDay day) => new()
    {
        Header = $"Day {day.DayNumber}\n{day.Date:ddd M/d}",
        TimeBlockEntries = AllBlocks.Select(block =>
            day.Entries[block].Select(entry => new EntryCellViewModel
            {
                Label = $"{entry.MedicationName} {entry.Dose}",
                FootnoteNumbers = entry.ConflictFootnotes
            }).ToList()
        ).ToList()
    };

    private static string BlockLabel(TimeBlock block) => block switch
    {
        TimeBlock.Morning => "Morning\n(7-9 AM)",
        TimeBlock.Midday => "Midday\n(11 AM-1 PM)",
        TimeBlock.Afternoon => "Afternoon\n(2-4 PM)",
        TimeBlock.Evening => "Evening\n(5-7 PM)",
        TimeBlock.Bedtime => "Bedtime\n(9-10 PM)",
        _ => block.ToString()
    };
}
