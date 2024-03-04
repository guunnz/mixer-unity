using UnityEngine;
using System.Collections;
using SimpleGraphQL;
using System.Collections.Generic;
using System.Linq;
using Game;

public class GetAxiesExample : MonoBehaviour
{
    private GraphQLClient graphQLClient;
    private string address = "0x5506e7c52163d07d9a42ce9514aecdb694d674e3";
    private string apiKey = "eE4lgygsFtLXak1lA60fimKyoSwT64v7"; // Replace with your actual API key
    public AxieSpawner axieSpawner;
    public int spawnCountMax = 0;
    public TeamToJSON teamToJson;
    void Start()
    {
        graphQLClient = new GraphQLClient("https://api-gateway.skymavis.com/graphql/marketplace");
        string query = @"
    query MyQuery {
      axies(owner: """ + address + @""") {
        results {
          birthDate
          name
          genes
          id
          class
          parts {
            class
            id
            name
            type
            abilities {
              attack
              attackType
              name
              id
              effectIconUrl
              defense
              backgroundUrl
            }
          }
          stats {
            speed
            skill
            morale
            hp
          }
          bodyShape
        }
      }
    }";
        StartCoroutine(RequestGraphQL(query));
    }

    private IEnumerator RequestGraphQL(string query)
    {
        var request = new Request
        {
            Query = query
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
            var axiesData = JsonUtility.FromJson<AxiesData>(responseString);
            var axiesResults = axiesData.data.axies.results;
            // for (int i = axiesResults.Length - 9; i >= 0; i--)
            // {
            //     if (spawnCountMax >= 3)
            //         break;
            //     spawnCountMax++;
            //     axieSpawner.SpawnAxieById(axiesResults[i].id);
            // }

            Axie bird = axiesResults.FirstOrDefault(x => x.@class.Contains("Bird"));
            Axie beast = axiesResults.FirstOrDefault(x => x.@class.Contains("Beast"));
            Axie dusk = axiesResults.FirstOrDefault(x => x.@class.Contains("Dusk"));
            Axie plant = axiesResults
                .Where(x => x.@class.Contains("Plant") && x.parts.Any(y => y.name.ToLower() == "rose bud")).ToList()[0];
            Axie aqua = axiesResults.FirstOrDefault(x => x.@class.Contains("Aqua"));

            List<string> axieIds = new List<string>();
            
            axieIds.Add(bird.id);
            axieIds.Add(beast.id);
            axieIds.Add(dusk.id);
            axieIds.Add(plant.id);
            axieIds.Add(aqua.id);
            
            Debug.Log(teamToJson.JsonConstructor(axieIds.ToArray()));

            axieSpawner.SpawnAxieById(bird.id, BodyPart.Tail, SkillName.RiskyFeather, AxieClass.Bird, bird.stats);
            axieSpawner.SpawnAxieById(beast.id, BodyPart.Back, SkillName.Ronin, AxieClass.Beast, beast.stats);
            axieSpawner.SpawnAxieById(dusk.id, BodyPart.Mouth, SkillName.RiskyFish, AxieClass.Dusk, dusk.stats);
            axieSpawner.SpawnAxieById(plant.id, BodyPart.Horn, SkillName.Rosebud, AxieClass.Plant, plant.stats);
            axieSpawner.SpawnAxieById(aqua.id, BodyPart.Horn, SkillName.HerosBane, AxieClass.Aquatic, aqua.stats);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Error processing response: " + ex.Message);
        }
    }

    public void SetAddress(string newAddress)
    {
        address = newAddress;
    }

    [System.Serializable]
    public class AxiesData
    {
        public Data data;
    }

    [System.Serializable]
    public class Data
    {
        public Axies axies;
    }

    [System.Serializable]
    public class Axies
    {
        public Axie[] results;
    }

    [System.Serializable]
    public class Axie
    {
        public long birthDate;
        public string name;
        public string genes;
        public string id;
        public string @class;
        public Part[] parts;
        public Stats stats;
        public string bodyShape;
    }

    [System.Serializable]
    public class Part
    {
        public string @class;
        public string id;
        public string name;
        public string type;
        public Ability[] abilities;
    }

    [System.Serializable]
    public class Ability
    {
        public int attack;
        public string attackType;
        public string name;
        public string id;
        public string effectIconUrl;
        public int defense;
        public string backgroundUrl;
    }

    [System.Serializable]
    public class Stats
    {
        public int speed;
        public int skill;
        public int morale;
        public int hp;
    }
}