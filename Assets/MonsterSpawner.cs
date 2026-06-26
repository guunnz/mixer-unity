using System;
using System.Collections.Generic;
using System.Linq;
using enemies;
using finished3;
using UnityEngine;
using UnityEngine.Serialization;

public enum MonsterClass
{
    Beast,
    Bug,
    Bird,
    Reptile,
    Plant,
    Aquatic,
    Mech,
    Dawn,
    Dusk,
}

[Serializable]
public class MonsterClassObject
{
    public Sprite classSprite;
    public MonsterClass monsterClass;
}

namespace Game
{
    public class MonsterSpawner : MonoBehaviour
    {
        public MonsterBodyPartsManager skillList;
        [SerializeField] private RectTransform rootTF;

        public Team goodTeam;
        public Team enemyTeam;

        public GameObject goodTeamHP;
        public GameObject badTeamHP;
        public GameObject monsterSkillEffectManager;

        public Transform MonstersParent;

        public MonsterLandBattleTarget landBattleTarget;

        public MonsterClassObject[] monsterClassObjects = Array.Empty<MonsterClassObject>();

        public static MonsterSpawner Instance;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private Sprite GetClassIcon(MonsterClass monsterClass)
        {
            if (monsterClassObjects == null)
                return null;

            foreach (MonsterClassObject classObject in monsterClassObjects)
            {
                if (classObject != null && classObject.monsterClass == monsterClass && classObject.classSprite != null)
                    return classObject.classSprite;
            }

            return null;
        }

        public void SpawnEnemyMonsterById(string monsterId, MonsterClass monsterClass,
            GetMonstersExample.Stats stats, MonsterForBackend monsterForBackend, List<GetMonstersExample.Monster> monsterEnemies,
            GetMonstersExample.Monster monsterEnemy, bool isOpponent = false)
        {
            GetMonstersGenesAndSpawn(monsterId, monsterClass, stats, monsterForBackend, monsterEnemies, monsterEnemy, isOpponent);
        }

        private void GetMonstersGenesAndSpawn(string monsterId, MonsterClass monsterClass, GetMonstersExample.Stats stats,
            MonsterForBackend monsterForBackend, List<GetMonstersExample.Monster> monsterEnemies, GetMonstersExample.Monster enemy,
            bool isOpponent = false)
        {
            ProcessMixer(monsterId, enemy.newGenes, false, monsterClass, stats, monsterForBackend, monsterEnemies, enemy, isOpponent);
        }

        public void ProcessMixer(string monsterId, string genesStr, bool isGraphic,
            MonsterClass monsterClass, GetMonstersExample.Stats stats, MonsterForBackend monsterForBackend,
            List<GetMonstersExample.Monster> monsterEnemies, GetMonstersExample.Monster enemy, bool isOpponent = false)
        {
            if (string.IsNullOrEmpty(genesStr))
            {
                Debug.LogError($"[{monsterId}] genes not found.");
                return;
            }

            MonsterVisualDescriptor descriptor = enemy != null
                ? MonsterVisualDescriptor.FromMonster(enemy)
                : BuildVisualDescriptor(monsterId, genesStr, monsterClass);

            if (isGraphic)
            {
                CreatePreviewGraphic(descriptor);
                return;
            }

            SpawnVanillaMonster(monsterId, monsterClass, stats, monsterForBackend, monsterEnemies, isOpponent, genesStr, descriptor);
        }

        public MonsterController ProcessMixer(string monsterId, string genesStr, bool isGraphic,
            MonsterClass monsterClass, GetMonstersExample.Stats stats, bool isOpponent = false)
        {
            if (string.IsNullOrEmpty(genesStr))
            {
                Debug.LogError($"[{monsterId}] genes not found.");
                return null;
            }

            MonsterVisualDescriptor descriptor = BuildVisualDescriptor(monsterId, genesStr, monsterClass);
            if (isGraphic)
            {
                CreatePreviewGraphic(descriptor);
                return null;
            }

            return SpawnVanillaMonster(monsterId, monsterClass, stats, null, null, isOpponent, genesStr, descriptor);
        }

        public MonsterVisualDescriptor ProcessMixer(GetMonstersExample.Monster monster)
        {
            return MonsterVisualDescriptor.FromMonster(monster);
        }

