using System;
using System.Collections;
using System.Collections.Generic;
using finished3;
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
    RiskyFeather,
    Merry,
    DualBlade,
    Jaguar,
    Shiba,
    Gerbil,
    Mosquito,
    Lagging,
    Caterpillars,
    Pliers,
    Parasite,
    LeafBug,
    BuzzBuzz,
    Scarab,
    Sandal,
    SpikyWing,
    Ant,
    TwinTail,
    FishSnack,
    GravelAnt,
    Pupae,
    ThornyCatterpilar,
    Catfish,
    Piranha,
    Babylonia,
    TealShell,
    Clamshell,
    Oranda,
    ShoalStar,
    Hermit,
    Goldfish,
    Perch,
    Nimo,
    Tadpole,
    Ranchu,
    Navaga,
    Shrimp,
    Herbivore,
    SilenceWhisper,
    RoseBud,
    StrawberryShortcake,
    Watermelon,
    Turnip,
    Bidens,
    WateringCan,
    Mint,
    Pumpkin,
    Yam,
    PotatoLeaf,
    HotButt,
    Doubletalk,
    HungryBird,
    LittleOwl,
    Eggshell,
    Cuckoo,
    Trump,
    WingHorn,
    FeatherSpear,
    Balloon,
    Cupid,
    Raven,
    PigeonPost,
    Kingfisher,
    TriFeather,
    Swallow,
    TheLastOne,
    Cloud,
    PostFight,
    ToothlessBite,
    RazorBite,
    TinyTurtle,
    Unko,
    ScalySpear,
    Cerastes,
    ScalySpoon,
    Incisor,
    Bumpy,
    BoneSail,
    GreenThorns,
    IndianStar,
    RedEar,
    Croc,
    WallGecko,
    Iguana,
    TinyDino,
    SnakeJar,
    Gila,
    GrassSnake,
}

public enum SkillTriggerType
{
    Active,
    Battlecry,
    PassivePermanent,
    PassiveOnDamageTaken,
    PassiveOnAttack,
}

public enum StatusEffectEnum
{
    None,
    Stun,
    Fear,
    Sleep,
    Jinx,
    Chill,
    Aroma,
    Stench,
    Fragile,
    Lethal,
    Poison,
    AttackPositive,
    SpeedPositive,
    MoralePositive,
    AttackNegative,
    SpeedNegative,
    MoraleNegative,
    Merry,
    Gecko,
    Lunge,
    Trump,
    Feather,
    Untargetable,
    HealingBlock,
    Wounds,
    FishSnack,
    CannotUseMeleeAbility,
    InmuneToDebuffs,
    HotButt,
    Kestrel,
    RangedReflect,
    MeleeReflect
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
    internal AxieBodyPart axieBodyPart;
    internal AxieController self;
    internal AxieController opponent;
    public float totalDuration;
    public float statusEffectsTiming;
    public float attackAudioTiming;
    internal bool debug;

    private void Start()
    {
        if (debug)
            return;
        StartCoroutine(LaunchSkill());
    }

    private IEnumerator LaunchSkill()
    {
        Invoke("SetStatusEffects", statusEffectsTiming == 0 ? totalDuration - 0.1f : statusEffectsTiming);

        string animationName = animationToPlay.ToString();

        StartCoroutine(Destroy(this.gameObject, totalDuration));

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
            yield return new WaitForSecondsRealtime(skill.SkillDelay);
            Vector3 pos = skill.VFXPrefab.transform.localPosition;
            GameObject vfxSpawned = Instantiate(skill.VFXPrefab,
                skill.StartFromOrigin ? origin.transform.position : target.transform.position,
                skill.VFXPrefab.transform.rotation,
                this.transform);

            vfxSpawned.transform.localPosition = new Vector3(vfxSpawned.transform.localPosition.x +
                                                             pos.x, vfxSpawned.transform.localPosition.y +
                                                                    pos.y, vfxSpawned.transform.localPosition.z +
                                                                           pos.z);

            VFXSkinChanger changer = vfxSpawned.GetComponent<VFXSkinChanger>();

            if (changer != null)
            {
                changer.ChangeBasedOnClass(@class);
            }

            if (skill.StartFromOrigin)
            {
                ProjectileMover projectileMover = vfxSpawned.GetComponent<ProjectileMover>();

                vfxSpawned.transform.localScale = new Vector3(
                    origin.transform.localScale.x > 0
                        ? -vfxSpawned.transform.localScale.x
                        : vfxSpawned.transform.localScale.x,
                    vfxSpawned.transform.localScale.y, vfxSpawned.transform.localScale.z);

                if (projectileMover != null)
                    projectileMover.MoveToTarget(this.target, skill.SkillDuration);
            }
        }
    }

    public IEnumerator LaunchSkillTest()
    {
        string animationName = animationToPlay.ToString();

        StartCoroutine(Destroy(this.gameObject, totalDuration));

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
            yield return new WaitForSecondsRealtime(skill.SkillDelay);
            Vector3 pos = skill.VFXPrefab.transform.localPosition;
            GameObject vfxSpawned = Instantiate(skill.VFXPrefab,
                skill.StartFromOrigin ? origin.transform.position : target.transform.position,
                skill.VFXPrefab.transform.rotation,
                this.transform);

            vfxSpawned.transform.localPosition = new Vector3(vfxSpawned.transform.localPosition.x +
                                                             pos.x, vfxSpawned.transform.localPosition.y +
                                                                    pos.y, vfxSpawned.transform.localPosition.z +
                                                                           pos.z);

            VFXSkinChanger changer = vfxSpawned.GetComponent<VFXSkinChanger>();

            if (changer != null)
            {
                changer.ChangeBasedOnClass(@class);
            }

            if (skill.StartFromOrigin)
            {
                ProjectileMover projectileMover = vfxSpawned.GetComponent<ProjectileMover>();

                vfxSpawned.transform.localScale = new Vector3(
                    origin.transform.localScale.x < 0
                        ? -vfxSpawned.transform.localScale.x
                        : vfxSpawned.transform.localScale.x,
                    vfxSpawned.transform.localScale.y, vfxSpawned.transform.localScale.z);

                if (origin.transform.localScale.x > 0)
                {
                    vfxSpawned.transform.localPosition -= new Vector3(pos.x * 2f, 0, 0);
                }

                if (projectileMover != null)
                    projectileMover.MoveToTarget(this.target, skill.SkillDuration);
            }
        }
    }

    private void SetStatusEffects()
    {
        if (axieBodyPart.statusEffects == null)
            return;

        foreach (var skillEffect in axieBodyPart.statusEffects)
        {
            StatusManager.Instance.SetStatus(skillEffect, self, opponent);
        }
    }

    private IEnumerator Destroy(GameObject obj, float timing)
    {
        yield return new WaitForSecondsRealtime(timing);
        skeletonAnimation.AnimationName = "action/idle/normal";
        Destroy(obj);
    }
}