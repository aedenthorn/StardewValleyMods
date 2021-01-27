using Harmony;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Network;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace PlaceShaft
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {

        public static ModEntry context;

        public static ModConfig Config;
        public IJsonAssetsApi JsonAssets { get; private set; }


        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            context = this;
            Config = Helper.ReadConfig<ModConfig>();
            helper.Events.GameLoop.GameLaunched += this.GameLoop_GameLaunched;

            var harmony = HarmonyInstance.Create(this.ModManifest.UniqueID);

            harmony.Patch(
               original: AccessTools.Method(typeof(StardewValley.Object), nameof(StardewValley.Object.placementAction)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.placementAction_prefix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(MineShaft), nameof(MineShaft.enterMineShaft)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.enterMineShaft_prefix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.createQuestionDialogue),new Type[] { typeof(string),typeof (Response[]), typeof(string) }),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.createQuestionDialogue_prefix))
            );
        }
        public void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {
            this.JsonAssets = base.Helper.ModRegistry.GetApi<IJsonAssetsApi>("spacechase0.JsonAssets");
            if (JsonAssets == null)
            {
                Monitor.Log("Can't load Json Assets API for Placeable Mine Shaft", 0);
            }
            else
            {
                JsonAssets.LoadAssets(Path.Combine(Helper.DirectoryPath, "assets/json-assets"));
            }
        }

        private static bool createQuestionDialogue_prefix(GameLocation __instance, string dialogKey)
        {
            if (dialogKey == "Shaft" && Config.SkipConfirmOnShaftJump) 
            {
                (__instance as MineShaft).enterMineShaft();
                return false;
            }
            return true;
        }

        private static bool enterMineShaft_prefix(MineShaft __instance, ref bool ___isFallingDownShaft)
        {
            if (Config.PercentDamage == 100 && Config.PercentLevels == 100 && !Config.PreventGoingToSkullCave)
                return true;

            DelayedAction.playSoundAfterDelay("fallDown", 1200, null, -1);
            DelayedAction.playSoundAfterDelay("clubSmash", 2200, null, -1);
            Random random = new Random(__instance.mineLevel + (int)Game1.uniqueIDForThisGame + Game1.Date.TotalDays);
            int levelsDown = Math.Max(1,random.Next((int)Math.Round(3 * Config.PercentLevels / 100f),(int)Math.Round(9 * Config.PercentLevels / 100f)));
            if (random.NextDouble() < 0.1)
            {
                levelsDown = levelsDown * 2 - 1;
            }
            if (__instance.mineLevel < 220 && __instance.mineLevel + levelsDown > 220)
            {
                levelsDown = 220 - __instance.mineLevel;
            }
            if ( Config.PreventGoingToSkullCave && __instance.mineLevel < 120 && __instance.mineLevel + levelsDown > 120)
            {
                levelsDown = 120 - __instance.mineLevel;
            }

                levelsFallen = levelsDown;
            mineLevel = __instance.mineLevel;

            MethodInfo afterFallInfo = __instance.GetType().GetMethod("afterFall",
            BindingFlags.NonPublic | BindingFlags.Instance);

            Game1.player.health = Math.Max(1, Game1.player.health - levelsDown * (int)Math.Round(3f * Config.PercentDamage/100f));
            ___isFallingDownShaft = true;
            Game1.globalFadeToBlack(new Game1.afterFadeFunction(ModEntry.afterFall), 0.045f);
            Game1.player.CanMove = false;
            Game1.player.jump();

            return false;
        }
        private static int mineLevel;
        private static int levelsFallen;
        private static void afterFall()
        {
            Game1.drawObjectDialogue(Game1.content.LoadString((levelsFallen > 7) ? "Strings\\Locations:Mines_FallenFar" : "Strings\\Locations:Mines_Fallen", levelsFallen));
            Game1.messagePause = true;
            Game1.enterMine(mineLevel + levelsFallen);
            Game1.fadeToBlackAlpha = 1f;
            Game1.player.faceDirection(2);
            Game1.player.showFrame(5, false);
        }

        private static bool placementAction_prefix(StardewValley.Object __instance, ref bool __result, GameLocation location, int x, int y)
        {
            if (__instance.name != "Mine Shaft")
                return true;

            Vector2 placementTile = new Vector2((float)(x / 64), (float)(y / 64));
            if (location is MineShaft)
            {
                if ((location as MineShaft).shouldCreateLadderOnThisLevel() && recursiveTryToCreateLadderDown(location as MineShaft, placementTile, "hoeHit", 16))
                {
                    __result = true;
                    return false;
                }
                Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13053"));
            }
            __result = false;
            return false;
        }

        private static bool recursiveTryToCreateLadderDown(MineShaft location, Vector2 centerTile, string sound, int maxIterations)
        {

            int iterations = 0;
            Queue<Vector2> positionsToCheck = new Queue<Vector2>();
            positionsToCheck.Enqueue(centerTile);
            List<Vector2> closedList = new List<Vector2>();
            while (iterations < maxIterations && positionsToCheck.Count > 0)
            {
                Vector2 currentPoint = positionsToCheck.Dequeue();
                closedList.Add(currentPoint);
                if (!location.isTileOccupied(currentPoint, "ignoreMe", false) && location.isTileOnClearAndSolidGround(currentPoint) && location.isTileOccupiedByFarmer(currentPoint) == null && location.doesTileHaveProperty((int)currentPoint.X, (int)currentPoint.Y, "Type", "Back") != null && location.doesTileHaveProperty((int)currentPoint.X, (int)currentPoint.Y, "Type", "Back").Equals("Stone"))
                {
                    location.playSound("hoeHit", NetAudio.SoundContext.Default);
                    location.createLadderDown((int)currentPoint.X, (int)currentPoint.Y, true);
                    return true;
                }
                foreach (Vector2 v in Utility.DirectionsTileVectors)
                {
                    if (!closedList.Contains(currentPoint + v))
                    {
                        positionsToCheck.Enqueue(currentPoint + v);
                    }
                }
                iterations++;
            }

            return false;
        }
    }

}