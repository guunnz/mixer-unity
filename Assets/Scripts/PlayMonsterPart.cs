using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayMonsterPart : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public MonstersView monsterView;
    public BodyPart bodyPart;


    public void OnPointerEnter(PointerEventData eventData)
    {
        monsterView.PlayAnimation(bodyPart);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        monsterView.StopAnimation(bodyPart);
    }
}
