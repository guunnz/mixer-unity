using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.EventSystems;

public class AbilityDescriptionTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private TooltipType[] tooltipsArray = new TooltipType[0];

    public void SetTooltips(TooltipType[] tooltips)
    {
        tooltipsArray = tooltips;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        foreach (var tooltip in tooltipsArray)
        {

            TooltipManagerSingleton.instance.EnableTooltip(tooltip);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {

        foreach (var tooltip in tooltipsArray)
        {

            TooltipManagerSingleton.instance.DisableTooltip(tooltip);
        }
    }

}
