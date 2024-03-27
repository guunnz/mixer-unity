using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOn : MonoBehaviour
{
    public float timer = 0.2f;

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(timer);

        Destroy(this.gameObject);
    }
}