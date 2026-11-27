using HarmonyLib;
using StardewValley;
using System;

namespace SharedPerfection
{
	public partial class ModEntry
    {

        [HarmonyPatch(typeof(Utility), nameof(Utility.percentGameComplete))]
        public static class Utility_percentGameComplete_Patch
        {
            public static bool Prefix(ref float __result)
            {
                if (!Config.ModEnabled)
                    return true;
                float total = 0f;
                float num = 0f;
                num += GetFarmerItemsShippedPercent() * 15f;
                total += 15f;
                num += Math.Min(Utility.GetObeliskTypesBuilt(), 4f);
                total += 4f;
                num += Game1.IsBuildingConstructed("Gold Clock") ? 10 : 0;
                total += 10f;
                num += (HasCompletedAllMonsterSlayerQuests() ? 10 : 0);
                total += 10f;
                float NPCFriendPercent = GetMaxedFriendshipPercent();
                num += NPCFriendPercent * 11f;
                total += 11f;
                float farmerLevelPercent = Math.Min(MaxFarmerLevel(), 25f) / 25f;
                num += farmerLevelPercent * 5f;
                total += 5f;
                num += (FoundAllStardrops() ? 10 : 0);
                total += 10f;
                num += GetCookedRecipesPercent() * 10f;
                total += 10f;
                num += GetCraftedRecipesPercent() * 10f;
                total += 10f;
                num += GetFishCaughtPercent() * 10f;
                total += 10f;
                float totalNuts = 130f;
                float walnutsFound = Math.Min(Game1.netWorldState.Value.GoldenWalnutsFound, totalNuts);
                num += walnutsFound / totalNuts * 5f;
                total += 5f;
                __result = num / total;
                return false;
            }
        }
    }
}
