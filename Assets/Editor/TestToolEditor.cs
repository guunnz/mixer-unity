using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TestTool))]
public class CustomAxieInspector : Editor
{
    private SerializedProperty coinsAmount;
    private SerializedProperty battleTimerStartIn;
    private SerializedProperty allyAxiesForTesting;
    private SerializedProperty enemyAxiesForTesting;

    private TestTool testToolScript;

    private Vector2 scrollPosition;

    private void OnEnable()
    {
        testToolScript = (TestTool)target;

        coinsAmount = serializedObject.FindProperty("CoinsAmount");
        battleTimerStartIn = serializedObject.FindProperty("BattleTimerStartIn");
        allyAxiesForTesting = serializedObject.FindProperty("allyAxiesForTesting");
        enemyAxiesForTesting = serializedObject.FindProperty("enemyAxiesForTesting");

        InitializeSpecialBodyParts(allyAxiesForTesting);
        InitializeSpecialBodyParts(enemyAxiesForTesting);
    }

    private void InitializeSpecialBodyParts(SerializedProperty axieList)
    {
        for (int i = 0; i < axieList.arraySize; i++)
        {
            InitializeSingleAxieSpecialBodyParts(axieList.GetArrayElementAtIndex(i));
        }
    }

    private void InitializeSingleAxieSpecialBodyParts(SerializedProperty axieProperty)
    {
        var specialBodyPartList = axieProperty.FindPropertyRelative("specialBodyPartForAbilityIfNeeded");

        specialBodyPartList.ClearArray();
        specialBodyPartList.InsertArrayElementAtIndex(0);
        specialBodyPartList.GetArrayElementAtIndex(0).enumValueIndex = (int)BodyPart.None;

        specialBodyPartList.InsertArrayElementAtIndex(1);
        specialBodyPartList.GetArrayElementAtIndex(1).enumValueIndex = (int)BodyPart.None;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Header
        EditorGUILayout.LabelField("Battle Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(coinsAmount, new GUIContent("Coins Amount"));

        if (GUILayout.Button("Set Coins"))
        {
            testToolScript.SetCoins();
        }

        if (GUILayout.Button("Morph Allied Axies"))
        {
            testToolScript.MorphAlliedAxies();
        }

        EditorGUILayout.PropertyField(battleTimerStartIn, new GUIContent("Battle Timer Starts In"));

        EditorGUILayout.Space(20);

        // Begin scroll view
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(600));

        // Ally Axies Section
        EditorGUILayout.LabelField("Ally Axies For Testing", new GUIStyle(EditorStyles.boldLabel) { fontSize = 16, normal = { textColor = Color.green } });
        DisplayAxieList(allyAxiesForTesting, Color.green);

        EditorGUILayout.Space(20);

        // Enemy Axies Section
        EditorGUILayout.LabelField("Enemy Axies For Testing", new GUIStyle(EditorStyles.boldLabel) { fontSize = 16, normal = { textColor = Color.red } });
        DisplayAxieList(enemyAxiesForTesting, Color.red);

        EditorGUILayout.EndScrollView(); // End scroll view

        serializedObject.ApplyModifiedProperties();
    }

    private void DisplayAxieList(SerializedProperty axieList, Color titleColor)
    {
        int count = axieList.arraySize;

        for (int i = 0; i < count; i += 2)
        {
            EditorGUILayout.BeginHorizontal();

            if (i < count)
            {
                DisplaySingleAxie(axieList, i, titleColor, 20);
            }

            if (i + 1 < count)
            {
                DisplaySingleAxie(axieList, i + 1, titleColor, 20);
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);
        }

        if (GUILayout.Button("Add New Axie"))
        {
            int newIndex = axieList.arraySize;
            axieList.InsertArrayElementAtIndex(newIndex);
            InitializeSingleAxieSpecialBodyParts(axieList.GetArrayElementAtIndex(newIndex));
        }
    }

    private void DisplaySingleAxie(SerializedProperty axieList, int index, Color titleColor, int margin)
    {
        if (index >= axieList.arraySize)
        {
            return; // Avoid accessing out of bounds
        }

        try
        {
            var axieProperty = axieList.GetArrayElementAtIndex(index);

            EditorGUILayout.BeginVertical("box", GUILayout.MaxWidth(Screen.width / 2 - margin));

            // Axie Title with Larger Font and Team Color
            EditorGUILayout.LabelField($"Axie {index + 1}", new GUIStyle(EditorStyles.boldLabel) { fontSize = 14, normal = { textColor = titleColor } });

            // Use Axie Stats Toggle
            SerializedProperty useAxieStats = axieProperty.FindPropertyRelative("UseAxieStats");
            useAxieStats.boolValue = EditorGUILayout.Toggle("Use Axie Stats", useAxieStats.boolValue);

            // Display Axie Stats, but disable editing if UseAxieStats is enabled
            GUI.enabled = !useAxieStats.boolValue; // Disable fields if UseAxieStats is enabled
            SerializedProperty axieStats = axieProperty.FindPropertyRelative("axieStats");
            EditorGUILayout.PropertyField(axieStats.FindPropertyRelative("speed"), new GUIContent("Speed"));
            EditorGUILayout.PropertyField(axieStats.FindPropertyRelative("skill"), new GUIContent("Skill"));
            EditorGUILayout.PropertyField(axieStats.FindPropertyRelative("morale"), new GUIContent("Morale"));
            EditorGUILayout.PropertyField(axieStats.FindPropertyRelative("hp"), new GUIContent("HP"));
            GUI.enabled = true; // Re-enable fields

            EditorGUILayout.Space(10);

            // Abilities to Use (fully expanded and accessible)
            EditorGUILayout.PropertyField(axieProperty.FindPropertyRelative("abilitiesToUse"), new GUIContent("Abilities to Use"), true);

            EditorGUILayout.Space(10);

            // Special Body Part For Ability If Needed
            EditorGUILayout.PropertyField(axieProperty.FindPropertyRelative("specialBodyPartForAbilityIfNeeded"), new GUIContent("Special Body Parts"), true);
            EditorGUILayout.HelpBox("For example, anemone exists as a horn and as a back. You can decide which one the axie should use with this.", MessageType.Info);

            EditorGUILayout.Space(10);

            // Starts with Buff/Debuff (fully expanded and accessible)
            EditorGUILayout.PropertyField(axieProperty.FindPropertyRelative("startsWithBuffDebuff"), new GUIContent("Buffs/Debuffs"), true);

            EditorGUILayout.Space(10);

            // Debuffs Duration
            EditorGUILayout.PropertyField(axieProperty.FindPropertyRelative("DebuffsDuration"), new GUIContent("Debuffs Duration"));

            EditorGUILayout.Space(10);

            // Button to remove axie
            if (GUILayout.Button("Remove Axie"))
            {
                axieList.DeleteArrayElementAtIndex(index);
                return; // Exit to avoid accessing invalid indices
            }

            EditorGUILayout.EndVertical();
        }
        catch
        {
            // Handle any unexpected errors gracefully
            Debug.LogWarning($"An error occurred while displaying axie at index {index}.");
        }
    }
}
