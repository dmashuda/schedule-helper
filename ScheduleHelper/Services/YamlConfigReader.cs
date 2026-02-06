using ScheduleHelper.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ScheduleHelper.Services;

public static class YamlConfigReader
{
    public static InputFile Read(string path)
    {
        var yaml = File.ReadAllText(path);
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .Build();

        var input = deserializer.Deserialize<InputFile>(yaml)
            ?? throw new InvalidOperationException("Failed to deserialize YAML input.");

        Validate(input);
        return input;
    }

    private static void Validate(InputFile input)
    {
        var medIds = new HashSet<string>(input.Medications.Select(m => m.Id));

        foreach (var conflict in input.Conflicts)
        {
            foreach (var id in conflict.Medications)
            {
                if (!medIds.Contains(id))
                    throw new InvalidOperationException(
                        $"Conflict references unknown medication ID '{id}'. Known IDs: {string.Join(", ", medIds)}");
            }
        }
    }
}
