using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using enemies;
using UnityEngine;
using UnityEngine.Serialization;

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
        foreach (var teamUpgrades in opponent.axie_team.team_upgrades)
        {
            index++;
            if (index >= RunManagerSingleton.instance.score)
            {
                break;
            }

            atiaBlessing.AugumentUpgrade((int)teamUpgrades.upgrade_id,
                teamUpgrades.axieClass?.Select(x => (AxieClass)x).ToList(),
                badTeam);
        }

        index = 0;
        foreach (var axie in opponent.axie_team.axie)
        {
            foreach (var upgrade in axie.upgrades.upgrades_id)
            {
                index++;
                if (index >= RunManagerSingleton.instance.score)
                {
                    break;
                }

                atiaBlessing.AugumentUpgrade(upgrade, axie.axie_id, badTeam);
            }

            index = 0;
        }
    }
}