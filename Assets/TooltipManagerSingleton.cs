using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public enum TooltipType
{
    Wombo,
    Poison,
    Lethal,
    Morale,
    Speed,
    Attack,
    Stench,
    Aroma,
    Chilled,
    Fear,
    Stun,
    Merry,
    Lunge,
    HotButt,
    Kestrel,
    FishSnack,
    Gecko,
    AxiePark,
    Savannah,
    Forest,
    Arctic,
    Mystic,
    Genesis,
    Prioritize,
    Jinx,
    Energy,
    LunaLanding,
    FreeAxie
}

[System.Serializable]
public class Tooltip
{
    public TooltipType TooltipType;
    public GameObject TooltipObject;
}


[System.Serializable]
public class ShopTooltip
{
    public GameObject TooltipObject;
    public TextMeshProUGUI ItemName;
    public TextMeshProUGUI ItemDescription;
}

public class TooltipManagerSingleton : MonoBehaviour
{
    public List<Tooltip> TooltipList = new List<Tooltip>();


    static public TooltipManagerSingleton instance;
    public ShopTooltip ShopItemTooltip;
    private void Awake()
    {
        instance = this;
    }
    public void EnableTooltip(ShopItem shopItem)
    {
        ShopItemTooltip.ItemName.text = "<color=\"yellow\">" + shopItem.ShopItemName + "</color>";
        ShopItemTooltip.ItemDescription.text = shopItem.description;
        ShopItemTooltip.TooltipObject.SetActive(true);
    }

    public void DisableTooltip(TooltipType TooltipType)
    {
        TooltipList.FirstOrDefault(x => x.TooltipType == TooltipType).TooltipObject.SetActive(false);
    }


    public void EnableTooltip(TooltipType TooltipType)
    {
        TooltipList.FirstOrDefault(x => x.TooltipType == TooltipType).TooltipObject.SetActive(true);
    }

    public void DisableTooltip()
    {
        ShopItemTooltip.TooltipObject.SetActive(false);
    }
}
