using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Skill))]
public class DotweenAnimationSkillEditor : Editor
{
   
    public override void OnInspectorGUI()
    {
        serializedObject.Update(); // Update the serialized object to fetch the latest data

        // Skill skill = (Skill)target; // Cast the target to Skill, if needed for specific non-serialized operations

        // Iterate over the vfxToThrow list and create a custom inspector for each entry
        SerializedProperty vfxList = serializedObject.FindProperty("vfxToThrow");
        if (vfxList != null && vfxList.isArray)
        {
            for (int i = 0; i < vfxList.arraySize; i++)
            {
                SerializedProperty vfxEntry = vfxList.GetArrayElementAtIndex(i);
                SerializedProperty animationMove = vfxEntry.FindPropertyRelative("AnimationMove");
                SerializedProperty enableAnimation = animationMove.FindPropertyRelative("EnableDotweenAnimation");

                EditorGUILayout.PropertyField(vfxEntry, new GUIContent("Skill VFX " + (i + 1)));

                // Handle enabling/disabling fields based on EnableDotweenAnimation
                enableAnimation.boolValue = EditorGUILayout.Toggle("Enable Dotween Animation", enableAnimation.boolValue);

                EditorGUI.BeginDisabledGroup(!enableAnimation.boolValue);
                {
                    EditorGUILayout.PropertyField(animationMove.FindPropertyRelative("StartFrom"), new GUIContent("Start From"));
                    EditorGUILayout.PropertyField(animationMove.FindPropertyRelative("GoTo"), new GUIContent("Go To"));
                    EditorGUILayout.PropertyField(animationMove.FindPropertyRelative("Time"), new GUIContent("Time"));
                }
                EditorGUI.EndDisabledGroup();
            }
        }

        serializedObject.ApplyModifiedProperties(); // Apply changes to the serialized object

        if (GUI.changed)
        {
            // Note: Deprecated in recent Unity versions in favor of serializedObject's handling
            // EditorUtility.SetDirty(skill);
        }
    }
}