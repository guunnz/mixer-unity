using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class AtiaAltar
{
    public float Aqua;
    public float Bird;
    public float Dawn;
    public float Plant;
    public float Dusk;
    public float Reptile;
    public float Bug;
    public float Beast;
    public float Mech;

    public Dictionary<string, float> GetClassChances()
    {
        return new Dictionary<string, float>()
        {
            { "Aqua", Aqua },
            { "Bird", Bird },
            { "Dawn", Dawn },
            { "Beast", Beast },
            { "Bug", Bug },
            { "Mech", Mech },
            { "Plant", Plant },
            { "Reptile", Reptile },
            { "Dusk", Dusk }
        };
    }

    public void SetClassChances(Dictionary<string, float> classChances)
    {
        Aqua = classChances.ContainsKey("Aqua") ? classChances["Aqua"] : 0f;
        Bird = classChances.ContainsKey("Bird") ? classChances["Bird"] : 0f;
        Dawn = classChances.ContainsKey("Dawn") ? classChances["Dawn"] : 0f;
        Plant = classChances.ContainsKey("Plant") ? classChances["Plant"] : 0f;
        Dusk = classChances.ContainsKey("Dusk") ? classChances["Dusk"] : 0f;
        Reptile = classChances.ContainsKey("Reptile") ? classChances["Reptile"] : 0f;
        Bug = classChances.ContainsKey("Bug") ? classChances["Bug"] : 0f;
        Beast = classChances.ContainsKey("Beast") ? classChances["Beast"] : 0f;
        Mech = classChances.ContainsKey("Mech") ? classChances["Mech"] : 0f;
    }
}

public class AtiaAltarManager : MonoBehaviour
{
    public AtiaAltar Atia1;
    public AtiaAltar Atia2;
    public AtiaAltar ResultAtia;
    private float weight => 1 + Weight / 100f;  // Weight value to control the drastic effects
    public float Weight = 0;
    private Dictionary<int, float> Boosts = new Dictionary<int, float>()
    {
        { 1, 1.4f },
        { 2, 1.4f },
        { 3, 1.2f },
        { 4, 1.0f },
        { 5, 0.8f }
    };

    public float removalThreshold = 5f;

    public AtiaAltar MergeAtias(AtiaAltar atia1, AtiaAltar atia2)
    {
        var classChances1 = atia1.GetClassChances();
        var classChances2 = atia2.GetClassChances();

        List<float> averages = new List<float>();
        List<float> differences = new List<float>();

        foreach (var stat in classChances1.Keys)
        {
            float avg = CalculateAverage(classChances1[stat], classChances2[stat]);
            float diff = CalculateDifference(classChances1[stat], classChances2[stat], avg);
            averages.Add(avg);
            differences.Add(diff);
        }

        int[] ranks = CalculateRank(differences);
        List<float> adjustedValues = new List<float>();

        for (int i = 0; i < differences.Count; i++)
        {
            adjustedValues.Add(CalculateAdjustedValue(differences[i], ranks[i]));
        }

        List<float> newAtiaResults = CalculateNewAtiaResult(adjustedValues);

        // Apply weight to make the drastic changes
        ApplyDrasticWeight(ref newAtiaResults);

        // Combine the new values into a merged Atia
        Dictionary<string, float> mergedStats = new Dictionary<string, float>();
        int index = 0;
        foreach (var stat in classChances1.Keys)
        {
            mergedStats[stat] = newAtiaResults[index];
            index++;
        }

        // Apply the removal and redistribution rules after generating the Atia
        ApplyRemovalAndRedistributionRules(mergedStats, classChances1, classChances2, differences, adjustedValues);

        // Adjust to ensure the values have at most 2 decimals and sum up to exactly 100%
        AdjustToExact100WithTwoDecimals(mergedStats);

        AtiaAltar resultAtia = new AtiaAltar();
        resultAtia.SetClassChances(mergedStats);

        return resultAtia;
    }

    private float CalculateAverage(float stat1, float stat2)
    {
        return (stat1 + stat2) / 2;
    }

    private float CalculateDifference(float stat1, float stat2, float average)
    {
        return Math.Abs(stat1 - stat2) + (stat1 + stat2) + average;
    }

    private int[] CalculateRank(List<float> differences)
    {
        return differences
            .Select((value, index) => new { Value = value, Index = index })
            .OrderByDescending(x => x.Value)
            .Select((x, i) => new { x.Index, Rank = i + 1 })
            .OrderBy(x => x.Index)
            .Select(x => x.Rank)
            .ToArray();
    }

    private float CalculateAdjustedValue(float difference, int rank)
    {
        return difference * (Boosts.ContainsKey(rank) ? Boosts[rank] : Boosts[5]);
    }

    private List<float> CalculateNewAtiaResult(List<float> adjustedValues)
    {
        float totalAdjustedValue = adjustedValues.Sum();
        return adjustedValues.Select(value => (value / totalAdjustedValue) * 100).ToList();
    }

    private void ApplyDrasticWeight(ref List<float> results)
    {
        // Amplify the differences based on the weight
        for (int i = 0; i < results.Count; i++)
        {
            if (results[i] >= 50)
            {
                // Increase higher values even more based on the weight
                results[i] += (results[i] - 50) * (weight - 1);
            }
            else
            {
                // Decrease lower values further based on the weight
                results[i] -= (50 - results[i]) * (weight - 1);
            }
        }
    }

