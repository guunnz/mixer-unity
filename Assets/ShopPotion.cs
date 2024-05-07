using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ShopPotion : MonoBehaviour
{
    public AtiaBlessing.Blessing upgrade;

    public GameObject AtiasBlessing;

    private void OnMouseDown()
    {
        if (AtiasBlessing.activeSelf)
            return;
        RunManagerSingleton.instance.BuyUpgrade((int)upgrade);
    }
}