using HarmonyLib;
using StardewValley;
using Object = StardewValley.Object;

namespace Bork
{
    public partial class ModEntry
    {

        [HarmonyPatch(typeof(Object), "ShowFarmComputerReport")]
        public class Object_ShowFarmComputerReport_Patch
        {
            public static bool Prefix(Farmer who)
            {
                if (!Config.ModEnabled)
                {
                    return true;
                }
                Game1.activeClickableMenu = new BorkMenu(GetDialogue());
                return false;
            }

        }
    }
}