    private void ApplyRemovalAndRedistributionRules(Dictionary<string, float> mergedStats, Dictionary<string, float> classChances1, Dictionary<string, float> classChances2, List<float> differences, List<float> adjustedValues)
    {
        var sortedStats = mergedStats
            .Where(x => x.Value > 0) // Exclude 0% stats
            .OrderByDescending(x => x.Value)
            .ToList();

        foreach(var stat in sortedStats)
        {
            Debug.Log(stat.Key + " " + stat.Value);
        }

        var topTwoStats = sortedStats.Take(2).Select(x => x.Key).ToHashSet();
        float purity = CalculatePurity(mergedStats);

        // Top 2 Redistribution
        for (int i = 0; i < sortedStats.Count && i < 2; i++)
        {
            string topStat = sortedStats[i].Key;
            float originalValue = Math.Max(classChances1[topStat], classChances2[topStat]);
            float profit = mergedStats[topStat] - originalValue;
            float redistributionChance = (differences[i] / adjustedValues[i]) / 100.0f * purity;

            if (profit > 0 && UnityEngine.Random.value < redistributionChance)
            {
                RedistributeProfit(mergedStats, topStat, profit);
                Debug.Log($"Top stat '{topStat}' redistributed its profit of {profit}% due to redistribution chance.");
            }
        }

        // Bottom 2 Removal
        if (sortedStats.Count > 2)
        {
            var bottomStats = sortedStats.Skip(sortedStats.Count - 2).ToList();

            foreach (var stat in bottomStats)
            {
                if (stat.Value < removalThreshold && !topTwoStats.Contains(stat.Key))
                {
                    float removalChance = (differences[sortedStats.IndexOf(stat)] / adjustedValues[sortedStats.IndexOf(stat)]) / 100.0f * purity;

                    if (UnityEngine.Random.value < removalChance)
                    {
                        Debug.Log($"Class '{stat.Key}' removed due to being below {removalThreshold}% and removal chance succeeded.");
                        mergedStats.Remove(stat.Key);
                    }
                }
            }
        }
        else if (sortedStats.Count == 2) // Special case for only 2 remaining stats
        {
            // Treat the lower stat as a bottom stat
            string bottomStat = sortedStats[1].Key;
            if (mergedStats[bottomStat] < removalThreshold)
            {
                float removalChance = (differences[sortedStats.IndexOf(sortedStats[1])] / adjustedValues[sortedStats.IndexOf(sortedStats[1])]) / 100.0f * purity;

                if (UnityEngine.Random.value < removalChance)
                {
                    Debug.Log($"Class '{bottomStat}' removed due to being below {removalThreshold}% and removal chance succeeded.");
                    mergedStats.Remove(bottomStat);
                }
            }
        }
        else if (sortedStats.Count == 1) // Special case for only 1 remaining stat
        {
            Debug.Log($"Only one stat '{sortedStats[0].Key}' remains, no further redistribution or removal possible.");
        }

        // Recalculate percentages for remaining stats
        float totalStats = mergedStats.Values.Sum();
        foreach (var stat in mergedStats.Keys.ToList())
        {
            mergedStats[stat] = (mergedStats[stat] / totalStats) * 100.0f;
        }
    }

    private void RedistributeProfit(Dictionary<string, float> mergedStats, string statName, float profit)
    {
        mergedStats[statName] -= profit;

        float totalStats = mergedStats.Values.Sum();
        foreach (var key in mergedStats.Keys.ToList())
        {
            if (key != statName)
            {
                mergedStats[key] += profit * (mergedStats[key] / totalStats);
            }
        }
    }

    private float CalculatePurity(Dictionary<string, float> classChances)
    {
        return classChances.Count(x => x.Value > 0);
    }

    private void AdjustToExact100WithTwoDecimals(Dictionary<string, float> stats)
    {
        foreach (var key in stats.Keys.ToList())
        {
            stats[key] = Mathf.Round(stats[key] * 100f) / 100f;
        }

        float total = stats.Values.Sum();
        float difference = 100f - total;

        if (difference != 0)
        {
            // Adjust the stat that was rounded the most to correct the total
            string keyToAdjust = stats.OrderByDescending(pair => Mathf.Abs(pair.Value - Mathf.Round(pair.Value * 100f) / 100f)).First().Key;
            stats[keyToAdjust] += difference;
            stats[keyToAdjust] = Mathf.Round(stats[keyToAdjust] * 100f) / 100f; // Re-round to two decimals
        }
    }

    private void PrintResult()
    {
        //Debug.Log("Merged Atia Stats:");
        //Debug.Log($"Aqua: {ResultAtia.Aqua}");
        //Debug.Log($"Bird: {ResultAtia.Bird}");
        //Debug.Log($"Dawn: {ResultAtia.Dawn}");
        //Debug.Log($"Plant: {ResultAtia.Plant}");
        //Debug.Log($"Dusk: {ResultAtia.Dusk}");
        //Debug.Log($"Reptile: {ResultAtia.Reptile}");
        //Debug.Log($"Bug: {ResultAtia.Bug}");
        //Debug.Log($"Beast: {ResultAtia.Beast}");
        //Debug.Log($"Mech: {ResultAtia.Mech}");
    }
}