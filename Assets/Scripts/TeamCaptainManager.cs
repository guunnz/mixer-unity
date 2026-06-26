using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TeamCaptainManager : MonoBehaviour
{
    public VanillaMonsterGraphic ProfilePicGraphic;
    public VanillaMonsterGraphic IngameCaptainGraphic;
    public VanillaMonsterGraphic OpponentCaptainGraphic;
    public VanillaMonsterGraphic CaptainShowcase;
    public List<GameObject> EnableOnCaptainSelection;

    public List<UIListMonsterForCaptain> monsterList = new List<UIListMonsterForCaptain>();
    public TMP_InputField GeneralFilterInput;
    public GetMonstersExample.Monster lastMonsterChosen;
    private int currentPage = 1;
    float PagesAmount = 0;
    int maxPagesAmount = 0;
    public string selectedMonster;
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
        EnsureGraphics();
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
            GeneralFilterInput.onEndEdit.AddListener(delegate { SetMonstersUI(); });
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
        EnsureGraphics();
        if (myTeamHP == null || opponentHP == null)
            return;

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
            OpenMenu();
        }
        else
        {
            while (!AccountManager.userMonsters.results.Select(x => x.id).Contains(captain))
            {
                yield return null;
            }

         
            SetMonsterSelected(AccountManager.userMonsters.results.FirstOrDefault(x => x.id == captain));
            SetProfilePicGraphic();
        }
    }

    public void OpenMenu()
    {
        Container.SetActive(true);
        EnableOnCaptainSelection.ForEach(x => x.SetActive(false));
        SetMonstersUI();
    }

    public List<GetMonstersExample.Monster> GetFilteredList()
    {
    
        var monstersList = AccountManager.userMonsters.results.Where(x => !x.f2p).ToList();
        if (monstersList.Count() == 0)
        {
            monstersList = AccountManager.userMonsters.results.ToList();
        }
            string filter = GeneralFilterInput != null ? GeneralFilterInput.text.ToLower() : "";

        if (!string.IsNullOrEmpty(filter))
        {
            monstersList = monstersList.Where(monster =>
                filter.Contains(monster.id.ToLower()) ||
                filter.Contains(monster.name.ToLower()) ||
                filter.Contains(monster.monsterClass.ToString().ToLower()) ||
                (monster.parts != null && monster.parts.Any(part =>
                    filter.Contains(part.name.ToLower()) ||
                    filter.Contains(part.abilityName.ToLower())))
            ).ToList();
        }

        return monstersList;
    }

    public void SetMonsterSelected(GetMonstersExample.Monster monster)
    {
        EnsureGraphics();
        selectedMonster = monster.id;
        foreach (var item in monsterList)
        {
            item.Refresh();
        }

        lastMonsterChosen = monster;

        VanillaMonsterGraphic showcase = CaptainShowcase;
        showcase.enabled = true;
        showcase.SetMonster(lastMonsterChosen);
        showcase.startingAnimation = "action/idle/normal";
        showcase.Initialize(true);
    }

    public void SetMonstersUI()
    {
        PagesAmount = AccountManager.userMonsters.results.Length / 12f;
        maxPagesAmount = Mathf.CeilToInt(PagesAmount);
        pageText.text = $"Page {currentPage}-{maxPagesAmount}";

        List<GetMonstersExample.Monster> filteredMonsterList = GetFilteredList();

        for (int i = 0; i < 12; i++)
        {
            monsterList[i].monster = null;

            int indexToSearch = Mathf.RoundToInt(i + (12 * (currentPage - 1)));
            if (indexToSearch < filteredMonsterList.Count)
            {
                GetMonstersExample.Monster monster = filteredMonsterList[indexToSearch];
                monsterList[i].monster = monster;
            }

            monsterList[i].Refresh(true);
        }

        if (string.IsNullOrEmpty(selectedMonster))
            monsterList.FirstOrDefault()?.SelectMonster();
    }

    public void GoNextPage()
    {
        currentPage++;
        if (currentPage > maxPagesAmount)
        {
            currentPage = maxPagesAmount;
            return;
        }

        SetMonstersUI();
    }

    public void GoPreviousPage()
    {
        currentPage--;
        if (currentPage <= 0)
        {
            currentPage = 1;
            return;
        }

        SetMonstersUI();
    }

    public void ResetPages()
    {
        currentPage = 0;
        SetMonstersUI();
    }

    public void SetProfilePicGraphic()
    {
        EnsureGraphics();
        PlayerPrefs.SetString("Captain" + RunManagerSingleton.instance.user_wallet_address, lastMonsterChosen.id);

        ProfilePicGraphic.enabled = true;
        ProfilePicGraphic.SetMonster(lastMonsterChosen);
        ProfilePicGraphic.startingAnimation = "action/idle/normal";

        ProfilePicGraphic.Initialize(true);

        IngameCaptainGraphic.enabled = true;
        IngameCaptainGraphic.SetMonster(lastMonsterChosen);
        IngameCaptainGraphic.startingAnimation = "action/idle/normal";
        IngameCaptainGraphic.Initialize(true);

        Container.SetActive(false);
        foreach (var item in EnableOnCaptainSelection)
        {
            item.gameObject.SetActive(true);
        }
    }

    public void SetOpponentCaptain(MonsterVisualDescriptor opponentGraphic)
    {
        EnsureGraphics();
        OpponentCaptainGraphic.enabled = true;
        OpponentCaptainGraphic.SetDescriptor(opponentGraphic);
        OpponentCaptainGraphic.Initialize(true);
    }

    private void EnsureGraphics()
    {
        ProfilePicGraphic = EnsureGraphic(ProfilePicGraphic, "Profile Captain Graphic");
        IngameCaptainGraphic = EnsureGraphic(IngameCaptainGraphic, "Ingame Captain Graphic");
        OpponentCaptainGraphic = EnsureGraphic(OpponentCaptainGraphic, "Opponent Captain Graphic");
        CaptainShowcase = EnsureGraphic(CaptainShowcase, "Captain Showcase Graphic");
    }

    private VanillaMonsterGraphic EnsureGraphic(VanillaMonsterGraphic graphic, string name)
    {
        if (graphic != null)
            return graphic;

        Transform parent = Container != null ? Container.transform : transform;
        GameObject graphicGo = new GameObject(name, typeof(RectTransform));
        RectTransform graphicRect = graphicGo.GetComponent<RectTransform>();
        graphicRect.SetParent(parent, false);
        graphicRect.sizeDelta = new Vector2(180f, 140f);
        return VanillaMonsterGraphic.Ensure(graphicGo);
    }
}
