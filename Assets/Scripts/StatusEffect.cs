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
    
    public float GainEnergy;
    public int MeleeReflect;
    public int RangedReflect;
    public int GainShield;
    public int GainHPPercentage;
    public bool HPBaseOnDamage;
    public int ShieldOnStart;
    public bool OnlyCanDamageShield; //Doubletalk can damage hp
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
    [Header( "Special Interactions" )]
    public bool UseSpecialsAsTrigger;
    public List<SpecialActivationAgainstAxieClass> specialActivationAgainstAxiesList;
    public List<SpecialComboWithAxieCard> specialActivationIfComboedWithList;
    public List<SpecialActivationWithBodyPart> specialActivationWithBodyParts;
    public List<SpecialComboWithAxiesInBattle> specialActivationBasedOnAxiesInBattle;
    public List<SpecialActivactionWhenReceiveDamage> specialActivactionWhenReceiveDamage;
    internal int timesSet;
    internal bool hasSpecialActivationBasedOnTargetAxieClass => specialActivationAgainstAxiesList.Count > 0;
    
    [Header("Targeting")] 
    public bool lowestHP;
    public bool FurthestTarget;
    public bool targetHighestEnergy;
    public bool targetHighestSpeed;
    public bool targetAxieClass;
    public AxieClass axieClassToTarget;
    
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
    public bool SelfIsDebuff;
    public bool RangeTarget;
    public int ShieldNotBrokenForXSeconds;
    public bool RangeAbility;
    public int SecondsOfFight;
    
    [Tooltip("My attack - target attack = difference. Ex: AttackStatDifference = -1. MyAttack - TargetAttack = -1. Target has more attack, this triggers")]
    public int AttackStatDifference;
    public int SpeedStatDifference;
    public int MoraleStatDifference;
    public int CurrentHPStatDifference;
    //
    [Header("Reactivation")] public bool AllowReactivation;
    public int ReactivateEffectEveryXSeconds;

    public List<int> arreglabugs;
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

[System.Serializable]
public class SpecialActivationWithBodyPart
{
    public AxieClass axieClassCard;
    public bool OnlyCareAboutClassCard;
    public SkillName axieCard;
    public int ExtraDamage;
    public int ExtraTimesAbilityCast;
    public int ExtraTimesStatusEffectApplied;
}
[System.Serializable]
public class SpecialActivactionWhenReceiveDamage
{
    public bool onlyAbilities;
    public AxieClass axieClass;
}