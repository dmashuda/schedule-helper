using ScheduleHelper.Models;

namespace ScheduleHelper.Services;

public static class ConflictChecker
{
    private static readonly Dictionary<TimeBlock, int> BlockOrder = new()
    {
        [TimeBlock.Morning] = 0,
        [TimeBlock.Midday] = 1,
        [TimeBlock.Afternoon] = 2,
        [TimeBlock.Evening] = 3,
        [TimeBlock.Bedtime] = 4
    };

    // Approximate hours between adjacent blocks
    private static readonly int[] HoursBetweenAdjacentBlocks = [3, 2, 3, 2]; // M->Mid, Mid->A, A->E, E->B

    public static List<ConflictFootnote> Check(List<ScheduleDay> days, List<ConflictRule> conflictRules)
    {
        var footnotes = new List<ConflictFootnote>();

        foreach (var rule in conflictRules)
        {
            var footnote = new ConflictFootnote
            {
                Number = footnotes.Count + 1,
                Note = rule.Note,
                MedicationIds = rule.Medications
            };

            var medIdSet = new HashSet<string>(rule.Medications);
            bool hasConflict = false;

            foreach (var day in days)
            {
                // Build map: block -> set of conflict med IDs in that block
                var blockMedMap = new Dictionary<TimeBlock, HashSet<string>>();
                foreach (var (block, entries) in day.Entries)
                {
                    var ids = entries.Where(e => medIdSet.Contains(e.MedicationId))
                                     .Select(e => e.MedicationId).ToHashSet();
                    if (ids.Count > 0)
                        blockMedMap[block] = ids;
                }

                if (blockMedMap.Count == 0)
                    continue;

                // Check within same block: multiple different conflict meds at 0 hours apart
                foreach (var (block, ids) in blockMedMap)
                {
                    if (ids.Count >= 2 && 0 < rule.MinHoursApart)
                    {
                        hasConflict = true;
                        foreach (var entry in day.Entries[block])
                        {
                            if (medIdSet.Contains(entry.MedicationId) && !entry.ConflictFootnotes.Contains(footnote.Number))
                                entry.ConflictFootnotes.Add(footnote.Number);
                        }
                    }
                }

                // Check across different blocks
                var sortedBlocks = blockMedMap.Keys.OrderBy(b => BlockOrder[b]).ToList();
                for (int i = 0; i < sortedBlocks.Count; i++)
                {
                    for (int j = i + 1; j < sortedBlocks.Count; j++)
                    {
                        var block1 = sortedBlocks[i];
                        var block2 = sortedBlocks[j];

                        int hoursBetween = EstimateHoursBetween(block1, block2);
                        if (hoursBetween < rule.MinHoursApart)
                        {
                            hasConflict = true;
                            foreach (var entry in day.Entries[block1].Concat(day.Entries[block2]))
                            {
                                if (medIdSet.Contains(entry.MedicationId) && !entry.ConflictFootnotes.Contains(footnote.Number))
                                    entry.ConflictFootnotes.Add(footnote.Number);
                            }
                        }
                    }
                }
            }

            if (hasConflict)
                footnotes.Add(footnote);
        }

        return footnotes;
    }

    private static int EstimateHoursBetween(TimeBlock a, TimeBlock b)
    {
        int orderA = BlockOrder[a];
        int orderB = BlockOrder[b];
        if (orderA > orderB)
            (orderA, orderB) = (orderB, orderA);

        int total = 0;
        for (int i = orderA; i < orderB; i++)
            total += HoursBetweenAdjacentBlocks[i];
        return total;
    }
}
