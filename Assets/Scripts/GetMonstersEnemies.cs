using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Game;
using Newtonsoft.Json;
using TMPro;
using static GetMonstersExample;
using System.Threading.Tasks;
using static BattleDataLoader;

namespace enemies
{
    public class GetMonstersEnemies : MonoBehaviour
    {
        public MonsterSpawner monsterSpawner;
        public int spawnCountMax = 0;

        public TextMeshProUGUI Countdown;
        public List<TextMeshProUGUI> EnemyUsernames;
        public GameObject FindingOpponent;
        public GameObject IngameOverlay;
        public GameObject BattleOverlay;

        public Team GoodTeam;

        public MonsterLandBattleTarget landBattleTarget;

        public TeamToJSON teamToJson;

        public EnemyLandAnimation enemyLandAnimation;
        public BattleDataLoader battleDataLoader;
        public GameObject Store;

        public async void GetEnemy()
        {

            FindingOpponent.SetActive(true);
            IngameOverlay.SetActive(false);
            await Task.Delay(10);
            string json = "";
        
                int num = RunManagerSingleton.instance.wins + RunManagerSingleton.instance.losses;
                json = await landBattleTarget.GetScoreAsync(num.ToString());
            

            if (string.IsNullOrEmpty(json))
            {
                MapManager.Instance.ToggleRectangles();
                Store.SetActive(true);
                FindingOpponent.SetActive(false);
                IngameOverlay.SetActive(true);
                NotificationErrorManager.instance.DoNotification("An error has ocurred when trying to find an opponent, please try again");
                return;
            }

            Opponent opponent = JsonConvert.DeserializeObject<Opponent>(json);
            if (string.IsNullOrEmpty(opponent.monster_captain_genes) || opponent.monster_team.monsters.Any(x => string.IsNullOrEmpty(x.genes)))
            {
                GetEnemy();
                return;
            }
            Debug.Log("Opponent land type is: " + opponent.land_type.ToString());
            BuildEnemyTeamMonsters(opponent, true);
        }

        void GetOpponentCaptain(string monsterId, string monsterGenes)
        {
            var descriptor = monsterSpawner.SimpleProcessMixer(monsterId, monsterGenes, false);

            TeamCaptainManager.Instance.SetOpponentCaptain(descriptor);
        }

