using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlusLessItemsRelic : MonoBehaviour
{
    public void OnMouseDown()
    {
        ItemRelicManager.Instance.ToggleShowMoreLess();
        SFXManager.instance.PlaySFX(SFXType.UIButtonConfirm);
    }
}
