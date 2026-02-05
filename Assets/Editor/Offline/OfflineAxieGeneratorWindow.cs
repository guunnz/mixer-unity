using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class OfflineAxieGeneratorWindow : EditorWindow
{
    private OfflineModeSettings settings;
    private OfflineAxieDatabase db;

    private enum CreateMode
    {
        FromParts,
        FromGenes,
    }

    private CreateMode createMode = CreateMode.FromParts;

    private string genesInput = "";
    private string nameInput = "";
    private string idInput = "";
    private enum BodyType
    {
        Normal,
        BigYak,
        Curly,
        Fuzzy,
        WetDog,
        Custom,
    }

    private BodyType bodyType = BodyType.Normal;
    private string bodyShapeInput = "normal"; // used only when BodyType.Custom
    private bool f2p = true;

    private AxieClass axieClass = AxieClass.Beast;
    private LandType landType = LandType.axiepark;

    private int eyesIndex;
    private int earsIndex;
    private int mouthIndex;
    private int hornIndex;
    private int backIndex;
    private int tailIndex;

    private Vector2 scroll;

    [MenuItem("Tools/Offline/Axie Generator")]
    public static void ShowWindow()
    {
        var window = GetWindow<OfflineAxieGeneratorWindow>("Offline Axie Generator");
        window.minSize = new Vector2(520, 420);
        window.Show();
    }

    private void OnEnable()
    {
        settings = Resources.Load<OfflineModeSettings>("OfflineModeSettings");
        db = settings != null ? settings.axieDatabase : Resources.Load<OfflineAxieDatabase>("OfflineAxieDatabase");
    }

    private void OnGUI()
    {
        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Offline Mode", EditorStyles.boldLabel);

        using (new EditorGUILayout.VerticalScope("box"))
        {
            settings = (OfflineModeSettings)EditorGUILayout.ObjectField("Settings (Resources)", settings, typeof(OfflineModeSettings), false);
            db = (OfflineAxieDatabase)EditorGUILayout.ObjectField("Axie Database", db, typeof(OfflineAxieDatabase), false);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Create/Repair Offline Assets"))
                {
                    OfflineAssetsMenu.CreateOfflineModeAssets();
                    settings = Resources.Load<OfflineModeSettings>("OfflineModeSettings");
                    db = settings != null ? settings.axieDatabase : null;
                }

                if (GUILayout.Button("Select Database") && db != null)
                {
                    Selection.activeObject = db;
                    EditorGUIUtility.PingObject(db);
                }
            }

            if (settings == null)
            {
                EditorGUILayout.HelpBox("Missing `Resources/OfflineModeSettings.asset`. Click 'Create/Repair Offline Assets'.", MessageType.Warning);
            }
            else if (!settings.useOfflineMode)
            {
                EditorGUILayout.HelpBox("Offline mode is disabled in settings. Enable it to bypass API calls at runtime.", MessageType.Info);
            }

            if (db == null)
            {
                EditorGUILayout.HelpBox("Missing `Resources/OfflineAxieDatabase.asset`. Click 'Create/Repair Offline Assets'.", MessageType.Warning);
            }
        }

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Add Axie(s)", EditorStyles.boldLabel);
        using (new EditorGUILayout.VerticalScope("box"))
        {
            createMode = (CreateMode)EditorGUILayout.EnumPopup("Create Mode", createMode);
            axieClass = (AxieClass)EditorGUILayout.EnumPopup("Axie Class", axieClass);
            landType = (LandType)EditorGUILayout.EnumPopup("Land Type", landType);

            idInput = EditorGUILayout.TextField("ID (optional)", idInput);
            nameInput = EditorGUILayout.TextField("Name (optional)", nameInput);
            bodyType = (BodyType)EditorGUILayout.EnumPopup("Body Type", bodyType);
            if (bodyType == BodyType.Custom)
                bodyShapeInput = EditorGUILayout.TextField("Body Type (Custom)", bodyShapeInput);
            f2p = EditorGUILayout.Toggle("F2P", f2p);

            if (createMode == CreateMode.FromParts)
            {
                DrawPartsPicker();
            }
            else
            {
                EditorGUILayout.Space(6);
                EditorGUILayout.LabelField("Genes (one per line)", EditorStyles.miniLabel);
                genesInput = EditorGUILayout.TextArea(genesInput, GUILayout.MinHeight(90));
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                GUI.enabled = db != null;
                if (GUILayout.Button("Add"))
                {
                    if (createMode == CreateMode.FromParts)
                        AddEntryFromParts();
                    else
                        AddEntriesFromGenesInput();
                }

                if (GUILayout.Button("Clear Form"))
                {
                    genesInput = "";
                    idInput = "";
                    nameInput = "";
                    bodyType = BodyType.Normal;
                    bodyShapeInput = "normal";
                    f2p = true;
                }

                GUI.enabled = true;
            }
        }

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Database Preview", EditorStyles.boldLabel);
        using (new EditorGUILayout.VerticalScope("box"))
        {
            if (db == null)
            {
                EditorGUILayout.LabelField("No database assigned.");
                return;
            }

            EditorGUILayout.LabelField($"Axies: {db.axies.Count}");
            scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.Height(170));
            for (int i = 0; i < db.axies.Count; i++)
            {
                var a = db.axies[i];
                if (a == null) continue;
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"{i + 1}. {a.id} ({a.axieClass})", GUILayout.Width(240));
                EditorGUILayout.LabelField(a.name, GUILayout.Width(160));
                if (GUILayout.Button("Remove", GUILayout.Width(70)))
                {
                    db.axies.RemoveAt(i);
                    EditorUtility.SetDirty(db);
                    AssetDatabase.SaveAssets();
                    break;
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Save"))
                {
                    EditorUtility.SetDirty(db);
                    AssetDatabase.SaveAssets();
                }

                if (GUILayout.Button("Clear All") && EditorUtility.DisplayDialog("Clear all axies?", "This will remove all offline axies from the database.", "Clear", "Cancel"))
                {
                    db.axies.Clear();
                    EditorUtility.SetDirty(db);
                    AssetDatabase.SaveAssets();
                }
            }
        }
    }

    private void DrawPartsPicker()
    {
        if (!PartStatesCatalog.TryLoad(out string error))
        {
            EditorGUILayout.HelpBox(error, MessageType.Error);
            return;
        }

        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("Parts", EditorStyles.boldLabel);

        DrawPartPopup("Eyes", "eyes", ref eyesIndex);
        DrawPartPopup("Ears", "ears", ref earsIndex);
        DrawPartPopup("Mouth", "mouth", ref mouthIndex);
        DrawPartPopup("Horn", "horn", ref hornIndex);
        DrawPartPopup("Back", "back", ref backIndex);
        DrawPartPopup("Tail", "tail", ref tailIndex);
    }

    private void DrawPartPopup(string label, string partType, ref int index)
    {
        var opts = PartStatesCatalog.GetOptions(partType);
        if (opts.Count == 0)
        {
            EditorGUILayout.LabelField(label, "No options (part_states missing or empty)");
            return;
        }

        index = Mathf.Clamp(index, 0, opts.Count - 1);
        index = EditorGUILayout.Popup(label, index, opts.ToArray());
    }

    private void AddEntryFromParts()
    {
        if (db == null) return;

        // Map selections to ability IDs and classes.
        if (!PartStatesCatalog.TryGetByPartTypeAndIndex("eyes", eyesIndex, out var eyesAbility, out var eyesClass, out _, out var eyesSkin) ||
            !PartStatesCatalog.TryGetByPartTypeAndIndex("ears", earsIndex, out var earsAbility, out var earsClass, out _, out var earsSkin) ||
            !PartStatesCatalog.TryGetByPartTypeAndIndex("mouth", mouthIndex, out var mouthAbility, out var mouthClass, out _, out var mouthSkin) ||
            !PartStatesCatalog.TryGetByPartTypeAndIndex("horn", hornIndex, out var hornAbility, out var hornClass, out _, out var hornSkin) ||
            !PartStatesCatalog.TryGetByPartTypeAndIndex("back", backIndex, out var backAbility, out var backClass, out _, out var backSkin) ||
            !PartStatesCatalog.TryGetByPartTypeAndIndex("tail", tailIndex, out var tailAbility, out var tailClass, out _, out var tailSkin))
        {
            EditorUtility.DisplayDialog("Invalid selection", "Failed to resolve one or more parts from part_states.json.", "OK");
            return;
        }

        // Build the adultCombo expected by the existing FakeAxie512 gene generator.
        // Format: "<class>-<partType>-<value>[.<skin>]" for each part, and "body-class".
        Dictionary<string, string> adultCombo = new Dictionary<string, string>
        {
            { "body-class", axieClass.ToString().ToLower() },
            { "eyes",  AppendSkin(eyesAbility, eyesSkin) },
            { "ears",  AppendSkin(earsAbility, earsSkin) },
            { "mouth", AppendSkin(mouthAbility, mouthSkin) },
            { "horn",  AppendSkin(hornAbility, hornSkin) },
            { "back",  AppendSkin(backAbility, backSkin) },
            { "tail",  AppendSkin(tailAbility, tailSkin) },
        };

        string hex = Genes.FakeAxie512.FakeAxie(adultCombo);
        string genes = "0x" + hex;

        var entry = new OfflineAxieDatabase.AxieEntry();
        entry.genes = genes;
        entry.id = string.IsNullOrEmpty(idInput) ? $"offline-{db.axies.Count + 1}" : idInput;
        entry.name = string.IsNullOrEmpty(nameInput) ? $"Axie {entry.id}" : nameInput;
        entry.bodyShape = GetBodyShapeValue();
        entry.f2p = f2p;
        entry.birthDate = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        entry.axieClass = axieClass.ToString();

        // Store/ensure there is at least one land matching the chosen land type.
        if (db.lands != null && db.lands.All(l => l == null || !string.Equals(l.landType, landType.ToString(), StringComparison.OrdinalIgnoreCase)))
        {
            db.lands.Add(new OfflineAxieDatabase.LandEntry
            {
                tokenId = $"offline-land-{landType}",
                landType = landType.ToString(),
                col = "0",
                row = "0",
                locked = false
            });
        }

        // Avoid duplicate IDs
        if (db.axies.Any(a => a != null && a.id == entry.id))
        {
            entry.id = $"{entry.id}-{Guid.NewGuid().ToString("N").Substring(0, 6)}";
        }

        db.axies.Add(entry);
        EditorUtility.SetDirty(db);
        AssetDatabase.SaveAssets();

        Debug.Log($"Added 1 offline axie (from parts) to database `{db.name}`. Genes auto-generated.");
    }

    private static string AppendSkin(string abilityId, int skin)
    {
        // FakeAxie512 supports optional ".<skin>" suffix.
        return skin == 0 ? abilityId : (abilityId + "." + skin);
    }

    private void AddEntriesFromGenesInput()
    {
        if (db == null) return;

        var lines = genesInput
            .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim())
            .Where(x => !string.IsNullOrEmpty(x))
            .ToList();

        if (lines.Count == 0)
        {
            EditorUtility.DisplayDialog("No genes", "Paste one or more gene strings (one per line).", "OK");
            return;
        }

        int added = 0;
        foreach (var genes in lines)
        {
            var entry = new OfflineAxieDatabase.AxieEntry();
            entry.genes = genes;
            entry.id = string.IsNullOrEmpty(idInput) ? $"offline-{db.axies.Count + 1}" : idInput;
            entry.name = string.IsNullOrEmpty(nameInput) ? $"Axie {entry.id}" : nameInput;
            entry.bodyShape = GetBodyShapeValue();
            entry.f2p = f2p;
            entry.birthDate = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            try
            {
                var cls = AxieGeneUtils.GetAxieClass(genes).ToString(); // lower-case enum names
                entry.axieClass = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(cls);
            }
            catch
            {
                entry.axieClass = "";
            }

            // Avoid duplicate IDs
            if (db.axies.Any(a => a != null && a.id == entry.id))
            {
                entry.id = $"{entry.id}-{Guid.NewGuid().ToString("N").Substring(0, 6)}";
            }

            db.axies.Add(entry);
            added++;
        }

        EditorUtility.SetDirty(db);
        AssetDatabase.SaveAssets();

        Debug.Log($"Added {added} offline axie(s) (from genes) to database `{db.name}`.");
    }

    private string GetBodyShapeValue()
    {
        if (bodyType == BodyType.Custom)
            return string.IsNullOrEmpty(bodyShapeInput) ? "normal" : bodyShapeInput.Trim().ToLowerInvariant();

        // Matches common Axie metadata values (e.g. "normal") used by existing filters.
        switch (bodyType)
        {
            case BodyType.BigYak: return "bigyak";
            case BodyType.Curly: return "curly";
            case BodyType.Fuzzy: return "fuzzy";
            case BodyType.WetDog: return "wetdog";
            case BodyType.Normal:
            default:
                return "normal";
        }
    }
}

