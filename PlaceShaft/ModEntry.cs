using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.BigCraftables;
using StardewValley.GameData.Objects;
using StardewValley.Locations;
using StardewValley.Network;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using xTile;

namespace PlaceShaft
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {

        public static ModEntry context;

        public static ModConfig Config;
        public static string itemName;


        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            itemName = ModManifest.UniqueID + "_MineShaft";
            context = this;
            Config = Helper.ReadConfig<ModConfig>();
            helper.Events.Content.AssetRequested += Content_AssetRequested;

            Harmony harmony = new(ModManifest.UniqueID);

            harmony.Patch(
               original: AccessTools.Method(typeof(StardewValley.Object), nameof(StardewValley.Object.placementAction)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.placementAction_prefix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(MineShaft), nameof(MineShaft.enterMineShaft)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.enterMineShaft_prefix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(MineShaft), nameof(MineShaft.checkAction)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.MineShaft_checkAction_prefix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.createQuestionDialogue),new Type[] { typeof(string),typeof (Response[]), typeof(string) }),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.createQuestionDialogue_prefix))
            );
        }
        private void Content_AssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("Data/BigCraftables"))
            {
                e.Edit(asset =>
                {
                    IDictionary<string, BigCraftableData> data = asset.AsDictionary<string, BigCraftableData>().Data;

                    data[itemName] = new BigCraftableData()
                    {
                        Name = itemName,
                        DisplayName = context.Helper.Translation.Get("DisplayName"),
                        Description = context.Helper.Translation.Get("Description"),
                        Price = 1000,
                        Fragility = 2,
                        CanBePlacedOutdoors = false,
                        CanBePlacedIndoors = true,
                        Texture = context.Helper.ModContent.GetInternalAssetName("assets/sprite").Name,
                        SpriteIndex = 0
                    };
                });
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Data/CraftingRecipes"))
            {
                e.Edit(asset =>
                {
                    IDictionary<string, string> data = asset.AsDictionary<string, string>().Data;
                    data[itemName] = $"{Config.ShaftCost}/Home/{itemName}/true/{(string.IsNullOrEmpty(Config.SkillReq) ? "null" : $"s {Config.SkillReq}")}/";
                });
            }
            else if (e.NameWithoutLocale.StartsWith("Maps/Mines/mine") && !e.NameWithoutLocale.StartsWith("Maps/Mines/mine_desert"))
            {
                e.Edit(asset =>
                {
                    var editor = asset.AsImage();
                    IRawTextureData sourceImage = this.Helper.ModContent.Load<IRawTextureData>("assets/sprite.png");
                    editor.PatchImage(sourceImage, sourceArea: new Rectangle(0, 16, 16, 16), targetArea: new Rectangle(224, 160, 16, 16));
                });
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
        
        private static void MineShaft_checkAction_prefix(MineShaft __instance, xTile.Dimensions.Location tileLocation)
        {
            int tileIndexAt = __instance.getTileIndexAt(tileLocation, "Buildings", "mine");

            if (tileIndexAt == 174)
            {
                playerLocation = Game1.player.position.Value;
                jumpLocation = new Vector2(tileLocation.X * 64, tileLocation.Y * 64);
            }
        }

        private static bool enterMineShaft_prefix(MineShaft __instance, ref bool ___isFallingDownShaft, ref int ___lastLevelsDownFallen, int ___deepestLevelOnCurrentDesertFestivalRun)
        {
            ticks = 0;
            lastYJumpVelocity = 0;
            context.Helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;

            if (Config.PercentDamage == 100 && Config.MaxLevels == 9 && Config.MinLevels == 3 && Config.PreventGoingToSkullCave)
                return true;
            DelayedAction.playSoundAfterDelay("fallDown", 800, __instance, null, -1, false);
            DelayedAction.playSoundAfterDelay("clubSmash", 1800, null, null, -1, false);
            Random random = Utility.CreateRandom((double)__instance.mineLevel, Game1.uniqueIDForThisGame, (double)Game1.Date.TotalDays, 0.0, 0.0);
            int levelsDown = random.Next(Config.MinLevels, Config.MaxLevels);
            if (random.NextDouble() < 0.1)
            {
                levelsDown = levelsDown * 2 - 1;
            }
            if (Config.PreventGoingToSkullCave && __instance.mineLevel < 220 && __instance.mineLevel + levelsDown > 220)
            {
                levelsDown = 220 - __instance.mineLevel;
            }
            ___lastLevelsDownFallen = levelsDown;
            Game1.player.health = Math.Max(1, Game1.player.health - levelsDown * (int)Math.Round(3f * Config.PercentDamage / 100f));
            ___isFallingDownShaft = true;
            Game1.globalFadeToBlack(new Game1.afterFadeFunction(afterFall), 0.045f);
            Game1.player.CanMove = false;
            Game1.player.jump();

            Game1.player.temporarilyInvincible = true;
            Game1.player.temporaryInvincibilityTimer = 0;
            Game1.player.flashDuringThisTemporaryInvincibility = false;
            Game1.player.currentTemporaryInvincibilityDuration = 700;
            if (Utility.GetDayOfPassiveFestival("DesertFestival") > 0 && Game1.IsMasterGame && ___lastLevelsDownFallen + __instance.mineLevel > ___deepestLevelOnCurrentDesertFestivalRun && __instance.isFallingDownShaft && (___lastLevelsDownFallen + __instance.mineLevel) / 5 > __instance.mineLevel / 5)
            {
                Game1.player.team.calicoEggSkullCavernRating.Value += (___lastLevelsDownFallen + __instance.mineLevel) / 5 - __instance.mineLevel / 5;
            }

            return false;
        }

        public static void GameLoop_UpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (Game1.player.yJumpVelocity == 0f && lastYJumpVelocity < 0f)
            {
                context.Helper.Events.GameLoop.UpdateTicked -= GameLoop_UpdateTicked;
                return;
            }
            ticks++;
            context.Monitor.Log($"{ticks}: {playerLocation}, {jumpLocation}, {Game1.player.position.Value}");
            Game1.player.position.Value = Vector2.Lerp(playerLocation, jumpLocation, 1f / 30f * ticks);
            lastYJumpVelocity = Game1.player.yJumpVelocity;
        }

        private static int mineLevel;
        private static int levelsFallen;
        private static Vector2 playerLocation;
        private static Vector2 jumpLocation;
        private static float lastYJumpVelocity;
        public static int ticks;

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
            if (__instance.name != itemName)
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
                if (!location.IsTileOccupiedBy(currentPoint) && location.isTileOnClearAndSolidGround(currentPoint) && location.isTileOccupiedByFarmer(currentPoint) == null && location.doesTileHaveProperty((int)currentPoint.X, (int)currentPoint.Y, "Type", "Back") != null && location.doesTileHaveProperty((int)currentPoint.X, (int)currentPoint.Y, "Type", "Back").Equals("Stone"))
                {
                    location.playSound("hoeHit");
                    location.createLadderDown((int)currentPoint.X, (int)currentPoint.Y, true);
                    location.updateMap();
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