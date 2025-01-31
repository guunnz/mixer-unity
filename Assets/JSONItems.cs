using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class JSONItems : MonoBehaviour
{
    public List<ShopItem> shopItems;

    private void Start()
    {
        string jsonOutput = GenerateJsonFromShopItems(shopItems);
        GUIUtility.systemCopyBuffer = jsonOutput;
        Debug.Log(jsonOutput);
    }

    public static string GenerateJsonFromShopItems(List<ShopItem> shopItems)
    {
        var shopItemsInfo = shopItems.Select(item => new
        {
            ShopItemName = item.ItemEffectName.ToString(),
            ShopItemImage = item.ShopItemImage != null ? item.ShopItemImage.name : "No Image",
            Description = item.description,
            Price = item.price
        }).ToList();

        return JsonConvert.SerializeObject(shopItemsInfo, Formatting.Indented);
    }
}
