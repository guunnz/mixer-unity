using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using enemies;
using UnityEngine;
using UnityEngine.Serialization;
using static AtiaBlessing;

public class OpponentTeamManager : MonoBehaviour
{
    static public OpponentTeamManager instance;

    public List<GetAxiesEnemies.AxieEnemy> AxieEnemyList = new List<GetAxiesEnemies.AxieEnemy>();

    [FormerlySerializedAs("enemyTeam")] public Team badTeam;

    public AtiaBlessing atiaBlessing;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this.gameObject);
            return;
        }

        instance = this;
    }

    public IEnumerator SetupTeam(Opponent opponent, List<GetAxiesEnemies.AxieEnemy> axieEnemyList)
    {
        this.AxieEnemyList = axieEnemyList;

        while (badTeam.GetCharactersAll().Count < 5)
        {
            yield return null;
        }

        int index = 0;

        foreach (var teamUpgrades in opponent.axie_team.team_upgrades_values_per_round)
        {
            foreach (var upgrades in teamUpgrades.upgrades_ids)
            {
                Debug.Log("ITEM: " + (BuffEffect)upgrades.id);
                atiaBlessing.AugumentUpgrade((int)upgrades.id,
                    upgrades.axie_class?.Select(x => (AxieClass)x).ToList(),
                    badTeam);
            }

            index++;
            if (index >= RunManagerSingleton.instance.score)
            {
                break;
            }
        }

        // index = 0;
        // foreach (var axie in opponent.axie_team.axies)
        // {
        //     if (axie.upgrades_values_per_round == null)
        //         continue;
        //     foreach (var upgrade in axie.upgrades_values_per_round)
        //     {
        //         foreach (var i in upgrade.upgrades_id)
        //         {
        //             atiaBlessing.AugumentUpgrade(i, axie.axie_id, badTeam);
        //
        //             index++;
        //             if (index >= RunManagerSingleton.instance.score)
        //             {
        //                 break;
        //             }
        //         }
        //     }
        //
        //     index = 0;
        // }
    }
}