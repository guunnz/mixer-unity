using UnityEngine;
using System.Collections;
using SimpleGraphQL;
using System.Collections.Generic;
using Game;

public class GetAxiesExample : MonoBehaviour
{
    private GraphQLClient graphQLClient;
    private string address = "0x5506e7c52163d07d9a42ce9514aecdb694d674e3";
    private string apiKey = "eE4lgygsFtLXak1lA60fimKyoSwT64v7"; // Replace with your actual API key
    public AxieSpawner axieSpawner;

    void Start()
    {
        graphQLClient = new GraphQLClient("https://api-gateway.skymavis.com/graphql/marketplace");
        string query = @"
        query MyQuery {
          axies(owner: """ + address + @""") {
            results {
              genes
              id
              image
              owner
              class
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
            foreach (var axiesResult in axiesData.data.axies.results)
            {
               axieSpawner.SpawnAxieById(axiesResult.id);
            }
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
        public string genes;
        public string id;
        public string image;
        public string owner;
        public string @class;
    }
}