using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TeamCaptainManager : MonoBehaviour
{
    public SkeletonGraphic ProfilePicGraphic;
    public SkeletonGraphic IngameCaptainGraphic;
    public SkeletonGraphic OpponentCaptainGraphic;
    public SkeletonGraphic CaptainShowcase;
    public List<GameObject> EnableOnCaptainSelection;

    public List<UIListAxieForCaptain> axieList = new List<UIListAxieForCaptain>();
    public TMP_InputField GeneralFilterInput;
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

    private void SetupGeneralFilterListener()
    {
        if (GeneralFilterInput != null)
        {
            GeneralFilterInput.onEndEdit.AddListener(delegate { SetAxiesUI(); });
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
        float myTeamHPFill = myTeamHP.fillAmount * 100f;
        float opponentHPFill = opponentHP.fillAmount * 100f;
        float healthDifference = myTeamHPFill - opponentHPFill;

        if (animationChangedMineBuffer < 0)
        {
            string newIngameAnimation = GetAnimationForHealthDifference(healthDifference, true);
            if (newIngameAnimation != currentIngameAnimation)
            {
                currentIngameAnimation = newIngameAnimation;
                IngameCaptainGraphic.startingAnimation = currentIngameAnimation;
                IngameCaptainGraphic.Initialize(true);
                animationChangedMineBuffer = 1;
            }
        }

        if (animationChangedOpponentBuffer < 0)
        {
            string newOpponentAnimation = GetAnimationForHealthDifference(healthDifference, false);
            if (newOpponentAnimation != currentOpponentAnimation)
            {
                currentOpponentAnimation = newOpponentAnimation;
                OpponentCaptainGraphic.startingAnimation = currentOpponentAnimation;
                OpponentCaptainGraphic.Initialize(true);
                animationChangedOpponentBuffer = 1;
            }
        }
    }

    private string GetAnimationForHealthDifference(float healthDifference, bool isIngameCaptain)
    {
        if (isIngameCaptain)
        {
            if (healthDifference < -14) return "activity/evolve";
            if (healthDifference < -3) return "action/idle/random-03";
            if (healthDifference > 14) return "action/idle/random-02";
            if (healthDifference > 5) return "action/move-forward";
            if (Mathf.Abs(healthDifference) < 3) return "action/idle/normal";
        }
        else
        {
            if (healthDifference > 14) return "activity/evolve";
            if (healthDifference > 3) return "action/idle/random-03";
            if (healthDifference < -14) return "action/idle/random-02";
            if (healthDifference < -5) return "action/move-forward";
            if (Mathf.Abs(healthDifference) < 3) return "action/idle/normal";
        }
        return "";
    }

    public System.Collections.IEnumerator Start()
    {
        SetupGeneralFilterListener();
        while (string.IsNullOrEmpty(RunManagerSingleton.instance.user_wallet_address))
        {
            yield return null;
        }
        var captain = PlayerPrefs.GetString("Captain" + RunManagerSingleton.instance.user_wallet_address);
        if (string.IsNullOrEmpty(captain))
        {
            Loading.instance.DisableLoading();
            OpenMenu();
        }
        else
        {
            while (!AccountManager.userAxies.results.Select(x => x.id).Contains(captain))
            {
                yield return null;
            }

            Loading.instance.DisableLoading();
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
    
        var axiesList = AccountManager.userAxies.results.Where(x => !x.f2p).ToList();
        if (axiesList.Count() == 0)
        {
            axiesList = AccountManager.userAxies.results.ToList();
        }
            string filter = GeneralFilterInput != null ? GeneralFilterInput.text.ToLower() : "";

        if (!string.IsNullOrEmpty(filter))
        {
            axiesList = axiesList.Where(axie =>
                filter.Contains(axie.id.ToLower()) ||
                filter.Contains(axie.name.ToLower()) ||
                filter.Contains(axie.axieClass.ToString().ToLower()) ||
                (axie.parts != null && axie.parts.Any(part =>
                    filter.Contains(part.name.ToLower()) ||
                    filter.Contains(part.abilityName.ToLower())))
            ).ToList();
        }

        return axiesList;
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

        List<GetAxiesExample.Axie> filteredAxieList = GetFilteredList();

        for (int i = 0; i < 12; i++)
        {
            axieList[i].axie = null;

            int indexToSearch = Mathf.RoundToInt(i + (12 * (currentPage - 1)));
            if (indexToSearch < filteredAxieList.Count)
            {
                GetAxiesExample.Axie axie = filteredAxieList[indexToSearch];
                axieList[i].axie = axie;
            }

            axieList[i].Refresh(true);
        }

        if (string.IsNullOrEmpty(selectedAxie))
            axieList.FirstOrDefault()?.SelectAxie();
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
        PlayerPrefs.SetString("Captain" + RunManagerSingleton.instance.user_wallet_address, lastAxieChosen.id);
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
