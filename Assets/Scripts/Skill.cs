using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using finished3;
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
    MonsterKiss,
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
    public int monsterId;
    public float Value;
    public bool onlyShield;
    public MonsterClass DamageType;

    public DamageTargetPair(int monsterId, float value, MonsterClass DamageType, bool onlyShield = false)
    {
        this.monsterId = monsterId;
        Value = value;
        this.onlyShield = onlyShield;
        this.DamageType = DamageType;
    }
}

public class HealTargetPair
{
    public int monsterId;
    public float Value;

    public HealTargetPair(int monsterId, float value)
    {
        this.monsterId = monsterId;
        Value = value;
    }
}

public class StatusEffectTargetPair
{
    public int monsterId;
    public SkillEffect[] skillEffects;
    public bool remove;

    public StatusEffectTargetPair(int monsterId, SkillEffect[] value, bool remove = false)
    {
        this.monsterId = monsterId;
        skillEffects = value;
        this.remove = remove;
    }
}

public class Skill : MonoBehaviour
{
    public List<SkillVFX> vfxToThrow = new List<SkillVFX>();
    public MonsterAnimation animationToPlay;
    internal MonsterClass @class;
    internal Transform origin;
    internal Transform target;
    internal VanillaMonsterVisual visual;
    internal MonsterController self;
    internal List<MonsterController> targetList = new List<MonsterController>();
    internal List<MonsterController> statusEffectTargetList = new List<MonsterController>();
    internal MonsterBodyPart monsterBodyPart;
    [SerializeField] private float monsterAnimationTiming;
    [SerializeField] private float statusEffectsTiming;

    private List<DamageTargetPair> damageTargetPairs = new List<DamageTargetPair>();
    private List<HealTargetPair> healTargetPairs = new List<HealTargetPair>();
    private List<StatusEffectTargetPair> statusEffectTargetPair = new List<StatusEffectTargetPair>();
    internal float damageOrHealTiming = 0.4f;
    internal float ExtraTimerCast;
    public float totalDuration;
    public float attackAudioTiming;
    internal bool debug;
    public bool DontPlayAnimation;

    public void AddDamageTargetPair(int monsterId, float damage, bool onlyShield = false)
    {
        damageTargetPairs.Add(new DamageTargetPair(monsterId, damage, this.monsterBodyPart.bodyPartClass, onlyShield));
    }

    public void AddHealTargetPair(int monsterId, float heal)
    {
        healTargetPairs.Add(new HealTargetPair(monsterId, heal));
    }

    private bool OriginFacesPositive()
    {
        if (self != null)
            return MonsterScale.IsFacingPositive(self.transform);

        if (visual != null)
            return visual.FacingPositiveX;

        return MonsterScale.IsFacingPositive(origin);
    }

    public void AddStatusEffectTargetPair(int monsterId, SkillEffect[] skillEffects, bool remove = false)
    {
        statusEffectTargetPair.Add(new StatusEffectTargetPair(monsterId, skillEffects, remove));
    }

