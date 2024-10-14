using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class AbilityTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public SkillName skillNameTooltip;

    public void OnPointerEnter(PointerEventData eventData)
    {
        TooltipManagerSingleton.instance.EnableTooltip(TooltipType.OnlyTitle, AxieGeneUtils.SpaceCamelCase(skillNameTooltip.ToString()));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipManagerSingleton.instance.DisableTooltip();
    }
}
