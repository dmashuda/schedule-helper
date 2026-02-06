# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Run Commands

```bash
# Build
dotnet build

# Run tests (xUnit)
dotnet test
dotnet test --filter "FullyQualifiedName~ScheduleBuilderTests.Build_ReturnsCorrectNumberOfDays"

# Run the app
dotnet run --project ScheduleHelper -- -i sample-input.yaml -o schedule.pdf

# Format code
dotnet format
```

## Quality Gates

After every change:
1. `dotnet test` must pass — run tests and fix any failures before considering work complete.
2. `dotnet format` must be run for consistent code formatting.

## Tech Stack

- .NET 10 (net10.0), C# with nullable enabled
- **QuestPDF 2025.12.4** — PDF generation (Community license)
- **YamlDotNet 16.3.0** — YAML deserialization
- **System.CommandLine 2.0.2** — CLI argument parsing (stable release, not beta)
- **xUnit** — test framework

## Architecture

The app reads a YAML config describing medications, their schedules, and conflict rules, then generates a printable PDF worksheet with checkboxes.

**Pipeline:** YAML input → InputModels → ScheduleBuilder (grid) → ConflictChecker (annotate) → ScheduleDocument (PDF)

- `Models/InputModels.cs` — YAML-mapped POCOs (`InputFile`, `MedicationRule`, `ConflictRule`). Uses `[YamlMember(Alias = "...")]` attributes.
- `Models/ScheduleModels.cs` — Computed grid types (`ScheduleDay`, `ScheduleEntry`, `ConflictFootnote`). `TimeBlock` enum defines the 5 time slots.
- `Services/YamlConfigReader` — Deserializes YAML, validates conflict medication IDs exist.
- `Services/ScheduleBuilder` — Expands medication rules into a `List<ScheduleDay>` grid, filtering by `DurationDays`.
- `Services/ConflictChecker` — Detects time-block proximity conflicts using estimated hours between blocks. Attaches footnote numbers to affected `ScheduleEntry` objects.
- `Pdf/ScheduleDocument` — QuestPDF `IDocument` implementation. Renders landscape A4 table (rows=time blocks, cols=days) with checkboxes and conflict footnotes.

## System.CommandLine 2.0.2 API Notes

The stable 2.0.2 release differs significantly from the beta API found in most tutorials:
- Constructor: `new Option<T>("--name")` — add aliases via `option.Aliases.Add("-x")`
- Use `Required` property (not `IsRequired`)
- Use `DefaultValueFactory = _ => value` (not `getDefaultValue:` constructor param)
- Use `SetAction(parseResult => { ... })` (not `SetHandler`)
- Use `parseResult.GetValue(option)` to retrieve values
- Invoke: `rootCommand.Parse(args).Invoke()` (not `rootCommand.Invoke(args)`)

## Test Conventions

Tests are in `ScheduleHelper.Tests/` using xUnit `[Fact]` attributes. Test fixtures (YAML files) go in `ScheduleHelper.Tests/Fixtures/` and are copied to output via `CopyToOutputDirectory`. Services are static classes tested directly without DI.
