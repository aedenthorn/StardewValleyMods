using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
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
                speed -= speedMod;
            }
            return speed;
        }

	}
}