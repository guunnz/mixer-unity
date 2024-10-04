using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
[CreateAssetMenu(fileName = "ShopItem", menuName = "Axie/ShopItem")]
public class ShopItem : ScriptableObject
{
    public string ShopItemName;
    public AtiaBlessing.BuffEffect ItemEffectName;
    public Sprite ShopItemImage;
    public ShopItemEffect ItemEffect;
    public GameObject shopPrefab; // Prefab can be assigned in the editor if needed
    public string description;
    public int price;
    public TooltipType[] tooltipType;

    public bool isPotion()
    {
        return ItemEffectName == AtiaBlessing.BuffEffect.Increase_HP || ItemEffectName == AtiaBlessing.BuffEffect.Increase_Morale || ItemEffectName == AtiaBlessing.BuffEffect.Increase_Speed || ItemEffectName == AtiaBlessing.BuffEffect.Increase_Skill;
    }

    public ShopItem CreateClone()
    {
        return new ShopItem()
        {
            ShopItemName = ShopItemName,
            ItemEffectName = ItemEffectName,
            ShopItemImage = ShopItemImage,
            ItemEffect = ItemEffect,
            shopPrefab = shopPrefab,
            description = description,
            price = price,
        };
    }
}