using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AxieIdsWrapper
{
    public string[] axieids;
}


public class TeamToJSON : MonoBehaviour
{
    public string JsonConstructor(string[] axieids)
    {
        AxieIdsWrapper wrapper = new AxieIdsWrapper();
        wrapper.axieids = axieids;
        return JsonUtility.ToJson(wrapper);
    }
}
