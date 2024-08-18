using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

[CustomEditor(typeof(AtiaAltarManager))]
public class AtiaAltarManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        AtiaAltarManager manager = (AtiaAltarManager)target;

        if (GUILayout.Button("Generate Random Atias"))
        {
            manager.Atia1 = GenerateRandomAtia();
            manager.Atia2 = GenerateRandomAtia();
            EditorUtility.SetDirty(manager);
        }

        if (GUILayout.Button("Merge Atias"))
        {
            manager.ResultAtia = manager.MergeAtias(manager.Atia1, manager.Atia2);
            Debug.Log("Atias merged successfully!");
            PrintResult(manager.ResultAtia);
            EditorUtility.SetDirty(manager);
        }
    }

    private AtiaAltar GenerateRandomAtia()
    {
        AtiaAltar atia = new AtiaAltar();
        Dictionary<string, float> randomStats = new Dictionary<string, float>()
        {
            { "Aqua", Random.value },
            { "Bird", Random.value },
            { "Dawn", Random.value },
            { "Plant", Random.value },
            { "Dusk", Random.value },
            { "Reptile", Random.value },
            { "Bug", Random.value },
            { "Beast", Random.value },
            { "Mech", Random.value }
        };

        // Normalize to ensure total adds up to 100%
        float total = randomStats.Values.Sum();
        Dictionary<string, float> normalizedStats = randomStats.ToDictionary(pair => pair.Key, pair => Mathf.Round((pair.Value / total) * 10000f) / 100f);

        // Adjust to ensure total is exactly 100%
        AdjustToExact100(normalizedStats);

        // Set small values to 0
        foreach (var key in normalizedStats.Keys.ToList())
        {
            if (normalizedStats[key] < 0.05f)
            {
                normalizedStats[key] = 0f;
            }
        }

        atia.SetClassChances(normalizedStats);
        return atia;
    }

    private void AdjustToExact100(Dictionary<string, float> stats)
    {
        float total = stats.Values.Sum();
        float difference = 100f - total;

        if (difference != 0)
        {
            // Find the key with the largest value and adjust it to make the total exactly 100%
            string keyToAdjust = stats.OrderByDescending(pair => pair.Value).First().Key;
            stats[keyToAdjust] = Mathf.Round((stats[keyToAdjust] + difference) * 100f) / 100f;
        }
    }

    private void PrintResult(AtiaAltar resultAtia)
    {
    }
}
