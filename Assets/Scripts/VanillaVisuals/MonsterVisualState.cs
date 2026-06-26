using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public enum MonsterVisualState
{
    Idle,
    Run,
    Hover,
    Grabbed,
    AttackMelee,
    AttackRanged,
    Cast,
    Hit,
    Victory,
    Shrimp,
    Appear,
    Hidden
}

public static class VanillaMonsterIconUtility
{
    public static Sprite GetClassSprite(MonsterClass monsterClass)
    {
        return VanillaSpriteCache.Get(VanillaSpriteShape.Circle);
    }

    public static Sprite GetClassSprite(MonsterClass monsterClass, IEnumerable<MonsterClassGraphic> classGraphics)
    {
        Sprite serializedSprite = FindClassSprite(monsterClass, classGraphics);
        return serializedSprite != null ? serializedSprite : GetClassSprite(monsterClass);
    }

    public static Sprite GetBodyPartSprite(BodyPart bodyPart)
    {
        return VanillaSpriteCache.Get(GetBodyPartShape(bodyPart));
    }

    public static Sprite GetBodyPartSprite(BodyPart bodyPart, MonsterClass monsterClass)
    {
        return VanillaMonsterSpriteLibrary.TryGetBodyPartSprite(monsterClass, bodyPart, out Sprite sprite)
            ? sprite
            : GetBodyPartSprite(bodyPart);
    }

    public static Sprite GetAbilitySprite(MonsterBodyPart ability)
    {
        if (VanillaMonsterSpriteLibrary.TryGetAbilitySprite(ability, out Sprite sprite))
            return sprite;

        return ability != null ? GetBodyPartSprite(ability.bodyPart) : GetBodyPartSprite(BodyPart.None);
    }

    public static void ApplyClass(Image image, MonsterClass monsterClass)
    {
        if (image == null)
            return;

        image.enabled = true;
        image.sprite = GetClassSprite(monsterClass);
        image.color = MonsterClassPalette.Main(monsterClass);
        image.preserveAspect = true;
    }

    public static void ApplyClass(Image image, MonsterClass monsterClass, IEnumerable<MonsterClassGraphic> classGraphics)
    {
        if (image == null)
            return;

        Sprite serializedSprite = FindClassSprite(monsterClass, classGraphics);
        if (serializedSprite == null)
        {
            ApplyClass(image, monsterClass);
            return;
        }

        image.enabled = true;
        image.sprite = serializedSprite;
        image.color = Color.white;
        image.preserveAspect = true;
    }

    public static void ApplyClass(SpriteRenderer renderer, MonsterClass monsterClass)
    {
        if (renderer == null)
            return;

        renderer.sprite = GetClassSprite(monsterClass);
        renderer.color = MonsterClassPalette.Main(monsterClass);
    }

    public static void ApplyBodyPart(Image image, BodyPart bodyPart, MonsterClass monsterClass)
    {
        if (image == null)
            return;

        image.enabled = true;
        bool hasArtSprite = VanillaMonsterSpriteLibrary.TryGetBodyPartSprite(monsterClass, bodyPart, out Sprite sprite);
        image.sprite = hasArtSprite ? sprite : GetBodyPartSprite(bodyPart);
        image.color = hasArtSprite
            ? Color.white
            : Color.Lerp(MonsterClassPalette.BodyPartColor(bodyPart), MonsterClassPalette.Main(monsterClass), 0.3f);
        image.preserveAspect = true;
    }

    public static void ApplyBodyPart(Image image, MonsterBodyPart ability)
    {
        if (image == null)
            return;

        if (ability == null)
        {
            image.enabled = false;
            return;
        }

        bool hasArtSprite = VanillaMonsterSpriteLibrary.TryGetAbilitySprite(ability, out Sprite sprite);
        image.enabled = true;
        image.sprite = hasArtSprite ? sprite : GetBodyPartSprite(ability.bodyPart);
        image.color = hasArtSprite
            ? Color.white
            : Color.Lerp(MonsterClassPalette.BodyPartColor(ability.bodyPart), MonsterClassPalette.Main(ability.bodyPartClass), 0.3f);
        image.preserveAspect = true;
    }

    private static Sprite FindClassSprite(MonsterClass monsterClass, IEnumerable<MonsterClassGraphic> classGraphics)
    {
        if (classGraphics == null)
            return null;

        foreach (MonsterClassGraphic graphic in classGraphics)
        {
            if (graphic != null && graphic.monsterClass == monsterClass && graphic.monsterClassSprite != null)
                return graphic.monsterClassSprite;
        }

        return null;
    }

