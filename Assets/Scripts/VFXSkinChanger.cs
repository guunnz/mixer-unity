using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VFXSkinChanger : MonoBehaviour
{
    public void ChangeBasedOnClass(MonsterClass @class)
    {
        Color color = MonsterClassPalette.Main(@class);
        foreach (SpriteRenderer spriteRenderer in GetComponentsInChildren<SpriteRenderer>(true))
            spriteRenderer.color = color;

        foreach (ParticleSystem particleSystem in GetComponentsInChildren<ParticleSystem>(true))
        {
            ParticleSystem.MainModule main = particleSystem.main;
            main.startColor = color;
        }
    }
}
