using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlaceShaft
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public string ShaftCost { get; set; } = "390 200";
        public string SkillReq { get; set; } = "Mining 6";
        public int PercentDamage { get; set; } = 100;
        public int MinLevels { get; set; } = 3;
        public int MaxLevels { get; set; } = 9;
        public bool PreventGoingToSkullCave { get; set; } = true;
        public bool SkipConfirmOnShaftJump { get; set; } = false;
    }
}
