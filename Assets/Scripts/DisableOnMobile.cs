using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableOnMobile : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
#if UNITY_ANDROID || UNITY_IOS
        gameObject.SetActive(false);
#endif
    }
}