using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
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
    private int pressTimes;
    public bool isPotion;
    public TooltipType[] tooltips;
    public TextMeshProUGUI[] otherTitles;
    private int actualPrice;


    private void OnEnable()
    {
        Poping.color = Color.clear;
        if (isPotion)
        {
            SetItem(shopItem);
        }
    }

    public void SetItem(ShopItem item)
    {
        tooltips = item.tooltipType;
        if (RunManagerSingleton.instance.landType == LandType.forest)
        {
            item = item.CreateClone();
            item.price--;
            if (RunManagerSingleton.instance.economyPassive.premiumForest)
                item.price--;
        }


        Sold.gameObject.SetActive(false);
        sold = false;
        shopItem = item;
        Poping.text = item.description.Replace("\\n", Environment.NewLine);
        ItemName.text = item.ShopItemName;


        if (RunManagerSingleton.instance.economyPassive.ItemCostPercentage != 100 || RunManagerSingleton.instance.landType == LandType.forest)
        {
            var price = Math.Floor(item.price * RunManagerSingleton.instance.economyPassive.ItemCostPercentage / 100f);
            ItemCost.text = "<color=\"green\">" + (price <= 0 ? 1 : price);
            actualPrice = (int)price;
        }
        else
        {
            actualPrice = item.price;
            ItemCost.text = item.price.ToString();
        }

        if (RunManagerSingleton.instance.economyPassive.frozenItemFree && this.Frozen)
        {
            ItemCost.text = 0.ToString();
        }
        itemImage.sprite = item.ShopItemImage;
        ItemEffect = item.ItemEffectName;
    }

    private void OnMouseOver()
    {
        if (AtiasBlessing.activeSelf)
            return;

        if (!scaling)
        {
            foreach (var otherTitle in otherTitles)
            {
                otherTitle.enabled = false;
            }
            if (tooltips != null)
            {
                foreach (var tooltip in tooltips)
                    TooltipManagerSingleton.instance.EnableTooltipItem(tooltip);

            }
            scaling = true;
            Poping.DOColor(Color.yellow, 0.5f);
            this.transform.DOScale(new Vector3(0.013f, 0.013f, 0.013f), 0.25f);
        }
    }

    private void OnMouseExit()
    {
        if (scaling)
        {
            foreach (var otherTitle in otherTitles)
            {
                otherTitle.enabled = true;
            }

            if (tooltips != null)
            {
                foreach (var tooltip in tooltips)
                    TooltipManagerSingleton.instance.DisableTooltip(tooltip);
            }
#if UNITY_ANDROID || UNITY_IOS
            pressTimes = 0;
#endif
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
        ItemRelicManager.Instance.InstantiateItem(this.transform.position, this.shopItem.CreateClone());
        yield return null;
    }

    public void SetActualPrice()
    {
        ItemCost.text = actualPrice.ToString();
    }

    private void OnMouseDown()
    {
        if (AtiasBlessing.activeSelf || sold)
            return;

#if UNITY_ANDROID || UNITY_IOS
        if (pressTimes == 0 && !ShopManager.instance.FreezeMode)
        {
            pressTimes++;
            return;
        }

#endif


        if (ShopManager.instance.FreezeMode)
        {
            if (RunManagerSingleton.instance.economyPassive.frozenItemFree && !this.Frozen)
            {

                ItemCost.text = 0.ToString();
            }

            SFXManager.instance.PlaySFX(SFXType.Freeze);
            this.Frozen = !Frozen;
            FreezeGraphics.SetActive(!FreezeGraphics.activeSelf);
            return;
        }

        if (RunManagerSingleton.instance.BuyUpgrade(shopItem, frozen: this.Frozen))
        {
            if (this.shopItem.ShopItemName.ToLower().Contains("smooth"))
            {
                RunManagerSingleton.instance.economyPassive.smoothPotionsPurchased++;
            }
            sold = true;
            this.Frozen = false;
            FreezeGraphics.SetActive(false);
            Dictionary<string, string> itemDict = new Dictionary<string, string>();
            itemDict["item_purchased_id"] = ((int)shopItem.ItemEffectName).ToString();
            itemDict["item_purchased_name"] = shopItem.ItemEffectName.ToString();
            MavisTracking.Instance.TrackAction("buy-item", itemDict);
            StartCoroutine(Purchased());
        }
    }
}