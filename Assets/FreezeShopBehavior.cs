using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class FreezeShopBehavior : MonoBehaviour
{
    private AtiaBlessing.BuffEffect ItemEffect;

    public GameObject AtiasBlessing;

    public TextMeshProUGUI Poping;
    private bool scaling = false;

    private void OnEnable()
    {
        Poping.color = Color.clear;
    }


    private void OnMouseOver()
    {
        if (!scaling)
        {
            scaling = true;
            this.transform.DOScale(new Vector3(0.013f, 0.013f, 0.013f), 0.25f);
            Poping.DOColor(Color.yellow, 0.5f);
        }
    }

    private void OnMouseExit()
    {
        if (scaling)
        {
            scaling = false;
            this.transform.DOScale(new Vector3(0.01f, 0.01f, 0.01f), 0.25f);
            Poping.DOColor(Color.clear, 0.5f);
        }
    }

    private void OnMouseDown()
    {
        if (AtiasBlessing.activeSelf)
            return;

        ShopManager.instance.ToggleFreezeMode();
    }
}
