using StardewModdingAPI;
using StardewValley;

namespace CustomSpouseRooms
{
    public partial class ModEntry
    {
        public static bool hasWorldStateID_Prefix(ref string id, ref bool __result)
        {
            if (!Config.EnableMod)
                return true;
            if(id == "sebastianFrogReal")
            {
                SMonitor.Log($"Allowing frogs");
                id = "sebastianFrog";
                return true;
            }
            if(id == "sebastianFrog")
            {
                SMonitor.Log($"Preventing frogs");
                __result = false;
                return false;
            }
            return true;
        }
    }
}