    public static VanillaSpriteShape GetBodyPartShape(BodyPart bodyPart)
    {
        switch (bodyPart)
        {
            case BodyPart.Eyes:
                return VanillaSpriteShape.Circle;
            case BodyPart.Ears:
            case BodyPart.Horn:
            case BodyPart.Tail:
                return VanillaSpriteShape.Triangle;
            case BodyPart.Mouth:
                return VanillaSpriteShape.RoundedBox;
            case BodyPart.Back:
                return VanillaSpriteShape.Diamond;
            default:
                return VanillaSpriteShape.Circle;
        }
    }
}

[Serializable]
public class MonsterVisualDescriptor
{
    public string MonsterId;
    public string DisplayName;
    public MonsterClass Class;
    public bool IsFree;
    public GetMonstersExample.Part[] Parts = Array.Empty<GetMonstersExample.Part>();

    public static MonsterVisualDescriptor FromMonster(GetMonstersExample.Monster monster)
    {
        if (monster == null)
            return Default();

        return new MonsterVisualDescriptor
        {
            MonsterId = monster.id,
            DisplayName = monster.name,
            Class = ResolveClass(monster.@class),
            IsFree = monster.f2p,
            Parts = monster.parts ?? Array.Empty<GetMonstersExample.Part>()
        };
    }

    public static MonsterVisualDescriptor FromController(MonsterController controller)
    {
        if (controller == null)
            return Default();

        return new MonsterVisualDescriptor
        {
            MonsterId = controller.MonsterId.ToString(),
            DisplayName = controller.MonsterId.ToString(),
            Class = controller.monsterIngameStats.monsterClass,
            Parts = Array.Empty<GetMonstersExample.Part>()
        };
    }

    public static MonsterVisualDescriptor Default(MonsterClass monsterClass = MonsterClass.Beast)
    {
        return new MonsterVisualDescriptor
        {
            MonsterId = string.Empty,
            DisplayName = string.Empty,
            Class = monsterClass,
            Parts = Array.Empty<GetMonstersExample.Part>()
        };
    }

    private static MonsterClass ResolveClass(string className)
    {
        if (string.IsNullOrWhiteSpace(className))
            return MonsterClass.Beast;

        return Enum.TryParse(className, true, out MonsterClass monsterClass)
            ? monsterClass
            : MonsterClass.Beast;
    }
}

public static class MonsterClassPalette
{
    public static Color Main(MonsterClass monsterClass)
    {
        switch (monsterClass)
        {
            case MonsterClass.Beast:
                return new Color(0.95f, 0.48f, 0.18f);
            case MonsterClass.Bug:
                return new Color(0.55f, 0.68f, 0.22f);
            case MonsterClass.Bird:
                return new Color(0.96f, 0.34f, 0.58f);
            case MonsterClass.Reptile:
                return new Color(0.52f, 0.38f, 0.74f);
            case MonsterClass.Plant:
                return new Color(0.28f, 0.68f, 0.32f);
            case MonsterClass.Aquatic:
                return new Color(0.18f, 0.67f, 0.86f);
            case MonsterClass.Mech:
                return new Color(0.55f, 0.61f, 0.66f);
            case MonsterClass.Dawn:
                return new Color(0.96f, 0.77f, 0.28f);
            case MonsterClass.Dusk:
                return new Color(0.28f, 0.32f, 0.58f);
            default:
                return Color.white;
        }
    }

    public static Color Accent(MonsterClass monsterClass)
    {
        Color main = Main(monsterClass);
        return Color.Lerp(main, Color.white, 0.45f);
    }

    public static Color Shadow(MonsterClass monsterClass)
    {
        Color main = Main(monsterClass);
        return Color.Lerp(main, Color.black, 0.35f);
    }

    public static Color BodyPartColor(BodyPart bodyPart)
    {
        switch (bodyPart)
        {
            case BodyPart.Eyes:
                return new Color(0.1f, 0.11f, 0.14f);
            case BodyPart.Ears:
                return new Color(0.92f, 0.82f, 0.62f);
            case BodyPart.Horn:
                return new Color(0.95f, 0.9f, 0.48f);
            case BodyPart.Mouth:
                return new Color(0.95f, 0.28f, 0.24f);
            case BodyPart.Back:
                return new Color(0.2f, 0.66f, 0.9f);
            case BodyPart.Tail:
                return new Color(0.92f, 0.55f, 0.22f);
            default:
                return Color.white;
        }
    }
}
