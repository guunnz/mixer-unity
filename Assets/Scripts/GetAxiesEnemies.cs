using System;
using UnityEngine;
using System.Collections;
using SimpleGraphQL;
using System.Collections.Generic;
using System.Linq;
using Game;
using Newtonsoft.Json;
using TMPro;

namespace enemies
{
    public class GetAxiesEnemies : MonoBehaviour
    {
        private GraphQLClient graphQLClient;
        private string address = "0x5506e7c52163d07d9a42ce9514aecdb694d674e3";
        private string apiKey = "eE4lgygsFtLXak1lA60fimKyoSwT64v7"; // Replace with your actual API key
        public AxieSpawner axieSpawner;
        public int spawnCountMax = 0;

        public TextMeshProUGUI Countdown;
        public GameObject FindingOpponent;
        public GameObject IngameOverlay;
        public GameObject BattleOverlay;

        public Team GoodTeam;

        public AxieLandBattleTarget landBattleTarget;

        public TeamToJSON teamToJson;

        public async void GetEnemy()
        {
            FindingOpponent.SetActive(true);
            IngameOverlay.SetActive(false);
       
            string json =
                await landBattleTarget.GetScoreAsync(
                    (RunManagerSingleton.instance.wins + RunManagerSingleton.instance.losses).ToString());

            Opponent opponent = JsonConvert.DeserializeObject<Opponent>(json);
            Debug.Log("Opponent land type is: " + opponent.land_type.ToString());
            GetOpponentTeam(opponent);
        }

        void GetOpponentTeam(Opponent jsonAxieIds)
        {
            graphQLClient = new GraphQLClient("https://api-gateway.skymavis.com/graphql/marketplace");
            List<AxieForBackend> axieIds = jsonAxieIds.axie_team.axies.ToList();
            string combinedQuery = "";

            for (int i = 0; i < axieIds.Count; i++)
            {
                string individualQuery = $@"
            axie{i}: axie(axieId: ""{axieIds[i].axie_id}"") {{
                birthDate
                name
                genes
                id
                class
                parts {{
                    class
                    id
                    name
                    type
                    abilities {{
                        attack
                        attackType
                        name
                        id
                        effectIconUrl
                        defense
                        backgroundUrl
                    }}
                }}
                stats {{
                    speed
                    skill
                    morale
                    hp
                }}
                bodyShape
            }}";
                combinedQuery += individualQuery;
            }

            StartCoroutine(RequestGraphQL(combinedQuery, jsonAxieIds, true));
        }

        private IEnumerator RequestGraphQL(string query, Opponent opponent, bool isOpponent = false)
        {
            var request = new Request()
            {
                Query = isOpponent ? "query {" + query + "}" : query
            };

            var headers = new Dictionary<string, string>
            {
                { "X-API-Key", apiKey }
            };

            var task = graphQLClient.Send(request, null, headers);

            while (!task.IsCompleted)
            {
                yield return null;
            }

            if (task.Exception != null)
            {
                GetEnemy();
                Debug.LogError("GraphQL Error: " + task.Exception.Message);
                yield break;
            }

            try
            {
                string responseString = task.Result;
                if (isOpponent)
                {
                    RunManagerSingleton.instance.currentOpponent = opponent.user_id;
                    RootObject axiesData = JsonConvert.DeserializeObject<RootObject>(responseString);
                    
                        GoodTeam.PostTeam();
                    List<AxieEnemy> axieEnemies = new List<AxieEnemy>();

                    // Assuming axieIds is a List<string>
                    AxieEnemy axie1 = axiesData.Data["axie0"];
                    AxieEnemy axie2 = axiesData.Data["axie1"];
                    AxieEnemy axie3 = axiesData.Data["axie2"];
                    AxieEnemy axie4 = axiesData.Data["axie3"];
                    AxieEnemy axie5 = axiesData.Data["axie4"];
                    axieEnemies.Add(axie1);
                    axieEnemies.Add(axie2);
                    axieEnemies.Add(axie3);
                    axieEnemies.Add(axie4);
                    axieEnemies.Add(axie5);

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

        IEnumerator SpawnAxies(List<AxieEnemy> axieList, Opponent opponent, bool isOpponent)
        {
            foreach (var axieEnemy in axieList)
            {
                AxieForBackend axieForBackend = opponent.axie_team.axies.Single(x => x.axie_id == axieEnemy.id);
                axieSpawner.SpawnEnemyAxieById(axieEnemy.id, BodyPart.Horn, SkillName.HerosBane,
                    axieEnemy.axieClass,
                    axieEnemy.stats, axieForBackend, axieList, isOpponent);
                yield return new WaitForSeconds(0.2f);
            }

            while (axieSpawner.enemyTeam.GetCharacters().Count != 5)
            {
                yield return null;
            }

            FindingOpponent.gameObject.SetActive(false);
            Countdown.gameObject.SetActive(true);
            Countdown.text = "3!";
            yield return new WaitForSeconds(1f);
            Countdown.text = "2!";
            yield return new WaitForSeconds(1f);
            Countdown.text = "1!";
            yield return new WaitForSeconds(0.5f);
            Countdown.text = "BATTLE!";
            yield return new WaitForSeconds(0.5f);
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
            public string id { get; set; }
            public string Class { get; set; }

            public AxieClass axieClass => (AxieClass)Enum.Parse(typeof(AxieClass), Class);
            public List<GetAxiesExample.Part> Parts { get; set; }
            public GetAxiesExample.Stats stats { get; set; }
            public string BodyShape { get; set; }
        }
    }
}