using ScheduleHelper.Models;

namespace ScheduleHelper.Services;

public static class ScheduleBuilder
{
    public static List<ScheduleDay> Build(InputFile input, DateOnly startDate)
    {
        var days = new List<ScheduleDay>();
        var totalDays = input.Schedule.Days;

        for (int d = 0; d < totalDays; d++)
        {
            var day = new ScheduleDay
            {
                Date = startDate.AddDays(d),
                DayNumber = d + 1,
                Entries = Enum.GetValues<TimeBlock>().ToDictionary(tb => tb, _ => new List<ScheduleEntry>())
            };

            foreach (var med in input.Medications)
            {
                if (d >= med.DurationDays)
                    continue;

                foreach (var blockName in med.TimeBlocks)
                {
                    if (!Enum.TryParse<TimeBlock>(blockName, ignoreCase: true, out var block))
                    {
                        Console.Error.WriteLine($"Warning: Unknown time block '{blockName}' for medication '{med.Id}', skipping.");
                        continue;
                    }

                    day.Entries[block].Add(new ScheduleEntry
                    {
                        MedicationId = med.Id,
                        MedicationName = med.Name,
                        Dose = med.Dose
                    });
                }
            }

            days.Add(day);
        }

        return days;
    }
}
