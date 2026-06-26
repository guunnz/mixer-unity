using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFXClassSelector : MonoBehaviour
{
    public string skinName;
    public bool AquaticIsAqua = false;

    public void SetAnimation(MonsterClass monsterClass)
    {
        if (monsterClass == MonsterClass.Dusk)
        {
            monsterClass = MonsterClass.Reptile;
        }
        else if (monsterClass == MonsterClass.Mech)
        {
            monsterClass = MonsterClass.Beast;
        }
        else if (monsterClass == MonsterClass.Dawn)
        {
            monsterClass = MonsterClass.Bird;
        }

        string skinNameString = monsterClass.ToString().ToLower() + skinName;
        if (AquaticIsAqua)
        {
            skinNameString = skinNameString.Replace("aquatic", "aqua");
        }

        switch (monsterClass)
        {
            case MonsterClass.Dusk:
                skinNameString = MonsterClass.Reptile.ToString().ToLower() + skinName;
                break;
            case MonsterClass.Dawn:
                skinNameString = MonsterClass.Bird.ToString().ToLower() + skinName;
                break;
            case MonsterClass.Mech:
                skinNameString = MonsterClass.Beast.ToString().ToLower() + skinName;
                break;
        }

        Color color = MonsterClassPalette.Main(monsterClass);
        foreach (SpriteRenderer spriteRenderer in GetComponentsInChildren<SpriteRenderer>(true))
            spriteRenderer.color = color;

        foreach (ParticleSystem particleSystem in GetComponentsInChildren<ParticleSystem>(true))
        {
            ParticleSystem.MainModule main = particleSystem.main;
            main.startColor = color;
        }
    }
}
