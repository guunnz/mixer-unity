using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Opponent
{
    public string user_wallet_address;
    public string username;
    public string axie_captain_id;
    public string axie_captain_genes;
    public int land_type;
    public AxieTeamDatabase axie_team;
}


public class TeamToJSON : MonoBehaviour
{
}