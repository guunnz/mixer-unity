using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using finished3;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public class SkillVFX
{
    public float ActivateTiming;
    public GameObject VFXPrefab;
    public bool StartFromOrigin;
    public float SkillDuration;
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
    OnReactivationPerSeconds
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

public class DamageTargetPair
{
    public int axieId;
    public float Value;

    public DamageTargetPair(int axieId, float value)
    {
        this.axieId = axieId;
        Value = value;
    }
}

public class HealTargetPair
{
    public int axieId;
    public float Value;

    public HealTargetPair(int axieId, float value)
    {
        this.axieId = axieId;
        Value = value;
    }
}

public class StatusEffectTargetPair
{
    public int axieId;
    public SkillEffect[] skillEffects;

    public StatusEffectTargetPair(int axieId, SkillEffect[] value)
    {
        this.axieId = axieId;
        skillEffects = value;
    }
}

public class Skill : MonoBehaviour
{
    public List<SkillVFX> vfxToThrow = new List<SkillVFX>();
    public AxieAnimation animationToPlay;
    internal AxieClass @class;
    internal SkeletonAnimation skeletonAnimation;
    internal AxieController self;
    internal List<AxieController> targetList;
    internal AxieBodyPart axieBodyPart;
    [SerializeField] private float axieAnimationTiming;
    [SerializeField] private float statusEffectsTiming;

    private List<DamageTargetPair> damageTargetPairs;
    private List<HealTargetPair> healTargetPairs;
    private List<StatusEffectTargetPair> statusEffectTargetPair;
    internal float damageOrHealTiming;

    public void AddDamageTargetPair(int axieId, float damage)
    {
        damageTargetPairs.Add(new DamageTargetPair(axieId, damage));
    }

    public void AddHealTargetPair(int axieId, float heal)
    {
        healTargetPairs.Add(new HealTargetPair(axieId, heal));
    }

    public void AddStatusEffectTargetPair(int axieId, SkillEffect[] skillEffects)
    {
        statusEffectTargetPair.Add(new StatusEffectTargetPair(axieId, skillEffects));
    }

    public SkillAction GetAxieAnimationAction()
    {
        return new SkillAction(PlayAxieAnimation, axieAnimationTiming);
    }

    public SkillAction GetDealDamageAction()
    {
        return new SkillAction(DoDamage, damageOrHealTiming);
    }

    public SkillAction GetHealAction()
    {
        return new SkillAction(DoHeal, damageOrHealTiming);
    }

    public List<SkillAction> GetAllVFXActions()
    {
        List<SkillAction> skillActions = new List<SkillAction>();
        foreach (var vfx in vfxToThrow)
        {
            skillActions.Add(new SkillAction(delegate { LaunchVFX(vfx); }, vfx.ActivateTiming));
        }

        return skillActions;
    }

    public SkillAction GetStatusEffectAction()
    {
        return new SkillAction(SetStatusEffects, statusEffectsTiming);
    }

    private void DoDamage()
    {
        foreach (var target in targetList)
        {
            target.axieIngameStats.currentHP -= damageTargetPairs.FirstOrDefault(x => x.axieId == target.AxieId)!.Value;
        }
    }

    private void DoHeal()
    {
        foreach (var target in targetList)
        {
            target.axieIngameStats.currentHP += healTargetPairs.FirstOrDefault(x => x.axieId == target.AxieId)!.Value;
        }
    }

    private void PlayAxieAnimation()
    {
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
    }

    private void LaunchVFX(SkillVFX skill)
    {
        foreach (var target in targetList)
        {
            GameObject vfxSpawned = Instantiate(skill.VFXPrefab,
                skill.StartFromOrigin ? self.GetPartPosition(BodyPart.Horn) : target.GetPartPosition(BodyPart.Horn),
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
                    projectileMover.MoveToTarget(target.GetPartPosition(BodyPart.Horn), skill.SkillDuration);
            }
        }
    }

    private void SetStatusEffects()
    {
        if (axieBodyPart.skillEffects == null)
            return;

        foreach (var target in targetList)
        {
            var skillEffects = statusEffectTargetPair.FirstOrDefault(x => x.axieId == target.AxieId)
                ?.skillEffects;

            if (skillEffects != null)
            {
                foreach (var skillEffect in skillEffects)
                {
                    StatusManager.Instance.SetStatus(skillEffect, self, target);
                }
            }
        }
    }
}