using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Objects;
using System.Collections.Generic;
using System.Linq;

namespace QualityCondensing
{
    public partial class ModEntry
    {
        public static int RequiredForCondensing(int quality, int target)
        {
            if (target <= quality)
                return -1;
            return quality switch
            {
                0 => target switch
                {
                    1 => Config.ToSilver,
                    2 => Config.ToSilver * Config.ToGold,
                    4 => Config.ToSilver * Config.ToGold * Config.ToIridium,
                    _ => -1,
                },
                1 => target switch
                {
                    2 => Config.ToGold,
                    4 => Config.ToGold * Config.ToIridium,
                    _ => -1,
                },
                2 => target == 4 ? Config.ToIridium : -1,
                _ => -1,
            };
        }
        public static int GetNextQuality(int quality)
        {
            return quality switch
            {
                0 => 1,
                1 => 2,
                2 => 4,
                _ => -1
            };
        }
    }
}