using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetDefaultOnMobileLayer : MonoBehaviour
{

    void Start()
    {
#if UNITY_ANDROID || UNITY_IOS
        SetLayerRecursively(transform, "Default");
#endif
    }
#if UNITY_ANDROID || UNITY_IOS
    void SetLayerRecursively(Transform root, string layerName)
    {
        // Set the layer of the current root
        root.gameObject.layer = LayerMask.NameToLayer(layerName);

        // Iterate over all children and apply the function recursively
        for (int i = 0; i < root.childCount; i++)
        {
            SetLayerRecursively(root.GetChild(i), layerName);
        }
    }
#endif
}
