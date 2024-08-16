using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static public class AxieStatCalculator
{

    public const float LungePercentage = .1f;
    static public float GetAttackSpeed(GetAxiesExample.Stats stats)
    {
        // el 50 implica que alguien con stats (speed + speed * 0,5) 50 va a atacar 1 vez por segundo
        // El speed + speed * 0,5 es para agrandar el rango de diferencia entre un axie con 61 y uno con 31
        return 50f / (stats.speed + stats.speed * 0.5f);
    }

    static public float GetHP(GetAxiesExample.Stats stats)
    {
        if (stats.hp == 0)
        {
            return 0;
        }
        float minHp = 600f;
        //el 27 son la stat mas baja de hp que puede tener un axie, se lo resto para unicamente tener en cuenta los stats extra a eso
        //el 4 es la importancia de estos stats sobrantes, ( si fuera 1, un axie con 61 de hp tendria 300 + (61-27)
        return (minHp + (4f * (stats.hp - 27f))) * 5f;
    }

    static public float GetPoisonDamage(float stacks)
    {
        float baseDmg = 10;
        //el 27 son la stat mas baja de hp que puede tener un axie, se lo resto para unicamente tener en cuenta los stats extra a eso
        //el 4 es la importancia de estos stats sobrantes, ( si fuera 1, un axie con 61 de hp tendria 300 + (61-27)
        return stacks * baseDmg;
    }

    static public float GetManaPerAttack(GetAxiesExample.Stats stats)
    {
        // le resto o sumo un porcentaje de la diferencia del skill con 35 (numero arbitrario) para marcar una diferencia entre los axies con mucho skill
        return 10 + (stats.skill - 35f) * 0.2f;
    }


    static public float GetMinMana(GetAxiesExample.Stats stats)
    {
        return stats.skill;
    }

    static public float GetMaxMana(GetAxiesExample.Stats stats)
    {
        return 100f;
    }

    static public int GetRealSpeed(int speed, int speedBuffAmount)
    {
        return Mathf.RoundToInt(speed + (speed * (speedBuffAmount * 0.2f)));
    }

    static public int GetRealMorale(int morale, int moraleBuffAmount)
    {
        return Mathf.RoundToInt(morale + (morale * (moraleBuffAmount * 0.2f)));
    }

    static public int GetRealAttack(GetAxiesExample.Stats stats, int attackBuffAmount)
    {
        int attack = Mathf.RoundToInt(GetAttackDamage(stats));
        return Mathf.RoundToInt((attack + (attack * (attackBuffAmount * 0.2f))));
    }

    static public float GetCritChance(GetAxiesExample.Stats stats)
    {
        // el 1 es para decir que todos los axies tienen 10% al menos
        // Esta cuenta devuelve un valor entre 0 y 1 para indicar el porcentaje de chance de critico
        // Le resto el 27 por que es el stat minimo de moral de un axie y en base a la diferencia la multiplico por 0,5 asi los que mas moral tienen tienen mas chances
        return (10f + ((stats.morale - 27f) * 0.5f)) * 0.01f;
    }

    static public float GetCritDamage(GetAxiesExample.Stats stats)
    {
        // el 2 es para indicar que se va a hacer el doble de daño si el axie critea
        // el 35 es elegido para que axies con menos moral que esa (ejemplo pez puro, tengan criticos que hacen menos daño)
        return 2f + (stats.morale - 35) * 0.02f;
    }

    static public float GetAttackDamage(GetAxiesExample.Stats stats)
    {
        // esta cuenta devuelve un valor que ronda entre los 10 y 20 para calcular el daño de cada auto ataque
        // speed afecta en 9% de su valor
        // moral afecta en 20% de su valor
        // hp afecta en 30% del valor que supera el hp minimo
        // skill suma o resta un 25% de la diferencia entre el skill y 31 (un numero arbitrario para marcar la norma de skill de un axie)
        // Este numero lo multiplico por un coeficiente para reducirlo levemente por las dudas, este coeficiente puede no estar
        return (stats.speed * 0.09f + stats.morale * 0.2f + (stats.hp - 27) * 0.3f + (stats.skill - 31f) * 0.25f) *
               5.9f; //era0.9
    }
}