using HarmonyLib;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace SetTreeSeason
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(Tree), "ChooseTexture")]
        public static class Tree_ChooseTexture_Patch
        {
            public static bool Prefix(Tree __instance, ref string __result)
            {
                if(!Config.ModEnabled || !__instance.modData.TryGetValue(seasonKey, out var season))
                    return true;
                var data = __instance.GetData();
                if (data == null)
                    return true;
                foreach(var entry in data.Textures)
                {
                    if(((season == "Spring" && entry.Season == null) || entry.Season.ToString() == season) && (entry.Condition == null || GameStateQuery.CheckConditions(entry.Condition, __instance.Location, null, null, null, null, null)))
                    {
                        __result = entry.Texture;
                        return false;
                    }
                }
                return true;
            }
        }
    }
}