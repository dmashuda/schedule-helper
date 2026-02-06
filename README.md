# Schedule Helper

A .NET command-line tool that generates printable medication schedule worksheets as PDFs. Define your medications, dosing schedules, and conflict rules in a YAML file, and the tool produces a landscape PDF with checkboxes for daily tracking.

## Features

- **YAML-driven configuration** — define medications, doses, frequencies, time blocks, and duration
- **Conflict detection** — specify minimum hours between medications; conflicts are flagged with footnotes in the PDF
- **Printable PDF output** — landscape A4 table with time-block rows, day columns, and checkboxes
- **Flexible scheduling** — 5 time blocks (Morning, Midday, Afternoon, Evening, Bedtime) with per-medication control

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download)

## Quick Start

```bash
# Build
dotnet build

# Generate a schedule from the sample input
dotnet run --project ScheduleHelper -- -i sample-input.yaml -o schedule.pdf
```

## Usage

```
schedule-helper -i <input.yaml> [-o <output.pdf>] [--start-date <yyyy-MM-dd>]
```

| Option | Description | Default |
|---|---|---|
| `-i`, `--input` | Path to the YAML configuration file (required) | — |
| `-o`, `--output` | Output PDF file path | `schedule.pdf` |
| `--start-date` | Override the start date | Value from YAML, or today |

## YAML Configuration

```yaml
schedule:
  days: 7
  start_date: "2026-02-06"

medications:
  - id: amoxicillin
    name: Amoxicillin
    dose: "500mg"
    frequency: "3x daily"
    duration_days: 10
    time_blocks: [Morning, Midday, Evening]
    notes: "Take with food"

  - id: iron
    name: Iron Supplement
    dose: "325mg"
    frequency: "1x daily"
    duration_days: 30
    time_blocks: [Midday]

conflicts:
  - medications: [amoxicillin, iron]
    min_hours_apart: 2
    note: "Iron reduces antibiotic absorption - take at least 2 hours apart"
```

### Time Blocks

`Morning` | `Midday` | `Afternoon` | `Evening` | `Bedtime`

## Disclaimer

This tool is a **scheduling aid only**. Always refer to your prescription and consult your pharmacist or doctor for authoritative dosing and interaction guidance. The generated schedule does not replace professional medical advice.

## Running Tests

```bash
dotnet test
```

## Dependencies

- [QuestPDF](https://www.questpdf.com/) — PDF generation (Community license)
- [YamlDotNet](https://github.com/aaubry/YamlDotNet) — YAML parsing
- [System.CommandLine](https://learn.microsoft.com/en-us/dotnet/standard/commandline/) — CLI argument parsing
