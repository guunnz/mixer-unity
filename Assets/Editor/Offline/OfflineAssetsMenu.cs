using System.IO;
using UnityEditor;
using UnityEngine;

public static class OfflineAssetsMenu
{
    private const string ResourcesDir = "Assets/Resources";
    private const string SettingsAssetPath = "Assets/Resources/OfflineModeSettings.asset";
    private const string DatabaseAssetPath = "Assets/Resources/OfflineAxieDatabase.asset";

    [MenuItem("Tools/Offline/Create Offline Mode Assets")]
    public static void CreateOfflineModeAssets()
    {
        if (!AssetDatabase.IsValidFolder(ResourcesDir))
        {
            AssetDatabase.CreateFolder("Assets", "Resources");
        }

        var db = AssetDatabase.LoadAssetAtPath<OfflineAxieDatabase>(DatabaseAssetPath);
        if (db == null)
        {
            db = ScriptableObject.CreateInstance<OfflineAxieDatabase>();
            AssetDatabase.CreateAsset(db, DatabaseAssetPath);
        }

        var settings = AssetDatabase.LoadAssetAtPath<OfflineModeSettings>(SettingsAssetPath);
        if (settings == null)
        {
            settings = ScriptableObject.CreateInstance<OfflineModeSettings>();
            settings.useOfflineMode = true;
            settings.offlineUsername = "Offline Player";
            settings.offlineWalletAddress = "offline";
            settings.axieDatabase = db;
            AssetDatabase.CreateAsset(settings, SettingsAssetPath);
        }
        else
        {
            if (settings.axieDatabase == null)
                settings.axieDatabase = db;
            EditorUtility.SetDirty(settings);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Selection.activeObject = settings;
        EditorGUIUtility.PingObject(settings);
        Debug.Log($"Offline assets ready. Settings: {SettingsAssetPath}, DB: {DatabaseAssetPath}");
    }

    [MenuItem("Tools/Offline/Open Axie Generator")]
    public static void OpenGenerator()
    {
        OfflineAxieGeneratorWindow.ShowWindow();
    }
}

