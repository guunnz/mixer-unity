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
    public List<ExtraDamageAgainst> extraDamageAgainstList;
    internal int timesSet;
    internal bool hasExtraDamageAgainstType => extraDamageAgainstList.Count > 0;
}

[System.Serializable]
public class ExtraDamageAgainst
{
    public AxieClass axieClass;
    public int ExtraDamage;
}