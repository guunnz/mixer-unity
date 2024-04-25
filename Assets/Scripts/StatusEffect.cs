using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Serialization;

[System.Serializable]
public class SkillEffect
{
    public StatusEffectEnum statusEffect;
    public StatusApplyType statusApplyType;
    public bool statusApplyTypeIsTarget;
    public SkillTriggerType skillTriggerType;
    public bool isPassive;
    public float skillDuration;
    public int PoisonStack;
    public int Attack;
    public int Speed;
    public int Morale;
    public int StealEnergyPercentage;
    internal bool isBuffOrDebuff;
    public float GainEnergy;
    public int MeleeReflect;
    public int RangedReflect;
    public int GainShield;
    public int GainHPPercentage;
    public bool HPBaseOnDamage;
    public int ShieldOnStart;
    public bool IgnoresShield;
    public bool Lethal;
    public bool Merry;
    public bool Gecko;
    public bool Stun;
    public bool Fear;
    public bool Sleep;
    public bool Jinx;
    public bool Chill;
    public bool Aroma;
    public bool Stench;
    public bool Fragile;
    public bool Poison;
    public bool Wombo;
    public bool AlwaysCritical;
    public bool RandomEffectIsDebuff;
    public bool RandomEffectIsBuff;
    public bool ApplyRandomEffect;
    public bool DamageEqualsBasicAttack;
    public int ExtraDamagePercentage;
    public int ExtraDamageOnCritical;
    public int MultiCastTimes;
    public bool InmuneToCriticalStrike;
    public int ReduceDamagePercentage;
    [Header("Special Interactions")] public bool UseSpecialsAsTrigger;
    public List<SpecialActivationAgainstAxieClass> specialActivationAgainstAxiesList;
    public List<SpecialComboWithAxieCard> specialActivationIfComboedWithList;
    public List<SpecialActivationWithBodyPart> specialActivationWithBodyParts;
    public List<SpecialComboWithAxiesInBattle> specialActivationBasedOnAxiesInBattle;
    public List<SpecialActivactionWhenReceiveDamage> specialActivactionWhenReceiveDamage;
    internal int timesSet;
    internal bool hasSpecialActivationBasedOnTargetAxieClass => specialActivationAgainstAxiesList.Count > 0;

    [Header("Targeting")] public bool lowestHP;
    public bool FurthestTarget;
    public bool targetHighestEnergy;
    public bool targetHighestSpeed;
    public bool targetAxieClass;
    public AxieClass axieClassToTarget;

    internal bool Prioritize => FurthestTarget || targetHighestEnergy || targetHighestSpeed || targetAxieClass;

    [Header("Trigger If")] public bool triggerIfCertainHPTreshold;

    [Tooltip("if above bool is enabled, and this is false. It is considered MoreThan")]
    public bool LessThan;

    public int HPTresholdPercentage;
    public int ComboAmount;
    public bool LastAxieAliveTeam;
    public bool LastAxieAliveOpponent;
    public bool Shielded;
    public bool OnShieldBreak;
    public bool TargetIsDebuff;
    public bool TargetIsPoisoned;
    public bool SelfIsDebuff;
    public bool RangeTarget;
    public int ShieldNotBrokenForXSeconds;
    public bool RangeAbility;
    public int SecondsOfFight;

    internal bool hasTriggerCondition => LessThan
                                         ||
                                         HPTresholdPercentage > 0
                                         || ComboAmount > 0
                                         || LastAxieAliveOpponent
                                         || LastAxieAliveTeam
                                         || Shielded
                                         || OnShieldBreak
                                         || TargetIsDebuff
                                         || TargetIsPoisoned
                                         || SelfIsDebuff
                                         || RangeTarget
                                         || ShieldNotBrokenForXSeconds > 0
                                         || RangeAbility
                                         || SecondsOfFight > 0
                                         || UseSpecialsAsTrigger
                                         || statDifferenceTrigger;

    internal bool statDifferenceTrigger => AttackStatDifference > 0 || SpeedStatDifference > 0 ||
                                           MoraleStatDifference > 0 || CurrentHPStatDifference > 0;

    [Tooltip(
        "My attack - target attack = difference. Ex: AttackStatDifference = -1. MyAttack - TargetAttack = -1. Target has more attack, this triggers")]
    public int AttackStatDifference;

    public int SpeedStatDifference;
    public int MoraleStatDifference;
    public int CurrentHPStatDifference;

    //
    [Header("Reactivation")] public bool AllowReactivation;
    public int ReactivateEffectEveryXSeconds;

    public List<int> arreglabugs;


    public bool IsOnlyBuffOrDebuff()
    {
        return (Aroma || Chill || Fear || Fragile || Jinx || Lethal || Poison || Stun || Sleep ||
                Stench || Attack != 0 || Morale != 0 || Speed != 0 || ApplyRandomEffect);
    }
}

[System.Serializable]
public class SpecialActivationAgainstAxieClass
{
    public AxieClass axieClass;
    public int ExtraDamage;
    public int ExtraTimesStatusEffectApplied;
}

[System.Serializable]
public class SpecialComboWithAxieCard
{
    public AxieClass axieClassCard;
    public bool OnlyCareAboutClassCard;
    public SkillName axieCard;
    public int ExtraDamage;
    public int ExtraTimesAbilityCast;
    public int ExtraTimesStatusEffectApplied;
}

[System.Serializable]
public class SpecialComboWithAxiesInBattle
{
    public AxieClass axieClass;
    public int ExtraTimesStatusEffectAppliedPerAxie;
    public int ExtraDamageAppliedPerAxie;
}

[System.Serializable]
public class SpecialActivationWithBodyPart
{
    public AxieClass axieClassCard;
    public bool OnlyCareAboutClassCard;
    public SkillName axieCard;
    public int ExtraDamage;
    public int ExtraTimesStatusEffectApplied;
}

[System.Serializable]
public class SpecialActivactionWhenReceiveDamage
{
    public bool onlyAbilities;
    public AxieClass axieClass;
}