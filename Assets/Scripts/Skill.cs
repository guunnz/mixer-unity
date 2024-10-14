using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using finished3;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Serialization;
using DG.Tweening;

[System.Serializable]
public class SkillVFX
{
    public float ActivateTiming;
    public GameObject VFXPrefab;
    public bool StartFromOrigin;
    public float SkillDuration;
    public DotweenAnimationSkill AnimationMove;
}

[System.Serializable]
public class DotweenAnimationSkill
{
    public bool EnableDotweenAnimation;
    public Vector3 StartFromExtra;
    public Vector3 GoTo;
    public float Time;
}

public enum BodyPart
{
    Eyes,
    Ears,
    Horn,
    Mouth,
    Back,
    Tail,
    None
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
    RosebudUNUSED,
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
    ThornyCaterpillar,
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
    Earwing,
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
    public bool onlyShield;
    public AxieClass DamageType;

    public DamageTargetPair(int axieId, float value, AxieClass DamageType, bool onlyShield = false)
    {
        this.axieId = axieId;
        Value = value;
        this.onlyShield = onlyShield;
        this.DamageType = DamageType;
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
    public bool remove;

    public StatusEffectTargetPair(int axieId, SkillEffect[] value, bool remove = false)
    {
        this.axieId = axieId;
        skillEffects = value;
        this.remove = remove;
    }
}

public class Skill : MonoBehaviour
{
    public List<SkillVFX> vfxToThrow = new List<SkillVFX>();
    public AxieAnimation animationToPlay;
    internal AxieClass @class;
    internal Transform origin;
    internal Transform target;
    internal SkeletonAnimation skeletonAnimation;
    internal AxieController self;
    internal List<AxieController> targetList = new List<AxieController>();
    internal List<AxieController> statusEffectTargetList = new List<AxieController>();
    internal AxieBodyPart axieBodyPart;
    [SerializeField] private float axieAnimationTiming;
    [SerializeField] private float statusEffectsTiming;

    private List<DamageTargetPair> damageTargetPairs = new List<DamageTargetPair>();
    private List<HealTargetPair> healTargetPairs = new List<HealTargetPair>();
    private List<StatusEffectTargetPair> statusEffectTargetPair = new List<StatusEffectTargetPair>();
    internal float damageOrHealTiming;
    internal float ExtraTimerCast;
    public float totalDuration;
    public float attackAudioTiming;
    internal bool debug;
    public bool DontPlayAnimation;

    public void AddDamageTargetPair(int axieId, float damage, bool onlyShield = false)
    {
        damageTargetPairs.Add(new DamageTargetPair(axieId, damage, this.axieBodyPart.bodyPartClass, onlyShield));
    }

    public void AddHealTargetPair(int axieId, float heal)
    {
        healTargetPairs.Add(new HealTargetPair(axieId, heal));
    }

    public void AddStatusEffectTargetPair(int axieId, SkillEffect[] skillEffects, bool remove = false)
    {
        statusEffectTargetPair.Add(new StatusEffectTargetPair(axieId, skillEffects, remove));
    }

    public SkillAction GetAxieAnimationAction()
    {
        return new SkillAction(PlayAxieAnimation, axieAnimationTiming + ExtraTimerCast);
    }

    public SkillAction GetDealDamageAction()
    {
        return new SkillAction(DoDamage, damageOrHealTiming + ExtraTimerCast);
    }

    public SkillAction GetHealAction()
    {
        if (healTargetPairs.Count == 0)
            return null;

        return new SkillAction(DoHeal, damageOrHealTiming + ExtraTimerCast);
    }

    public List<SkillAction> GetAllVFXActions()
    {
        List<SkillAction> skillActions = new List<SkillAction>();
        if (vfxToThrow.Count == 0)
            return null;
        foreach (var vfx in vfxToThrow)
        {
            skillActions.Add(new SkillAction(delegate { LaunchVFX(vfx); }, vfx.ActivateTiming + ExtraTimerCast));
        }

        return skillActions;
    }

