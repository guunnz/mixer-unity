using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenShopOnEnable : MonoBehaviour
{
    public GameObject shop;
    public bool reverse = false;
    private void OnEnable()
    {
        shop.SetActive((!reverse));
    }

    private void OnDisable()
    {
        shop.SetActive(reverse);
    }
}