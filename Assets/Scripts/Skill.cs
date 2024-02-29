using System;
using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Serialization;
using CharacterInfo = finished3.CharacterInfo;

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
    HerosBane,

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
    Ronin,

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
    Furball,
    RiskyFeather
}


public class Skill : MonoBehaviour
{
    public List<SkillVFX> vfxToThrow = new List<SkillVFX>();
    internal Transform target;
    internal Transform origin;
    internal AxieClass @class;
    public float Damage;
    public AxieAnimation animationToPlay;
    internal SkeletonAnimation skeletonAnimation;
    internal BodyPart bodyPart;
    internal CharacterInfo opponent;
    public float totalDuration;

    private void Start()
    {
        StartCoroutine(LaunchSkill());
    }

    private IEnumerator LaunchSkill()
    {
        Destroy(this.gameObject, totalDuration);

        string animationName = animationToPlay.ToString();

// Find the last underscore and replace it with a hyphen
        int lastUnderscoreIndex = animationName.LastIndexOf('_');
        if (lastUnderscoreIndex != -1)
        {
            animationName = animationName.Substring(0, lastUnderscoreIndex) + "-" +
                            animationName.Substring(lastUnderscoreIndex + 1);
        }

// Replace the remaining underscores with slashes
        animationName = animationName.Replace("_", "/");

        skeletonAnimation.AnimationName = animationName;

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
                ProjectileMover projectileMover = vfxSpawned.GetComponent<ProjectileMover>();

                if (projectileMover != null)
                    projectileMover.MoveToTarget(this.target, skill.SkillDuration);
            }

            StartCoroutine(Destroy(vfxSpawned.gameObject, skill.SkillDuration));
        }

        opponent.HP -= this.Damage;
    }

    private IEnumerator Destroy(GameObject obj, float timing)
    {
        yield return new WaitForSeconds(timing);
        Destroy(obj);
    }
}