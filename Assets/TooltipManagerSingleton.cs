using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    Jinx
}

[System.Serializable]
public class Tooltip
{
    public TooltipType TooltipType;
    public GameObject TooltipObject;
}

public class TooltipManagerSingleton : MonoBehaviour
{
    public List<Tooltip> TooltipList = new List<Tooltip>();

    static public TooltipManagerSingleton instance;
    private void Awake()
    {
        instance = this;
    }
    public void EnableTooltip(TooltipType TooltipType)
    {
        TooltipList.FirstOrDefault(x => x.TooltipType == TooltipType).TooltipObject.SetActive(true);
    }

    public void DisableTooltip(TooltipType TooltipType)
    {
        TooltipList.FirstOrDefault(x => x.TooltipType == TooltipType).TooltipObject.SetActive(false);
    }
}
