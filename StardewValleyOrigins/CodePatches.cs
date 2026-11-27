using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using xTile.Dimensions;
using xTile.ObjectModel;
using xTile.Tiles;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using StardewValley.Menus;
using Netcode;

namespace StardewValleyOrigins
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(FarmHouse), new Type[] { typeof(string), typeof(string) })]
        [HarmonyPatch(MethodType.Constructor)]
        public class FarmHouse_Patch
        {
            public static void Postfix(FarmHouse __instance)
            {
                if (!Config.ModEnabled || farmHouse)
                    return;
                __instance.furniture.Clear();
            }

        }
        [HarmonyPatch(typeof(Game1), nameof(Game1.addMailForTomorrow))]
        public class Game1_addMailForTomorrow_Patch
        {
            public static bool Prefix(string mailName)
            {
                return !Config.ModEnabled || allowedMail.Contains(mailName);
            }
        }
        [HarmonyPatch(typeof(Game1), nameof(Game1.addMail))]
        public class Game1_addMail_Patch
        {
            public static bool Prefix(string mailName)
            {
                return !Config.ModEnabled || allowedMail.Contains(mailName);
            }
        }
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.UpdateWhenCurrentLocation))]
        public class GameLocation_UpdateWhenCurrentLocation_Patch
        {
            public static void Prefix(GameLocation __instance, GameTime time)
            {
            }
        }
        [HarmonyPatch(typeof(SpecialOrder), nameof(SpecialOrder.IsSpecialOrdersBoardUnlocked))]
        public class SpecialOrder_IsSpecialOrdersBoardUnlocked_Patch
        {
            public static bool Prefix()
            {
                return !Config.ModEnabled || specialOrdersBoard;
            }
        }
        [HarmonyPatch(typeof(Event), nameof(Event.tryToLoadFestival))]
        public class Event_tryToLoadFestival_Patch
        {
            public static bool Prefix(string festival)
            {
                return !Config.ModEnabled || allowedEvents.Contains(festival);
            }
        }
        [HarmonyPatch(typeof(Forest), new Type[] { typeof(string), typeof(string) })]
        [HarmonyPatch(MethodType.Constructor)]
        public class Forest_Patch
        {
            public static void Postfix(Forest __instance)
            {
                if (!Config.ModEnabled)
                    return;
                if (!marniesLivestock)
                {
                    __instance.marniesLivestock.Clear();
                }
            }
        }
        [HarmonyPatch(typeof(Game1), nameof(Game1.CanAcceptDailyQuest))]
        public class Game1_CanAcceptDailyQuest_Patch
        {
            public static bool Prefix()
            {
                return !Config.ModEnabled || townBoard;
            }
        }
        [HarmonyPatch(typeof(Town), "resetLocalState")]
        public class Town_resetLocalState_Patch
        {
            public static void Postfix(Town __instance, ref TemporaryAnimatedSprite ___minecartSteam, ref bool ___playerCheckedBoard)
            {
                if (!Config.ModEnabled)
                    return;
                if (!minecarts)
                    ___minecartSteam = null;
                if (!townBoard)
                    ___playerCheckedBoard = true;
                if (!blacksmith)
                    AmbientLocationSounds.removeSound(new Vector2(100f, 79f));
            }
        }
        [HarmonyPatch(typeof(BusStop), "resetLocalState")]
        public class BusStop_resetLocalState_Patch
        {
            public static void Postfix(BusStop __instance, ref Vector2 ___busPosition, ref TemporaryAnimatedSprite ___busDoor)
            {
                if (!Config.ModEnabled)
                    return;
                if (!bus)
                {
                    ___busDoor = null;
                    ___busPosition = new Vector2(-10000,-10000);
                }
            }
        }
        [HarmonyPatch(typeof(Farm), nameof(Farm.draw))]
        public class Farm_draw_Patch
        {
            public static void Prefix(ref Rectangle[] __state)
            {
                if (!Config.ModEnabled || farmHouse)
                    return;
                __state = new Rectangle[] {
                    Building.leftShadow,
                    Building.rightShadow,
                    Building.middleShadow
                };
                Building.leftShadow = new Rectangle();
                Building.rightShadow = new Rectangle();
                Building.middleShadow = new Rectangle();

            }
            public static void Postfix(ref Rectangle[] __state)
            {
                if(__state is null)
                    return;
                Building.leftShadow = __state[0];
                Building.rightShadow = __state[1];
                Building.middleShadow = __state[2];

            }
        }
        [HarmonyPatch(typeof(Town), "addClintMachineGraphics")]
        public class Town_addClintMachineGraphics_Patch
        {
            public static bool Prefix()
            {
                return !Config.ModEnabled || blacksmith;
            }
        }
        [HarmonyPatch(typeof(Beach), "draw")]
        public class Beach_Patch
        {
            public static void Prefix(Beach __instance, ref bool __state)
            {
                if (!Config.ModEnabled)
                    return;
                __state = __instance.bridgeFixed.Value;
                __instance.bridgeFixed.Value = true;
            }
            public static void Postfix(Beach __instance, bool __state)
            {
                if (!Config.ModEnabled)
                    return;
                __instance.bridgeFixed.Value = __state;
            }
        }
        [HarmonyPatch(typeof(Beach), nameof(Beach.fixBridge))]
        public class Beach_fixBridge_Patch
        {
            public static bool Prefix()
            {
                return !Config.ModEnabled;

            }
        }
        [HarmonyPatch(typeof(MapPage), new Type[] { typeof(int), typeof(int), typeof(int), typeof(int) })]
        [HarmonyPatch(MethodType.Constructor)]
        public class MapPage_Patch
        {
            public static void Postfix(MapPage __instance)
            {
                if (!Config.ModEnabled)
                    return;

                for(int i = __instance.points.Count - 1; i >= 0; i--)
                {
                    if (!allowedMapPoints.Contains(__instance.points[i].myID))
                        __instance.points.RemoveAt(i);
                }
            }
        }
        [HarmonyPatch(typeof(Mountain), "resetSharedState")]
        public class Mountain_resetSharedState_Patch
        {
            public static void Postfix(Mountain __instance, NetBool ___landslide)
            {
                if (!Config.ModEnabled)
                    return;
                ___landslide.Value = false;
                if (!linusCampfire)
                    __instance.Objects.Remove(new Vector2(29, 9));
            }
        }
        [HarmonyPatch(typeof(Farm), nameof(Farm.AddModularShippingBin))]
        public class Farm_AddModularShippingBin_Patch
        {
            public static bool Prefix()
            {
                return !Config.ModEnabled || shippingBin;
            }
        }
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.GetLocationEvents))]
        public class GameLocation_GetLocationEvents_Patch
        {
            public static void Postfix(GameLocation __instance, Dictionary<string, string> __result)
            {
                if (!Config.ModEnabled)
                    return;
                foreach(var k in __result.Keys.ToArray())
                {
                    if (!allowedEvents.Contains(k))
                    {
                        __result.Remove(k);
                    }
                }
            }
        }
        [HarmonyPatch(typeof(SocialPage), new Type[] { typeof(int), typeof(int), typeof(int), typeof(int) })]
        [HarmonyPatch(MethodType.Constructor)]
        public class SocialPage_Patch
        {
            public static void Postfix()
            {
            }
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling SocialPage.SocialPage");
                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Ldsfld && codes[i].operand is FieldInfo && (FieldInfo)codes[i].operand == AccessTools.Field(typeof(SocialPage), nameof(SocialPage.defaultFriendships)))
                    {
                        SMonitor.Log($"removing default friendships");
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(RemoveDefaultFriendships))));
                    }
                }

                return codes.AsEnumerable();
            }
        }
    }
}