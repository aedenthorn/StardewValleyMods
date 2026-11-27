using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.GameData.Objects;
using StardewValley.Locations;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using static StardewValley.Minigames.MineCart.Whale;
using Object = StardewValley.Object;

namespace SittingAction
{
    public partial class ModEntry
    {
        //[HarmonyPatch(typeof(Game1), nameof(Game1.pressActionButton))]
        public static class Game1_pressActionButton_Patch
        {
            public static bool Prefix(ref bool __result, KeyboardState currentKBState, MouseState currentMouseState, GamePadState currentPadState)
            {

                if (!Config.ModEnabled || __result || !Game1.player.IsSitting())
                    return true;
                if (!Game1.player.isEating && Game1.player.ActiveObject != null && !Game1.dialogueUp && !Game1.eventUp && !Game1.player.canOnlyWalk && !Game1.player.FarmerSprite.PauseForSingleAnimation && !Game1.fadeToBlack && Game1.player.ActiveObject.Edibility != -300 && Game1.didPlayerJustRightClick(true))
                {
                    if (Game1.player.team.SpecialOrderRuleActive("SC_NO_FOOD", null))
                    {
                        MineShaft mineShaft = Game1.player.currentLocation as MineShaft;
                        if (mineShaft != null && mineShaft.getMineArea(-1) == 121)
                        {
                            Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13053"), 3));
                            __result = false;
                            return false;
                        }
                    }
                    if (Game1.player.hasBuff("25") && Game1.player.ActiveObject != null && !Game1.player.ActiveObject.HasContextTag("ginger_item"))
                    {
                        Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Nauseous_CantEat"), 3));
                        __result = false;
                        return false;
                    }
                    Game1.player.itemToEat = Game1.player.ActiveObject;
                    Game1.player.mostRecentlyGrabbedItem = Game1.player.ActiveObject;
                    ObjectData objectData;
                    if (Game1.objectData.TryGetValue(Game1.player.ActiveObject.ItemId, out objectData))
                    {
                        Game1.currentLocation.createQuestionDialogue((objectData.IsDrink && Game1.player.ActiveObject.preserve.Value.GetValueOrDefault() != Object.PreserveType.Pickle) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3159", Game1.player.ActiveObject.DisplayName) : Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3160", Game1.player.ActiveObject.DisplayName), Game1.currentLocation.createYesNoResponses(), "Eat");
                    }
                    Game1.oldKBState = currentKBState;
                    Game1.oldMouseState = Game1.input.GetMouseState();
                    Game1.oldPadState = currentPadState;
                    __result = false;
                    return false;
                }
                return true;
            }
        }
        //[HarmonyPatch(typeof(Farmer), nameof(Farmer.eatHeldObject))]
        public static class Farmer_eatHeldObject_Patch
        {
            public static void Postfix(Farmer __instance)
            {
                if (Config.ModEnabled && __instance.IsSitting())
                {
                    __instance.isEating = false;
                    __instance.FarmerSprite.ClearAnimation();
                }
            }
        }
        [HarmonyPatch(typeof(Utility), nameof(Utility.withinRadiusOfPlayer))]
        public static class Utility_withinRadiusOfPlayer_Patch
        {
            public static void Prefix(ref int tileRadius, Farmer f)
            {
                if (Config.ModEnabled && f.IsSitting())
                {
                    tileRadius = Config.MaxRadius;
                }

            }
        }
        [HarmonyPatch(typeof(Utility), nameof(Utility.tileWithinRadiusOfPlayer))]
        public static class Utility_tileWithinRadiusOfPlayer_Patch
        {
            public static void Prefix(ref int tileRadius, Farmer f)
            {
                if (Config.ModEnabled && f.IsSitting())
                {
                    tileRadius = Config.MaxRadius;
                }

            }
        }
        [HarmonyPatch(typeof(Character), nameof(Character.GetGrabTile))]
        public static class Character_GetGrabTile_Patch
        {
            public static bool Prefix(Character __instance, ref Vector2 __result)
            {
                if (!Config.ModEnabled || __instance is not Farmer f || f != Game1.player || !f.IsSitting())
                    return true;
                var x = Environment.StackTrace;
                var tile = new Vector2((float)(Game1.getOldMouseX() + Game1.viewport.X), (float)(Game1.getOldMouseY() + Game1.viewport.Y)) / 64f;
                var distance = Vector2.Distance(Game1.player.Tile, tile);
                if (distance > Config.MaxRadius)
                {
                    tile = Vector2.Lerp(Game1.player.Tile, tile, Config.MaxRadius / distance);
                }
                __result = tile;
                return false;
            }
        }
        [HarmonyPatch(typeof(Farmer), nameof(Farmer.CanEmote))]
        public static class Farmer_CanEmote_Patch
        {
            public static void Prefix()
            {
                if (!Config.ModEnabled)
                    return;
            }
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) // reduce height for fewer weeds
            {
                SMonitor.Log($"Transpiling Farmer.CanEmote");

                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].operand is MethodInfo mi && mi == AccessTools.Method(typeof(Farmer), nameof(Farmer.IsSitting)))
                    {
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.SwitchBool))));
                        i++;
                    }
                    else if (codes[i].operand is MethodInfo mi2 && mi2 == AccessTools.Method(typeof(Farmer), nameof(Farmer.isRidingHorse)))
                    {
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.SwitchBool))));
                        i++;
                    }
                }

                return codes.AsEnumerable();
            }
        }
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.checkAction))]
        public static class GameLocation_checkAction_Patch
        {
            public static void Prefix()
            {
                if (!Config.ModEnabled)
                    return;
            }
            public static void Postfix(Farmer who, ref bool __result)
            {
                if (!Config.ModEnabled || __result || !Game1.player.IsSitting())
                    return;
                Game1.player.StopSitting(true);
            }
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) // reduce height for fewer weeds
            {
                SMonitor.Log($"Transpiling GameLocation.checkAction");

                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].operand is MethodInfo mi && mi == AccessTools.Method(typeof(Farmer), nameof(Farmer.IsSitting)))
                    {
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.SwitchBool))));
                        i++;
                    }
                    else if (codes[i].operand is MethodInfo mi2 && mi2 == AccessTools.Method(typeof(Farmer), nameof(Farmer.isRidingHorse)))
                    {
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.SwitchBool))));
                        i++;
                    }
                }

                return codes.AsEnumerable();
            }
        }
    }
}