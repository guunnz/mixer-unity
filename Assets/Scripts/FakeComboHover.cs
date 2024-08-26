using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class FakeComboHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public BodyPart bodyPart;
    public FakeAxieComboManager manager;

    public void OnPointerEnter(PointerEventData eventData)
    {
        manager.BodyPartHover(bodyPart);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        manager.BodyPartStopHover();
    }
}
