using System;
using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public class SkillVFX
{
    public float SkillDelay;
    public GameObject VFXPrefab;
    [FormerlySerializedAs("SkillTiming")] public float SkillDuration;
    public bool StartFromOrigin;
}

public enum BodyPart
{
    Eye,
    Ear,
    Horn,
    Mouth,
    Back,
    Tail
}

public enum SkillName
{
    // Eyes
    Mystic,
    Chubby,
    Dreamy,
    Gecko,
    Sleepless,
    Bulb,
    Papi,
    Sleepy,
    Goo,
    Cute,
    Clear,
    Sky,
    Confused,
    Kawaii,

    // Ears
    NutCracker,
    Zigzag,
    Hamaya,
    Innocent,
    Belieber,
    Nyan,
    Vector,
    Friezard,
    Humorless,
    Curly,
    Leafy,
    Pointy,

    // Horns
    Antenna,
    BambooShoot,
    Cactus,
    Kendama,
    LittleBranch,
    Rosebud,
    Beech,
    Arco,
    Shoal,
    Pocky,
    Kestrel,
    Anemone,

    // Mouths
    Confident,
    CuteBunny,
    Goda,
    SquareTeeth,
    AxieKiss,
    Serious,
    PeaceMaker,
    Imp,
    RiskyFish,
    Lam,
    Pincer,
    Dango,
    Kotaro,

    // Backs
    BlueMoon,
    Hero,
    RiskyBeast,
    Shiitake,
    TriSpikes,
    Timber,
    Clover,
    Sponge,
    GarishWorm,
    SnailShell,
    Bunny,

    // Tails
    GranmasFan,
    Hare,
    Cottontail,
    Koi,
    Rice,
    Carrot,
    Hatsune,
    FeatherFan,
    Brush,
    Cattail,
    Cucumber,
    Furball
}


public class Skill : MonoBehaviour
{
    public List<SkillVFX> vfxToThrow = new List<SkillVFX>();
    internal Transform target;
    internal Transform origin;
    internal AxieClass @class;
    internal BodyPart bodyPart;
    public float totalDuration;

    private void Start()
    {
        StartCoroutine(LaunchSkill());
    }

    private IEnumerator LaunchSkill()
    {
        Destroy(this.gameObject, totalDuration);

        foreach (SkillVFX skill in vfxToThrow)
        {
            yield return new WaitForSeconds(skill.SkillDelay);
            GameObject vfxSpawned = Instantiate(skill.VFXPrefab,
                skill.StartFromOrigin ? origin.transform.position : target.transform.position,
                skill.VFXPrefab.transform.rotation,
                null);
            VFXSkinChanger changer = vfxSpawned.GetComponent<VFXSkinChanger>();

            if (changer != null)
            {
                changer.ChangeBasedOnClass(@class);
            }

            if (skill.StartFromOrigin)
            {
                vfxSpawned.GetComponent<ProjectileMover>().MoveToTarget(this.target, skill.SkillDuration);
            }

            StartCoroutine(Destroy(vfxSpawned, skill.SkillDuration));
        }
    }

    private IEnumerator Destroy(GameObject obj, float timing)
    {
        yield return new WaitForSeconds(timing);
        Destroy(obj);
    }
}