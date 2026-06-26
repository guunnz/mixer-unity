using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillEffectGraphic : MonoBehaviour
{
    private const float StackFontSize = 13f;
    private static readonly Vector3 IconLocalScale = Vector3.one * 0.012f;
    private static readonly Vector2 StackAnchoredPosition = new Vector2(15f, 11f);
    private static readonly Vector2 StackSize = new Vector2(26f, 18f);

    private Image image;
    public TextMeshProUGUI tmPro;
    public StatusEffectEnum statusEffect;
    internal int Times = 1;

    private void Awake()
    {
        image = GetComponent<Image>();
        transform.localScale = IconLocalScale;
        if (image != null)
            image.raycastTarget = false;
        NormalizeStackText();
    }

    private void Start()
    {
        if (image == null)
            image = GetComponent<Image>();

        NormalizeStackText();

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

    private void LateUpdate()
    {
        NormalizeStackText();
    }

    public void SetSprite(Sprite sprite)
    {
        if (image == null)
            image = GetComponent<Image>();
        image.sprite = sprite;
    }

    public void IncreaseNumber(int number)
    {
        NormalizeStackText();

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

    private void NormalizeStackText()
    {
        if (tmPro == null)
            return;

        RectTransform rectTransform = tmPro.rectTransform;
        float xSign = rectTransform.parent != null && rectTransform.parent.lossyScale.x < 0f ? -1f : 1f;
        rectTransform.localScale = new Vector3(xSign, 1f, 1f);
        rectTransform.localRotation = Quaternion.identity;
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = StackAnchoredPosition;
        rectTransform.sizeDelta = StackSize;

        tmPro.fontSize = StackFontSize;
        tmPro.fontSizeMin = 10f;
        tmPro.fontSizeMax = StackFontSize;
        tmPro.enableAutoSizing = true;
        tmPro.alignment = TextAlignmentOptions.Center;
        tmPro.raycastTarget = false;
    }
}
