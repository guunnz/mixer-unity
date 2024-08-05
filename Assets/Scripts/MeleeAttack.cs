using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeAttack : MonoBehaviour
{
    void Start()
    {
        SFXManager.instance.PlaySFX(SFXType.Hit);
    }
}
