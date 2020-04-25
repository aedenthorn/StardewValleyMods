using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomMonsterFloors
{
    public class ModConfig 
    {
        public int PercentChanceMonsterFloor { get; set; } = 20;
        public string SlimeDinoMonsterSplitPercents { get; set; } = "33:33:34";
        public string SlimeMonsterSplitPercents { get; set; } = "50:50";
        public int MinFloorsBetweenMonsterFloors { get; set; } = 4;
        public int MonsterMultiplierOnDinoFloors { get; set; } = 1;
        public int MonsterMultiplierOnSlimeFloors { get; set; } = 1;
        public int MonsterMultiplierOnMonsterFloors { get; set; } = 1;
        public int MonsterMultiplierOnRegularFloors { get; set; } = 1;
        public int ItemMultiplierOnDinoFloors { get; set; } = 1;
        public int ItemMultiplierOnSlimeFloors { get; set; } = 1;
        public int ItemMultiplierOnMonsterFloors { get; set; } = 1;
        public int ItemMultiplierOnRegularFloors { get; set; } = 1;
        public int StoneMultiplierOnRegularFloors { get; set; } = 1;
        public int GemstoneMultiplierOnRegularFloors { get; set; } = 1;
        public int TreasureChestFloorMultiplier { get; set; } = 1;
        public int MinFloorsBetweenTreasureFloors { get; set; } = 4;
        public int PurpleStoneMultiplier { get; set; } = 1;
        public int MysticStoneMultiplier { get; set; } = 1;
        public int ChanceForOreMultiplier { get; set; } = 1;
        public int ChanceForIridiumMultiplier { get; set; } = 1;
        public int ChanceForGoldMultiplier { get; set; } = 1;
        public int ChanceForIronMultiplier { get; set; } = 1;
        public double ChanceForLadderInStoneMultiplier { get; set; } = 1;
        public double ChanceLadderIsShaftMultiplier { get; set; } = 1;
    }
}
