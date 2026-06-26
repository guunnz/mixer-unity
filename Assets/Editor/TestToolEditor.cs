using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TestTool))]
public class CustomMonsterInspector : Editor
{
    private SerializedProperty coinsAmount;
    private SerializedProperty battleTimerStartIn;
    private SerializedProperty allyMonstersForTesting;
    private SerializedProperty enemyMonstersForTesting;

    private TestTool testToolScript;

    private Vector2 scrollPosition;

    private void OnEnable()
    {
        testToolScript = (TestTool)target;

        coinsAmount = serializedObject.FindProperty("CoinsAmount");
        battleTimerStartIn = serializedObject.FindProperty("BattleTimerStartIn");
        allyMonstersForTesting = serializedObject.FindProperty("allyMonstersForTesting");
        enemyMonstersForTesting = serializedObject.FindProperty("enemyMonstersForTesting");

        InitializeSpecialBodyParts(allyMonstersForTesting);
        InitializeSpecialBodyParts(enemyMonstersForTesting);
    }

    private void InitializeSpecialBodyParts(SerializedProperty monsterList)
    {
        for (int i = 0; i < monsterList.arraySize; i++)
        {
            InitializeSingleMonsterSpecialBodyParts(monsterList.GetArrayElementAtIndex(i));
        }
    }

    private void InitializeSingleMonsterSpecialBodyParts(SerializedProperty monsterProperty)
    {
        var specialBodyPartList = monsterProperty.FindPropertyRelative("specialBodyPartForAbilityIfNeeded");

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

        if (GUILayout.Button("Morph Allied Monsters"))
        {
            testToolScript.MorphAlliedMonsters();
        }

        EditorGUILayout.PropertyField(battleTimerStartIn, new GUIContent("Battle Timer Starts In"));

        EditorGUILayout.Space(20);

        // Begin scroll view
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(600));

        // Ally Monsters Section
        EditorGUILayout.LabelField("Ally Monsters For Testing", new GUIStyle(EditorStyles.boldLabel) { fontSize = 16, normal = { textColor = Color.green } });
        DisplayMonsterList(allyMonstersForTesting, Color.green);

        EditorGUILayout.Space(20);

        // Enemy Monsters Section
        EditorGUILayout.LabelField("Enemy Monsters For Testing", new GUIStyle(EditorStyles.boldLabel) { fontSize = 16, normal = { textColor = Color.red } });
        DisplayMonsterList(enemyMonstersForTesting, Color.red);

        EditorGUILayout.EndScrollView(); // End scroll view

        serializedObject.ApplyModifiedProperties();
    }

    private void DisplayMonsterList(SerializedProperty monsterList, Color titleColor)
    {
        int count = monsterList.arraySize;

        for (int i = 0; i < count; i += 2)
        {
            EditorGUILayout.BeginHorizontal();

            if (i < count)
            {
                DisplaySingleMonster(monsterList, i, titleColor, 20);
            }

            if (i + 1 < count)
            {
                DisplaySingleMonster(monsterList, i + 1, titleColor, 20);
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);
        }

        if (GUILayout.Button("Add New Monster"))
        {
            int newIndex = monsterList.arraySize;
            monsterList.InsertArrayElementAtIndex(newIndex);
            InitializeSingleMonsterSpecialBodyParts(monsterList.GetArrayElementAtIndex(newIndex));
        }
    }

    private void DisplaySingleMonster(SerializedProperty monsterList, int index, Color titleColor, int margin)
    {
        if (index >= monsterList.arraySize)
        {
            return; // Avoid accessing out of bounds
        }

        try
        {
            var monsterProperty = monsterList.GetArrayElementAtIndex(index);

            EditorGUILayout.BeginVertical("box", GUILayout.MaxWidth(Screen.width / 2 - margin));

            // Monster Title with Larger Font and Team Color
            EditorGUILayout.LabelField($"Monster {index + 1}", new GUIStyle(EditorStyles.boldLabel) { fontSize = 14, normal = { textColor = titleColor } });

            // Use Monster Stats Toggle
            SerializedProperty useMonsterStats = monsterProperty.FindPropertyRelative("UseMonsterStats");
            useMonsterStats.boolValue = EditorGUILayout.Toggle("Use Monster Stats", useMonsterStats.boolValue);

            // Display Monster Stats, but disable editing if UseMonsterStats is enabled
            GUI.enabled = !useMonsterStats.boolValue; // Disable fields if UseMonsterStats is enabled
            SerializedProperty monsterStats = monsterProperty.FindPropertyRelative("monsterStats");
            EditorGUILayout.PropertyField(monsterStats.FindPropertyRelative("speed"), new GUIContent("Speed"));
            EditorGUILayout.PropertyField(monsterStats.FindPropertyRelative("skill"), new GUIContent("Skill"));
            EditorGUILayout.PropertyField(monsterStats.FindPropertyRelative("morale"), new GUIContent("Morale"));
            EditorGUILayout.PropertyField(monsterStats.FindPropertyRelative("hp"), new GUIContent("HP"));
            GUI.enabled = true; // Re-enable fields

            EditorGUILayout.Space(10);

            // Abilities to Use (fully expanded and accessible)
            EditorGUILayout.PropertyField(monsterProperty.FindPropertyRelative("abilitiesToUse"), new GUIContent("Abilities to Use"), true);

            EditorGUILayout.Space(10);

            // Special Body Part For Ability If Needed
            EditorGUILayout.PropertyField(monsterProperty.FindPropertyRelative("specialBodyPartForAbilityIfNeeded"), new GUIContent("Special Body Parts"), true);
            EditorGUILayout.HelpBox("For example, anemone exists as a horn and as a back. You can decide which one the monster should use with this.", MessageType.Info);

            EditorGUILayout.Space(10);

            // Starts with Buff/Debuff (fully expanded and accessible)
            EditorGUILayout.PropertyField(monsterProperty.FindPropertyRelative("startsWithBuffDebuff"), new GUIContent("Buffs/Debuffs"), true);

            EditorGUILayout.Space(10);

            // Debuffs Duration
            EditorGUILayout.PropertyField(monsterProperty.FindPropertyRelative("DebuffsDuration"), new GUIContent("Debuffs Duration"));

            EditorGUILayout.Space(10);

            // Button to remove monster
            if (GUILayout.Button("Remove Monster"))
            {
                monsterList.DeleteArrayElementAtIndex(index);
                return; // Exit to avoid accessing invalid indices
            }

            EditorGUILayout.EndVertical();
        }
        catch
        {
            // Handle any unexpected errors gracefully
            Debug.LogWarning($"An error occurred while displaying monster at index {index}.");
        }
    }
}
