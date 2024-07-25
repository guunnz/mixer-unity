using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using TMPro;
using SimpleGraphQL;
using UnityEngine.SceneManagement;

public class AccountManager : MonoBehaviour
{
    private GraphQLClient graphQLClient;

    internal string wallet = "0x46571200388f6dce5416e552e28caa7a6833c88e";
    private string apiKey = "eE4lgygsFtLXak1lA60fimKyoSwT64v7";
    public static GetAxiesExample.Axies userAxies;
    public static GetAxiesExample.Lands userLands;
    public TextMeshProUGUI IncorrectWallet;
    public GameObject NextStepAfterLogin;
    public GameObject MainMenu;
    public GameObject RoninMenu;
    public bool LoadInstantly = false;
    private bool loggingIn;

    public IEnumerator IncorrectWalletDo()
    {
        IncorrectWallet.DOColor(Color.white, 0.2f);
        yield return new WaitForSeconds(1);
        IncorrectWallet.DOColor(Color.clear, 0.2f);
    }

    private void Start()
    {
        string lastWallet = PlayerPrefs.GetString("LastWallet");
        if (!string.IsNullOrEmpty(lastWallet))
            LoginAccount();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            SceneManager.LoadScene(0);
        }
    }

    private void OnApplicationQuit()
    {
        PlayerPrefs.SetString("LastWallet", "");
    }

    public void LoginAccount()
    {
        if (loggingIn)
        {
            return;
        }

        string lastWallet = PlayerPrefs.GetString("LastWallet");
        loggingIn = true;

        // PlayerPrefs.GetString(RoninWallet.text);
        // string cache = 
        // if (!string.IsNullOrEmpty(cache))
        // {
        //     GetAxiesExample.AxiesData axiesData =
        //         JsonUtility.FromJson<GetAxiesExample.AxiesData>(PlayerPrefs.GetString(address));
        //
        //     userAxies = axiesData.data.axies;
        //     userLands = axiesData.data.lands;
        // }

        if (!string.IsNullOrEmpty(lastWallet))
        {
            wallet = lastWallet;
            RunManagerSingleton.instance.userId = wallet;

            LoadAssets(PlayerPrefs.GetString(wallet));
            return;
        }

        RunManagerSingleton.instance.userId = wallet;

        graphQLClient = new GraphQLClient("https://api-gateway.skymavis.com/graphql/marketplace");
        string query = @"
    query MyQuery {
      axies(owner: """ + wallet + @""") {
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
      lands(owner: {address: """ + wallet + @""", ownerships: Owned}) {
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

        LoadAssets(task.Result);
    }

    public void LoadAssets(string responseString)
    {
        try
        {
            // StartCoroutine(SpawnAxies(responseString));
            PlayerPrefs.SetString(wallet, responseString);
            PlayerPrefs.SetString("LastWallet", wallet);

            GetAxiesExample.AxiesData axiesData = JsonUtility.FromJson<GetAxiesExample.AxiesData>(responseString);
            userAxies = axiesData.data.axies;
            foreach (var userAxiesResult in userAxies.results)
            {
                userAxiesResult.LoadGraphicAssets();
                userAxiesResult.maxBodyPartAmount = 2;
            }

            userLands = axiesData.data.lands;
            loadLand();
            // TeamManager.instance.LoadLastAccountAxies();
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Error processing response: " + ex.Message);
        }
    }

    public void loadLand()
    {
        RoninMenu.SetActive(false);
        MainMenu.SetActive(false);
        NextStepAfterLogin.SetActive(true);
        LandManager.instance.InitializeLand();
    }
}