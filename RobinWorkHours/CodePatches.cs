using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;

namespace RobinWorkHours
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry
    {
        private static bool isWarping;
        private static bool NPC_updateConstructionAnimation_Prefix(NPC __instance)
        {
            var x = !Config.EnableMod || !__instance.Name.Equals("Robin") || isWarping || (Game1.timeOfDay >= Config.StartTime && Game1.timeOfDay < Config.EndTime);
            isWarping = false;
            return x;
        }
        private static void Farm_resetLocalState_Postfix(Farm __instance)
        {
            if (!Config.EnableMod || !__instance.isThereABuildingUnderConstruction() || __instance.getBuildingUnderConstruction().daysOfConstructionLeft.Value <= 0)
                return;
            __instance.removeTemporarySpritesWithIDLocal(16846f);
            Building b = __instance.getBuildingUnderConstruction();
            __instance.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(399, 262, (b.daysOfConstructionLeft.Value == 1) ? 29 : 9, 43), new Vector2(b.tileX.Value + b.tilesWide.Value / 2, b.tileY.Value + b.tilesHigh.Value / 2) * 64f + new Vector2(-16f, -144f), false, 0f, Color.White)
            {
                id = 16846f,
                scale = 4f,
                interval = 999999f,
                animationLength = 1,
                totalNumberOfLoops = 99999,
                layerDepth = ((b.tileY.Value + b.tilesHigh.Value / 2) * 64 + 32) / 10000f
            });
        }
        private static void GameLocation_isCollidingWithWarp_Postfix(GameLocation __instance, Character character, ref Warp __result)
        {
            if (!Config.EnableMod || character is not NPC || !character.Name.Equals("Robin") || __instance is not BusStop || __result is null || !__result.TargetName.Equals(Game1.getFarm().Name))
                return;
            isWarping = true;
            (character as NPC).clearSchedule();
            (character as NPC).controller = null;
            (character as NPC).Halt();
            SMonitor.Log($"Robin is starting work in the farm at {Game1.timeOfDay}", LogLevel.Debug);
            AccessTools.Method(typeof(NPC), "updateConstructionAnimation").Invoke(character as NPC, new object[0]);

            __result = null;
        }
    }
}