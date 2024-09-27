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
        if (!EnableInstead)
            gameObject.SetActive(false);
#endif


#if UNITY_STANDALONE || UNITY_WEBGL
 if (EnableInstead)
        {
         gameObject.SetActive(false);
        }
#endif
    }
}