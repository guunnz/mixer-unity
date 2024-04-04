using System;
using UnityEngine;
using System.Collections;
using SimpleGraphQL;
using System.Collections.Generic;
using System.Linq;
using Game;
using Spine.Unity;

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
          lands(owner: {address: """ + address + @""", ownerships: Owned}) {
            total
            results {
              landType
              tokenId
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
            StartCoroutine(SpawnAxies(responseString));
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Error processing response: " + ex.Message);
        }
    }

    IEnumerator SpawnAxies(string response)
    {
        var axiesData = JsonUtility.FromJson<AxiesData>(response);
        var axiesResults = axiesData.data.axies.results;

        Axie bird = axiesResults[0];
        Axie beast = axiesResults[1];
        Axie dusk = axiesResults[2];
        Axie plant = axiesResults[3];
        Axie aqua = axiesResults[4];

        List<string> axieIds = new List<string>();

        axieIds.Add(bird.id);
        axieIds.Add(beast.id);
        axieIds.Add(dusk.id);
        axieIds.Add(plant.id);
        axieIds.Add(aqua.id);
        yield break;
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
        public Lands lands;
    }

    [System.Serializable]
    public class Lands
    {
        public int total;
        public Land[] results;
    }

    [System.Serializable]
    public class Land
    {
        public string landType;
        public string tokenId;
        public string col;
        public string row;
        public LandType LandTypeEnum => (LandType)Enum.Parse(typeof(LandType), landType, true);
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
        public string newGenes;
        public string id;
        public string @class;
        public AxieClass axieClass => (AxieClass)Enum.Parse(typeof(AxieClass), @class, true);
        public Part[] parts;

        public Stats stats;
        public string bodyShape;
        public SkeletonDataAsset skeletonDataAsset;
        public int maxBodyPartAmount = 2;
        public Material skeletonDataAssetMaterial;
    }

    [System.Serializable]
    public class Part
    {
        public string @class;
        public string id;
        public string name;
        public string type;
        public int order;
        public AxieClass partClass => (AxieClass)Enum.Parse(typeof(AxieClass), @class, true);
        public BodyPart BodyPart => (BodyPart)Enum.Parse(typeof(BodyPart), type, true);
        public SkillName SkillName => (SkillName)Enum.Parse(typeof(SkillName), name.Replace(" ", ""), true);
        public Ability[] abilities;
        public bool selected;
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