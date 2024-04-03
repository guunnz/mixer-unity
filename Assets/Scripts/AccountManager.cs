using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using SimpleGraphQL;

public class AccountManager : MonoBehaviour
{
    private GraphQLClient graphQLClient;
    public TMP_InputField RoninWallet;
    private string address = "0x5506e7c52163d07d9a42ce9514aecdb694d674e3";
    private string apiKey = "eE4lgygsFtLXak1lA60fimKyoSwT64v7";
    public static GetAxiesExample.Axies userAxies;
    public static GetAxiesExample.Lands userLands;
    
    public void LoginAccount()
    {
        string cache = PlayerPrefs.GetString(RoninWallet.text);
        if (!string.IsNullOrEmpty(cache))
        {
            GetAxiesExample.AxiesData axiesData =
                JsonUtility.FromJson<GetAxiesExample.AxiesData>(PlayerPrefs.GetString(address));

            userAxies = axiesData.data.axies;
            userLands = axiesData.data.lands;
        }

        graphQLClient = new GraphQLClient("https://api-gateway.skymavis.com/graphql/marketplace");
        string query = @"
    query MyQuery {
      axies(owner: """ + address + @""") {
        results {
          birthDate
          name
          genes
          newGenes
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
          col
          row
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
            // StartCoroutine(SpawnAxies(responseString));
            // PlayerPrefs.SetString(address, responseString);
            GetAxiesExample.AxiesData axiesData = JsonUtility.FromJson<GetAxiesExample.AxiesData>(responseString);
            userAxies = axiesData.data.axies;
            userLands = axiesData.data.lands;
            axiesData.data.lands.results = new GetAxiesExample.Land[]
            {
                new GetAxiesExample.Land()
                    { col = "-10", row = "10", landType = LandType.savannah.ToString(), tokenId = "12313232" },
                new GetAxiesExample.Land()
                    { col = "-13", row = "13", landType = LandType.arctic.ToString(), tokenId = "12332" },
                new GetAxiesExample.Land()
                    { col = "-1", row = "5", landType = LandType.forest.ToString(), tokenId = "1231322" },
                new GetAxiesExample.Land()
                    { col = "-1", row = "7", landType = LandType.genesis.ToString(), tokenId = "13232" },
                new GetAxiesExample.Land()
                    { col = "-20", row = "4", landType = LandType.mystic.ToString(), tokenId = "323389" }
            };
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Error processing response: " + ex.Message);
        }
    }
}