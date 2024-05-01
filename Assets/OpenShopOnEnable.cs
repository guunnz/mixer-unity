using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenShopOnEnable : MonoBehaviour
{
    public GameObject shop;

    private void OnEnable()
    {
        shop.SetActive(true);
    }

    private void OnDisable()
    {
        shop.SetActive(false);
    }
}