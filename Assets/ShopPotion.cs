using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ShopPotion : MonoBehaviour
{
    public Upgrades upgrade;

    private void OnMouseDown()
    {
        RunManagerSingleton.instance.BuyUpgrade((int)upgrade);
    }
}