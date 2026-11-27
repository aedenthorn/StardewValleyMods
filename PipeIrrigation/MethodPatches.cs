using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Network;
using System.Collections.Generic;

namespace PipeIrrigation
{
    public partial class ModEntry
    {
        public static void FarmAnimal_dayUpdate_Postfix(FarmAnimal __instance, GameLocation environtment)
        {
            if (!Config.EnableMod)
                return;

        }
    }
}