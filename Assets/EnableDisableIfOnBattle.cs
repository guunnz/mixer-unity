using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableDisableIfOnBattle : MonoBehaviour
{
    public GameObject BattleUI;
    public GameObject WhatToEnable;
    void OnEnable()
    {
        if (!BattleUI.activeSelf)
        {
            WhatToEnable.SetActive(false);
        }
        else
        {
            WhatToEnable.SetActive(true);
        }
    }
}
