using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System.Collections.Generic;
using System;
using Microsoft.Xna.Framework;

namespace SDIEmily
{
    public interface ICharacterData
    {
        public string Name { get; set; }
        public Color CharacterColor { get; set; }
        public float CurrentEnergy { get; set; }
        public float EnergyPerSkill { get; set; }
        public float EnergyPerHit { get; set; }
        public float BurstEnergyCost { get; set; }
        public float BurstCooldown { get; set; }
        public float SkillCooldown { get; set; }
        public float BurstCooldownValue { get; set; }
        public float SkillCooldownValue { get; set; }
        public string PortraitPath { get; set; }
        public Texture2D Portrait { get; set; }
        public string SpritePath { get; set; }
        public Texture2D Sprite { get; set; }
        public string SkillIconPath { get; set; }
        public Texture2D SkillIcon { get; set; }
        public string BurstIconPath { get; set; }
        public Texture2D BurstIcon { get; set; }
        public List<Action<string, Farmer>> SkillEvent { get; set; }
        public List<Action<string, Farmer>> BurstEvent { get; set; }
    }
}