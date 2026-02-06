using ScheduleHelper.Services;

namespace ScheduleHelper.Tests;

public class YamlConfigReaderTests
{
    [Fact]
    public void Read_ValidYaml_ParsesCorrectly()
    {
        var yaml = """
            schedule:
              days: 3
              start_date: "2026-01-01"

            medications:
              - id: aspirin
                name: Aspirin
                dose: "100mg"
                frequency: "1x daily"
                duration_days: 5
                time_blocks: [Morning]

            conflicts: []
            """;

        var path = Path.GetTempFileName();
        try
        {
            File.WriteAllText(path, yaml);
            var input = YamlConfigReader.Read(path);

            Assert.Equal(3, input.Schedule.Days);
            Assert.Equal("2026-01-01", input.Schedule.StartDate);
            Assert.Single(input.Medications);
            Assert.Equal("aspirin", input.Medications[0].Id);
            Assert.Equal("Aspirin", input.Medications[0].Name);
            Assert.Equal("100mg", input.Medications[0].Dose);
            Assert.Equal(5, input.Medications[0].DurationDays);
            Assert.Equal(["Morning"], input.Medications[0].TimeBlocks);
            Assert.Empty(input.Conflicts);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Read_UnknownMedicationInConflict_ThrowsInvalidOperationException()
    {
        var yaml = """
            schedule:
              days: 1
            medications:
              - id: real_med
                name: Real
                dose: "10mg"
                frequency: "1x daily"
                duration_days: 1
                time_blocks: [Morning]
            conflicts:
              - medications: [real_med, ghost_med]
                min_hours_apart: 2
                note: "This references a non-existent medication"
            """;

        var path = Path.GetTempFileName();
        try
        {
            File.WriteAllText(path, yaml);
            var ex = Assert.Throws<InvalidOperationException>(() => YamlConfigReader.Read(path));
            Assert.Contains("ghost_med", ex.Message);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void Read_NonExistentFile_ThrowsIOException()
    {
        var path = Path.Combine(Path.GetTempPath(), "nonexistent_" + Guid.NewGuid() + ".yaml");
        Assert.Throws<FileNotFoundException>(() => YamlConfigReader.Read(path));
    }

    [Fact]
    public void Read_FixtureFile_ParsesCorrectly()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Fixtures", "test-input.yaml");
        var input = YamlConfigReader.Read(path);

        Assert.Equal(3, input.Schedule.Days);
        Assert.Equal(3, input.Medications.Count);
        Assert.Single(input.Conflicts);
    }
}
