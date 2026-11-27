using HarmonyLib;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Object = StardewValley.Object;

namespace PrismaticFlowers
{
	public partial class ModEntry
    {

        [HarmonyPatch(typeof(Crop), new Type[] { typeof(string), typeof(int), typeof(int), typeof(GameLocation) })]
        [HarmonyPatch(MethodType.Constructor)]
        public static class Crop_Patch
        {
            public static void Postfix(Crop __instance)
            {
                if (!Config.ModEnabled || !__instance.programColored.Value || Config.Ignore.Contains(__instance.indexOfHarvest.Value))
                    return;
                ParsedItemData data = ItemRegistry.GetData(__instance.indexOfHarvest.Value);
                if (data?.Category != Object.flowersCategory)
                    return;

                if (Game1.random.Next(100) < Config.PrismaticChance)
                {
                    MakePrismatic(__instance);
                }
            }
        }

        [HarmonyPatch(typeof(Crop), nameof(Crop.harvest))]
        public static class Crop_harvest_Patch
        {
            public static bool Prefix(Crop __instance)
            {
                if(!Config.ModEnabled || Game1.player.ActiveObject is not Object obj || obj.QualifiedItemId != "(O)872" || !__instance.programColored.Value) 
                    return true;
                ParsedItemData data = ItemRegistry.GetData(__instance.indexOfHarvest.Value);
                if (data?.Category != Object.flowersCategory)
                    return true;
                if (__instance.modData.ContainsKey(prismaticKey))
                {
                    __instance.modData.Remove(prismaticKey);
                }
                else
                {
                    __instance.modData[prismaticKey] = Game1.random.Next(Utility.PRISMATIC_COLORS.Length) + "";
                }
                Game1.playSound("yoba");
                Game1.player.reduceActiveItemByOne();
                return false;
            }
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling Crop.harvest");
                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Newobj && codes[i].operand is ConstructorInfo ctor && ctor.DeclaringType == typeof(ColoredObject))
                    {
                        SMonitor.Log($"adding check for prismatic color");
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(CheckPrismaticHarvest))));
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0));
                    }
                }

                return codes.AsEnumerable();
            }
        }

        [HarmonyPatch(typeof(Item), nameof(Item.CompareTo))]
        public static class Item_CompareTo_Patch
        {
            public static void Postfix(Item __instance, Item other, ref int __result)
            {
                if (!Config.ModEnabled || __instance is not ColoredObject co || other is not ColoredObject coo || co.Category != Object.flowersCategory || co.ItemId != coo.ItemId || co.Quality != coo.Quality)
                    return;
                bool prismatic1 = __instance.modData.ContainsKey(prismaticKey);
                bool prismatic2 = other.modData.ContainsKey(prismaticKey);
                if (!prismatic1 && !prismatic2)
                    return;
                if (__result == 0 != (prismatic1 == prismatic2))
                    __result = prismatic1.CompareTo(prismatic2);
            }
        }

        [HarmonyPatch(typeof(Item), nameof(Item.canStackWith))]
        public static class Item_canStackWith_Patch
        {
            public static void Postfix(Item __instance, ISalable other, ref bool __result)
            {
                if (!Config.ModEnabled || __instance is not ColoredObject co || other is not ColoredObject coo || co.Category != Object.flowersCategory || co.ItemId != coo.ItemId || co.Quality != coo.Quality)
                    return;
                bool prismatic1 = co.modData.ContainsKey(prismaticKey);
                bool prismatic2 = coo.modData.ContainsKey(prismaticKey);
                if (!prismatic1 && !prismatic2)
                    return;
                if (__result != (prismatic1 == prismatic2))
                    __result = !__result;
            }
        }

        [HarmonyPatch(typeof(Object), nameof(Object.sellToStorePrice))]
        public static class Object_sellToStorePrice_Patch
        {
            public static void Postfix(Object __instance, ref int __result)
            {
                if (!Config.ModEnabled || !__instance.modData.ContainsKey(prismaticKey))
                    return;
                __result = (int)Math.Round(__result * Config.PriceMultiplier);
            }
        }

        public static IEnumerable<CodeInstruction> ColoredObject_Draw_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            SMonitor.Log($"Transpiling ColoredObject.draw");
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Callvirt && codes[i].operand is MethodInfo mi && mi == AccessTools.PropertyGetter(typeof(NetFieldBase<Color, NetColor>), nameof(NetFieldBase<Color, NetColor>.Value)))
                {
                    SMonitor.Log($"adding check for prismatic color");
                    codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(GetPrismaticColorForObject))));
                    codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0));
                }
            }

            return codes.AsEnumerable();
        }

        public static IEnumerable<CodeInstruction> Crop_Draw_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            SMonitor.Log($"Transpiling Crop.draw");
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Callvirt && codes[i].operand is MethodInfo mi && mi == AccessTools.PropertyGetter(typeof(NetFieldBase<Color, NetColor>), nameof(NetFieldBase<Color, NetColor>.Value)))
                {
                    SMonitor.Log($"adding check for prismatic color");
                    codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(GetPrismaticColorForObject))));
                    codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0));
                }
            }

            return codes.AsEnumerable();
        }
    }
}
