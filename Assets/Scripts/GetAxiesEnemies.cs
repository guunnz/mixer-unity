using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Game;
using Newtonsoft.Json;
using TMPro;
using static GetAxiesExample;
using System.Threading.Tasks;
using static BattleDataLoader;

namespace enemies
{
    public class GetAxiesEnemies : MonoBehaviour
    {
        public AxieSpawner axieSpawner;
        public int spawnCountMax = 0;

        public TextMeshProUGUI Countdown;
        public List<TextMeshProUGUI> EnemyUsernames;
        public GameObject FindingOpponent;
        public GameObject IngameOverlay;
        public GameObject BattleOverlay;

        public Team GoodTeam;

        public AxieLandBattleTarget landBattleTarget;

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
            if (string.IsNullOrEmpty(opponent.axie_captain_genes) || opponent.axie_team.axies.Any(x => string.IsNullOrEmpty(x.genes)))
            {
                GetEnemy();
                return;
            }
            Debug.Log("Opponent land type is: " + opponent.land_type.ToString());
            BuildEnemyTeamAxies(opponent, true);
        }

        void GetOpponentCaptain(string axieId, string axieGenes)
        {
            var builderResult = axieSpawner.SimpleProcessMixer(axieId, axieGenes, true);

            TeamCaptainManager.Instance.SetOpponentCaptain(builderResult.skeletonDataAsset, builderResult.sharedGraphicMaterial);
        }

        private void BuildEnemyTeamAxies(Opponent opponent, bool isOpponent = false)
        {
            try
            {
                if (isOpponent)
                {

                    if (!AccountManager.TestMode)
                        GoodTeam.PostTeam();

                    List<GetAxiesExample.Axie> axieEnemies = new List<GetAxiesExample.Axie>();

                    foreach (var axiesInList in opponent.axie_team.axies)
                    {
                        GetAxiesExample.Axie axie = new GetAxiesExample.Axie();

                        axie.genes = axiesInList.genes;


                        axie.@class = AxieGeneUtils.GetAxieClass(axie.genes).ToString();
                        axie.id = axiesInList.axie_id;
                        axie.name = "";
                        axie.birthDate = 0;
                        axie.newGenes = axie.genes;
                        axie.stats = AxieGeneUtils.GetStatsByGenesAndAxieClass(axie.genes, axie.axieClass);

                        Debug.Log("Loading parts");
                        List<string> partsClasses = AxieGeneUtils.GetAxiePartsClasses(axie.genes);
                        Debug.Log("Loading abilities");
                        List<string> partsAbilities = AxieGeneUtils.ParsePartIdsFromHex(axie.genes);

                        if (partsAbilities == null)
                            continue;

                        Debug.Log("Abilities loaded");

                        List<GetAxiesExample.Part> axieParts = new List<GetAxiesExample.Part>();

                        GetAxiesExample.Part horn = new GetAxiesExample.Part(partsClasses[2],
                            "", "horn", 0, false, partsAbilities[2]);
                        GetAxiesExample.Part tail = new GetAxiesExample.Part(partsClasses[5],
                            "", "tail", 0, false, partsAbilities[5]);
                        GetAxiesExample.Part back = new GetAxiesExample.Part(partsClasses[4],
                            "", "back", 0, false, partsAbilities[4]);
                        GetAxiesExample.Part mouth = new GetAxiesExample.Part(partsClasses[3],
                            "", "mouth", 0, false, partsAbilities[3]);

                        axieParts.Add(horn);
                        axieParts.Add(tail);
                        axieParts.Add(back);
                        axieParts.Add(mouth);

                        axie.parts = axieParts.ToArray();

                        axieEnemies.Add(axie);
                    }

                    axieEnemies = axieEnemies.OrderBy(x => int.Parse(x.id)).ToList();
                    StartCoroutine(SpawnAxies(axieEnemies, opponent, isOpponent));
                    StartCoroutine(OpponentTeamManager.instance.SetupTeam(opponent, axieEnemies));
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Error processing response: " + ex.Message);
            }
        }

        IEnumerator SpawnAxies(List<GetAxiesExample.Axie> axieList, Opponent opponent, bool isOpponent)
        {
            axieSpawner.enemyTeam.landType = (LandType)opponent.land_type;
            axieSpawner.enemyTeam.OnBattleStartActions.Clear();

            //if (AccountManager.TestMode)
            //{
            //    axieList = TestTool.Instance.GetEnemyAxies(axieList);
            //}

            foreach (var axieEnemy in axieList)
            {
                AxieForBackend axieForBackend = opponent.axie_team.axies.Single(x => x.axie_id == axieEnemy.id);
                axieSpawner.SpawnEnemyAxieById(axieEnemy.id,
                    axieEnemy.axieClass,
                    axieEnemy.stats, axieForBackend, axieList, axieEnemy, isOpponent);
                yield return new WaitForSeconds(0.2f);
            }
            EnemyUsernames.ForEach(x => x.text = string.IsNullOrEmpty(opponent.username) ? "Lunacian #" + UnityEngine.Random.Range(1000000, 9999999).ToString() : opponent.username);
            //If test mode
            //Grab testing values of abilities, stats and 

            MusicManager.Instance.FadeOut(1);

            while (axieSpawner.enemyTeam.GetAliveCharacters().Count != 5)
            {
                yield return null;
            }

            if (string.IsNullOrEmpty(opponent.axie_captain_id))
            {
                var captainToSelect = axieSpawner.enemyTeam.GetCharactersAll().First();
                TeamCaptainManager.Instance.SetOpponentCaptain(captainToSelect.skeletonDataAsset, captainToSelect.skeletonMaterial);
            }
            else
            {
                GetOpponentCaptain(opponent.axie_captain_id, opponent.axie_captain_genes);
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
            //    TestTool.Instance.SetEnemyAxiesStatuses();
            //    TestTool.Instance.SetAllyAxiesStatuses();
            //}
            BattleOverlay.SetActive(true);
            Countdown.gameObject.SetActive(false);
            axieSpawner.enemyTeam.StartBattle();
            axieSpawner.goodTeam.StartBattle();
        }

        public class RootObject
        {
            public Dictionary<string, AxieEnemy> Data { get; set; }
        }

        [System.Serializable]
        public class AxiesEnemies
        {
            public AxieEnemy[] results;
        }

        public class AxieEnemy
        {
            public long BirthDate { get; set; }
            public string Name { get; set; }
            public string Genes { get; set; }
            public string NewGenes { get; set; }
            public string id { get; set; }
            public string Class { get; set; }
            public AxieClass axieClass => (AxieClass)Enum.Parse(typeof(AxieClass), Class);
            public List<GetAxiesExample.Part> Parts { get; set; }
            public GetAxiesExample.Stats stats { get; set; }
            public string BodyShape { get; set; }

            public Dictionary<string, string> cursedMeta = new Dictionary<string, string>();
        }
    }
}