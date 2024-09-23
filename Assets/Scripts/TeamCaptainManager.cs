using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GetAxiesExample;

public class TeamCaptainManager : MonoBehaviour
{
    public SkeletonGraphic ProfilePicGraphic;
    public SkeletonGraphic IngameCaptainGraphic;
    public SkeletonGraphic OpponentCaptainGraphic;
    public SkeletonGraphic CaptainShowcase;
    public List<GameObject> EnableOnCaptainSelection;

    public List<UIListAxieForCaptain> axieList = new List<UIListAxieForCaptain>();
    public GetAxiesExample.Axie lastAxieChosen;
    private int currentPage = 1;
    float PagesAmount = 0;
    int maxPagesAmount = 0;
    public string selectedAxie;
    public TextMeshProUGUI pageText;
    public GameObject Container;
    public Image myTeamHP;
    public Image opponentHP;
    private string currentIngameAnimation = "";
    private string currentOpponentAnimation = "";
    static public TeamCaptainManager Instance;
    private bool isCaptainAnimationBehaviorEnabled = false;
    private float animationChangedMineBuffer = 0;
    private float animationChangedOpponentBuffer = 0;
    private void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (isCaptainAnimationBehaviorEnabled)
        {
            animationChangedMineBuffer -= Time.deltaTime;
            animationChangedOpponentBuffer -= Time.deltaTime;
            UpdateCaptainAnimations();
        }
    }

    public void EnableCaptainBehavior()
    {
        animationChangedMineBuffer = 0;
        animationChangedOpponentBuffer = 0;
        isCaptainAnimationBehaviorEnabled = true;
    }

    public void DisableCaptainBehavior()
    {
        isCaptainAnimationBehaviorEnabled = false;
    }

    private void UpdateCaptainAnimations()
    {
        float myTeamHPFill = myTeamHP.fillAmount * 100f; // Convert to percentage
        float opponentHPFill = opponentHP.fillAmount * 100f; // Convert to percentage
        float healthDifference = myTeamHPFill - opponentHPFill;

        // Determine new animation for Ingame Captain
        if (animationChangedMineBuffer < 0)
        {
            string newIngameAnimation = GetAnimationForHealthDifference(healthDifference, true);
            if (newIngameAnimation != currentIngameAnimation)
            {
                currentIngameAnimation = newIngameAnimation;
                IngameCaptainGraphic.startingAnimation = currentIngameAnimation;
                IngameCaptainGraphic.Initialize(true); // Apply animation only when changed
                animationChangedMineBuffer = 1;
            }
        }

        if (animationChangedOpponentBuffer < 0)
        {
            // Determine new animation for Opponent Captain
            string newOpponentAnimation = GetAnimationForHealthDifference(healthDifference, false);
            if (newOpponentAnimation != currentOpponentAnimation)
            {
                currentOpponentAnimation = newOpponentAnimation;
                OpponentCaptainGraphic.startingAnimation = currentOpponentAnimation;
                OpponentCaptainGraphic.Initialize(true); // Apply animation only when changed
                animationChangedOpponentBuffer = 1;
            }
        }
    }

    private string GetAnimationForHealthDifference(float healthDifference, bool isIngameCaptain)
    {
        if (isIngameCaptain)
        {
            if (healthDifference < -14)
            {
                return "activity/evolve";
            }
            else if (healthDifference < -3)
            {
                return "action/idle/random-03";
            }
            else if (healthDifference > 14)
            {
                return "action/idle/random-02";
            }
            else if (healthDifference > 5)
            {
                return "action/move-forward";
            }
            else if (Mathf.Abs(healthDifference) < 3)
            {
                return "action/idle/normal";
            }
        }
        else
        {
            if (healthDifference > 14)
            {
                return "activity/evolve";
            }
            else if (healthDifference > 3)
            {
                return "action/idle/random-03";
            }
            else if (healthDifference < -14)
            {
                return "action/idle/random-02";
            }
            else if (healthDifference < -5)
            {
                return "action/move-forward";
            }
            else if (Mathf.Abs(healthDifference) < 3)
            {
                return "action/idle/normal";
            }
        }
        return "";
    }
    public void Start()
    {
        var captain = PlayerPrefs.GetString("Captain");
        if (string.IsNullOrEmpty(captain))
        {
            OpenMenu();
        }
        else
        {
            SetAxieSelected(AccountManager.userAxies.results.FirstOrDefault(x => x.id == captain));
            SetProfilePicGraphic();
        }
    }

    public void OpenMenu()
    {
        Container.SetActive(true);
        EnableOnCaptainSelection.ForEach(x => x.SetActive(false));
        SetAxiesUI();
    }

    public List<GetAxiesExample.Axie> GetFilteredList()
    {
        return AccountManager.userAxies.results.ToList();
    }

    public void SetAxieSelected(GetAxiesExample.Axie axie)
    {
        selectedAxie = axie.id;
        foreach (var item in axieList)
        {
            item.Refresh();
        }

        lastAxieChosen = axie;

        var profilePicSkeleton = lastAxieChosen.skeletonDataAsset;
        var skeletonMaterial = lastAxieChosen.skeletonDataAssetMaterial;

        CaptainShowcase.UpdateMode = UpdateMode.FullUpdate;
        CaptainShowcase.enabled = true;
        CaptainShowcase.skeletonDataAsset = profilePicSkeleton;
        CaptainShowcase.material = skeletonMaterial;
        CaptainShowcase.startingAnimation = "action/idle/normal";
        CaptainShowcase.Initialize(true);
    }

    public void SetAxiesUI()
    {
        PagesAmount = AccountManager.userAxies.results.Length / 12f;
        maxPagesAmount = Mathf.CeilToInt(PagesAmount);
        pageText.text = $"Page {currentPage}-{maxPagesAmount}";
        if (lastAxieChosen == null)
        {
            if (TeamManager.instance.currentTeam != null)
            {
                lastAxieChosen = TeamManager.instance.currentTeam.AxieIds[0];
            }
        }

        List<GetAxiesExample.Axie> filteredAxieList = GetFilteredList();

        for (int i = 0; i < 12; i++)
        {
            axieList[i].axie = null;

            int indexToSearch = Mathf.RoundToInt(i + ((12 * (currentPage == 0 ? 0 : currentPage - 1))));
            if (indexToSearch < filteredAxieList.Count)
            {
                GetAxiesExample.Axie axie = filteredAxieList[indexToSearch];

                axieList[i].axie = axie;
            }


            axieList[i].Refresh(true);
        }

        if (string.IsNullOrEmpty(selectedAxie))
            axieList.First().SelectAxie();
    }

    public void GoNextPage()
    {
        currentPage++;

        if (currentPage > maxPagesAmount)
        {
            currentPage = maxPagesAmount;
            return;
        }

        SetAxiesUI();
    }

    public void GoPreviousPage()
    {
        currentPage--;
        if (currentPage <= 0)
        {
            currentPage = 1;
            return;
        }

        SetAxiesUI();
    }

    public void ResetPages()
    {
        currentPage = 0;
        SetAxiesUI();
    }

    public void SetProfilePicGraphic()
    {
        PlayerPrefs.SetString("Captain", lastAxieChosen.id);
        var profilePicSkeleton = lastAxieChosen.skeletonDataAsset;
        var skeletonMaterial = lastAxieChosen.skeletonDataAssetMaterial;

        ProfilePicGraphic.UpdateMode = UpdateMode.FullUpdate;
        ProfilePicGraphic.enabled = true;
        ProfilePicGraphic.skeletonDataAsset = profilePicSkeleton;
        ProfilePicGraphic.material = skeletonMaterial;
        ProfilePicGraphic.startingAnimation = "action/idle/normal";

        ProfilePicGraphic.Initialize(true);

        IngameCaptainGraphic.UpdateMode = UpdateMode.FullUpdate;
        IngameCaptainGraphic.enabled = true;
        IngameCaptainGraphic.skeletonDataAsset = profilePicSkeleton;
        IngameCaptainGraphic.startingAnimation = "action/idle/normal";
        IngameCaptainGraphic.material = skeletonMaterial;
        IngameCaptainGraphic.Initialize(true);

        Container.SetActive(false);
        foreach (var item in EnableOnCaptainSelection)
        {
            item.gameObject.SetActive(true);
        }
    }

    public void SetOpponentCaptain(SkeletonDataAsset opponentGraphic, Material opponentMaterial)
    {
        OpponentCaptainGraphic.UpdateMode = UpdateMode.FullUpdate;
        OpponentCaptainGraphic.enabled = true;
        OpponentCaptainGraphic.skeletonDataAsset = opponentGraphic;
        OpponentCaptainGraphic.material = opponentMaterial;
        OpponentCaptainGraphic.Initialize(true);
    }
}
