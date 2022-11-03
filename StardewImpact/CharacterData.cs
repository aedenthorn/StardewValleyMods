using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System;
using System.Collections.Generic;

namespace StardewImpact
{
    public class CharacterData : ICharacterData
    {
        private string name;
        private Color skillColor = Color.White;
        private float currentEnergy;
        private float energyPerSkill;
        private float energyPerHit;
        private float burstEnergyCost;
        private float skillCooldown;
        private float burstCooldown;
        private float skillCooldownValue;
        private float burstCooldownValue;
        private string portraitPath;
        private Texture2D portrait;
        private string spritePath;
        private Texture2D sprite;
        private string skillIconPath;
        private Texture2D skillIcon;
        private string burstIconPath;
        private Texture2D burstIcon;
        private List<Action<string, Farmer>> skillEvent = new List<Action<string, Farmer>>();
        private List<Action<string, Farmer>> burstEvent = new List<Action<string, Farmer>>();

        public CharacterData(string name, Color skillColor, float currentEnergy, float energyPerSkill, float energyPerHit, float burstEnergyCost, float skillCooldown, float burstCooldown, string portraitPath, string spritePath, string skillIconPath, string burstIconPath, List<Action<string, Farmer>> skillEvent, List<Action<string, Farmer>> burstEvent)
        {
            this.name = name;
            this.skillColor = skillColor;
            this.currentEnergy = currentEnergy;
            this.energyPerSkill = energyPerSkill;
            this.energyPerHit = energyPerHit;
            this.burstEnergyCost = burstEnergyCost;
            this.skillCooldown = skillCooldown;
            this.burstCooldown = burstCooldown;
            this.portraitPath = portraitPath;
            this.spritePath = spritePath;
            this.skillIconPath = skillIconPath;
            this.burstIconPath = burstIconPath;
            this.skillEvent = skillEvent;
            this.burstEvent = burstEvent;
        }

        public string Name { get => name; set => name = value; }
        public Color SkillColor { get => ModEntry.characterDict[name].skillColor; set => ModEntry.characterDict[name].skillColor = value; }
        public float CurrentEnergy { get => ModEntry.characterDict[name].currentEnergy; set => ModEntry.characterDict[name].currentEnergy = value; }
        public float EnergyPerSkill { get => ModEntry.characterDict[name].energyPerSkill; set => ModEntry.characterDict[name].energyPerSkill = value; }
        public float EnergyPerHit { get => ModEntry.characterDict[name].energyPerHit; set => ModEntry.characterDict[name].energyPerHit = value; }
        public float BurstEnergyCost { get => ModEntry.characterDict[name].burstEnergyCost; set => ModEntry.characterDict[name].burstEnergyCost = value; }
        public float SkillCooldown { get => ModEntry.characterDict[name].skillCooldown; set => ModEntry.characterDict[name].skillCooldown = value; }
        public float BurstCooldown { get => ModEntry.characterDict[name].burstCooldown; set => ModEntry.characterDict[name].burstCooldown = value; }
        public float SkillCooldownValue { get => ModEntry.characterDict[name].skillCooldownValue; set => ModEntry.characterDict[name].skillCooldownValue = value; }
        public float BurstCooldownValue { get => ModEntry.characterDict[name].burstCooldownValue; set => ModEntry.characterDict[name].burstCooldownValue = value; }
        public string PortraitPath { get => ModEntry.characterDict[name].portraitPath; set => ModEntry.characterDict[name].portraitPath = value; }
        public Texture2D Portrait { get => ModEntry.characterDict[name].portrait; set => ModEntry.characterDict[name].portrait = value; }
        public string SpritePath { get => ModEntry.characterDict[name].spritePath; set => ModEntry.characterDict[name].spritePath = value; }
        public Texture2D Sprite { get => ModEntry.characterDict[name].sprite; set => ModEntry.characterDict[name].sprite = value; }
        public string SkillIconPath { get => ModEntry.characterDict[name].skillIconPath; set => ModEntry.characterDict[name].skillIconPath = value; }
        public Texture2D SkillIcon { get => ModEntry.characterDict[name].skillIcon; set => ModEntry.characterDict[name].skillIcon = value; }
        public string BurstIconPath { get => ModEntry.characterDict[name].burstIconPath; set => ModEntry.characterDict[name].burstIconPath = value; }
        public Texture2D BurstIcon { get => ModEntry.characterDict[name].burstIcon; set => ModEntry.characterDict[name].burstIcon = value; }
        public List<Action<string, Farmer>> SkillEvent { get => ModEntry.characterDict[name].skillEvent; set => ModEntry.characterDict[name].skillEvent = value; }
        public List<Action<string, Farmer>> BurstEvent { get => ModEntry.characterDict[name].burstEvent; set => ModEntry.characterDict[name].burstEvent = value; }
    }
}