using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class StatTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public TooltipType tooltip;

    public void OnPointerEnter(PointerEventData eventData)
    {
        TooltipManagerSingleton.instance.EnableTooltip(tooltip);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipManagerSingleton.instance.DisableTooltip(tooltip);
    }
}