        public MonsterVisualDescriptor ProcessMixer(string genes, string monsterId)
        {
            return BuildVisualDescriptor(monsterId, genes, TryClassFromGenes(genes));
        }

        public MonsterVisualDescriptor SimpleProcessMixer(string monsterId, string genesStr, bool isGraphic)
        {
            if (string.IsNullOrEmpty(genesStr))
            {
                Debug.LogError($"[{monsterId}] genes not found.");
                return MonsterVisualDescriptor.Default();
            }

            MonsterVisualDescriptor descriptor = BuildVisualDescriptor(monsterId, genesStr, TryClassFromGenes(genesStr));
            if (isGraphic)
                CreatePreviewGraphic(descriptor);

            return descriptor;
        }

        public void SimpleProcessCursedMixer(string monsterId, string genesStr, bool isGraphic,
            Dictionary<string, string> cursedMeta, MonsterController controller)
        {
            if (controller == null)
                return;

            MonsterVisualDescriptor descriptor = BuildVisualDescriptor(monsterId, genesStr, controller.monsterIngameStats.monsterClass);
            controller.visualDescriptor = descriptor;
            controller.Visual?.SetDescriptor(descriptor);
            controller.Visual?.Play(MonsterVisualState.Hit, false);
        }

        private MonsterController SpawnVanillaMonster(string monsterId, MonsterClass monsterClass, GetMonstersExample.Stats stats,
            MonsterForBackend monsterForBackend, List<GetMonstersExample.Monster> monsterEnemies, bool isEnemy = false,
            string genes = "", MonsterVisualDescriptor descriptor = null)
        {
            GameObject go = new GameObject("Monster");
            return CreateMonster(go, monsterId, monsterClass, stats, monsterForBackend, monsterEnemies, isEnemy, genes, descriptor);
        }

        private void SetLayerRecursively(Transform root, string layerName)
        {
            root.gameObject.layer = LayerMask.NameToLayer(layerName);

            for (int i = 0; i < root.childCount; i++)
                SetLayerRecursively(root.GetChild(i), layerName);
        }

