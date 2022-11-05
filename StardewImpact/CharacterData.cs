using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace StardewImpact
{
    public class CharacterData : ICharacterData
    {
        public string name;
        public Color characterColor = Color.White;
        public float currentEnergy;
        public float energyPerSkill;
        public float energyPerHit;
        public float burstEnergyCost;
        public float skillCooldown;
        public float burstCooldown;
        public float skillCooldownValue;
        public float burstCooldownValue;
        public string portraitPath;
        public Texture2D portrait;
        public string spritePath;
        public Texture2D sprite;
        public string skillIconPath;
        public Texture2D skillIcon;
        public string burstIconPath;
        public Texture2D burstIcon;
        public List<Action<string, Farmer>> skillEvent = new List<Action<string, Farmer>>();
        public List<Action<string, Farmer>> burstEvent = new List<Action<string, Farmer>>();

        public CharacterData(string name, Color characterColor, float energyPerSkill, float energyPerHit, float burstEnergyCost, float skillCooldown, float burstCooldown, string portraitPath, string spritePath, string skillIconPath, string burstIconPath, List<Action<string, Farmer>> skillEvent, List<Action<string, Farmer>> burstEvent)
        {
            this.name = name;
            this.characterColor = characterColor;
            this.energyPerSkill = energyPerSkill;
            this.energyPerHit = energyPerHit;
            this.burstEnergyCost = burstEnergyCost;
            this.skillCooldown = skillCooldown;
            this.burstCooldown = burstCooldown;
            this.portraitPath = portraitPath;
            this.spritePath = spritePath;
            this.skillIconPath = skillIconPath;
            this.burstIconPath = burstIconPath;
            if (skillEvent is not null)
                this.skillEvent = skillEvent;
            if(burstEvent is not null)
                this.burstEvent = burstEvent;
        }

        [JsonIgnore]
        public string Name { get => name; set => name = value; }
        [JsonIgnore]
        public Color CharacterColor { get => ModEntry.characterDict[name].characterColor; set => ModEntry.characterDict[name].characterColor = value; }
        [JsonIgnore]
        public float CurrentEnergy { get => ModEntry.characterDict[name].currentEnergy; set => ModEntry.characterDict[name].currentEnergy = value; }
        [JsonIgnore]
        public float EnergyPerSkill { get => ModEntry.characterDict[name].energyPerSkill; set => ModEntry.characterDict[name].energyPerSkill = value; }
        [JsonIgnore]
        public float EnergyPerHit { get => ModEntry.characterDict[name].energyPerHit; set => ModEntry.characterDict[name].energyPerHit = value; }
        [JsonIgnore]
        public float BurstEnergyCost { get => ModEntry.characterDict[name].burstEnergyCost; set => ModEntry.characterDict[name].burstEnergyCost = value; }
        [JsonIgnore]
        public float SkillCooldown { get => ModEntry.characterDict[name].skillCooldown; set => ModEntry.characterDict[name].skillCooldown = value; }
        [JsonIgnore]
        public float BurstCooldown { get => ModEntry.characterDict[name].burstCooldown; set => ModEntry.characterDict[name].burstCooldown = value; }
        [JsonIgnore]
        public float SkillCooldownValue { get => ModEntry.characterDict[name].skillCooldownValue; set => ModEntry.characterDict[name].skillCooldownValue = value; }
        [JsonIgnore]
        public float BurstCooldownValue { get => ModEntry.characterDict[name].burstCooldownValue; set => ModEntry.characterDict[name].burstCooldownValue = value; }
        [JsonIgnore]
        public string PortraitPath { get => ModEntry.characterDict[name].portraitPath; set => ModEntry.characterDict[name].portraitPath = value; }
        [JsonIgnore]
        public Texture2D Portrait { get => ModEntry.characterDict[name].portrait; set => ModEntry.characterDict[name].portrait = value; }
        [JsonIgnore]
        public string SpritePath { get => ModEntry.characterDict[name].spritePath; set => ModEntry.characterDict[name].spritePath = value; }
        [JsonIgnore]
        public Texture2D Sprite { get => ModEntry.characterDict[name].sprite; set => ModEntry.characterDict[name].sprite = value; }
        [JsonIgnore]
        public string SkillIconPath { get => ModEntry.characterDict[name].skillIconPath; set => ModEntry.characterDict[name].skillIconPath = value; }
        [JsonIgnore]
        public Texture2D SkillIcon { get => ModEntry.characterDict[name].skillIcon; set => ModEntry.characterDict[name].skillIcon = value; }
        [JsonIgnore]
        public string BurstIconPath { get => ModEntry.characterDict[name].burstIconPath; set => ModEntry.characterDict[name].burstIconPath = value; }
        [JsonIgnore]
        public Texture2D BurstIcon { get => ModEntry.characterDict[name].burstIcon; set => ModEntry.characterDict[name].burstIcon = value; }
        [JsonIgnore]
        public List<Action<string, Farmer>> SkillEvent { get => ModEntry.characterDict[name].skillEvent; set => ModEntry.characterDict[name].skillEvent = value; }
        [JsonIgnore]
        public List<Action<string, Farmer>> BurstEvent { get => ModEntry.characterDict[name].burstEvent; set => ModEntry.characterDict[name].burstEvent = value; }
    }
}