using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "ShopItem", menuName = "Axie/ShopItem")]
public class ShopItem : ScriptableObject
{
    public string ShopItemName;
    public Sprite ShopItemImage;
    public ShopItemEffect ItemEffect;
    public GameObject prefab; // Prefab can be assigned in the editor if needed
    public string description;
    public int price;
}