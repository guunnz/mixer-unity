using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FightManagerSingleton : MonoBehaviour
{
    public float SecondsOfFight = 0;

    private bool FightStarted;

    static public FightManagerSingleton Instance;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
    }

    public void StartFight()
    {
        SecondsOfFight = 0;
        FightStarted = true;
    }

    private void FixedUpdate()
    {
        SecondsOfFight += Time.fixedDeltaTime;
    }
}