using UnityEngine;
using System.Collections.Generic;

public class AxieTestToolUI : MonoBehaviour
{
#if UNITY_EDITOR

    private bool showUI = false;
    private Vector2 scrollPosition;

    private Dictionary<string, DropdownState> dropdownStates = new Dictionary<string, DropdownState>();

    private const int MaxDropdownItems = 10; // Limit the number of dropdown items displayed
    private GUIStyle backgroundStyle;
    private GUIStyle labelStyle;
    private GUIStyle boxStyle;
    public GameObject testUI;

    //private void Start()
    //{
    //    // Cache GUI styles at the start
    //    backgroundStyle = new GUIStyle(GUI.skin.box)
    //    {
    //        normal = { background = MakeTex(620, 1600, Color.black) }
    //    };

    //    labelStyle = new GUIStyle(GUI.skin.label)
    //    {
    //        fontSize = 16,
    //        fontStyle = FontStyle.Bold
    //    };

    //    boxStyle = new GUIStyle(GUI.skin.box)
    //    {
    //        padding = new RectOffset(10, 10, 10, 10),
    //        margin = new RectOffset(5, 5, 5, 5)
    //    };
    //}
    private Vector2 dropdownScrollPosition;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F12))
        {
            showUI = !showUI;
            Cursor.visible = showUI;
            testUI.SetActive(showUI);
        }
    }

    private void OnGUI()
    {
        if (!showUI) return;

        // Initialize styles if they are null
        if (backgroundStyle == null)
        {
            backgroundStyle = new GUIStyle(GUI.skin.box)
            {
                normal = { background = MakeTex(620, 1600, Color.black) }
            };
        }

        if (labelStyle == null)
        {
            labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold
            };
        }

        if (boxStyle == null)
        {
            boxStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(10, 10, 10, 10),
                margin = new RectOffset(5, 5, 5, 5)
            };
        }

        // Calculate the x-coordinate for right-side alignment
        float xPosition = Screen.width - 650;

        // Draw background
        GUI.Box(new Rect(xPosition, 10, 640, Screen.height - 20), "", backgroundStyle);

        // Begin the scrollable window
        GUILayout.BeginArea(new Rect(xPosition + 10, 10, 620, Screen.height - 20));
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(620));
        GUILayout.BeginVertical();

        RenderBattleSettings();
        RenderAxieSection("Ally Axies For Testing", TestTool.Instance.allyAxiesForTesting, Color.green);
        RenderAxieSection("Enemy Axies For Testing", TestTool.Instance.enemyAxiesForTesting, Color.red);

        GUILayout.EndVertical();
        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }



    private void RenderBattleSettings()
    {
        GUILayout.Label("Battle Settings", labelStyle);
        TestTool.Instance.CoinsAmount = IntField("Coins Amount", TestTool.Instance.CoinsAmount);

        if (GUILayout.Button("Set Coins")) TestTool.Instance.SetCoins();
        if (GUILayout.Button("Morph Allied Axies")) TestTool.Instance.MorphAlliedAxies();

        TestTool.Instance.BattleTimerStartIn = IntField("Battle Timer Starts In", TestTool.Instance.BattleTimerStartIn);
        GUILayout.Space(20);
    }

    private void RenderAxieSection(string label, List<AxieForTesting> axieList, Color labelColor)
    {
        labelStyle.normal.textColor = labelColor;
        GUILayout.Label(label, labelStyle);

        DisplayAxieList(axieList);
        GUILayout.Space(20);
    }

    private void DisplayAxieList(List<AxieForTesting> axieList)
    {
        for (int i = 0; i < axieList.Count; i += 2)
        {
            GUILayout.BeginHorizontal();

            DisplaySingleAxie(axieList, i);
            DisplaySingleAxie(axieList, i + 1);

            GUILayout.EndHorizontal();
            GUILayout.Space(10);
        }

        if (GUILayout.Button("Add New Axie"))
        {
            axieList.Add(new AxieForTesting
            {
                UseAxieStats = false,
                axieStats = new GetAxiesExample.Stats(),
                abilitiesToUse = new List<SkillName>(),
                specialBodyPartForAbilityIfNeeded = new List<BodyPart> { BodyPart.None, BodyPart.None },
                startsWithBuffDebuff = new List<StatusEffectEnum>(),
                DebuffsDuration = 1
            });
        }
    }

    private void DisplaySingleAxie(List<AxieForTesting> axieList, int index)
    {
        if (index >= axieList.Count) return;

        var axie = axieList[index];

        GUILayout.BeginVertical(boxStyle, GUILayout.MaxWidth(290));

        GUILayout.Label($"Axie {index + 1}", GUILayout.Height(30));
        axie.UseAxieStats = GUILayout.Toggle(axie.UseAxieStats, "Use Axie Stats");

        RenderAxieStats(axie);
        RenderAxieAbilities(axie);
        RenderSpecialBodyParts(axie);
        RenderBuffsDebuffs(axie);

        axie.DebuffsDuration = FloatField("Debuffs Duration", axie.DebuffsDuration);

        if (GUILayout.Button("Remove Axie"))
        {
            axieList.RemoveAt(index);
        }

        GUILayout.EndVertical();
        GUILayout.Space(10);
    }

    private void RenderAxieStats(AxieForTesting axie)
    {
        GUILayout.Label("Axie Stats", GUILayout.Height(20));
        GUI.enabled = !axie.UseAxieStats;
        axie.axieStats.speed = IntField("Speed", axie.axieStats.speed);
        axie.axieStats.skill = IntField("Skill", axie.axieStats.skill);
        axie.axieStats.morale = IntField("Morale", axie.axieStats.morale);
        axie.axieStats.hp = IntField("HP", axie.axieStats.hp);
        GUI.enabled = true;
        GUILayout.Space(10);
    }

    private void RenderAxieAbilities(AxieForTesting axie)
    {
        GUILayout.Label("Abilities to Use", GUILayout.Height(20));
        RenderEnumDropdownList(axie.abilitiesToUse, "Ability", SkillName.Anemone, 100, ref scrollPosition);
        GUILayout.Space(10);
    }

    private void RenderSpecialBodyParts(AxieForTesting axie)
    {
        GUILayout.Label("Special Body Parts", GUILayout.Height(20));
        RenderEnumDropdownList(axie.specialBodyPartForAbilityIfNeeded, "Part", BodyPart.None, 100, ref scrollPosition);
        GUILayout.Space(10);
    }





    private void RenderBuffsDebuffs(AxieForTesting axie)
    {
        GUILayout.Label("Buffs/Debuffs", GUILayout.Height(20));
        RenderEnumDropdownList(axie.startsWithBuffDebuff, "Buff/Debuff", StatusEffectEnum.None, 100, ref scrollPosition);
        GUILayout.Space(10);
    }
    private void RenderEnumDropdownList<T>(List<T> list, string labelPrefix, T defaultValue, float width, ref Vector2 scrollPosition) where T : System.Enum
    {
        for (int i = 0; i < list.Count; i++)
        {
            GUILayout.BeginHorizontal();

            list[i] = EnumDropdown($"{labelPrefix} {i + 1}", list[i], width, ref scrollPosition);

            if (GUILayout.Button("Remove", GUILayout.Width(60)))
            {
                list.RemoveAt(i);
                i--; // Adjust the index to account for the removed item
            }

            GUILayout.EndHorizontal();
            GUILayout.Space(5);
        }

        if (GUILayout.Button($"Add {labelPrefix}", GUILayout.Width(width)))
        {
            list.Add(defaultValue);
        }
    }



    private int IntField(string label, int value)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(label, GUILayout.Width(120));
        value = int.TryParse(GUILayout.TextField(value.ToString(), GUILayout.Width(100)), out int result) ? result : value;
        GUILayout.EndHorizontal();
        return value;
    }

    private float FloatField(string label, float value)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(label, GUILayout.Width(120));
        value = float.TryParse(GUILayout.TextField(value.ToString(), GUILayout.Width(100)), out float result) ? result : value;
        GUILayout.EndHorizontal();
        return value;
    }

    //private T EnumDropdown<T>(string label, T selected, float width) where T : System.Enum
    //{
    //    GUILayout.BeginVertical(); // Begin vertical layout for dropdown
    //    GUILayout.Label(label, GUILayout.Width(120));

    //    if (!dropdownStates.TryGetValue(label, out DropdownState state))
    //    {
    //        state = new DropdownState();
    //        dropdownStates[label] = state;
    //    }

    //    if (GUILayout.Button(selected.ToString(), GUILayout.Width(width)))
    //    {
    //        state.IsOpen = !state.IsOpen;
    //    }

    //    if (state.IsOpen)
    //    {
    //        state.SearchFilter = GUILayout.TextField(state.SearchFilter, GUILayout.Width(width));
    //        var filteredValues = FilterEnumValues<T>(state.SearchFilter);

    //        RenderDropdownItems(ref selected, ref state, filteredValues, width);
    //    }

    //    GUILayout.EndVertical(); // End vertical layout for dropdown
    //    return selected;
    //}



    private T EnumDropdown<T>(string label, T selected, float width, ref Vector2 scrollPosition) where T : System.Enum
    {
        GUILayout.BeginVertical(); // Begin vertical layout for dropdown
        GUILayout.Label(label, GUILayout.Width(120));

        if (!dropdownStates.TryGetValue(label, out DropdownState state))
        {
            state = new DropdownState();
            dropdownStates[label] = state;
        }

        if (GUILayout.Button(selected.ToString(), GUILayout.Width(width)))
        {
            state.IsOpen = !state.IsOpen;
        }

        if (state.IsOpen)
        {
            // Save current scroll position before modifying dropdown
            Vector2 previousScrollPosition = scrollPosition;

            // Use a separate scroll area for the dropdown
            dropdownScrollPosition = GUILayout.BeginScrollView(dropdownScrollPosition, GUILayout.Width(width), GUILayout.Height(200)); // Adjust height as needed
            state.SearchFilter = GUILayout.TextField(state.SearchFilter, GUILayout.Width(width));
            var filteredValues = FilterEnumValues<T>(state.SearchFilter);

            RenderDropdownItems(ref selected, ref state, filteredValues, width);

            GUILayout.EndScrollView();

            // Restore the main scroll position
            scrollPosition = previousScrollPosition;
        }

        GUILayout.EndVertical(); // End vertical layout for dropdown
        return selected;
    }



    private void RenderDropdownItems<T>(ref T selected, ref DropdownState state, List<T> filteredValues, float width) where T : System.Enum
    {
        state.ScrollIndex = Mathf.Clamp(state.ScrollIndex, 0, Mathf.Max(0, filteredValues.Count - MaxDropdownItems));

        for (int i = state.ScrollIndex; i < Mathf.Min(state.ScrollIndex + MaxDropdownItems, filteredValues.Count); i++)
        {
            if (GUILayout.Button(filteredValues[i].ToString(), GUILayout.Width(width)))
            {
                selected = filteredValues[i];
                state.IsOpen = false;
            }
        }

        if (filteredValues.Count > MaxDropdownItems)
        {
            RenderScrollButtons(ref state);
        }
    }

    private void RenderScrollButtons(ref DropdownState state)
    {
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Up", GUILayout.Width(50)))
        {
            state.ScrollIndex = Mathf.Max(0, state.ScrollIndex - 1);
        }
        if (GUILayout.Button("Down", GUILayout.Width(50)))
        {
            state.ScrollIndex = Mathf.Min(state.ScrollIndex + 1, state.ScrollIndex + MaxDropdownItems);
        }
        GUILayout.EndHorizontal();
    }

    private List<T> FilterEnumValues<T>(string filter) where T : System.Enum
    {
        var values = System.Enum.GetValues(typeof(T));
        var filteredValues = new List<T>();

        foreach (var value in values)
        {
            if (value.ToString().ToLower().Contains(filter.ToLower()))
            {
                filteredValues.Add((T)value);
            }
        }

        return filteredValues;
    }

    private Texture2D MakeTex(int width, int height, Color col)
    {
        var pix = new Color[width * height];
        for (int i = 0; i < pix.Length; i++)
        {
            pix[i] = col;
        }

        var result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }

    private class DropdownState
    {
        public bool IsOpen = false;
        public int ScrollIndex = 0;
        public string SearchFilter = "";
    }
#endif
}