using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class Part
{
    public string part_id;
    public string part_type;
    public int part_value;
    public int part_skin;
    public string class_type;
    public string special_genes;
    public string type;
    public string name;
    public string ability_id;
}

[Serializable]
public class PartsData
{
    public List<Part> parts;
}

public static class PartFinder
{
    private static PartsData partsData;

    // Method to initialize the PartFinder with JSON data
    public static void Initialize(string jsonData)
    {
        partsData = JsonUtility.FromJson<PartsData>("{\"parts\":" + jsonData + "}");
    }

    public static string GetOriginalPartId(string abilityId)
    {
        var matchedParts = partsData.parts.Where(p => p.ability_id == abilityId).ToList();
        var originalPart = matchedParts.FirstOrDefault(p => string.IsNullOrEmpty(p.special_genes));

        return originalPart?.part_id ?? "No original part found";
    }

    // Static method to load JSON from Resources and initialize PartFinder
    public static void LoadFromResources()
    {
        TextAsset jsonTextFile = Resources.Load<TextAsset>("part_states");
        if (jsonTextFile != null)
        {
            Initialize(jsonTextFile.text);
        }
        else
        {
            Debug.LogError("Failed to load JSON data from resources.");
        }
    }
}