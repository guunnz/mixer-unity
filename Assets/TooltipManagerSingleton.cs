using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using static AtiaBlessing;

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
    FreeAxie,
    HpStat,
    MoraleStat,
    SkillStat,
    SpeedStat
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

    private void Start()
    {
#if UNITY_ANDROID || UNITY_IOS
        this.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 85f);
#endif
    }
    public void EnableTooltipItem(TooltipType TooltipType)
    {
#if UNITY_ANDROID || UNITY_IOS
        this.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -80f);
#else
        this.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -50f);
#endif
        TooltipList.FirstOrDefault(x => x.TooltipType == TooltipType).TooltipObject.SetActive(true);
    }
    public string GetClassType(BuffEffect effect)
    {
        // Convert the enum to string
        string enumName = effect.ToString();

        // Extract and return the class type based on the suffix
        if (enumName.EndsWith("Aqua")) return "Aqua";
        if (enumName.EndsWith("Bird")) return "Bird";
        if (enumName.EndsWith("Dawn")) return "Dawn";
        if (enumName.EndsWith("Beast")) return "Beast";
        if (enumName.EndsWith("Bug")) return "Bug";
        if (enumName.EndsWith("Mech")) return "Mech";
        if (enumName.EndsWith("Plant")) return "Plant";
        if (enumName.EndsWith("Dusk")) return "Dusk";
        if (enumName.EndsWith("Reptile")) return "Reptile";

        // If no match found, return "Unknown"
        return "Unknown";
    }


    public void EnableTooltip(ShopItem shopItem, bool offset = false)
    {
#if UNITY_ANDROID || UNITY_IOS
        ShopItemTooltip.TooltipObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 85f);
#else
        ShopItemTooltip.TooltipObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 63.5f);
#endif
        if ((int)shopItem.ItemEffectName <= 67)
        {
            if (offset)
            {
                ShopItemTooltip.TooltipObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(-100, 0f);
            }
            ShopItemTooltip.ItemName.text = "<color=\"yellow\">" + shopItem.ShopItemName + "</color>";
        }
        else
        {
            if (offset)
            {
                ShopItemTooltip.TooltipObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -100f);
            }
            ShopItemTooltip.ItemName.text = "<color=\"yellow\">" + GetClassType(shopItem.ItemEffectName) + " Blessing" + "</color>";
        }

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
