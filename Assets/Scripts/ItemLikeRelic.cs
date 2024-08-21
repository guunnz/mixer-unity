using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI; // Required for the Image component

public class ItemLikeRelic : MonoBehaviour
{
    private ShopItem shopItem;
    public TMP_Text countText;
    public SpriteRenderer itemImage; // Public Image to display the ShopItem sprite
    private int count = 1;

    void Start()
    {
        countText.text = count.ToString();
        UpdateUI(); // Update UI components including the image at start
    }

    public void SetShopItem(ShopItem newShopItem)
    {
        shopItem = newShopItem;
        UpdateUI(); // Update UI elements whenever a new ShopItem is set
    }

    public AtiaBlessing.BuffEffect GetItemEffectName()
    {
        return shopItem.ItemEffectName;
    }

    private void UpdateUI()
    {
        // Set the sprite of the Image component to the ShopItem's sprite
        itemImage.sprite = shopItem.ShopItemImage;
    }

    public void IncrementCount()
    {
        count++;
        countText.gameObject.SetActive(true);
        countText.text = count.ToString();
    }

    public void OnMouseEnter()
    {
        TooltipManagerSingleton.instance.EnableTooltip(shopItem);
    }

    public void OnMouseExit()
    {
        TooltipManagerSingleton.instance.DisableTooltip();
    }
}
