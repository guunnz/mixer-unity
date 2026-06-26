using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Game;
using UnityEngine.Serialization;

public class GetMonstersExample : MonoBehaviour
{
    private string address = "";
    public MonsterSpawner monsterSpawner;
    public int spawnCountMax = 0;
    public TeamToJSON teamToJson;


    public void SetAddress(string newAddress)
    {
        address = newAddress;
    }

    [System.Serializable]
    public class Data
    {
        public Monsters monsters;
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
        [FormerlySerializedAs("landType")] public string land_type;
        [FormerlySerializedAs("tokenId")] public string token_id;
        public string col;
        public string row;
        public bool locked = false;
        public LandType LandTypeEnum => (LandType)Enum.Parse(typeof(LandType), land_type, true);
    }

    [System.Serializable]
    public class Monsters
    {
        public Monster[] results;
    }

    [System.Serializable]
    public class Monster
    {
        public long birthDate;
        public string name;
        public string genes;
        public string newGenes;
        public string id;
        public string @class;
        public bool f2p;
        public MonsterClass monsterClass => (MonsterClass)Enum.Parse(typeof(MonsterClass), @class, true);
        public Part[] parts;

        public Stats stats;
        public string bodyShape;
        internal int maxBodyPartAmount = 2;
        public MonsterVisualDescriptor visualDescriptor;

        public Monster(Monster monster)
        {
            birthDate = monster.birthDate;
            name = monster.name;
            genes = monster.genes;
            newGenes = monster.newGenes;
            id = monster.id;
            @class = monster.@class;
            parts = monster.parts;

            stats = monster.stats;
            bodyShape = monster.bodyShape;
            visualDescriptor = monster.visualDescriptor;
            maxBodyPartAmount = 2;
        }

        public Monster()
        {
        }

        public void LoadGraphicAssets()
        {
            try
            {
                Monster monster = AccountManager.userMonsters.results.FirstOrDefault(x => x.id == this.id);
                if (monster != null && monster.visualDescriptor != null)
                {
                    this.visualDescriptor = monster.visualDescriptor;
                }
                else
                {
                    visualDescriptor = MonsterSpawner.Instance.ProcessMixer(this);
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
        public MonsterClass partClass => (MonsterClass)Enum.Parse(typeof(MonsterClass), @class, true);
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
                this.name = ProcessDisplaySkillName(originalPartName);
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
                    string originalPartName = PartFinder.GetOriginalPartId(abilityId, RemoveTrailingDashNumber(name));
                    this.name = ProcessDisplaySkillName(originalPartName);
                    this.abilityName = ProcessSkillName(originalPartName);
                    SkillName = (SkillName)Enum.Parse(typeof(SkillName), this.abilityName, true);
                }
            }

        }
        private string RemoveTrailingDashNumber(string input)
        {
            return System.Text.RegularExpressions.Regex.Replace(input, @"-\d+$", "");
        }

        private string ProcessSkillName(string partName)
        {
            // Remove the prefix up to and including the first '-'
            int dashIndex = partName.IndexOf('-');
            string processedName = (dashIndex == -1) ? partName : partName.Substring(dashIndex + 1);

            // Remove all remaining '-' and spaces
            processedName = processedName.Replace("-", "").Replace(" ", "");

            // Remove any trailing digits
            processedName = System.Text.RegularExpressions.Regex.Replace(processedName, @"\d+$", "");

            return processedName;
        }

        private string ProcessDisplaySkillName(string partName)
        {
            // Remove the prefix up to and including the first '-'
            int dashIndex = partName.IndexOf('-');
            string processedName = (dashIndex == -1) ? partName : partName.Substring(dashIndex + 1);

            // Remove trailing dash-number pattern (e.g., "-2" at the end)
            processedName = System.Text.RegularExpressions.Regex.Replace(processedName, @"-\d+$", "");

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
