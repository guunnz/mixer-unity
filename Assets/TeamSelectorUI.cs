using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamSelectorUI : MonoBehaviour
{
    public GameObject FakeUI;
    public GameObject RealAxies;
    public GameObject RealLand;


    private void OnEnable()
    {
        RealAxies.SetActive(false);
        RealLand.SetActive(false);
        FakeUI.SetActive(true);
    }

    public void Exit()
    {
        RealAxies.SetActive(true);
        RealLand.SetActive(true);
        FakeUI.SetActive(false);
    }
}