using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GraphQLClientExample : MonoBehaviour
{
    [System.Serializable]
    public class LandsData
    {
        public Data data;
    }

    [System.Serializable]
    public class Data
    {
        public Lands lands;
    }

    [System.Serializable]
    public class Lands
    {
        public int total;
        public Land[] results;
    }

    [System.Serializable]
    public class Land
    {
        public string landType;
        public string owner;
        public string tokenId;
    }
}