using System;
using UnityEngine;
using System.Collections;
using SimpleGraphQL;
using System.Collections.Generic;
using System.Linq;
using Game;
using Newtonsoft.Json;

namespace enemies
{
    public class GetAxiesEnemies : MonoBehaviour
    {
        private GraphQLClient graphQLClient;
        private string address = "0x5506e7c52163d07d9a42ce9514aecdb694d674e3";
        private string apiKey = "eE4lgygsFtLXak1lA60fimKyoSwT64v7"; // Replace with your actual API key
        public AxieSpawner axieSpawner;
        public int spawnCountMax = 0;

        public AxieLandBattleTarget landBattleTarget;

        public TeamToJSON teamToJson;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.M))
            {
                GetEnemy();
            }
        }

        public async void GetEnemy()
        {
            string json = await landBattleTarget.GetScoreAsync("3");

            AxieIdsWrapper wrapper = JsonConvert.DeserializeObject<AxieIdsWrapper>(json);
            GetOpponentTeam(wrapper);
        }

        void GetOpponentTeam(AxieIdsWrapper jsonAxieIds)
        {
            graphQLClient = new GraphQLClient("https://api-gateway.skymavis.com/graphql/marketplace");
            List<AxieForBackend> axieIds = jsonAxieIds.axies.ToList();
            string combinedQuery = "";

            for (int i = 0; i < axieIds.Count; i++)
            {
                string individualQuery = $@"
            axie{i}: axie(axieId: ""{axieIds[i].axieid}"") {{
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


        private IEnumerator RequestGraphQL(string query, AxieIdsWrapper axieIdsWrapper, bool isOpponent = false)
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
                Debug.LogError("GraphQL Error: " + task.Exception.Message);
                yield break;
            }

            try
            {
                string responseString = task.Result;
                if (isOpponent)
                {
                    RootObject axiesData = JsonConvert.DeserializeObject<RootObject>(responseString);

                    List<AxieEnemy> axieIds = new List<AxieEnemy>();

                    // Assuming axieIds is a List<string>
                    AxieEnemy axie1 = axiesData.Data["axie0"];
                    AxieEnemy axie2 = axiesData.Data["axie1"];
                    AxieEnemy axie3 = axiesData.Data["axie2"];
                    AxieEnemy axie4 = axiesData.Data["axie3"];
                    AxieEnemy axie5 = axiesData.Data["axie4"];
                    axieIds.Add(axie1);
                    axieIds.Add(axie2);
                    axieIds.Add(axie3);
                    axieIds.Add(axie4);
                    axieIds.Add(axie5);

                    axieIds = axieIds.OrderBy(x => int.Parse(x.id)).ToList();
                    StartCoroutine(SpawnAxies(axieIds, axieIdsWrapper, isOpponent));
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Error processing response: " + ex.Message);
            }
        }

        IEnumerator SpawnAxies(List<AxieEnemy> axieIds, AxieIdsWrapper axieIdsWrapper, bool isOpponent)
        {
            foreach (var VARIABLE in axieIds)
            {
                AxieForBackend axieForBackend = axieIdsWrapper.axies.Single(x => x.axieid == VARIABLE.id);
                axieSpawner.SpawnAxieById(VARIABLE.id, BodyPart.Horn, SkillName.HerosBane,
                    VARIABLE.axieClass,
                    VARIABLE.stats, axieForBackend, isOpponent);
                yield return new WaitForSeconds(0.2f);
            }

            while (axieSpawner.enemyTeam.GetCharacters().Count != 5)
            {
                yield return null;
            }

            yield return new WaitForSeconds(3f);

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