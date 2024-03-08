using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillEffectGraphic : MonoBehaviour
{
    private Image image;
    public TextMeshProUGUI tmPro;
    public StatusEffectEnum statusEffect;
    private int Times = 1;

    private void Start()
    {
        image = GetComponent<Image>();

        switch (statusEffect)
        {
            case StatusEffectEnum.Aroma:
            case StatusEffectEnum.Chill:
            case StatusEffectEnum.Fear:
            case StatusEffectEnum.Stench:
            case StatusEffectEnum.Fragile:
            case StatusEffectEnum.Jinx:
            case StatusEffectEnum.Sleep:
            case StatusEffectEnum.Lethal:
            case StatusEffectEnum.Stun:
                tmPro.enabled = false;
                return;
            default:
                break;
        }

        tmPro.text = Times.ToString();
    }

    public void SetSprite(Sprite sprite)
    {
        if (image == null)
            image = GetComponent<Image>();
        image.sprite = sprite;
    }

    public void IncreaseNumber(int number)
    {
        switch (statusEffect)
        {
            case StatusEffectEnum.Aroma:
            case StatusEffectEnum.Chill:
            case StatusEffectEnum.Fear:
            case StatusEffectEnum.Stench:
            case StatusEffectEnum.Fragile:
            case StatusEffectEnum.Jinx:
            case StatusEffectEnum.Sleep:
            case StatusEffectEnum.Lethal:
            case StatusEffectEnum.Stun:
                tmPro.enabled = false;
                return;
            default:
                break;
        }

        Times += number;
        tmPro.text = Times.ToString();
    }
}