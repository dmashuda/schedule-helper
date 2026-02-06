namespace ScheduleHelper.Models;

public enum TimeBlock
{
    Morning,
    Midday,
    Afternoon,
    Evening,
    Bedtime
}

public class ScheduleDay
{
    public DateOnly Date { get; set; }
    public int DayNumber { get; set; }
    public Dictionary<TimeBlock, List<ScheduleEntry>> Entries { get; set; } = [];
}

public class ScheduleEntry
{
    public string MedicationId { get; set; } = "";
    public string MedicationName { get; set; } = "";
    public string Dose { get; set; } = "";
    public List<int> ConflictFootnotes { get; set; } = [];
}

public class ConflictFootnote
{
    public int Number { get; set; }
    public string Note { get; set; } = "";
    public List<string> MedicationIds { get; set; } = [];
}
