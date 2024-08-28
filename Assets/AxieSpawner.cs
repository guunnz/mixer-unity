using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AxieCore.AxieMixer;
using AxieMixer.Unity;
using enemies;
using finished3;
using Newtonsoft.Json.Linq;
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
            GetAxiesExample.Stats stats, AxieForBackend axieForBackend, List<GetAxiesEnemies.AxieEnemy> axieEnemies, GetAxiesEnemies.AxieEnemy axieEnemy,
            bool isOpponent = false)
        {
            StartCoroutine(
                GetAxiesGenesAndSpawn(axieId, @class, stats, axieForBackend, axieEnemies, axieEnemy,
                    isOpponent));
        }

        bool isFetchingGenes = false;

        private IEnumerator GetAxiesGenesAndSpawn(string axieId,
            AxieClass @class, GetAxiesExample.Stats stats, AxieForBackend axieForBackend,
            List<GetAxiesEnemies.AxieEnemy> axieEnemies, GetAxiesEnemies.AxieEnemy enemy, bool isOpponent = false)
        {
            isFetchingGenes = true;
            string searchString = "{ axie (axieId: \"" + axieId + "\") { id, genes, newGenes}}";
            JObject jPayload = new JObject();
            jPayload.Add(new JProperty("query", searchString));

            var wr = new UnityWebRequest("https://graphql-gateway.axieinfinity.com/graphql", "POST");
            //var wr = new UnityWebRequest("https://testnet-graphql.skymavis.one/graphql", "POST");
            byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jPayload.ToString().ToCharArray());
            wr.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
            wr.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            wr.SetRequestHeader("Content-Type", "application/json");
            wr.timeout = 10;
            yield return wr.SendWebRequest();
            if (wr.error == null)
            {
                var result = wr.downloadHandler != null ? wr.downloadHandler.text : null;
                if (!string.IsNullOrEmpty(result))
                {
                    JObject jResult = JObject.Parse(result);
                    string genesStr = (string)jResult["data"]["axie"]["newGenes"];
                    ProcessMixer(axieId, genesStr, USE_GRAPHIC, @class, stats, axieForBackend, axieEnemies, enemy, isOpponent);
                }
            }

            isFetchingGenes = false;
        }




        public void ProcessMixer(string axieId, string genesStr, bool isGraphic,
            AxieClass @class, GetAxiesExample.Stats stats, AxieForBackend axieForBackend,
            List<GetAxiesEnemies.AxieEnemy> axieEnemies, GetAxiesEnemies.AxieEnemy enemy, bool isOpponent = false)
        {
            if (string.IsNullOrEmpty(genesStr))
            {
                Debug.LogError($"[{axieId}] genes not found!!!");
                return;
            }

            float scale = 0.007f;
            var meta = new Dictionary<string, string>();

            if (enemy.cursedMeta.Count > 0)
            {
                var builderResult = builder.BuildSpineFromGeneCursed(axieId, genesStr, enemy.cursedMeta, scale);

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
            else
            {
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
            List<GetAxiesEnemies.AxieEnemy> axieEnemies, bool isOpponent = false, string genes = "")
        {
            GameObject go = new GameObject("Axie");
            return CreateAxie(go, builderResult, axieId, @class, stats, axieForBackend, axieEnemies, isOpponent, genes);
        }


        private AxieController CreateAxie(GameObject go, Axie2dBuilderResult builderResult, string axieId,
            AxieClass @class,
            GetAxiesExample.Stats stats, AxieForBackend axieForBackend, List<GetAxiesEnemies.AxieEnemy> axieEnemies,
            bool isEnemy = false, string genes = "")
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
            controller.axieIngameStats.HP = stats.hp * 2;
            controller.axieIngameStats.axieClass = @class;
            controller.imGood = !isEnemy;
            controller.stats = stats;
            controller.axieIngameStats.MinEnergy = 0;
            controller.axieIngameStats.CurrentEnergy = AxieStatCalculator.GetAxieMinEnergy(controller.stats);

            controller.axieIngameStats.currentHP = stats.skill;
            controller.Genes = genes;
            controller.axieBodyParts = isEnemy
                ? axieEnemies.Single(x => x.id == axieId).Parts
                    .Where(x => x.BodyPart != BodyPart.Ears && x.BodyPart != BodyPart.Eyes)
                    .Select(x => x.SkillName).ToList()
                : AccountManager.userAxies.results.Single(x => x.id == axieId).parts
                    .Where(x => x.BodyPart != BodyPart.Ears && x.BodyPart != BodyPart.Eyes)
                    .Select(x => x.SkillName).ToList();


            controller.axieSkillController = controller.gameObject.AddComponent<AxieSkillController>();

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

            SkeletonAnimation runtimeSkeletonAnimation =
                SkeletonAnimation.NewSkeletonAnimationGameObject(builderResult.skeletonDataAsset);

            runtimeSkeletonAnimation.transform.SetParent(go.transform, false);

            runtimeSkeletonAnimation.state.SetAnimation(0, "action/idle/normal", true);

            controller.SkeletonAnim = runtimeSkeletonAnimation;
            controller.statsManagerUI =
                Instantiate(isEnemy ? badTeamHP : goodTeamHP, runtimeSkeletonAnimation.transform)
                    .GetComponent<StatsManager>();
            controller.axieSkillEffectManager =
                Instantiate(axieSkillEffectManager, runtimeSkeletonAnimation.transform)
                    .GetComponent<AxieSkillEffectManager>();
            controller.statsManagerUI.SetSR(axieClassObjects.FirstOrDefault(x => x.axieClass == @class)?.classSprite);
            controller.transform.parent = AxiesParent;
            return controller;
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