        private MonsterController CreateMonster(GameObject go, string monsterId, MonsterClass monsterClass,
            GetMonstersExample.Stats stats, MonsterForBackend monsterForBackend, List<GetMonstersExample.Monster> monsterEnemies,
            bool isEnemy = false, string genes = "", MonsterVisualDescriptor descriptor = null)
        {
            try
            {
                if (rootTF != null)
                    go.transform.SetParent(rootTF, false);

                go.transform.localScale = Vector3.one;
                go.transform.eulerAngles = new Vector3(55.26f, go.transform.eulerAngles.y, go.transform.eulerAngles.z);
                go.AddComponent<MonsterController>();

                MonsterController controller = go.GetComponent<MonsterController>();
                controller.monsterBehavior = go.AddComponent<MonsterBehavior>();
                controller.MonsterId = int.Parse(monsterId);
                controller.monsterIngameStats = new IngameStats();
                controller.monsterIngameStats.monsterId = monsterId;
                controller.monsterIngameStats.maxHP = stats.hp * 2;
                controller.monsterIngameStats.monsterClass = monsterClass;
                controller.imGood = !isEnemy;
                controller.stats = stats;
                controller.monsterIngameStats.MinEnergy = 0;
                controller.monsterIngameStats.CurrentEnergy = MonsterStatCalculator.GetMonsterMinEnergy(controller.stats);
                controller.monsterIngameStats.currentHP = stats.skill;
                controller.Genes = genes;

                descriptor ??= BuildVisualDescriptor(monsterId, genes, monsterClass);
                controller.visualDescriptor = descriptor;

                controller.monsterBodyParts = ResolveBodyParts(monsterId, isEnemy, monsterEnemies);
                controller.monsterSkillController = controller.gameObject.AddComponent<MonsterSkillController>();

                VanillaMonsterVisual visual = VanillaMonsterVisual.Create(go.transform, descriptor);
                visual.Play(MonsterVisualState.Idle, true);
                controller.Visual = visual;

                controller.statsManagerUI = Instantiate(isEnemy ? badTeamHP : goodTeamHP, visual.OverheadAnchor)
                    .GetComponent<StatsManager>();

                if (isEnemy)
                {
                    controller.startingCol =
                        monsterForBackend.position_values_per_round[RunManagerSingleton.instance.score].col;
                    controller.startingRow =
                        monsterForBackend.position_values_per_round[RunManagerSingleton.instance.score].row;
                    enemyTeam.AddCharacter(controller);

                    List<MonsterBodyPart> skillsSelected = skillList.monsterBodyParts
                        .Where(x => monsterForBackend.combos_values_per_round[RunManagerSingleton.instance.score]
                            .combos_id.Select(comboId => (SkillName)comboId).Contains(x.skillName))
                        .ToList();

                    controller.monsterSkillController.SetMonsterSkills(skillsSelected.Select(x => x.skillName).ToList(),
                        skillsSelected.Select(x => x.bodyPart).ToList());
                }
                else
                {
                    Position position =
                        TeamManager.instance.currentTeam.position[
                            TeamManager.instance.currentTeam.MonsterIds.FindIndex(x => x.id == monsterId)];

                    goodTeam.AddCharacter(controller, new Vector2Int(position.row, position.col));
                }

                MonsterScale.ApplyGrabCollider(go);
                controller.monsterIngameStats.MaxEnergy = controller.monsterSkillController.GetComboCost();
                go.tag = "Character";

                controller.monsterSkillEffectManager = Instantiate(monsterSkillEffectManager, visual.BodyAnchor)
                    .GetComponent<MonsterSkillEffectManager>();
                Sprite classIcon = GetClassIcon(monsterClass);
                controller.statsManagerUI.SetSR(
                    classIcon != null ? classIcon : VanillaMonsterIconUtility.GetClassSprite(monsterClass),
                    classIcon != null ? Color.white : MonsterClassPalette.Main(monsterClass));

                if (MonstersParent != null)
                    controller.transform.parent = MonstersParent;

                if (isEnemy)
                {
                    go.layer = LayerMask.NameToLayer("LandEnemy");
                    SetLayerRecursively(go.transform, "LandEnemy");
                }

                return controller;
            }
            catch (Exception ex)
            {
                Debug.LogError("CHECKING TOKEN : " + PlayerPrefs.GetString("Auth"));
                Debug.LogError("ERROR CREATING MONSTER: " + ex.Message + " MONSTER: " + monsterId);
                return null;
            }
        }

        private List<SkillName> ResolveBodyParts(string monsterId, bool isEnemy, List<GetMonstersExample.Monster> monsterEnemies)
        {
            IEnumerable<GetMonstersExample.Part> parts = Array.Empty<GetMonstersExample.Part>();
            if (isEnemy && monsterEnemies != null)
                parts = monsterEnemies.Single(x => x.id == monsterId).parts;
            else if (AccountManager.userMonsters?.results != null)
                parts = AccountManager.userMonsters.results.Single(x => x.id == monsterId).parts;

            return parts
                .Where(x => x.BodyPart != BodyPart.Ears && x.BodyPart != BodyPart.Eyes)
                .Select(x => x.SkillName)
                .ToList();
        }

        private MonsterVisualDescriptor BuildVisualDescriptor(string monsterId, string genes, MonsterClass monsterClass)
        {
            GetMonstersExample.Monster monster = AccountManager.userMonsters?.results?.FirstOrDefault(x => x.id == monsterId);
            if (monster != null)
                return MonsterVisualDescriptor.FromMonster(monster);

            return new MonsterVisualDescriptor
            {
                MonsterId = monsterId,
                DisplayName = monsterId,
                Class = monsterClass,
                Parts = Array.Empty<GetMonstersExample.Part>()
            };
        }

        private MonsterClass TryClassFromGenes(string genes)
        {
            try
            {
                string className = MonsterGeneUtils.GetMonsterClass(genes).ToString();
                return (MonsterClass)Enum.Parse(typeof(MonsterClass), className, true);
            }
            catch
            {
                return MonsterClass.Beast;
            }
        }

        private void CreatePreviewGraphic(MonsterVisualDescriptor descriptor)
        {
            if (rootTF == null)
                return;

            VanillaMonsterGraphic graphic = VanillaMonsterGraphic.Ensure(rootTF.gameObject);
            graphic.SetDescriptor(descriptor);
            graphic.Initialize(true);
        }
    }
}