    public SkillAction GetMonsterAnimationAction()
    {
        return new SkillAction(PlayMonsterAnimation, monsterAnimationTiming + ExtraTimerCast);
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
        {
            skillActions.Add(new SkillAction(delegate { LaunchFallbackVFX(null); },
                Mathf.Max(0.05f, damageOrHealTiming * 0.5f) + ExtraTimerCast));
            return skillActions;
        }

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
        var AttackBuff = self.monsterSkillEffectManager.GetAttackBuff();
        foreach (var target in targetList)
        {
            DamageTargetPair pair = damageTargetPairs.FirstOrDefault(x => x.monsterId == target.MonsterId);

            if (pair == null)
                return;

            if (target.monsterSkillController.passives.PotatoLeaf && pair.DamageType == MonsterClass.Aquatic)
                pair.Value = 0;

            if (pair.onlyShield)
            {
                var dmg = pair.Value > target.monsterIngameStats.currentShield ? target.monsterIngameStats.currentShield : pair.Value;
                target.monsterIngameStats.currentShield -= dmg;

                target.monsterSkillController.DamageReceived(@class, dmg, self, true);
            }
            else
            {
                var dmg = pair.Value;
                dmg = MonsterStatCalculator.GetSkillDamage(dmg, self.stats, AttackBuff, self.monsterSkillController.skillList.Count);
                float shieldDamage = dmg - target.monsterIngameStats.currentShield;

                if (shieldDamage < 0)
                {
                    target.monsterIngameStats.currentShield -= dmg;
                }
                else
                {
                    target.monsterIngameStats.currentShield = 0;

                    target.monsterIngameStats.currentHP -= shieldDamage;
                    target.statsManagerUI.SetHP(target.monsterIngameStats.currentHP / target.monsterIngameStats.maxHP);
                    target.monsterSkillController.DamageReceived(@class, dmg, self, true);
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
                var monsterTargetHealingPair = healTargetPairs.FirstOrDefault(x => x.monsterId == target.MonsterId);
                if (monsterTargetHealingPair == null)
                    continue;

                target.DoHeal(monsterTargetHealingPair.Value, self.MonsterId.ToString());
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
        }

    }

    private void PlayMonsterAnimation()
    {
        if (DontPlayAnimation)
            return;

        float attackSpeedMulti = 1;

        if (self.monsterSkillEffectManager.IsAromad())
        {
            attackSpeedMulti = 0.75f;
        }

        MonsterVisualState state = MonsterVisualStateMapper.FromMonsterAnimation(animationToPlay);
        float speed = visual != null
            ? visual.GetDuration(state) / (self.monsterBehavior.AttackSpeed * attackSpeedMulti)
            : 1f;
        visual?.Play(state, false, speed);
    }

    private void LaunchVFX(SkillVFX skill)
    {
        try
        {
            if (skill == null || skill.VFXPrefab == null)
            {
                LaunchFallbackVFX(skill);
                return;
            }

            SpendSkillEnergy();

            foreach (var target in targetList)
            {
                Vector3 pos = skill.VFXPrefab.transform.localPosition;
                GameObject vfxSpawned = Instantiate(skill.VFXPrefab,
                    skill.StartFromOrigin ? self.GetPartPosition(BodyPart.Horn) : target.GetPartPosition(BodyPart.Horn),
                    skill.VFXPrefab.transform.rotation,
                    this.transform);
                MonsterVfxLimiter.NormalizeSpawned(vfxSpawned, false, skill.SkillDuration);



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
                    classSelector.SetAnimation(self.monsterIngameStats.monsterClass);
                }

                if (skill.StartFromOrigin)
                {
                    ProjectileMover projectileMover = vfxSpawned.GetComponent<ProjectileMover>();

                    bool originFacesPositive = OriginFacesPositive();
                    vfxSpawned.transform.localScale = new Vector3(
                        !originFacesPositive
                            ? -vfxSpawned.transform.localScale.x
                            : vfxSpawned.transform.localScale.x,
                        vfxSpawned.transform.localScale.y, vfxSpawned.transform.localScale.z);

                    if (originFacesPositive)
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
            Debug.LogError(e.Message + this.monsterBodyPart.skillName);
        }
    }

    private void LaunchFallbackVFX(SkillVFX skill, bool small = false)
    {
        SpendSkillEnergy();

        BodyPart bodyPart = monsterBodyPart != null ? monsterBodyPart.bodyPart : BodyPart.Horn;
        Vector3 originPosition = self != null
            ? self.GetPartPosition(bodyPart)
            : origin != null ? origin.position : transform.position;

        List<Vector3> fallbackTargets = new List<Vector3>();
        if (targetList != null && targetList.Count > 0)
        {
            fallbackTargets.AddRange(targetList.Select(x => x != null ? x.GetPartPosition(BodyPart.Horn) : originPosition));
        }
        else if (target != null)
        {
            fallbackTargets.Add(target.position);
        }
        else
        {
            fallbackTargets.Add(originPosition + transform.right);
        }

        MonsterVisualState state = MonsterVisualStateMapper.FromMonsterAnimation(animationToPlay);
        if (skill != null && skill.StartFromOrigin)
            state = MonsterVisualState.AttackRanged;

        float duration = skill != null && skill.SkillDuration > 0f
            ? skill.SkillDuration
            : Mathf.Clamp(totalDuration > 0f ? totalDuration * 0.35f : 0.45f, 0.25f, 0.8f);

        foreach (Vector3 targetPosition in fallbackTargets)
            VanillaSkillVfx.Play(transform, originPosition, targetPosition, @class, state, duration, small);
    }

    private void SpendSkillEnergy()
    {
        if (self == null || monsterBodyPart == null || self.monsterIngameStats.totalComboCost <= 0)
            return;

        self.monsterIngameStats.CurrentEnergy -= monsterBodyPart.energy / self.monsterIngameStats.totalComboCost;
        if (self.monsterIngameStats.CurrentEnergy < 0)
            self.monsterIngameStats.CurrentEnergy = 0;

        self.SetEnergy();
    }

    public IEnumerator LaunchSkillTest(bool loop)
    {
        StartCoroutine(Destroy());

        visual?.Play(MonsterVisualStateMapper.FromMonsterAnimation(animationToPlay), false);
        List<SkillVFX> VFXLIST = new List<SkillVFX>();
        VFXLIST.AddRange(vfxToThrow);
        float timer = 0;
        while (timer < totalDuration)
        {
            foreach (SkillVFX skill in VFXLIST)
            {
                if (timer >= skill.ActivateTiming)
                {

                    if (skill == null || skill.VFXPrefab == null || this == null)
                    {
                        LaunchFallbackVFX(skill);
                        continue;
                    }

                    Vector3 pos = skill.VFXPrefab.transform.localPosition;
                    GameObject vfxSpawned = Instantiate(skill.VFXPrefab,
                        skill.StartFromOrigin ? origin.transform.position : target.transform.position,
                        skill.VFXPrefab.transform.rotation,
                        this.transform);
                    MonsterVfxLimiter.NormalizeSpawned(vfxSpawned, false, skill.SkillDuration);




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

                        bool originFacesPositive = OriginFacesPositive();
                        vfxSpawned.transform.localScale = new Vector3(
                            !originFacesPositive
                                ? -vfxSpawned.transform.localScale.x
                                : vfxSpawned.transform.localScale.x,
                            vfxSpawned.transform.localScale.y, vfxSpawned.transform.localScale.z);

                        if (originFacesPositive)
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

            visual?.Play(MonsterVisualState.Idle, true);
        }
    }

    public IEnumerator LaunchSkillTestSmall(bool loop)
    {
        visual?.Play(MonsterVisualStateMapper.FromMonsterAnimation(animationToPlay), false);
        List<SkillVFX> VFXLIST = new List<SkillVFX>();
        VFXLIST.AddRange(vfxToThrow);
        float timer = 0;
        while (timer < totalDuration)
        {
            foreach (SkillVFX skill in VFXLIST)
            {
                if (timer >= skill.ActivateTiming)
                {
                    if (skill == null || skill.VFXPrefab == null)
                    {
                        LaunchFallbackVFX(skill, true);
                        continue;
                    }

                    Vector3 pos = skill.VFXPrefab.transform.localPosition;
                    GameObject vfxSpawned = Instantiate(skill.VFXPrefab,
                        skill.StartFromOrigin ? origin.transform.position : target.transform.position,
                        skill.VFXPrefab.transform.rotation,
                        this.transform);
                    MonsterVfxLimiter.NormalizeSpawned(vfxSpawned, true, skill.SkillDuration);

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

                        bool originFacesPositive = OriginFacesPositive();
                        vfxSpawned.transform.localScale = new Vector3(
                            !originFacesPositive
                                ? -vfxSpawned.transform.localScale.x
                                : vfxSpawned.transform.localScale.x,
                            vfxSpawned.transform.localScale.y, vfxSpawned.transform.localScale.z);

                        if (originFacesPositive)
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

            visual?.Play(MonsterVisualState.Idle, true);
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
            var pair = statusEffectTargetPair.FirstOrDefault(x => x.monsterId == target.MonsterId);
            var skillEffects = pair?.skillEffects;

            if (skillEffects != null)
            {
                foreach (var skillEffect in skillEffects)
                {
                    StatusManager.Instance.SetStatus(skillEffect, target, pair.remove, self.MonsterId.ToString());
                }
            }
        }
    }
}
