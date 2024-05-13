using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public enum TeamBuilderMenu
{
    Lands,
    Axies,
    Atia,
    Combo
}

public class TeamBuilderManager : MonoBehaviour
{
    public GameObject LandsContent;
    public GameObject AxiesContent;

    public List<UIListLand> landList = new List<UIListLand>();
    public List<UIListAxie> axieList = new List<UIListAxie>();

    private int currentPage = 1;

    public TextMeshProUGUI pageText;

    public TeamBuilderMenu currentTeamBuilderMenu;
    float PagesAmount = 0;
    int maxPagesAmount = 0;
    public GameObject TeamLandBuildingUI;
    public GameObject ComboUI;
    public FakeMapManager fakeMap;

    private void OnEnable()
    {
        fakeMap.ToggleRectangles();
    }
    private void OnDisable()
    {
        fakeMap.ToggleRectanglesFalse();
    }
    public void Start()
    {
        SetMenu(TeamBuilderMenu.Lands, true);
    }

    public void SetAxiesMenu()
    {
        SetMenu(TeamBuilderMenu.Axies, true);
    }

    public void SetLandsMenu()
    {
        SetMenu(TeamBuilderMenu.Lands, true);
    }

    public void SetComboMenu()
    {
        SetMenu(TeamBuilderMenu.Combo, false);
    }


    public void SetMenu(TeamBuilderMenu teamBuilderMenu, bool ResetPage = false)
    {
        if (ResetPage)
        {
            currentPage = 1;
        }

        currentTeamBuilderMenu = teamBuilderMenu;
        switch (teamBuilderMenu)
        {
            case TeamBuilderMenu.Atia:
                SetAtiaUI();
                break;
            case TeamBuilderMenu.Axies:
                PagesAmount = AccountManager.userAxies.results.Length / 12f;
                maxPagesAmount = Mathf.CeilToInt(PagesAmount);
                pageText.text = $"Page {currentPage}-{maxPagesAmount}";
                SetAxiesUI();
                break;
            case TeamBuilderMenu.Combo:
                SetComboUI();
                break;
            case TeamBuilderMenu.Lands:
                PagesAmount = AccountManager.userLands.results.Length / 12f;
                maxPagesAmount = Mathf.CeilToInt(PagesAmount);
                pageText.text = $"Page {currentPage}-{maxPagesAmount}";
                SetLandUI();
                break;
        }
    }

    public void SetComboUI()
    {
        TeamLandBuildingUI.SetActive(false);
        ComboUI.SetActive(true);
    }

    public void SetAtiaUI()
    {
    }

    public void SetAxiesUI()
    {
        TeamLandBuildingUI.SetActive(true);
        ComboUI.SetActive(false);
        LandsContent.SetActive(false);
        AxiesContent.SetActive(true);
        for (int i = 0; i < 12; i++)
        {
            axieList[i].axie = null;

            int indexToSearch = Mathf.RoundToInt(i + (12 * (currentPage - 1)));
            if (indexToSearch < AccountManager.userAxies.results.Length)
            {
                GetAxiesExample.Axie axie = AccountManager.userAxies.results[indexToSearch];

                axieList[i].axie = axie;
            }

            axieList[i].Refresh();
        }
    }

    public void SetLandUI()
    {
        TeamLandBuildingUI.SetActive(true);
        ComboUI.SetActive(false);
        LandsContent.SetActive(true);
        AxiesContent.SetActive(false);
        for (int i = 0; i < 12; i++)
        {
            landList[i].land = null;

            if (i < AccountManager.userLands.results.Length)
            {
                GetAxiesExample.Land land = AccountManager.userLands.results[Mathf.RoundToInt(i * currentPage)];

                landList[i].land = land;
            }

            landList[i].Refresh();
        }
    }

    public void GoNextPage()
    {
        currentPage++;

        if (currentPage > maxPagesAmount)
        {
            currentPage = maxPagesAmount;
            return;
        }

        SetMenu(currentTeamBuilderMenu, false);
    }

    public void GoPreviousPage()
    {
        currentPage--;
        if (currentPage <= 0)
        {
            currentPage = 1;
            return;
        }

        SetMenu(currentTeamBuilderMenu, false);
    }
}