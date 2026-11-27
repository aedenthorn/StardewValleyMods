using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomMonsterFloors
{
    public class ModConfig 
    {
        public bool EnableMod { get; set; } = true;
        public bool EnableFloorTypeChanges { get; set; } = true;
        public bool EnableTileChanges { get; set; } = true;
        public int PercentChanceMonsterFloor { get; set; } = 20;
        public string SlimeDinoMonsterSplitPercents { get; set; } = "33:33:34";
        public string SlimeMonsterSplitPercents { get; set; } = "50:50";
        public int MinFloorsBetweenMonsterFloors { get; set; } = 4;
        public float TreasureChestFloorMultiplier { get; set; } = 1f;
        public int MinFloorsBetweenTreasureFloors { get; set; } = 4;
        public float MonsterMultiplierOnDinoFloors { get; set; } = 1f;
        public float MonsterMultiplierOnSlimeFloors { get; set; } = 1f;
        public float MonsterMultiplierOnMonsterFloors { get; set; } = 1f;
        public float MonsterMultiplierOnRegularFloors { get; set; } = 1f;
        public float ItemMultiplierOnDinoFloors { get; set; } = 1f;
        public float ItemMultiplierOnSlimeFloors { get; set; } = 1f;
        public float ItemMultiplierOnMonsterFloors { get; set; } = 1f;
        public float ItemMultiplierOnRegularFloors { get; set; } = 1f;
        public float StoneMultiplierOnRegularFloors { get; set; } = 1f;
        public float GemstoneMultiplierOnRegularFloors { get; set; } = 1f;
        public float ChanceForOresMultiplierInMines { get; set; } = 1f;
        public float ChanceForOreMultiplier { get; set; } = 1f;
        public float ChanceForIridiumMultiplier { get; set; } = 1f;
        public float ChanceForGoldMultiplier { get; set; } = 1f;
        public float ChanceForIronMultiplier { get; set; } = 1f;
        public float PurpleStoneMultiplier { get; set; } = 1f;
        public float MysticStoneMultiplier { get; set; } = 1f;
        public double ResourceClumpChance { get; set; } = 0.005;
        public double WeedsChance { get; set; } = 0.1;
        public double WeedsMultiplier { get; set; } = 1.0;
        public float ChanceForLadderInStoneMultiplier { get; set; } = 1f;
        public float ChanceLadderIsShaftMultiplier { get; set; } = 1f;
    }
}
