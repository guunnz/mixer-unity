using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Game;
using Spine.Unity;

public class GetAxiesExample : MonoBehaviour
{
    private string address = "";
    public AxieSpawner axieSpawner;
    public int spawnCountMax = 0;
    public TeamToJSON teamToJson;


    public void SetAddress(string newAddress)
    {
        address = newAddress;
    }

    [System.Serializable]
    public class Data
    {
        public Axies axies;
        public Lands lands;
    }

    [System.Serializable]
    public class Lands
    {
        public int total;
        public Land[] results;
    }

    [System.Serializable]
    public class Land
    {
        public string landType;
        public string tokenId;
        public string col;
        public string row;
        public bool locked = false;
        public LandType LandTypeEnum => (LandType)Enum.Parse(typeof(LandType), landType, true);
    }

    [System.Serializable]
    public class Axies
    {
        public Axie[] results;
    }

    [System.Serializable]
    public class Axie
    {
        public long birthDate;
        public string name;
        public string genes;
        public string newGenes;
        public string id;
        public string @class;
        public bool f2p;
        public AxieClass axieClass => (AxieClass)Enum.Parse(typeof(AxieClass), @class, true);
        public Part[] parts;

        public Stats stats;
        public string bodyShape;
        internal int maxBodyPartAmount = 2;
        public SkeletonDataAsset skeletonDataAsset;
        public Material skeletonDataAssetMaterial;

        public Axie(Axie axie)
        {
            birthDate = axie.birthDate;
            name = axie.name;
            genes = axie.genes;
            newGenes = axie.newGenes;
            id = axie.id;
            @class = axie.@class;
            parts = axie.parts;

            stats = axie.stats;
            bodyShape = axie.bodyShape;
            skeletonDataAsset = axie.skeletonDataAsset;
            skeletonDataAssetMaterial = axie.skeletonDataAssetMaterial;
            maxBodyPartAmount = 2;
        }

        public Axie()
        {
        }

        public void LoadGraphicAssets()
        {
            try
            {
                Axie axie = AccountManager.userAxies.results.FirstOrDefault(x => x.id == this.id);
                if (axie != null && axie.skeletonDataAsset != null)
                {
                    this.skeletonDataAsset = axie.skeletonDataAsset;
                    this.skeletonDataAssetMaterial = axie.skeletonDataAssetMaterial;
                }
                else
                {
                    AxieSpawner.Instance.ProcessMixer(this);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }

        }
    }

    [System.Serializable]
    public class Part
    {
        public string @class;
        public string id;
        public string name;
        public string type;
        public int order;
        public string abilityName;
        public AxieClass partClass => (AxieClass)Enum.Parse(typeof(AxieClass), @class, true);
        public BodyPart BodyPart => (BodyPart)Enum.Parse(typeof(BodyPart), type, true);
        public SkillName SkillName;
        public bool selected;

        public Part(string @class, string name, string type, int order, bool selected, string abilityId)
        {
            this.@class = @class;
            if (!string.IsNullOrEmpty(name))
            {
                this.name = ProcessDisplaySkillName(name);
                this.abilityName = ProcessSkillName(name);
            }
            this.type = type;
            this.order = order;
            this.selected = selected;

            if (type.ToLower() == "eyes" || type.ToLower() == "ears")
                return;


            if (string.IsNullOrEmpty(name))
            {
                // Fetch the original part name from PartFinder and retry parsing
                string originalPartName = PartFinder.GetPartById(abilityId);
                this.abilityName = ProcessSkillName(originalPartName);
                SkillName = (SkillName)Enum.Parse(typeof(SkillName), this.abilityName, true);
            }
            else
            {

                // Attempt to parse the SkillName
                try
                {
                    SkillName = (SkillName)Enum.Parse(typeof(SkillName), this.abilityName, true);
                }
                catch
                {
                    Debug.Log(name);
                    // Fetch the original part name from PartFinder and retry parsing
                    string originalPartName = PartFinder.GetOriginalPartId(abilityId, name);
                    this.abilityName = ProcessSkillName(originalPartName);
                    SkillName = (SkillName)Enum.Parse(typeof(SkillName), this.abilityName, true);
                }
            }

        }

        private string ProcessSkillName(string partName)
        {
            // Remove the prefix up to and including the first '-'
            int dashIndex = partName.IndexOf('-');
            string processedName = (dashIndex == -1) ? partName : partName.Substring(dashIndex + 1);

            // Remove all remaining '-' and convert to the correct format for enum parsing
            processedName = processedName.Replace("-", "").Replace(" ", "");

            return processedName;
        }

        private string ProcessDisplaySkillName(string partName)
        {
            // Remove the prefix up to and including the first '-'
            int dashIndex = partName.IndexOf('-');
            string processedName = (dashIndex == -1) ? partName : partName.Substring(dashIndex + 1);

            // Replace all remaining '-' with a space
            processedName = processedName.Replace("-", " ");

            // Capitalize the first letter of each word
            processedName =
                System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(processedName.ToLower());

            return processedName;
        }
    }

    [System.Serializable]
    public class Ability
    {
        public int attack;
        public string attackType;
        public string name;
        public string id;
        public string effectIconUrl;
        public int defense;
        public string backgroundUrl;
    }

    [System.Serializable]
    public class Stats
    {
        public int speed;
        public int skill;
        public int morale;
        public int hp;
    }
}