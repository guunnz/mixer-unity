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
            string json = await landBattleTarget.GetScoreAsync("1");

            AxieIdsWrapper wrapper = JsonConvert.DeserializeObject<AxieIdsWrapper>(json);
            GetOpponentTeam(wrapper);
        }

        void GetOpponentTeam(AxieIdsWrapper jsonAxieIds)
        {
   
            graphQLClient = new GraphQLClient("https://api-gateway.skymavis.com/graphql/marketplace");
            List<string> axieIds = jsonAxieIds.axieids.ToList();
            string combinedQuery = "";

            for (int i = 0; i < axieIds.Count; i++)
            {
                string individualQuery = $@"
            axie{i}: axie(axieId: ""{axieIds[i]}"") {{
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

            StartCoroutine(RequestGraphQL(combinedQuery, true));
        }


        private IEnumerator RequestGraphQL(string query, bool isOpponent = false)
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

                    List<string> axieIds = new List<string>();

                    // Assuming axieIds is a List<string>
                    axieIds.Add(axiesData.Data["axie0"].id);
                    axieIds.Add(axiesData.Data["axie1"].id);
                    axieIds.Add(axiesData.Data["axie2"].id);
                    axieIds.Add(axiesData.Data["axie3"].id);
                    axieIds.Add(axiesData.Data["axie4"].id);

                    Debug.Log(teamToJson.JsonConstructor(axieIds.ToArray()));

// Assuming these are the correct method calls and enum values
                    axieSpawner.SpawnAxieById(axiesData.Data["axie0"].id, BodyPart.Tail, SkillName.RiskyFeather,
                        AxieClass.Bird,
                        axiesData.Data["axie0"].stats, isOpponent);
                    axieSpawner.SpawnAxieById(axiesData.Data["axie1"].id, BodyPart.Back, SkillName.Ronin,
                        AxieClass.Beast,
                        axiesData.Data["axie1"].stats, isOpponent);
                    axieSpawner.SpawnAxieById(axiesData.Data["axie2"].id, BodyPart.Mouth, SkillName.RiskyFish,
                        AxieClass.Dusk,
                        axiesData.Data["axie2"].stats, isOpponent);
                    axieSpawner.SpawnAxieById(axiesData.Data["axie3"].id, BodyPart.Horn, SkillName.Rosebud,
                        AxieClass.Plant,
                        axiesData.Data["axie3"].stats, isOpponent);
                    axieSpawner.SpawnAxieById(axiesData.Data["axie4"].id, BodyPart.Horn, SkillName.HerosBane,
                        AxieClass.Aquatic,
                        axiesData.Data["axie4"].stats, isOpponent);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Error processing response: " + ex.Message);
            }
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
            public List<GetAxiesExample.Part> Parts { get; set; }
            public GetAxiesExample.Stats stats { get; set; }
            public string BodyShape { get; set; }
        }
    }
}