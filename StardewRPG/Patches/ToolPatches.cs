using HarmonyLib;
using StardewValley;
using StardewValley.Projectiles;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace StardewRPG
{
    public partial class ModEntry
    {
        public static IEnumerable<CodeInstruction> MeleeWeapon_setFarmerAnimating_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            SMonitor.Log($"Transpiling MeleeWeapon.setFarmerAnimating");

            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ldc_I4 && (int)codes[i].operand == 400)
                {
                    SMonitor.Log("Overriding base weapon speed");
                    codes[i].opcode = OpCodes.Ldarg_0;
                    codes[i].operand = null;
                    codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_1));
                    codes.Insert(i + 2, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.GetBaseWeaponSpeed))));
                    return codes.AsEnumerable();
                }
            }
            return codes.AsEnumerable();
        }

        private static int GetBaseWeaponSpeed(MeleeWeapon weapon, Farmer who)
        {
            int speed = 400;
            if (Config.EnableMod && who == Game1.player)
            {
                int speedMod = 0;
                var str = GetStatValue(who, "str", Config.BaseStatValue);
                var con = GetStatValue(who, "con", Config.BaseStatValue);
                var dex = GetStatValue(who, "dex", Config.BaseStatValue);
                switch (weapon.type.Value)
                {
                    case 1: // dagger
                        speedMod = Config.DexDaggerSpeedBonus * GetStatMod(dex);
                        break;
                    case 2: // club
                        speedMod = Config.StrClubSpeedBonus * GetStatMod(str);
                        break;
                    case 3: // sword
                        speedMod = Config.ConSwordSpeedBonus * GetStatMod(con);
                        break;
                }
                SMonitor.Log($"Modifying weapon speed {speed} - {speedMod}");
                speed -= speedMod;
            }
            return speed;
        }

        private static void Pickaxe_DoFunction_Prefix(Pickaxe __instance, Farmer who, ref int __state)
        {
            if (!Config.EnableMod || who == null)
                return;
            __state = __instance.additionalPower.Value;
            var power = GetStatMod(GetStatValue(who, "str", Config.BaseStatValue)) * Config.StrPickaxeDamageBonus;
            SMonitor.Log($"Modifying pickaxe power by {power}");
            __instance.additionalPower.Value += power;
        }

        private static void Pickaxe_DoFunction_Postfix(Pickaxe __instance, int __state)
        {
            if (!Config.EnableMod)
                return;
            __instance.additionalPower.Value = __state;
        }

        private static void Axe_DoFunction_Prefix(Axe __instance, Farmer who, ref int __state)
        {
            if (!Config.EnableMod || who == null)
                return;
            __state = __instance.additionalPower.Value;
            var power = GetStatMod(GetStatValue(who, "str", Config.BaseStatValue)) * Config.StrAxeDamageBonus;
            SMonitor.Log($"Modifying axe power by {power}");
            __instance.additionalPower.Value += power;
        }

        private static void Axe_DoFunction_Postfix(Axe __instance, int __state)
        {
            if (!Config.EnableMod)
                return;
            __instance.additionalPower.Value = __state;
        }

        private static void BasicProjectile_Postfix(BasicProjectile __instance, Character firer)
        {
            if (!Config.EnableMod || firer is not Farmer)
                return;
            var dam = GetStatMod(GetStatValue(firer as Farmer, "dex", Config.BaseStatValue)) * Config.DexRangedDamageBonus;
            SMonitor.Log($"Modifying projectile damage by {dam}");
            __instance.damageToFarmer.Value = (int)Math.Max(0, Math.Round(__instance.damageToFarmer.Value * (1 + dam)));
        }
    }
}