using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AtiaSimulator : MonoBehaviour
{
    public AtiaAltar Atia1;
    public AtiaAltar Atia2;
    public AtiaAltar ResultAtia;

    private float weight => 1 + Weight / 100f;  // Weight value to control the drastic effects
    public float Weight = 0;
    public int DissapearMultiplier = 1;

    public TMP_InputField[] atia1Fields;
    public TMP_InputField[] atia2Fields;
    public TextMeshProUGUI[] results;
    public TextMeshProUGUI atia1SumText;
    public TextMeshProUGUI atia2SumText;
    public Button randomizeButton1;
    public Button randomizeButton2;
    public Button mergeButton;

    private Dictionary<int, float> Boosts = new Dictionary<int, float>()
    {
        { 1, 1.4f },
        { 2, 1.4f },
        { 3, 1.2f },
        { 4, 1.0f },
        { 5, 0.8f }
    };

    public float removalThreshold = 5f;

    private void Start()
    {
        // Initialize all input fields to zero
        InitializeInputFields(atia1Fields);
        InitializeInputFields(atia2Fields);

        // Set up event listeners for input fields
        foreach (var field in atia1Fields)
        {
            field.onValueChanged.AddListener(delegate { UpdateSumText(atia1Fields, atia1SumText); });
        }

        foreach (var field in atia2Fields)
        {
            field.onValueChanged.AddListener(delegate { UpdateSumText(atia2Fields, atia2SumText); });
        }

        randomizeButton1.onClick.AddListener(() => RandomizeAtiaStats(atia1Fields));
        randomizeButton2.onClick.AddListener(() => RandomizeAtiaStats(atia2Fields));
        mergeButton.onClick.AddListener(MergeAtias);

        // Update the initial sum display
        UpdateSumText(atia1Fields, atia1SumText);
        UpdateSumText(atia2Fields, atia2SumText);
    }

    private void InitializeInputFields(TMP_InputField[] fields)
    {
        foreach (var field in fields)
        {
            field.text = "0";
        }
    }

    private void RandomizeAtiaStats(TMP_InputField[] fields)
    {
        AtiaAltar randomAtia = GenerateRandomAtia();
        UpdateInputFields(fields, randomAtia);
        UpdateSumText(fields, fields == atia1Fields ? atia1SumText : atia2SumText);
    }

    private AtiaAltar GenerateRandomAtia()
    {
        AtiaAltar atia = new AtiaAltar();
        Dictionary<string, float> randomStats = new Dictionary<string, float>()
    {
        { "Aqua", Random.value },
        { "Bird", Random.value },
        { "Dawn", Random.value },
        { "Beast", Random.value },
        { "Bug", Random.value },
        { "Mech", Random.value },
        { "Plant", Random.value },
        { "Reptile", Random.value },
        { "Dusk", Random.value }
    };

        // Normalize to ensure total adds up to 100%
        float total = randomStats.Values.Sum();
        Dictionary<string, float> normalizedStats = randomStats.ToDictionary(pair => pair.Key, pair => (pair.Value / total) * 100f);

        // Set any value below 1% to 0% and re-normalize
        foreach (var key in normalizedStats.Keys.ToList())
        {
            if (normalizedStats[key] < 1f)
            {
                normalizedStats[key] = 0f;
            }
        }

        // Recalculate the total after setting values below 1% to 0%
        total = normalizedStats.Values.Sum();

        // If the total isn't 100%, adjust the remaining stats proportionally
        if (total > 0)
        {
            foreach (var key in normalizedStats.Keys.ToList())
            {
                normalizedStats[key] = (normalizedStats[key] / total) * 100f;
            }
        }

        // Finally, ensure the stats sum to exactly 100% by making a small adjustment
        AdjustToExact100WithTwoDecimals(normalizedStats);

        atia.SetClassChances(normalizedStats);
        return atia;
    }

    private void UpdateInputFields(TMP_InputField[] fields, AtiaAltar atia)
    {
        Dictionary<string, float> classChances = atia.GetClassChances();
        int index = 0;
        foreach (var stat in classChances.Values)
        {
            fields[index].text = stat.ToString("F2");
            index++;
        }
    }

    private void UpdateSumText(TMP_InputField[] fields, TextMeshProUGUI sumText)
    {
        float sum = 0f;
        foreach (var field in fields)
        {
            sum += float.Parse(field.text);
        }
        sumText.text = $"{sum:F2}%";
    }

    private void MergeAtias()
    {
        Atia1 = GetAtiaFromInputFields(atia1Fields);
        Atia2 = GetAtiaFromInputFields(atia2Fields);
        ResultAtia = MergeAtias(Atia1, Atia2);
        DisplayResult(ResultAtia);
    }

    private AtiaAltar GetAtiaFromInputFields(TMP_InputField[] fields)
    {
        AtiaAltar atia = new AtiaAltar();
        atia.Aqua = float.Parse(fields[0].text);
        atia.Bird = float.Parse(fields[1].text);
        atia.Dawn = float.Parse(fields[2].text);
        atia.Beast = float.Parse(fields[3].text);
        atia.Bug = float.Parse(fields[4].text);
        atia.Mech = float.Parse(fields[5].text);
        atia.Plant = float.Parse(fields[6].text);
        atia.Reptile = float.Parse(fields[7].text);
        atia.Dusk = float.Parse(fields[8].text);
        return atia;
    }

    private void DisplayResult(AtiaAltar result)
    {
        results[0].text = $"{result.Aqua:F2}%";
        results[1].text = $"{result.Bird:F2}%";
        results[2].text = $"{result.Dawn:F2}%";
        results[3].text = $"{result.Beast:F2}%";
        results[4].text = $"{result.Bug:F2}%";
        results[5].text = $"{result.Mech:F2}%";
        results[6].text = $"{result.Plant:F2}%";
        results[7].text = $"{result.Reptile:F2}%";
        results[8].text = $"{result.Dusk:F2}%";
    }

    // Merging logic based on your algorithm
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
        return System.Math.Abs(stat1 - stat2) + (stat1 + stat2) + average;
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
            if (results[i] > 0)
            {
                if (results[i] >= 50)
                {
                    // Increase higher values even more based on the weight
                    results[i] += (results[i] - 50) * (weight - 1);
                }
                else
                {
                    // Decrease lower values further based on the weight, but not below 0
                    results[i] -= (50 - results[i]) * (weight - 1);
                    if (results[i] < 0)
                    {
                        results[i] = 0;
                    }
                }
            }
        }
    }


    private void ApplyRemovalAndRedistributionRules(Dictionary<string, float> mergedStats, Dictionary<string, float> classChances1, Dictionary<string, float> classChances2, List<float> differences, List<float> adjustedValues)
    {
        var sortedStats = mergedStats
            .Where(x => x.Value > 0) // Exclude 0% stats
            .OrderByDescending(x => x.Value)
            .ToList();

        var topTwoStats = sortedStats.Take(2).Select(x => x.Key).ToHashSet();
        float purity = CalculatePurity(mergedStats) * DissapearMultiplier;

        // Top 2 Redistribution
        for (int i = 0; i < sortedStats.Count && i < 2; i++)
        {
            string topStat = sortedStats[i].Key;
            float originalValue = System.Math.Max(classChances1[topStat], classChances2[topStat]);
            float profit = mergedStats[topStat] - originalValue;
            float redistributionChance = (differences[i] / adjustedValues[i]) / 100.0f * purity;

            if (profit > 0 && UnityEngine.Random.value < redistributionChance)
            {
                RedistributeProfit(mergedStats, topStat, profit);
                Debug.Log($"Top stat '{topStat}' redistributed its profit of {profit}% due to redistribution chance.");
            }
        }

        // Special case for exactly 3 remaining stats
        if (sortedStats.Count == 3)
        {
            // Consider the highest as the top stat and the other two as bottom stats
            var bottomStats = sortedStats.Skip(1).ToList();

            foreach (var stat in bottomStats)
            {
                if (stat.Value < removalThreshold)
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
        // Bottom 2 Removal for more than 3 stats
        else if (sortedStats.Count > 3)
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
        // First, round each stat to two decimal places
        foreach (var key in stats.Keys.ToList())
        {
            stats[key] = Mathf.Round(stats[key] * 100f) / 100f;
        }

        // Calculate the total sum after rounding
        float total = stats.Values.Sum();
        float difference = 100f - total;

        if (Mathf.Abs(difference) > 0.01f) // If there's a difference greater than a small tolerance
        {
            // Adjust the stat that was rounded the most to correct the total
            string keyToAdjust = stats.OrderByDescending(pair => Mathf.Abs(pair.Value - Mathf.Round(pair.Value * 100f) / 100f)).First().Key;
            stats[keyToAdjust] += difference;
            stats[keyToAdjust] = Mathf.Round(stats[keyToAdjust] * 100f) / 100f; // Re-round to two decimals
        }
    }
}
