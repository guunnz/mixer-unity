using System.Collections;
using System.Collections.Generic;
using AxieMixer.Unity;
using finished3;
using Newtonsoft.Json.Linq;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Networking;
using CharacterInfo = finished3.CharacterInfo;

namespace Game
{
    public class AxieSpawner : MonoBehaviour
    {
        [SerializeField] RectTransform rootTF; // Assign this in the inspector
        private Axie2dBuilder builder => Mixer.Builder;
        const bool USE_GRAPHIC = false;

        public MouseController overlay;

        private int spawnCountMax = 6;

        private void Start()
        {
            Mixer.Init();
        }

        public void SpawnAxieById(string axieId)
        {
            StartCoroutine(GetAxiesGenesAndSpawn(axieId));
        }

        bool isFetchingGenes = false;

        private IEnumerator GetAxiesGenesAndSpawn(string axieId)
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
                    ProcessMixer(axieId, genesStr, USE_GRAPHIC);
                }
            }

            isFetchingGenes = false;
        }


        private void ProcessMixer(string axieId, string genesStr, bool isGraphic)
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
                SpawnSkeletonAnimation(builderResult, axieId);
            }
        }

        private void SpawnSkeletonAnimation(Axie2dBuilderResult builderResult, string axieId)
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
            go.GetComponent<CharacterInfo>().axieId = axieId;
            overlay.AddCharacter(go.GetComponent<CharacterInfo>());
            SkeletonAnimation runtimeSkeletonAnimation =
                SkeletonAnimation.NewSkeletonAnimationGameObject(builderResult.skeletonDataAsset);
            runtimeSkeletonAnimation.transform.SetParent(go.transform, false);
            runtimeSkeletonAnimation.state.SetAnimation(0, "action/idle/normal", true);
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