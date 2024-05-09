using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class ShopPotion : MonoBehaviour
{
    public AtiaBlessing.Blessing upgrade;

    public GameObject AtiasBlessing;

    public TextMeshProUGUI Poping;


    private bool scaling = false;

    private void OnMouseOver()
    {
        if (!scaling)
        {
            scaling = true;
            this.transform.DOScale(new Vector3(0.013f, 0.013f, 0.013f), 0.25f);
        }
    }

    private void OnMouseExit()
    {
        if (scaling)
        {
            scaling = false;
            this.transform.DOScale(new Vector3(0.01f, 0.01f, 0.01f), 0.25f);
        }
    }

    public IEnumerator Purchased()
    {
        Poping.DOColor(Color.white, 0.5f);
        yield return new WaitForSeconds(1f);
        Poping.DOColor(Color.clear, 0.5f);
    }

    private void OnMouseDown()
    {
        if (AtiasBlessing.activeSelf)
            return;
        RunManagerSingleton.instance.BuyUpgrade((int)upgrade);
        StartCoroutine(Purchased());
    }
}