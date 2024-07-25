using System.Collections;
using System.Collections.Generic;
using AxieMixer.Unity;
using UnityEngine;

public class InitializationSystem : MonoBehaviour
{
    void Start()
    {
        Mixer.initialized = false;
        Mixer.Init();
    }
}