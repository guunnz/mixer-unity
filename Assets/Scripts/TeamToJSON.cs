using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Opponent
{
    public string user_wallet_address;
    public string username;
    public string monster_captain_id;
    public string monster_captain_genes;
    public int land_type;
    public MonsterTeamDatabase monster_team;
}


public class TeamToJSON : MonoBehaviour
{
}