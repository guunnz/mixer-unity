using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayAxiePart : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public AxiesView axieView;
    public BodyPart bodyPart;

    public void OnPointerEnter(PointerEventData eventData)
    {
        axieView.PlayAnimation(bodyPart);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        axieView.StopAnimation(bodyPart);
    }
}
