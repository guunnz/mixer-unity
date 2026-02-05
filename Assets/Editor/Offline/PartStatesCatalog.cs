using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor-only loader for `Assets/Resources/part_states.json`.
/// We use it to offer dropdown selections for parts without requiring users to type gene hex strings.
/// </summary>
public static class PartStatesCatalog
{
    [Serializable]
    private class PartState
    {
        public string part_id;
        public string part_type;
        public int part_value;
        public int part_skin;
        public string @class; // JSON key is "class"
        public string special_genes;
        public string name;
        public string ability_id;
    }

    [Serializable]
    private class Wrapper
    {
        public List<PartState> parts;
    }

    private const string PartStatesAssetPath = "Assets/Resources/part_states.json";
    private static List<PartState> cached;

    public static bool TryLoad(out string error)
    {
        error = null;
        if (cached != null)
            return true;

        TextAsset textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(PartStatesAssetPath);
        if (textAsset == null)
        {
            error = $"Missing `{PartStatesAssetPath}`.";
            return false;
        }

        try
        {
            // File is a JSON array, wrap it to use JsonUtility.
            string wrapped = "{\"parts\":" + textAsset.text + "}";
            var w = JsonUtility.FromJson<Wrapper>(wrapped);
            if (w?.parts == null)
            {
                error = "Failed to parse part_states.json (no parts list).";
                return false;
            }

            // Prefer original parts: special_genes null/empty.
            cached = w.parts
                .Where(p => p != null && !string.IsNullOrEmpty(p.part_id) && !string.IsNullOrEmpty(p.part_type) && !string.IsNullOrEmpty(p.ability_id))
                .Where(p => string.IsNullOrEmpty(p.special_genes))
                .ToList();

            return true;
        }
        catch (Exception ex)
        {
            error = "Failed to parse part_states.json: " + ex.Message;
            return false;
        }
    }

    public static IReadOnlyList<string> GetOptions(string partType)
    {
        if (!TryLoad(out _))
            return Array.Empty<string>();

        return cached
            .Where(p => string.Equals(p.part_type, partType, StringComparison.OrdinalIgnoreCase))
            .OrderBy(p => p.@class)
            .ThenBy(p => p.part_value)
            .ThenBy(p => p.name)
            .Select(p => $"{p.@class}/{p.part_type}: {p.name} ({p.part_id})")
            .ToList();
    }

    public static bool TryGetByPartTypeAndIndex(string partType, int index, out string abilityId, out string partClass, out int partValue, out int partSkin)
    {
        abilityId = null;
        partClass = null;
        partValue = 0;
        partSkin = 0;

        if (!TryLoad(out _))
            return false;

        var list = cached
            .Where(p => string.Equals(p.part_type, partType, StringComparison.OrdinalIgnoreCase))
            .OrderBy(p => p.@class)
            .ThenBy(p => p.part_value)
            .ThenBy(p => p.name)
            .ToList();

        if (index < 0 || index >= list.Count)
            return false;

        var psel = list[index];
        abilityId = psel.ability_id;
        partClass = psel.@class;
        partValue = psel.part_value;
        partSkin = psel.part_skin;
        return true;
    }
}

