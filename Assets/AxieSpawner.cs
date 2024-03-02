using System.Collections;
using System.Collections.Generic;
using AxieMixer.Unity;
using finished3;
using Newtonsoft.Json.Linq;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Networking;
using CharacterInfo = finished3.CharacterInfo;

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
    Dusk
}

namespace Game
{
    public class AxieSpawner : MonoBehaviour
    {
        [SerializeField] RectTransform rootTF; // Assign this in the inspector
        private Axie2dBuilder builder => Mixer.Builder;
        const bool USE_GRAPHIC = false;

        public MyTeam overlay;
        public EnemyTeam enemyOverlay;

        private int spawnCountMax = 6;

        public GameObject goodTeamHP;
        public GameObject badTeamHP;

        private void Start()
        {
            Mixer.Init();
        }

        public void SpawnAxieById(string axieId, BodyPart bodyPart, SkillName skillName, AxieClass @class, GetAxiesExample.Stats stats)
        {
            StartCoroutine(GetAxiesGenesAndSpawn(axieId, bodyPart, skillName, @class, stats));
        }

        bool isFetchingGenes = false;

        private IEnumerator GetAxiesGenesAndSpawn(string axieId, BodyPart bodyPart, SkillName skillName,
            AxieClass @class, GetAxiesExample.Stats stats)
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
                    Debug.Log(genesStr);
                    ProcessMixer(axieId, genesStr, USE_GRAPHIC, bodyPart, skillName, @class,stats);
                }
            }

            isFetchingGenes = false;
        }


        private void ProcessMixer(string axieId, string genesStr, bool isGraphic, BodyPart bodyPart,
            SkillName skillName,
            AxieClass @class,GetAxiesExample.Stats stats)
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
                SpawnSkeletonAnimation(builderResult, axieId, bodyPart,
                    skillName,
                    @class,stats);
            }
        }

        private void SpawnSkeletonAnimation(Axie2dBuilderResult builderResult, string axieId, BodyPart bodyPart,
            SkillName skillName,
            AxieClass @class,GetAxiesExample.Stats stats)
        {
            GameObject go = new GameObject("Axie");
            go.transform.SetParent(rootTF, false);
            go.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
            go.transform.eulerAngles = new Vector3(55.26f, go.transform.eulerAngles.y, go.transform.eulerAngles.z);
            go.AddComponent<CharacterInfo>();
            go.tag = "Character";
            go.AddComponent<BoxCollider>();
            go.GetComponent<BoxCollider>().size = new Vector3(4.5f, 4, 1);
            go.GetComponent<BoxCollider>().center = new Vector3(0, 2, 0);

            CharacterInfo info = go.GetComponent<CharacterInfo>();
            info.axieId = axieId;
            info.skillName = skillName;
            info.axieClass = @class;
            info.bodyPart = bodyPart;
            info.MinManaAux = stats.skill;
            info.Mana = stats.skill;
            Instantiate(badTeamHP, info.SkeletonAnim.transform);
            overlay.AddCharacter(info);
            info.hpManager = Instantiate(goodTeamHP, info.SkeletonAnim.transform).GetComponent<HPManager>();
            SkeletonAnimation runtimeSkeletonAnimation =
                SkeletonAnimation.NewSkeletonAnimationGameObject(builderResult.skeletonDataAsset);
            runtimeSkeletonAnimation.transform.SetParent(go.transform, false);
            runtimeSkeletonAnimation.state.SetAnimation(0, "action/idle/normal", true);
            ///////////////////////////////////////////////////////////////////////////////////////////////////
            GameObject go2 = new GameObject("Axie");
            go2.transform.SetParent(rootTF, false);
            go2.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
            go2.transform.eulerAngles = new Vector3(55.26f, go2.transform.eulerAngles.y, go2.transform.eulerAngles.z);
            go2.AddComponent<CharacterInfo>();
            CharacterInfo info2 = go2.GetComponent<CharacterInfo>();
            info2.hpManager = Instantiate(badTeamHP, info2.SkeletonAnim.transform).GetComponent<HPManager>();
            info2.axieId = axieId;
            info2.skillName = skillName;
            info2.axieClass = @class;
            info2.bodyPart = bodyPart;
            info2.MinManaAux = stats.skill;
            info2.Mana = stats.skill;
            go2.tag = "Character";
            enemyOverlay.AddCharacter(info2);

            SkeletonAnimation runtimeSkeletonAnimation2 =
                SkeletonAnimation.NewSkeletonAnimationGameObject(builderResult.skeletonDataAsset);
            runtimeSkeletonAnimation2.transform.SetParent(go2.transform, false);
            runtimeSkeletonAnimation2.state.SetAnimation(0, "action/idle/normal", true);
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