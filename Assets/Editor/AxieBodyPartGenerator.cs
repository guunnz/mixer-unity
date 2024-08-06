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
            { SkillName.Mystic, BodyPart.Eyes },
            { SkillName.Chubby, BodyPart.Eyes },
            { SkillName.Dreamy, BodyPart.Eyes },
            { SkillName.Gecko, BodyPart.Eyes },
            { SkillName.Sleepless, BodyPart.Eyes },
            { SkillName.Bulb, BodyPart.Eyes },
            { SkillName.Papi, BodyPart.Eyes },
            { SkillName.Sleepy, BodyPart.Eyes },
            { SkillName.Goo, BodyPart.Eyes },
            { SkillName.Cute, BodyPart.Eyes },
            { SkillName.Clear, BodyPart.Eyes },
            { SkillName.Sky, BodyPart.Eyes },
            { SkillName.Confused, BodyPart.Eyes },
            { SkillName.Kawaii, BodyPart.Eyes },

            // Ears
            { SkillName.NutCracker, BodyPart.Ears },
            { SkillName.Zigzag, BodyPart.Ears },
            { SkillName.Hamaya, BodyPart.Ears },
            { SkillName.Innocent, BodyPart.Ears },
            { SkillName.Belieber, BodyPart.Ears },
            { SkillName.Nyan, BodyPart.Ears },
            { SkillName.Vector, BodyPart.Ears },
            { SkillName.Friezard, BodyPart.Ears },
            { SkillName.Humorless, BodyPart.Ears },
            { SkillName.Curly, BodyPart.Ears },
            { SkillName.Leafy, BodyPart.Ears },
            { SkillName.Pointy, BodyPart.Ears },

            // Horns
            { SkillName.Antenna, BodyPart.Horn },
            { SkillName.BambooShoot, BodyPart.Horn },
            { SkillName.Cactus, BodyPart.Horn },
            { SkillName.Kendama, BodyPart.Horn },
            { SkillName.LittleBranch, BodyPart.Horn },
            { SkillName.RoseBud, BodyPart.Horn },
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