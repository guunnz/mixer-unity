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

    public static string GetOriginalPartId(string abilityId, string partName)
    {
        var matchedParts = partsData.parts.Where(p => p.ability_id == abilityId).ToList();
        var originalPart = matchedParts.FirstOrDefault(p => string.IsNullOrEmpty(p.special_genes));

        if (originalPart == null)
        {
            matchedParts = partsData.parts.Where(p => p.name.ToLower().Replace(" ", "") == partName.ToLower().Replace(" ", "")).ToList();
            originalPart = matchedParts.FirstOrDefault(p => p.name.ToLower().Replace(" ", "") == partName.ToLower().Replace(" ", ""));
            abilityId = originalPart.ability_id;
            matchedParts = partsData.parts.Where(p => p.ability_id == abilityId).ToList();
            originalPart = matchedParts.FirstOrDefault(p => string.IsNullOrEmpty(p.special_genes));
        }

        return originalPart?.part_id ?? "No original part found";
    }

    public static Part GetOriginalPartIdTesting(string partName, string bodypart = "")
    {
        partName = partName.ToLower().Replace(" ", "");
        bodypart = bodypart.ToLower();

        Part originalPart = partsData.parts
            .FirstOrDefault(p => p.name.ToLower().Replace(" ", "") == partName &&
                                 (string.IsNullOrEmpty(bodypart) || p.part_type.ToLower() == bodypart) &&
                                 string.IsNullOrEmpty(p.special_genes));

        if (originalPart == null)
        {
            originalPart = partsData.parts
                .Where(p => p.name.ToLower().Replace(" ", "") == partName &&
                            (string.IsNullOrEmpty(bodypart) || p.part_type.ToLower() == bodypart))
                .FirstOrDefault();

            if (originalPart != null)
            {
                string abilityId = originalPart.ability_id;

                originalPart = partsData.parts
                    .FirstOrDefault(p => p.ability_id == abilityId &&
                                         string.IsNullOrEmpty(p.special_genes));
            }
        }

        return originalPart;
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