using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static public class AxieStatCalculator
{
    static public float GetAttackSpeed(GetAxiesExample.Stats stats)
    {
        return 10f / (stats.speed * 0.3f);
    }
    
    static public float GetHP(GetAxiesExample.Stats stats)
    {
        return stats.hp * 5f;
    }
    
    static public float GetManaPerAttack(GetAxiesExample.Stats stats)
    {
        return 10;
    }


    static public float GetMinMana(GetAxiesExample.Stats stats)
    {
        return stats.skill;
    }

    static public float GetMaxMana(GetAxiesExample.Stats stats)
    {
        return 100;
    }

    static public float GetAttackDamage(GetAxiesExample.Stats stats)
    {
        return (stats.speed + stats.morale) * 0.1f;
    }
}