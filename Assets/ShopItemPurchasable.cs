using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItemPurchasable : MonoBehaviour
{
    private AtiaBlessing.BuffEffect ItemEffect;

    public GameObject AtiasBlessing;

    public TextMeshProUGUI Poping;
    public TextMeshProUGUI ItemName;
    public TextMeshProUGUI ItemCost;
    public SpriteRenderer itemImage;
    public TextMeshProUGUI Sold;
    public ShopItem shopItem;
    public bool sold;
    private bool scaling = false;
    public bool Frozen;
    public GameObject FreezeGraphics;
    public bool PreventSetOnStart;

    private void OnEnable()
    {
        Poping.color = Color.clear;
    }

    public void SetItem(ShopItem item)
    {
        Sold.gameObject.SetActive(false);
        sold = false;
        shopItem = item;
        Poping.text = item.description;
        ItemName.text = item.ShopItemName;
        ItemCost.text = Math.Floor(item.price * RunManagerSingleton.instance.economyPassive.ItemCostPercentage / 100f).ToString();
        itemImage.sprite = item.ShopItemImage;
        ItemEffect = item.ItemEffectName;
    }

    private void OnMouseOver()
    {
        if (!scaling)
        {
            scaling = true;
            Poping.DOColor(Color.yellow, 0.5f);
            this.transform.DOScale(new Vector3(0.013f, 0.013f, 0.013f), 0.25f);
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

    private void Start()
    {
        if (PreventSetOnStart)
            return;
        SetItem(shopItem);
    }

    public IEnumerator Purchased()
    {
        Sold.gameObject.SetActive(true);

        yield return null;
    }

    private void OnMouseDown()
    {
        if (AtiasBlessing.activeSelf || sold)
            return;

        if (ShopManager.instance.FreezeMode)
        {
            this.Frozen = !this.Frozen;
            FreezeGraphics.SetActive(!FreezeGraphics.activeSelf);
            return;
        }
        else if (this.Frozen)
        {
            return;
        }

        if (RunManagerSingleton.instance.BuyUpgrade(shopItem))
        {
            sold = true;
            StartCoroutine(Purchased());
        }
    }
}