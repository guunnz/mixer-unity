using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ComboHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public BodyPart bodyPart;
    public AbilitiesManager manager;

    public void OnPointerEnter(PointerEventData eventData)
    {
        manager.BodyPartHover(bodyPart);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        manager.BodyPartStopHover();
    }
}