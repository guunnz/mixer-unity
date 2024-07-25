using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleGraphQL;

public class GraphQLClientExample : MonoBehaviour
{
    private GraphQLClient graphQLClient;
    private string address = "0x5506e7c52163d07d9a42ce9514aecdb694d674e3";
    private string apiKey = "eE4lgygsFtLXak1lA60fimKyoSwT64v7";

    void Start()
    {
        graphQLClient = new GraphQLClient("https://api-gateway.skymavis.com/graphql/axie-marketplace");
        string query = @"
        query MyQuery {
          lands(
            owner: {address: """ + address + @""", ownerships: Owned}
          ) {
            total
            results {
              landType
              owner
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
            var landsData = JsonUtility.FromJson<LandsData>(responseString);
            Debug.Log(landsData.data.lands.results[0].landType);
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
    public class LandsData
    {
        public Data data;
    }

    [System.Serializable]
    public class Data
    {
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
        public string owner;
        public string tokenId;
    }
}