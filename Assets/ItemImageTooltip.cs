using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemImageTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public ShopItem ShopItem;

    public Image ImageItem;
    public TextMeshProUGUI TimesText;

    public int Times;

    public void IncreaseTimes()
    {
        Times++;
        if (Times > 1)
        {
            TimesText.gameObject.SetActive(true);
            TimesText.text = Times.ToString();
        }
    }

    public void SetItem(ShopItem shopItem)
    {
        Times++;
        ShopItem = shopItem;
        ImageItem.sprite = shopItem.ShopItemImage;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        TooltipManagerSingleton.instance.EnableTooltip(ShopItem, true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipManagerSingleton.instance.DisableTooltip();
    }
}
