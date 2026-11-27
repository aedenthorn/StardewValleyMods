
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using System.Collections.Generic;

namespace FoodBuffMultiplier
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;
        public float DurationMult { get; set; } = 2f;
        public float HealthMult { get; set; } = 2f;
        public float EnergyMult { get; set; } = 2f;
        public float SkillMult { get; set; } = 2f;
        public float MaxStaminaMult { get; set; } = 2f;
        public float MagneticRadiusMult { get; set; } = 2f;
        public float SpeedMult { get; set; } = 2f;
        public float DefenseMult { get; set; } = 2f;
        public float AttackMult { get; set; } = 2f;
        public float ImmunityMult { get; set; } = 2f;
        public float KnockbackMultiplierMult { get; set; } = 2f;
        public float WeaponSpeedMultiplierMult { get; set; } = 2f;
        public float CriticalChanceMultiplierMult { get; set; } = 2f;
        public float CriticalPowerMultiplierMult { get; set; } = 2f;
        public float WeaponPrecisionMultiplierMult { get; set; } = 2f;
        public List<string> ExcludedFoods { get; set; } = new List<string>();
        public List<string> ExcludedBuffs { get; set; } = new List<string>();
    }
}
