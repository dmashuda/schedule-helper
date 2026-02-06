using YamlDotNet.Serialization;

namespace ScheduleHelper.Models;

public class InputFile
{
    [YamlMember(Alias = "schedule")]
    public ScheduleConfig Schedule { get; set; } = new();

    [YamlMember(Alias = "medications")]
    public List<MedicationRule> Medications { get; set; } = [];

    [YamlMember(Alias = "conflicts")]
    public List<ConflictRule> Conflicts { get; set; } = [];
}

public class ScheduleConfig
{
    [YamlMember(Alias = "days")]
    public int Days { get; set; } = 7;

    [YamlMember(Alias = "start_date")]
    public string? StartDate { get; set; }
}

public class MedicationRule
{
    [YamlMember(Alias = "id")]
    public string Id { get; set; } = "";

    [YamlMember(Alias = "name")]
    public string Name { get; set; } = "";

    [YamlMember(Alias = "dose")]
    public string Dose { get; set; } = "";

    [YamlMember(Alias = "frequency")]
    public string Frequency { get; set; } = "";

    [YamlMember(Alias = "duration_days")]
    public int DurationDays { get; set; }

    [YamlMember(Alias = "time_blocks")]
    public List<string> TimeBlocks { get; set; } = [];

    [YamlMember(Alias = "notes")]
    public string? Notes { get; set; }
}

public class ConflictRule
{
    [YamlMember(Alias = "medications")]
    public List<string> Medications { get; set; } = [];

    [YamlMember(Alias = "min_hours_apart")]
    public int MinHoursApart { get; set; }

    [YamlMember(Alias = "note")]
    public string Note { get; set; } = "";
}
