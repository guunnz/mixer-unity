using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class OfflineMonsterGeneratorWindow : EditorWindow
{
    private const string DefaultOfflineGenes = "0x00000000000000000000000000000000000000100880440200000010088044020000001008804402000000100880440200000010088044020000001008804402";

    private OfflineModeSettings settings;
    private OfflineMonsterDatabase db;

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

    private MonsterClass monsterClass = MonsterClass.Beast;
    private LandType landType = LandType.monsterpark;

    private int eyesIndex;
    private int earsIndex;
    private int mouthIndex;
    private int hornIndex;
    private int backIndex;
    private int tailIndex;

    private Vector2 scroll;

    [MenuItem("Tools/Offline/Monster Generator")]
    public static void ShowWindow()
    {
        var window = GetWindow<OfflineMonsterGeneratorWindow>("Offline Monster Generator");
        window.minSize = new Vector2(520, 420);
        window.Show();
    }

    private void OnEnable()
    {
        settings = Resources.Load<OfflineModeSettings>("OfflineModeSettings");
        db = settings != null ? settings.monsterDatabase : Resources.Load<OfflineMonsterDatabase>("OfflineMonsterDatabase");
    }

    private void OnGUI()
    {
        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Offline Mode", EditorStyles.boldLabel);

        using (new EditorGUILayout.VerticalScope("box"))
        {
            settings = (OfflineModeSettings)EditorGUILayout.ObjectField("Settings (Resources)", settings, typeof(OfflineModeSettings), false);
            db = (OfflineMonsterDatabase)EditorGUILayout.ObjectField("Monster Database", db, typeof(OfflineMonsterDatabase), false);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Create/Repair Offline Assets"))
                {
                    OfflineAssetsMenu.CreateOfflineModeAssets();
                    settings = Resources.Load<OfflineModeSettings>("OfflineModeSettings");
                    db = settings != null ? settings.monsterDatabase : null;
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
                EditorGUILayout.HelpBox("Missing `Resources/OfflineMonsterDatabase.asset`. Click 'Create/Repair Offline Assets'.", MessageType.Warning);
            }
        }

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Add Monster(s)", EditorStyles.boldLabel);
        using (new EditorGUILayout.VerticalScope("box"))
        {
            createMode = (CreateMode)EditorGUILayout.EnumPopup("Create Mode", createMode);
            monsterClass = (MonsterClass)EditorGUILayout.EnumPopup("Monster Class", monsterClass);
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

            EditorGUILayout.LabelField($"Monsters: {db.monsters.Count}");
            scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.Height(170));
            for (int i = 0; i < db.monsters.Count; i++)
            {
                var a = db.monsters[i];
                if (a == null) continue;
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"{i + 1}. {a.id} ({a.monsterClass})", GUILayout.Width(240));
                EditorGUILayout.LabelField(a.name, GUILayout.Width(160));
                if (GUILayout.Button("Remove", GUILayout.Width(70)))
                {
                    db.monsters.RemoveAt(i);
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

                if (GUILayout.Button("Clear All") && EditorUtility.DisplayDialog("Clear all monsters?", "This will remove all offline monsters from the database.", "Clear", "Cancel"))
                {
                    db.monsters.Clear();
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

        // The old part-to-gene builder belonged to the removed graphical/runtime stack.
        // Keep generated offline monsters playable with a stable placeholder gene.
        string genes = DefaultOfflineGenes;

        var entry = new OfflineMonsterDatabase.MonsterEntry();
        entry.genes = genes;
        entry.id = string.IsNullOrEmpty(idInput) ? $"offline-{db.monsters.Count + 1}" : idInput;
        entry.name = string.IsNullOrEmpty(nameInput) ? $"Monster {entry.id}" : nameInput;
        entry.bodyShape = GetBodyShapeValue();
        entry.f2p = f2p;
        entry.birthDate = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        entry.monsterClass = monsterClass.ToString();

        // Store/ensure there is at least one land matching the chosen land type.
        if (db.lands != null && db.lands.All(l => l == null || !string.Equals(l.landType, landType.ToString(), StringComparison.OrdinalIgnoreCase)))
        {
            db.lands.Add(new OfflineMonsterDatabase.LandEntry
            {
                tokenId = $"offline-land-{landType}",
                landType = landType.ToString(),
                col = "0",
                row = "0",
                locked = false
            });
        }

        // Avoid duplicate IDs
        if (db.monsters.Any(a => a != null && a.id == entry.id))
        {
            entry.id = $"{entry.id}-{Guid.NewGuid().ToString("N").Substring(0, 6)}";
        }

        db.monsters.Add(entry);
        EditorUtility.SetDirty(db);
        AssetDatabase.SaveAssets();

        Debug.Log($"Added 1 offline monster (from parts) to database `{db.name}`. Genes auto-generated.");
    }

    private static string AppendSkin(string abilityId, int skin)
    {
        // The gene generator supports optional ".<skin>" suffix.
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
            var entry = new OfflineMonsterDatabase.MonsterEntry();
            entry.genes = genes;
            entry.id = string.IsNullOrEmpty(idInput) ? $"offline-{db.monsters.Count + 1}" : idInput;
            entry.name = string.IsNullOrEmpty(nameInput) ? $"Monster {entry.id}" : nameInput;
            entry.bodyShape = GetBodyShapeValue();
            entry.f2p = f2p;
            entry.birthDate = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            try
            {
                var cls = MonsterGeneUtils.GetMonsterClass(genes).ToString(); // lower-case enum names
                entry.monsterClass = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(cls);
            }
            catch
            {
                entry.monsterClass = "";
            }

            // Avoid duplicate IDs
            if (db.monsters.Any(a => a != null && a.id == entry.id))
            {
                entry.id = $"{entry.id}-{Guid.NewGuid().ToString("N").Substring(0, 6)}";
            }

            db.monsters.Add(entry);
            added++;
        }

        EditorUtility.SetDirty(db);
        AssetDatabase.SaveAssets();

        Debug.Log($"Added {added} offline monster(s) (from genes) to database `{db.name}`.");
    }

    private string GetBodyShapeValue()
    {
        if (bodyType == BodyType.Custom)
            return string.IsNullOrEmpty(bodyShapeInput) ? "normal" : bodyShapeInput.Trim().ToLowerInvariant();

        // Matches common metadata values (e.g. "normal") used by existing filters.
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