    public SkillAction GetStatusEffectAction()
    {
        if (statusEffectTargetPair.Count == 0)
            return null;

        return new SkillAction(SetStatusEffects, statusEffectsTiming + ExtraTimerCast);
    }

    public SkillAction GetDestroyAction()
    {
        return new SkillAction(delegate { StartCoroutine(Destroy()); }, this.totalDuration + ExtraTimerCast + 0.05f);
    }

    private void DoDamage()
    {
        var AttackBuff = self.axieSkillEffectManager.GetAttackBuff();
        foreach (var target in targetList)
        {
            DamageTargetPair pair = damageTargetPairs.FirstOrDefault(x => x.axieId == target.AxieId);

            if (pair == null)
                return;

            if (target.axieSkillController.passives.PotatoLeaf && pair.DamageType == AxieClass.Aquatic)
                pair.Value = 0;

            if (pair.onlyShield)
            {
                var dmg = pair.Value > target.axieIngameStats.currentShield ? target.axieIngameStats.currentShield : pair.Value;
                target.axieIngameStats.currentShield -= dmg;

                target.axieSkillController.DamageReceived(@class, dmg, self, true);
            }
            else
            {
                var dmg = pair.Value;
                dmg = AxieStatCalculator.GetSkillDamage(dmg, self.stats, AttackBuff, self.axieSkillController.skillList.Count);
                float shieldDamage = dmg - target.axieIngameStats.currentShield;

                if (shieldDamage < 0)
                {
                    target.axieIngameStats.currentShield -= dmg;
                }
                else
                {
                    target.axieIngameStats.currentShield = 0;

                    target.axieIngameStats.currentHP -= shieldDamage;
                    target.statsManagerUI.SetHP(target.axieIngameStats.currentHP / target.axieIngameStats.maxHP);
                    target.axieSkillController.DamageReceived(@class, dmg, self, true);
                }
            }
        }
    }

