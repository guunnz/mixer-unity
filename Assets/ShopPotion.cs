using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ShopPotion : MonoBehaviour
{
    private AtiaBlessing.BuffEffect ItemEffect;

    private GameObject AtiasBlessing;

    public TextMeshProUGUI Poping;
    public TextMeshProUGUI ItemName;
    public TextMeshProUGUI ItemCost;
    public TextMeshProUGUI Sold;
    public SpriteRenderer itemImage;
    public ShopItem shopItem;

    private bool scaling = false;

    public bool sold;
    public bool Frozen;
    public GameObject FreezeGraphics;
    
    private void OnEnable()
    {
        Poping.color = Color.clear;
    }

    public void SetItem(ShopItem item)
    {
        Poping.text = item.description;
        ItemName.text = item.name;
        ItemCost.text = item.price.ToString();
        itemImage.sprite = item.ShopItemImage;
        ItemEffect = item.ItemEffectName;
    }

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
        Sold.gameObject.SetActive(true);
        Poping.DOColor(Color.white, 0.5f);
        yield return new WaitForSeconds(1f);
        Poping.DOColor(Color.clear, 0.5f);
    }

    private void OnMouseDown()
    {
        if (AtiasBlessing.activeSelf || sold || this.Frozen)
            return;

        if (ShopManager.instance.FreezeMode)
        {
            this.Frozen = !this.Frozen;
            FreezeGraphics.SetActive(!FreezeGraphics.activeSelf);
            return;
        }

        if (RunManagerSingleton.instance.BuyUpgrade(shopItem))
        {
            sold = true;
            StartCoroutine(Purchased());
        }
    }
}