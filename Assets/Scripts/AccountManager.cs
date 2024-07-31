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

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(0.1f);
        string lastWallet = PlayerPrefs.GetString("LastWallet");
        if (!string.IsNullOrEmpty(lastWallet))
            LoginAccount(PlayerPrefs.GetString(wallet));
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            SceneManager.LoadScene(0);
        }
    }

    public void LoginAccount(string userInfoResponse)
    {
        if (loggingIn)
        {
            return;
        }

        Debug.Log("User info: " + userInfoResponse);

        SkyMavisLogin.Root userInfo = JsonUtility.FromJson<SkyMavisLogin.Root>(userInfoResponse);

        loggingIn = true;

        RunManagerSingleton.instance.userId = userInfo.userInfo.addr;

        PlayerPrefs.SetString(wallet, userInfoResponse);
        LoadAssets(userInfo.nftsResponse);
    }


    public void LoadAssets(SkyMavisLogin.NftsResponse nftsResponse)
    {
        try
        {
            PlayerPrefs.SetString("LastWallet", wallet);

            List<GetAxiesExample.Axie> axies = new List<GetAxiesExample.Axie>();
            List<GetAxiesExample.Land> lands = new List<GetAxiesExample.Land>();

            foreach (var nft in nftsResponse.result.items)
            {
                if (nft.tokenSymbol.ToUpper() == "AXIE" && nft.rawMetadata.genes != "0x0")
                {
                    GetAxiesExample.Axie axie = new GetAxiesExample.Axie();

                    axie.genes = nft.rawMetadata.genes;
                    axie.maxBodyPartAmount = 2;
                    axie.@class = nft.rawMetadata.properties.@class;
                    axie.id = nft.tokenId.ToString();
                    axie.name = nft.rawMetadata.name;
                    axie.birthDate = nft.rawMetadata.properties.birthdate;
                    axie.newGenes = nft.rawMetadata.genes;
                    axie.bodyShape = nft.rawMetadata.properties.bodyshape;

                    axie.stats = AxieGeneUtils.GetStatsByGenes(axie.genes);

                    List<string> partsClasses = AxieGeneUtils.GetAxiePartsClasses(axie.genes);
                    List<string> partsAbilities = AxieGeneUtils.ParsePartIdsFromHex(axie.genes);

                    List<GetAxiesExample.Part> axieParts = new List<GetAxiesExample.Part>();

                    GetAxiesExample.Part horn = new GetAxiesExample.Part(partsClasses[2],
                        nft.rawMetadata.properties.horn_id, "horn", 0, false, partsAbilities[2]);
                    GetAxiesExample.Part tail = new GetAxiesExample.Part(partsClasses[5],
                        nft.rawMetadata.properties.tail_id, "tail", 0, false, partsAbilities[5]);
                    GetAxiesExample.Part back = new GetAxiesExample.Part(partsClasses[4],
                        nft.rawMetadata.properties.back_id, "back", 0, false, partsAbilities[4]);
                    GetAxiesExample.Part mouth = new GetAxiesExample.Part(partsClasses[3],
                        nft.rawMetadata.properties.mouth_id, "mouth", 0, false, partsAbilities[3]);

                    axieParts.Add(horn);
                    axieParts.Add(tail);
                    axieParts.Add(back);
                    axieParts.Add(mouth);

                    axie.parts = axieParts.ToArray();

                    axies.Add(axie);
                }
                else if (nft.tokenSymbol.ToUpper() == "LAND")
                {
                    GetAxiesExample.Land land = new GetAxiesExample.Land();

                    land.tokenId = nft.tokenId;
                    land.landType = nft.rawMetadata.properties.land_type;
                    land.col = nft.rawMetadata.properties.col;
                    land.row = nft.rawMetadata.properties.row;
                    lands.Add(land);
                }
            }


            userAxies = new GetAxiesExample.Axies();

            userAxies.results = axies.ToArray();

            foreach (var userAxiesResult in userAxies.results)
            {
                userAxiesResult.LoadGraphicAssets();
                userAxiesResult.maxBodyPartAmount = 2;
            }

            userLands = new GetAxiesExample.Lands();
            userLands.results = lands.ToArray();
            loadLand();
            TeamManager.instance.LoadLastAccountAxies();
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