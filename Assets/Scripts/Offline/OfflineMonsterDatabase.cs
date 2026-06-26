using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Offline/Offline Monster Database", fileName = "OfflineMonsterDatabase")]
public class OfflineMonsterDatabase : ScriptableObject
{
    public List<MonsterEntry> monsters = new List<MonsterEntry>();
    public List<LandEntry> lands = new List<LandEntry>();

    [Serializable]
    public class MonsterEntry
    {
        [Tooltip("Any unique ID string. Used across UI/teams.")]
        public string id;

        public string name;

        [Tooltip("Hex genes string (typically starts with 0x...)")]
        public string genes;

        [Tooltip("Optional. If empty, it will be derived from genes at runtime.")]
        public string monsterClass;

        public string bodyShape = "normal";
        public bool f2p = true;
        public long birthDate;
    }

    [Serializable]
    public class LandEntry
    {
        public string tokenId;
        public string landType = "monsterpark";
        public string col = "0";
        public string row = "0";
        public bool locked = false;
    }
}

