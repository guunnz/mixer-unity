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


    public void EnableTooltip(ShopItem shopItem)
    {
#if UNITY_ANDROID || UNITY_IOS
        this.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 85f);
#else
        this.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 63.5f);
#endif
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