        private void BuildEnemyTeamMonsters(Opponent opponent, bool isOpponent = false)
        {
            try
            {
                if (isOpponent)
                {

                    if (!AccountManager.TestMode)
                        GoodTeam.PostTeam();

                    List<GetMonstersExample.Monster> monsterEnemies = new List<GetMonstersExample.Monster>();

                    foreach (var monstersInList in opponent.monster_team.monsters)
                    {
                        GetMonstersExample.Monster monster = new GetMonstersExample.Monster();

                        monster.genes = monstersInList.genes;


                        monster.@class = MonsterGeneUtils.GetMonsterClass(monster.genes).ToString();
                        monster.id = monstersInList.monster_id;
                        monster.name = "";
                        monster.birthDate = 0;
                        monster.newGenes = monster.genes;
                        monster.stats = MonsterGeneUtils.GetStatsByGenesAndMonsterClass(monster.genes, monster.monsterClass);

                        Debug.Log("Loading parts");
                        List<string> partsClasses = MonsterGeneUtils.GetMonsterPartsClasses(monster.genes);
                        Debug.Log("Loading abilities");
                        List<string> partsAbilities = MonsterGeneUtils.ParsePartIdsFromHex(monster.genes);

                        if (partsAbilities == null)
                            continue;

                        Debug.Log("Abilities loaded");

                        List<GetMonstersExample.Part> monsterParts = new List<GetMonstersExample.Part>();

                        GetMonstersExample.Part horn = new GetMonstersExample.Part(partsClasses[2],
                            "", "horn", 0, false, partsAbilities[2]);
                        GetMonstersExample.Part tail = new GetMonstersExample.Part(partsClasses[5],
                            "", "tail", 0, false, partsAbilities[5]);
                        GetMonstersExample.Part back = new GetMonstersExample.Part(partsClasses[4],
                            "", "back", 0, false, partsAbilities[4]);
                        GetMonstersExample.Part mouth = new GetMonstersExample.Part(partsClasses[3],
                            "", "mouth", 0, false, partsAbilities[3]);

                        monsterParts.Add(horn);
                        monsterParts.Add(tail);
                        monsterParts.Add(back);
                        monsterParts.Add(mouth);

                        monster.parts = monsterParts.ToArray();

                        monsterEnemies.Add(monster);
                    }

                    monsterEnemies = monsterEnemies.OrderBy(x => int.Parse(x.id)).ToList();
                    StartCoroutine(SpawnMonsters(monsterEnemies, opponent, isOpponent));
                    StartCoroutine(OpponentTeamManager.instance.SetupTeam(opponent, monsterEnemies));
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Error processing response: " + ex.Message);
            }
        }

        IEnumerator SpawnMonsters(List<GetMonstersExample.Monster> monsterList, Opponent opponent, bool isOpponent)
        {
            monsterSpawner.enemyTeam.landType = (LandType)opponent.land_type;
            monsterSpawner.enemyTeam.OnBattleStartActions.Clear();

            //if (AccountManager.TestMode)
            //{
            //    monsterList = TestTool.Instance.GetEnemyMonsters(monsterList);
            //}

            foreach (var monsterEnemy in monsterList)
            {
                MonsterForBackend monsterForBackend = opponent.monster_team.monsters.Single(x => x.monster_id == monsterEnemy.id);
                monsterSpawner.SpawnEnemyMonsterById(monsterEnemy.id,
                    monsterEnemy.monsterClass,
                    monsterEnemy.stats, monsterForBackend, monsterList, monsterEnemy, isOpponent);
                yield return new WaitForSeconds(0.2f);
            }
            EnemyUsernames.ForEach(x => x.text = string.IsNullOrEmpty(opponent.username) ? "Lunacian #" + UnityEngine.Random.Range(1000000, 9999999).ToString() : opponent.username);
            //If test mode
            //Grab testing values of abilities, stats and 

            MusicManager.Instance.FadeOut(1);

            while (monsterSpawner.enemyTeam.GetAliveCharacters().Count != 5)
            {
                yield return null;
            }

            if (string.IsNullOrEmpty(opponent.monster_captain_id))
            {
                var captainToSelect = monsterSpawner.enemyTeam.GetCharactersAll().First();
                TeamCaptainManager.Instance.SetOpponentCaptain(captainToSelect.visualDescriptor);
            }
            else
            {
                GetOpponentCaptain(opponent.monster_captain_id, opponent.monster_captain_genes);
            }

            RunManagerSingleton.instance.ScoreRect.gameObject.SetActive(false);
            RunManagerSingleton.instance.ItemsTooltip.gameObject.SetActive(false);
            FindingOpponent.gameObject.SetActive(false);
            enemyLandAnimation.DoAnimation((LandType)opponent.land_type);
            //enemyLandAnimation.DoAnimation((LandType)opponent.land_type);
            TooltipManagerSingleton.instance.DisableTooltip();
            Countdown.gameObject.SetActive(true);
            Countdown.text = "3!";
            SFXManager.instance.PlaySFX(SFXType.ThreeTwoOne, 0.06f, false, 1);
            yield return new WaitForSeconds(1f);
            SFXManager.instance.PlaySFX(SFXType.ThreeTwoOne, 0.06f, false, 1);
            Countdown.text = "2!";
            yield return new WaitForSeconds(1f);
            SFXManager.instance.PlaySFX(SFXType.ThreeTwoOne, 0.06f, false, 1);
            Countdown.text = "1!";
            yield return new WaitForSeconds(1f);
            SFXManager.instance.PlaySFX(SFXType.ThreeTwoOne, 0.06f, false, 1.4f);
            Countdown.text = "BATTLE!";
            yield return new WaitForSeconds(0.5f);
            FightManagerSingleton.Instance.StartFight();

            //if (AccountManager.TestMode)
            //{
            //    TestTool.Instance.SetEnemyMonstersStatuses();
            //    TestTool.Instance.SetAllyMonstersStatuses();
            //}
            BattleOverlay.SetActive(true);
            Countdown.gameObject.SetActive(false);
            monsterSpawner.enemyTeam.StartBattle();
            monsterSpawner.goodTeam.StartBattle();
        }

        public class RootObject
        {
            public Dictionary<string, MonsterEnemy> Data { get; set; }
        }

        [System.Serializable]
        public class MonstersEnemies
        {
            public MonsterEnemy[] results;
        }

        public class MonsterEnemy
        {
            public long BirthDate { get; set; }
            public string Name { get; set; }
            public string Genes { get; set; }
            public string NewGenes { get; set; }
            public string id { get; set; }
            public string Class { get; set; }
            public MonsterClass monsterClass => (MonsterClass)Enum.Parse(typeof(MonsterClass), Class);
            public List<GetMonstersExample.Part> Parts { get; set; }
            public GetMonstersExample.Stats stats { get; set; }
            public string BodyShape { get; set; }

            public Dictionary<string, string> cursedMeta = new Dictionary<string, string>();
        }
    }
}
