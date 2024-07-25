using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Serialization;
using System;
using System.Collections.Generic;

[System.Serializable]
public class ShopItemEffect : ICloneable
{
    public object Clone()
    {
        return this.MemberwiseClone();
    }

    internal bool TriggerOnSecondsOfFight => SecondsOfFight > 0f;
    public int SecondsOfFight = 0;
    public bool OnBattleStart;
}