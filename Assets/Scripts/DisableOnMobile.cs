using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableOnMobile : MonoBehaviour
{
    // Start is called before the first frame update
    public bool EnableInstead;
    void Start()
    {
#if UNITY_ANDROID || UNITY_IOS
        gameObject.SetActive(EnableInstead ? true : false);
#endif
    }
}