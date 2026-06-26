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

    public List<GetMonstersExample.Monster> MonsterEnemyList = new List<GetMonstersExample.Monster>();

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

    public IEnumerator SetupTeam(Opponent opponent, List<GetMonstersExample.Monster> monsterEnemyList)
    {
        this.MonsterEnemyList = monsterEnemyList;

        while (badTeam.GetCharactersAll().Count < 5)
        {
            yield return null;
        }

        int index = 0;

        InBattleGraphicsManager.Instance.CleanBlessingsOpponent();

        foreach (var teamUpgrades in opponent.monster_team.team_upgrades_values_per_round)
        {
            foreach (var upgrades in teamUpgrades.upgrades_ids)
            {
                InBattleGraphicsManager.Instance.AddUpgrade(upgrades.id);
                atiaBlessing.AugumentUpgrade((int)upgrades.id,
                    upgrades.monster_class?.Select(x => (MonsterClass)x).ToList(),
                    badTeam);
            }

            index++;
            if (index > RunManagerSingleton.instance.score)
            {
                break;
            }
        }

        // index = 0;
        // foreach (var monster in opponent.monster_team.monsters)
        // {
        //     if (monster.upgrades_values_per_round == null)
        //         continue;
        //     foreach (var upgrade in monster.upgrades_values_per_round)
        //     {
        //         foreach (var i in upgrade.upgrades_id)
        //         {
        //             atiaBlessing.AugumentUpgrade(i, monster.monster_id, badTeam);
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