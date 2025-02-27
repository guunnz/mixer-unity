using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AxieCore.AxieMixer;
using AxieMixer.Unity;
using enemies;
using finished3;
using Newtonsoft.Json.Linq;
using SkyMavis.AxieMixer.Unity;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public enum AxieClass
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

[System.Serializable]
public class AxieClassObject
{
    public Sprite classSprite;
    public AxieClass axieClass;
}

namespace Game
{
    public class AxieSpawner : MonoBehaviour
    {
        public AxieBodyPartsManager skillList;
        [SerializeField] RectTransform rootTF; // Assign this in the inspector
        private Axie2dBuilder builder => Mixer.Builder;
        const bool USE_GRAPHIC = false;

        public Team goodTeam;
        public Team enemyTeam;

        private int spawnCountMax = 6;

        public GameObject goodTeamHP;
        public GameObject badTeamHP;
        public GameObject axieSkillEffectManager;

        public Transform AxiesParent;

        public AxieLandBattleTarget landBattleTarget;

        public AxieClassObject[] axieClassObjects = new AxieClassObject[] { };

        static public AxieSpawner Instance;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(this.gameObject);
                return;
            }

            Instance = this;

        }

        private void Start()
        {
            Mixer.initialized = false;
            Mixer.Init();
        }


        public void SpawnEnemyAxieById(string axieId, AxieClass @class,
            GetAxiesExample.Stats stats, AxieForBackend axieForBackend, List<GetAxiesExample.Axie> axieEnemies, GetAxiesExample.Axie axieEnemy,
            bool isOpponent = false)
        {
            GetAxiesGenesAndSpawn(axieId, @class, stats, axieForBackend, axieEnemies, axieEnemy,
                isOpponent);
        }


        private void GetAxiesGenesAndSpawn(string axieId,
            AxieClass @class, GetAxiesExample.Stats stats, AxieForBackend axieForBackend,
            List<GetAxiesExample.Axie> axieEnemies, GetAxiesExample.Axie enemy, bool isOpponent = false)
        {

            ProcessMixer(axieId, enemy.newGenes, USE_GRAPHIC, @class, stats, axieForBackend, axieEnemies, enemy, isOpponent);
        }




        public void ProcessMixer(string axieId, string genesStr, bool isGraphic,
            AxieClass @class, GetAxiesExample.Stats stats, AxieForBackend axieForBackend,
            List<GetAxiesExample.Axie> axieEnemies, GetAxiesExample.Axie enemy, bool isOpponent = false)
        {
            if (string.IsNullOrEmpty(genesStr))
            {
                Debug.LogError($"[{axieId}] genes not found!!!");
                return;
            }

            float scale = 0.007f;
            var meta = new Dictionary<string, string>();

            var builderResult = builder.BuildSpineFromGene(axieId, genesStr, meta, scale, isGraphic);

            if (isGraphic)
            {
                SpawnSkeletonGraphic(builderResult);
            }
            else
            {
                SpawnSkeletonAnimation(builderResult, axieId,
                    @class, stats, axieForBackend, axieEnemies, isOpponent, genesStr);
            }


        }

        public AxieController ProcessMixer(string axieId, string genesStr, bool isGraphic,
            AxieClass @class, GetAxiesExample.Stats stats, bool isOpponent = false)
        {
            if (string.IsNullOrEmpty(genesStr))
            {
                Debug.LogError($"[{axieId}] genes not found!!!");
                return null;
            }

            float scale = 0.007f;
            var meta = new Dictionary<string, string>();

            var builderResult = builder.BuildSpineFromGene(axieId, genesStr, meta, scale, isGraphic);

            if (isGraphic)
            {
                SpawnSkeletonGraphic(builderResult);
                return null;
            }
            else
            {
                return SpawnSkeletonAnimation(builderResult, axieId,
                    @class, stats, null, null, isOpponent, genesStr);
            }
        }

        public SkeletonDataAsset ProcessMixer(GetAxiesExample.Axie axie)
        {
            if (axie.skeletonDataAsset != null)
            {
                return axie.skeletonDataAsset;
            }

            if (string.IsNullOrEmpty(axie.newGenes))
            {
                Debug.LogError($"[{axie.id}] genes not found!!!");
                return null;
            }

            float scale = 0.007f;
            var meta = new Dictionary<string, string>();

            var builderResult = builder.BuildSpineFromGene(axie.id, axie.newGenes, meta, scale, false);

            axie.skeletonDataAssetMaterial = builderResult.sharedGraphicMaterial;
            axie.skeletonDataAsset = builderResult.skeletonDataAsset;
            return builderResult.skeletonDataAsset;
        }

        public KeyValuePair<SkeletonDataAsset, Material> ProcessMixer(string genes, string axieid)
        {

            float scale = 0.007f;
            var meta = new Dictionary<string, string>();

            var builderResult = builder.BuildSpineFromGene(axieid, genes, meta, scale, false);

            return new KeyValuePair<SkeletonDataAsset, Material>(builderResult.skeletonDataAsset, builderResult.sharedGraphicMaterial);
        }


        public Axie2dBuilderResult SimpleProcessMixer(string axieId, string genesStr, bool isGraphic)
        {
            if (string.IsNullOrEmpty(genesStr))
            {
                Debug.LogError($"[{axieId}] genes not found!!!");
                return null;
            }

            float scale = 0.007f;
            var meta = new Dictionary<string, string>();

            var builderResult = builder.BuildSpineFromGene(axieId, genesStr, meta, scale, isGraphic);

            return builderResult;
        }

        public void SimpleProcessCursedMixer(string axieId, string genesStr, bool isGraphic,
           Dictionary<string, string> cursedMeta, AxieController controller)
        {
            if (string.IsNullOrEmpty(genesStr))
            {
                Debug.LogError($"[{axieId}] genes not found!!!");
                return;
            }

            float scale = 0.007f;

            var builderResult = builder.BuildSpineFromGeneCursed(axieId, genesStr, cursedMeta, scale, isGraphic);

            Destroy(controller.SkeletonAnim.gameObject);

            SkeletonAnimation runtimeSkeletonAnimation =
                    SkeletonAnimation.NewSkeletonAnimationGameObject(builderResult.skeletonDataAsset);

            runtimeSkeletonAnimation.transform.SetParent(controller.transform, false);

            runtimeSkeletonAnimation.state.SetAnimation(0, "action/idle/normal", true);

            controller.SkeletonAnim = runtimeSkeletonAnimation;
            controller.statsManagerUI =
     Instantiate(!controller.imGood ? badTeamHP : goodTeamHP, runtimeSkeletonAnimation.transform)
         .GetComponent<StatsManager>();
            controller.axieSkillEffectManager =
                Instantiate(axieSkillEffectManager, runtimeSkeletonAnimation.transform)
                    .GetComponent<AxieSkillEffectManager>();
            controller.statsManagerUI.SetSR(axieClassObjects.FirstOrDefault(x => x.axieClass == controller.axieIngameStats.axieClass)?.classSprite);
        }

        private AxieController SpawnSkeletonAnimation(Axie2dBuilderResult builderResult, string axieId,
            AxieClass @class, GetAxiesExample.Stats stats, AxieForBackend axieForBackend,
            List<GetAxiesExample.Axie> axieEnemies, bool isOpponent = false, string genes = "")
        {
            GameObject go = new GameObject("Axie");
            return CreateAxie(go, builderResult, axieId, @class, stats, axieForBackend, axieEnemies, isOpponent, genes);
        }

        void SetLayerRecursively(Transform root, string layerName)
        {
            // Set the layer of the current root
            root.gameObject.layer = LayerMask.NameToLayer(layerName);

            // Iterate over all children and apply the function recursively
            for (int i = 0; i < root.childCount; i++)
            {
                SetLayerRecursively(root.GetChild(i), layerName);
            }
        }
        private AxieController CreateAxie(GameObject go, Axie2dBuilderResult builderResult, string axieId,
            AxieClass @class,
            GetAxiesExample.Stats stats, AxieForBackend axieForBackend, List<GetAxiesExample.Axie> axieEnemies,
            bool isEnemy = false, string genes = "")
        {
            try
            {


                go.transform.SetParent(rootTF, false);
                go.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                go.transform.eulerAngles = new Vector3(55.26f, go.transform.eulerAngles.y, go.transform.eulerAngles.z);
                go.AddComponent<AxieController>();

                AxieController controller = go.GetComponent<AxieController>();
                controller.axieBehavior = go.AddComponent<AxieBehavior>();
                controller.AxieId = int.Parse(axieId);
                controller.axieIngameStats = new IngameStats();
                controller.axieIngameStats.axieId = axieId;
                controller.axieIngameStats.maxHP = stats.hp * 2;
                controller.axieIngameStats.axieClass = @class;
                controller.imGood = !isEnemy;
                controller.stats = stats;
                controller.axieIngameStats.MinEnergy = 0;
                controller.axieIngameStats.CurrentEnergy = AxieStatCalculator.GetAxieMinEnergy(controller.stats);

                controller.axieIngameStats.currentHP = stats.skill;
                controller.Genes = genes;
                controller.axieBodyParts = isEnemy
                    ? axieEnemies.Single(x => x.id == axieId).parts
                        .Where(x => x.BodyPart != BodyPart.Ears && x.BodyPart != BodyPart.Eyes)
                        .Select(x => x.SkillName).ToList()
                    : AccountManager.userAxies.results.Single(x => x.id == axieId).parts
                        .Where(x => x.BodyPart != BodyPart.Ears && x.BodyPart != BodyPart.Eyes)
                        .Select(x => x.SkillName).ToList();


                controller.axieSkillController = controller.gameObject.AddComponent<AxieSkillController>();

                SkeletonAnimation runtimeSkeletonAnimation =
        SkeletonAnimation.NewSkeletonAnimationGameObject(builderResult.skeletonDataAsset);

                controller.statsManagerUI =
        Instantiate(isEnemy ? badTeamHP : goodTeamHP, runtimeSkeletonAnimation.transform)
            .GetComponent<StatsManager>();

                if (isEnemy)
                {
                    controller.startingCol =
                         axieForBackend.position_values_per_round[RunManagerSingleton.instance.score].col;
                    controller.startingRow =
                        axieForBackend.position_values_per_round[RunManagerSingleton.instance.score].row;
                    enemyTeam.AddCharacter(controller);

                    List<AxieBodyPart> skillsSelected = skillList.axieBodyParts
                        .Where(x => axieForBackend.combos_values_per_round[RunManagerSingleton.instance.score]
                            .combos_id.Select(x => (SkillName)x).Contains(x.skillName))
                        .ToList();

                    controller.axieSkillController.SetAxieSkills(skillsSelected.Select(x => x.skillName).ToList(),
                        skillsSelected.Select(x => x.bodyPart).ToList());
                }
                else
                {
                    Position position =
                        TeamManager.instance.currentTeam.position[
                            TeamManager.instance.currentTeam.AxieIds.FindIndex(x => x.id == axieId)];

                    goodTeam.AddCharacter(controller,
                        new Vector2Int(position.row,
                            position.col));
                }

                go.AddComponent<BoxCollider>().isTrigger = true;
                go.GetComponent<BoxCollider>().size = new Vector3(5, 3.7f, 1);
                go.GetComponent<BoxCollider>().center = new Vector3(0, 1.25f, 0);
                controller.axieIngameStats.MaxEnergy = controller.axieSkillController.GetComboCost();
                go.tag = "Character";

                controller.skeletonDataAsset = builderResult.skeletonDataAsset;
                controller.skeletonMaterial = builderResult.sharedGraphicMaterial;

                runtimeSkeletonAnimation.transform.SetParent(go.transform, false);

                runtimeSkeletonAnimation.state.SetAnimation(0, "action/idle/normal", true);

                controller.SkeletonAnim = runtimeSkeletonAnimation;

                controller.axieSkillEffectManager =
                    Instantiate(axieSkillEffectManager, runtimeSkeletonAnimation.transform)
                        .GetComponent<AxieSkillEffectManager>();
                controller.statsManagerUI.SetSR(axieClassObjects.FirstOrDefault(x => x.axieClass == @class)?.classSprite);
                controller.transform.parent = AxiesParent;

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
                Debug.LogError("ERROR CREATING AXIE: " + ex.Message + "AXIE: " + axieId);
                return null;
            }
        }

        private void SpawnSkeletonGraphic(Axie2dBuilderResult builderResult)
        {
            var skeletonGraphic = SkeletonGraphic.NewSkeletonGraphicGameObject(builderResult.skeletonDataAsset, rootTF,
                builderResult.sharedGraphicMaterial);
            skeletonGraphic.Initialize(true);
            skeletonGraphic.AnimationState.SetAnimation(0, "action/idle/normal", true);
        }
    }
}