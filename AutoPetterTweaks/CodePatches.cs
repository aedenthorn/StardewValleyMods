using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Buildings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace AutoPetterTweaks
{
    public partial class ModEntry
    {
        public static bool allowPet = false;

        [HarmonyPatch(typeof(AnimalHouse), nameof(AnimalHouse.DayUpdate))]
        public class AnimalHouse_DayUpdate_Patch
        {
            public static void Prefix(AnimalHouse __instance)
            {
                if (!Config.ModEnabled || !Config.PetAtEndOfDay)
                    return;
                allowPet = true;
                foreach (var obj in __instance.Objects.Values)
                {
                    if(obj.ParentSheetIndex == 272 || obj.Name == "Auto-Petter")
                    {
                        foreach (var a in __instance.animals.Values)
                        {
                            a.pet(Game1.player, true);
                        }
                        break;
                    }
                }
                allowPet = false;
            }
        }

        [HarmonyPatch(typeof(FarmAnimal), nameof(FarmAnimal.pet))]
        public class FarmAnimal_pet_Patch
        {
            public static bool Prefix(bool is_auto_pet)
            {
                if (!is_auto_pet || !Config.ModEnabled || !Config.PetAtEndOfDay || allowPet)
                    return true;
                return false;
            }
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling FarmAnimal.pet");
                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (i < codes.Count - 1 && codes[i].opcode == OpCodes.Ldc_I4_7 && codes[i + 1].opcode == OpCodes.Stloc_1)
                    {
                        SMonitor.Log($"Setting custom friendship reduction");
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.GetFriendshipReduction))));
                        break;
                    }
                }

                return codes.AsEnumerable();
            }
        }

        private static int GetFriendshipReduction(int reduction)
        {
            if(!Config.ModEnabled)
                return reduction;
            return Config.FriendshipReduction;
        }
    }
}