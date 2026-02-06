using System.CommandLine;
using QuestPDF;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using ScheduleHelper.Pdf;
using ScheduleHelper.Services;

Settings.License = LicenseType.Community;

var inputOption = new Option<FileInfo>("--input")
{
    Description = "Path to the YAML configuration file",
    Required = true
};
inputOption.Aliases.Add("-i");

var outputOption = new Option<string>("--output")
{
    Description = "Output PDF file path",
    DefaultValueFactory = _ => "schedule.pdf"
};
outputOption.Aliases.Add("-o");

var startDateOption = new Option<string?>("--start-date")
{
    Description = "Override start date (yyyy-MM-dd)"
};

var rootCommand = new RootCommand("Generate a printable medication schedule worksheet PDF");
rootCommand.Options.Add(inputOption);
rootCommand.Options.Add(outputOption);
rootCommand.Options.Add(startDateOption);

rootCommand.SetAction(parseResult =>
{
    var input = parseResult.GetValue(inputOption)!;
    var output = parseResult.GetValue(outputOption)!;
    var startDateStr = parseResult.GetValue(startDateOption);

    if (!input.Exists)
    {
        Console.Error.WriteLine($"Error: Input file not found: {input.FullName}");
        return 1;
    }

    var config = YamlConfigReader.Read(input.FullName);

    DateOnly startDate;
    if (!string.IsNullOrEmpty(startDateStr))
    {
        startDate = DateOnly.Parse(startDateStr);
    }
    else if (!string.IsNullOrEmpty(config.Schedule.StartDate))
    {
        startDate = DateOnly.Parse(config.Schedule.StartDate);
    }
    else
    {
        startDate = DateOnly.FromDateTime(DateTime.Today);
    }

    var days = ScheduleBuilder.Build(config, startDate);
    var footnotes = ConflictChecker.Check(days, config.Conflicts);

    var document = new ScheduleDocument(days, footnotes, config.Medications);
    document.GeneratePdf(output);

    Console.WriteLine($"Schedule generated: {output}");
    Console.WriteLine($"  Date range: {startDate:yyyy-MM-dd} to {startDate.AddDays(config.Schedule.Days - 1):yyyy-MM-dd}");
    Console.WriteLine($"  Medications: {config.Medications.Count}");
    Console.WriteLine($"  Conflict warnings: {footnotes.Count}");

    return 0;
});

return rootCommand.Parse(args).Invoke();
