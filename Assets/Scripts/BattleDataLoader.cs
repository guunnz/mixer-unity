using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static BattleDataLoader;

public class BattleDataLoader : MonoBehaviour
{
    [Serializable]
    public class Opponent
    {
        public string user_id;
        public int land_type;
        public AxieTeamDatabase axie_team;
    }

    [Serializable]
    public class AxieTeamDatabase
    {
        public AxieForBackend[] axies;
        public UpgradeValuesPerRound[] team_upgrades_values_per_round;
    }

    [Serializable]
    public class AxieForBackend
    {
        public string axie_id;
        public Combos[] combos_values_per_round;
        public Position[] position_values_per_round;
        public int[] upgrades_values_per_round;
    }

    [Serializable]
    public class Combos
    {
        public int[] combos_id;
    }

    [Serializable]
    public class Position
    {
        public int row;
        public int col;
    }

    [Serializable]
    public class OpponentList
    {
        public Opponent[] opponents;
    }

    private string jsonContent;

    void Start()
    {
        jsonContent = LoadJsonFromFile("axielandbattles.runs", true);
        GetTestRandomOpponent();
    }

    public string GetTestRandomOpponent(int inputScore = 0)
    {

        OpponentList opponentList = JsonUtility.FromJson<OpponentList>("{\"opponents\":" + jsonContent + "}");
        string result = GetRandomOpponentByScore(opponentList, inputScore);
        return result;
    }

    string LoadJsonFromFile(string path, bool isResourceFolder)
    {
        TextAsset file = Resources.Load<TextAsset>(path);
        return file.text;
    }

    string GetRandomOpponentByScore(OpponentList opponentList, int score)
    {
        List<Opponent> filteredOpponents = new List<Opponent>();
        foreach (var opponent in opponentList.opponents)
        {
            if (opponent.axie_team.team_upgrades_values_per_round.Length >= score)
            {
                filteredOpponents.Add(opponent);
                break;
            }
        }

        if (filteredOpponents.Count > 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, filteredOpponents.Count);
            return JsonUtility.ToJson(filteredOpponents[randomIndex]);
        }
        return null;
    }
}