    private void DoHeal()
    {
        try
        {
            foreach (var target in statusEffectTargetList)
            {
                var axieTargetHealingPair = healTargetPairs.FirstOrDefault(x => x.axieId == target.AxieId);
                if (axieTargetHealingPair == null)
                    continue;

                target.DoHeal(axieTargetHealingPair.Value, self.AxieId.ToString());
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
        }

    }

    private void PlayAxieAnimation()
    {
        if (DontPlayAnimation)
            return;

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
        if (animationName.Contains("shrimp"))
        {
            animationName = animationName.Replace("-", "/");
        }

        if (animationName.Contains("tail/multi"))
        {
            animationName = animationName.Replace("tail/multi", "tail-multi");
        }

        float attackSpeedMulti = 1;

        if (self.axieSkillEffectManager.IsAromad())
        {
            attackSpeedMulti = 0.75f;
        }
        self.SkeletonAnim.timeScale =
            self.SkeletonAnim.AnimationState.GetCurrent(0).AnimationEnd / (self.axieBehavior.AttackSpeed * attackSpeedMulti);

        skeletonAnimation.AnimationName = animationName;
    }

    private void LaunchVFX(SkillVFX skill)
    {
        try
        {
            self.axieIngameStats.CurrentEnergy -= this.axieBodyPart.energy / self.axieIngameStats.totalComboCost;
            self.SetEnergy();
            if (self.axieIngameStats.CurrentEnergy < 0)
            {
                self.axieIngameStats.CurrentEnergy = 0;
            }
            foreach (var target in targetList)
            {
                Vector3 pos = skill.VFXPrefab.transform.localPosition;
                GameObject vfxSpawned = Instantiate(skill.VFXPrefab,
                    skill.StartFromOrigin ? self.GetPartPosition(BodyPart.Horn) : target.GetPartPosition(BodyPart.Horn),
                    skill.VFXPrefab.transform.rotation,
                    this.transform);



                vfxSpawned.transform.localPosition = new Vector3(vfxSpawned.transform.localPosition.x +
                                                                 pos.x, vfxSpawned.transform.localPosition.y +
                                                                        pos.y,
                    vfxSpawned.transform.localPosition.z +
                    pos.z);

                if (skill.AnimationMove.EnableDotweenAnimation)
                {
                    vfxSpawned.transform.localPosition += skill.AnimationMove.StartFromExtra;
                    vfxSpawned.transform.DOLocalMove(vfxSpawned.transform.localPosition + skill.AnimationMove.GoTo,
                        skill.AnimationMove.Time);
                }

                VFXSkinChanger changer = vfxSpawned.GetComponent<VFXSkinChanger>();

                if (changer != null)
                {
                    changer.ChangeBasedOnClass(@class);
                }

                VFXClassSelector classSelector = vfxSpawned.GetComponent<VFXClassSelector>();

                if (classSelector != null)
                {
                    classSelector.SetAnimation(self.axieIngameStats.axieClass);
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
                        projectileMover.MoveToTarget(target.transform, skill.SkillDuration);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message + this.axieBodyPart.skillName);
        }
    }

    public IEnumerator LaunchSkillTest(bool loop)
    {
        string animationName = animationToPlay.ToString();

        StartCoroutine(Destroy());

        // Find the last underscore and replace it with a hyphen
        int lastUnderscoreIndex = animationName.LastIndexOf('_');

        if (lastUnderscoreIndex != -1)
        {
            animationName = animationName.Substring(0, lastUnderscoreIndex) + "-" +
                            animationName.Substring(lastUnderscoreIndex + 1);
        }

        // Replace the remaining underscores with slashes
        animationName = animationName.Replace("_", "/");
        if (animationName.Contains("shrimp"))
        {
            animationName = animationName.Replace("-", "/");
        }

        if (animationName.Contains("tail/multi"))
        {
            animationName = animationName.Replace("tail/multi", "tail-multi");
        }

        skeletonAnimation.AnimationName = animationName;
        skeletonAnimation.loop = false;
        skeletonAnimation.Initialize(true);
        List<SkillVFX> VFXLIST = new List<SkillVFX>();
        VFXLIST.AddRange(vfxToThrow);
        float timer = 0;
        while (timer < totalDuration)
        {
            foreach (SkillVFX skill in VFXLIST)
            {
                if (timer >= skill.ActivateTiming)
                {

                    if (skill == null || this == null)
                        yield break;
                    Vector3 pos = skill.VFXPrefab.transform.localPosition;
                    GameObject vfxSpawned = Instantiate(skill.VFXPrefab,
                        skill.StartFromOrigin ? origin.transform.position : target.transform.position,
                        skill.VFXPrefab.transform.rotation,
                        this.transform);




                    vfxSpawned.transform.localPosition = new Vector3(vfxSpawned.transform.localPosition.x +
                                                                     pos.x, vfxSpawned.transform.localPosition.y +
                                                                            pos.y,
                        vfxSpawned.transform.localPosition.z +
                        pos.z);

                    if (skill.AnimationMove.EnableDotweenAnimation)
                    {
                        vfxSpawned.transform.localPosition += skill.AnimationMove.StartFromExtra;
                        vfxSpawned.transform.DOLocalMove(vfxSpawned.transform.localPosition + skill.AnimationMove.GoTo,
                            skill.AnimationMove.Time);
                    }

                    VFXSkinChanger changer = vfxSpawned.GetComponent<VFXSkinChanger>();

                    if (changer != null)
                    {
                        changer.ChangeBasedOnClass(@class);
                    }

                    VFXClassSelector classSelector = vfxSpawned.GetComponent<VFXClassSelector>();

                    if (classSelector != null)
                    {
                        classSelector.SetAnimation(this.@class);
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
                            projectileMover.MoveToTarget(this.target, skill.SkillDuration + .1f);
                    }
                }
            }

            VFXLIST.RemoveAll(x => timer >= x.ActivateTiming);
            timer += Time.deltaTime;
            yield return null;
        }
        if (loop)
        {

        }
        else
        {

            skeletonAnimation.AnimationName = "action/idle/normal";
            skeletonAnimation.loop = true;
        }
    }

    public IEnumerator LaunchSkillTestSmall(bool loop)
    {
        string animationName = animationToPlay.ToString();

        //StartCoroutine(Destroy());

        // Find the last underscore and replace it with a hyphen
        int lastUnderscoreIndex = animationName.LastIndexOf('_');

        if (lastUnderscoreIndex != -1)
        {
            animationName = animationName.Substring(0, lastUnderscoreIndex) + "-" +
                            animationName.Substring(lastUnderscoreIndex + 1);
        }

        // Replace the remaining underscores with slashes
        animationName = animationName.Replace("_", "/");
        if (animationName.Contains("shrimp"))
        {
            animationName = animationName.Replace("-", "/");
        }

        if (animationName.Contains("tail/multi"))
        {
            animationName = animationName.Replace("tail/multi", "tail-multi");
        }

        skeletonAnimation.AnimationName = animationName;
        skeletonAnimation.loop = false;
        skeletonAnimation.Initialize(true);
        List<SkillVFX> VFXLIST = new List<SkillVFX>();
        VFXLIST.AddRange(vfxToThrow);
        float timer = 0;
        while (timer < totalDuration)
        {
            foreach (SkillVFX skill in VFXLIST)
            {
                if (timer >= skill.ActivateTiming)
                {

                    Vector3 pos = skill.VFXPrefab.transform.localPosition;
                    GameObject vfxSpawned = Instantiate(skill.VFXPrefab,
                        skill.StartFromOrigin ? origin.transform.position : target.transform.position,
                        skill.VFXPrefab.transform.rotation,
                        this.transform);


                    vfxSpawned.transform.localScale /= 8;

                    vfxSpawned.transform.localPosition = new Vector3(vfxSpawned.transform.localPosition.x +
                                                                     pos.x, vfxSpawned.transform.localPosition.y +
                                                                            pos.y,
                        vfxSpawned.transform.localPosition.z +
                        pos.z);

                    if (skill.AnimationMove.EnableDotweenAnimation)
                    {
                        vfxSpawned.transform.localPosition += skill.AnimationMove.StartFromExtra;
                        vfxSpawned.transform.DOLocalMove(vfxSpawned.transform.localPosition + skill.AnimationMove.GoTo,
                            skill.AnimationMove.Time);
                    }

                    VFXSkinChanger changer = vfxSpawned.GetComponent<VFXSkinChanger>();

                    if (changer != null)
                    {
                        changer.ChangeBasedOnClass(@class);
                    }

                    VFXClassSelector classSelector = vfxSpawned.GetComponent<VFXClassSelector>();

                    if (classSelector != null)
                    {
                        classSelector.SetAnimation(this.@class);
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
                            projectileMover.MoveToTarget(this.target, skill.SkillDuration + .1f);
                    }
                }
            }

            VFXLIST.RemoveAll(x => timer >= x.ActivateTiming);
            timer += Time.deltaTime;
            yield return null;
        }
        if (loop)
        {

        }
        else
        {

            skeletonAnimation.AnimationName = "action/idle/normal";
            skeletonAnimation.loop = true;
        }
    }
    private IEnumerator Destroy()
    {
        yield return new WaitForSecondsRealtime(totalDuration);
        if (this != null)
            Destroy(this.gameObject);
    }

    private void SetStatusEffects()
    {
        foreach (var target in statusEffectTargetList)
        {
            var pair = statusEffectTargetPair.FirstOrDefault(x => x.axieId == target.AxieId);
            var skillEffects = pair?.skillEffects;

            if (skillEffects != null)
            {
                foreach (var skillEffect in skillEffects)
                {
                    StatusManager.Instance.SetStatus(skillEffect, target, pair.remove, self.AxieId.ToString());
                }
            }
        }
    }
}