using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class AxieBodyPartsGenerator
{
    [MenuItem("Axie/Generate Body Parts Manager")]
    public static void GenerateBodyPartsManager()
    {
        var manager = ScriptableObject.CreateInstance<AxieBodyPartsManager>();
        manager.axieBodyParts = new List<AxieBodyPart>();

            var skillToBodyPartMap = new Dictionary<SkillName, BodyPart>
        {
            // Eyes
            { SkillName.Mystic, BodyPart.Eye },
            { SkillName.Chubby, BodyPart.Eye },
            { SkillName.Dreamy, BodyPart.Eye },
            { SkillName.Gecko, BodyPart.Eye },
            { SkillName.Sleepless, BodyPart.Eye },
            { SkillName.Bulb, BodyPart.Eye },
            { SkillName.Papi, BodyPart.Eye },
            { SkillName.Sleepy, BodyPart.Eye },
            { SkillName.Goo, BodyPart.Eye },
            { SkillName.Cute, BodyPart.Eye },
            { SkillName.Clear, BodyPart.Eye },
            { SkillName.Sky, BodyPart.Eye },
            { SkillName.Confused, BodyPart.Eye },
            { SkillName.Kawaii, BodyPart.Eye },

            // Ears
            { SkillName.NutCracker, BodyPart.Ear },
            { SkillName.Zigzag, BodyPart.Ear },
            { SkillName.Hamaya, BodyPart.Ear },
            { SkillName.Innocent, BodyPart.Ear },
            { SkillName.Belieber, BodyPart.Ear },
            { SkillName.Nyan, BodyPart.Ear },
            { SkillName.Vector, BodyPart.Ear },
            { SkillName.Friezard, BodyPart.Ear },
            { SkillName.Humorless, BodyPart.Ear },
            { SkillName.Curly, BodyPart.Ear },
            { SkillName.Leafy, BodyPart.Ear },
            { SkillName.Pointy, BodyPart.Ear },

            // Horns
            { SkillName.Antenna, BodyPart.Horn },
            { SkillName.BambooShoot, BodyPart.Horn },
            { SkillName.Cactus, BodyPart.Horn },
            { SkillName.Kendama, BodyPart.Horn },
            { SkillName.LittleBranch, BodyPart.Horn },
            { SkillName.Rosebud, BodyPart.Horn },
            { SkillName.Beech, BodyPart.Horn },
            { SkillName.Arco, BodyPart.Horn },
            { SkillName.Shoal, BodyPart.Horn },
            { SkillName.Pocky, BodyPart.Horn },
            { SkillName.Kestrel, BodyPart.Horn },
            { SkillName.Anemone, BodyPart.Horn },

            // Mouths
            { SkillName.Confident, BodyPart.Mouth },
            { SkillName.CuteBunny, BodyPart.Mouth },
            { SkillName.Goda, BodyPart.Mouth },
            { SkillName.SquareTeeth, BodyPart.Mouth },
            { SkillName.AxieKiss, BodyPart.Mouth },
            { SkillName.Serious, BodyPart.Mouth },
            { SkillName.PeaceMaker, BodyPart.Mouth },
            { SkillName.Imp, BodyPart.Mouth },
            { SkillName.RiskyFish, BodyPart.Mouth },
            { SkillName.Lam, BodyPart.Mouth },
            { SkillName.Pincer, BodyPart.Mouth },
            { SkillName.Dango, BodyPart.Mouth },
            { SkillName.Kotaro, BodyPart.Mouth },

            // Backs
            { SkillName.BlueMoon, BodyPart.Back },
            { SkillName.Hero, BodyPart.Back },
            { SkillName.RiskyBeast, BodyPart.Back },
            { SkillName.Shiitake, BodyPart.Back },
            { SkillName.TriSpikes, BodyPart.Back },
            { SkillName.Timber, BodyPart.Back },
            { SkillName.Clover, BodyPart.Back },
            { SkillName.Sponge, BodyPart.Back },
            { SkillName.GarishWorm, BodyPart.Back },
            { SkillName.SnailShell, BodyPart.Back },
            { SkillName.Bunny, BodyPart.Back },

            // Tails
            { SkillName.GranmasFan, BodyPart.Tail },
            { SkillName.Hare, BodyPart.Tail },
            { SkillName.Cottontail, BodyPart.Tail },
            { SkillName.Koi, BodyPart.Tail },
            { SkillName.Rice, BodyPart.Tail },
            { SkillName.Carrot, BodyPart.Tail },
            { SkillName.Hatsune, BodyPart.Tail },
            { SkillName.FeatherFan, BodyPart.Tail },
            { SkillName.Brush, BodyPart.Tail },
            { SkillName.Cattail, BodyPart.Tail },
            { SkillName.Cucumber, BodyPart.Tail },
            { SkillName.Furball, BodyPart.Tail }
        };

        foreach (var pair in skillToBodyPartMap)
        {
            var bodyPart = ScriptableObject.CreateInstance<AxieBodyPart>();
            bodyPart.skillName = pair.Key;
            bodyPart.bodyPart = pair.Value;

            manager.AddBodyPart(bodyPart);

            // Save each AxieBodyPart as a ScriptableObject in the project
            AssetDatabase.CreateAsset(bodyPart, $"Assets/AxieBodyParts/{pair.Key}_{pair.Value}.asset");
        }

        // Save the AxieBodyPartsManager
        AssetDatabase.CreateAsset(manager, "Assets/AxieBodyPartsManager.asset");
        AssetDatabase.SaveAssets();
    }
}