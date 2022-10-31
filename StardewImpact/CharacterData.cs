using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System;
using System.Collections.Generic;

namespace StardewImpact
{
    public class CharacterData : ICharacterData
    {
        public string name;
        public Color skillColor = Color.White;
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
        public Color SkillColor { get => skillColor; set => skillColor = value; }
        public float CurrentEnergy { get => currentEnergy; set => currentEnergy = value; }
        public float EnergyPerSkill { get => energyPerSkill; set => energyPerSkill = value; }
        public float EnergyPerHit { get => energyPerHit; set => energyPerHit = value; }
        public float BurstEnergyCost { get => burstEnergyCost; set => burstEnergyCost = value; }
        public float SkillCooldown { get => skillCooldown; set => skillCooldown = value; }
        public float BurstCooldown { get => burstCooldown; set => burstCooldown = value; }
        public float SkillCooldownValue { get => skillCooldownValue; set => skillCooldownValue = value; }
        public float BurstCooldownValue { get => burstCooldownValue; set => burstCooldownValue = value; }
        public string PortraitPath { get => portraitPath; set => portraitPath = value; }
        public Texture2D Portrait { get => portrait; set => portrait = value; }
        public string SpritePath { get => spritePath; set => spritePath = value; }
        public Texture2D Sprite { get => sprite; set => sprite = value; }
        public string SkillIconPath { get => skillIconPath; set => skillIconPath = value; }
        public Texture2D SkillIcon { get => skillIcon; set => skillIcon = value; }
        public string BurstIconPath { get => burstIconPath; set => burstIconPath = value; }
        public Texture2D BurstIcon { get => burstIcon; set => burstIcon = value; }
        public List<Action<string, Farmer>> SkillEvent { get => skillEvent; set => skillEvent = value; }
        public List<Action<string, Farmer>> BurstEvent { get => burstEvent; set => burstEvent = value; }
    }
}