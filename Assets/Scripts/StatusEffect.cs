using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Serialization;

[System.Serializable]
public class SkillEffect
{
    public StatusEffectEnum statusEffect;
    public StatusApplyType statusApplyType;
    public SkillTriggerType skillTriggerType;
    public bool isPassive;
    public float skillDuration;
    public int PoisonStack;
    public int Attack;
    public int Speed;
    public int Morale;
    public int StealEnergyPercentage;
    
    public int GainEnergy;
    public int MeleeReflect;
    public int RangedReflect;
    public int GainShieldOnAttack;
    public int GainHPPercentage;
    public int ShieldOnStart;
    public bool OnlyCanDamageShield; //Doubletalk can damage hp
    public bool StunOnShieldBreak;
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
    public bool DamageEqualsBasicAttack;
    public int ExtraDamagePercentage;
    public int ExtraDamageOnCritical;
    public int MultiCastTimes;
    [Header("Special Interactions")] public List<SpecialActivationAgainstAxieClass> specialActivationAgainstAxiesList;
    public List<SpecialComboWithAxieCard> specialActivationIfComboedWithList;
    public List<SpecialComboWithAxiesInBattle> specialActivationBasedOnAxiesInBattle;
    internal int timesSet;
    internal bool hasSpecialActivationBasedOnTargetAxieClass => specialActivationAgainstAxiesList.Count > 0;
    [Header("Trigger If")] public bool triggerIfCertainHPTreshold;
    [Tooltip("if above bool is enabled, and this is false. It is considered MoreThan")]
    public bool LessThan;
    public int HPTresholdPercentage;
    public int ComboAmount;
    public bool LastAxieAliveTeam;
    public bool LastAxieAliveOpponent;
    [Tooltip("My attack - target attack = difference. Ex: AttackStatDifference = -1. MyAttack - TargetAttack = -1. Target has more attack, this triggers")]
    public int AttackStatDifference;
    public int SpeedStatDifference;
    public int MoraleStatDifference;
    [Header("Reactivation")] public bool AllowReactivation;
    public int ReactivateEffectEveryXSeconds;
}

[System.Serializable]
public class SpecialActivationAgainstAxieClass
{
    public AxieClass axieClass;
    public int ExtraDamage;
    public int ExtraTimesAbilityCast;
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
    public int ExtraTimesAbilityCastPerAxie;
    public int ExtraTimesStatusEffectAppliedPerAxie;
    public int ExtraDamageAppliedPerAxie;
}