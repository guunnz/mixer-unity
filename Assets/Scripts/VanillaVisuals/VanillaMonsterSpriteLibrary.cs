using System.Collections.Generic;
using UnityEngine;

public static class VanillaMonsterSpriteLibrary
{
    private const string PartsRoot = "VanillaMons/Parts";
    private const string AbilitiesRoot = "VanillaMons/Abilities";
    private static readonly Dictionary<string, Sprite> SpriteCache = new Dictionary<string, Sprite>();

    public static bool TryGetPartSprite(MonsterClass monsterClass, string partKey, out Sprite sprite)
    {
        sprite = LoadSprite($"{PartsRoot}/{monsterClass}/{partKey}");
        return sprite != null;
    }

    public static bool TryGetBodyPartSprite(MonsterClass monsterClass, BodyPart bodyPart, out Sprite sprite)
    {
        return TryGetPartSprite(monsterClass, BodyPartKey(bodyPart), out sprite);
    }

    public static bool TryGetAbilitySprite(MonsterBodyPart ability, out Sprite sprite)
    {
        sprite = null;
        if (ability == null)
            return false;

        if (ability.bodyPartSprite != null)
        {
            sprite = ability.bodyPartSprite;
            return true;
        }

        sprite = LoadSprite($"{AbilitiesRoot}/{ability.bodyPartClass}/{ability.skillName}");
        if (sprite != null)
            return true;

        return TryGetBodyPartSprite(ability.bodyPartClass, ability.bodyPart, out sprite);
    }

    public static string BodyPartKey(BodyPart bodyPart)
    {
        return bodyPart.ToString().ToLowerInvariant();
    }

    private static Sprite LoadSprite(string resourcePath)
    {
        if (SpriteCache.TryGetValue(resourcePath, out Sprite cachedSprite))
            return cachedSprite;

        Sprite sprite = Resources.Load<Sprite>(resourcePath);
        SpriteCache[resourcePath] = sprite;
        return sprite;
    }
}
