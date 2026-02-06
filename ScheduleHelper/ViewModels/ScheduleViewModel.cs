namespace ScheduleHelper.ViewModels;

public class ScheduleViewModel
{
    public string Title { get; set; } = "";
    public string DateRange { get; set; } = "";
    public List<string> TimeBlockLabels { get; set; } = [];
    public List<DayColumnViewModel> Days { get; set; } = [];
    public List<FootnoteViewModel> ConflictWarnings { get; set; } = [];
    public List<MedicationNoteViewModel> MedicationNotes { get; set; } = [];
    public string TimeBlockColumnHeader { get; set; } = "Time Block";
    public string TimeEntryPlaceholder { get; set; } = "Time: ____";
    public string ConflictWarningsHeader { get; set; } = "Conflict Warnings:";
    public string NotesHeader { get; set; } = "Notes:";
}

public class DayColumnViewModel
{
    public string Header { get; set; } = "";
    public List<List<EntryCellViewModel>> TimeBlockEntries { get; set; } = [];
}

public class EntryCellViewModel
{
    public string Label { get; set; } = "";
    public List<int> FootnoteNumbers { get; set; } = [];
}

public class FootnoteViewModel
{
    public int Number { get; set; }
    public string Note { get; set; } = "";
}

public class MedicationNoteViewModel
{
    public string MedicationName { get; set; } = "";
    public string Note { get; set; } = "";
}
