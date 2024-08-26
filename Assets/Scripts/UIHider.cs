using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UIHider : MonoBehaviour
{

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            FindObjectsByType<AxieSkillEffectManager>(FindObjectsSortMode.None).ToList().ForEach(x => x.GetComponent<Canvas>().enabled = !x.GetComponent<Canvas>().enabled);
        }
    }
}
