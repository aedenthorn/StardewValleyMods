using StardewValley;
using StardewValley.Tools;
using System;

namespace StardewRPG
{
    public partial class ModEntry
    {

		private static void GameLocation_damageMonster_Prefix(ref int minDamage, ref int maxDamage, bool isBomb, ref int addedPrecision, ref float critChance, ref float critMultiplier, Farmer who)
        {
            if (!Config.EnableMod || who != Game1.player || isBomb)
                return;
            if (who.CurrentTool is not MeleeWeapon)
                return;

            float damageMod = 0;
            var str = GetStatValue(who, "str", Config.BaseStatValue);
            var con = GetStatValue(who, "con", Config.BaseStatValue);
            var dex = GetStatValue(who, "dex", Config.BaseStatValue);
            switch ((who.CurrentTool as MeleeWeapon).type.Value)
            {
                case 1: // dagger
                    damageMod = Config.DexDaggerDamageBonus * GetStatMod(dex);
                    break;
                case 2: // club
                    damageMod = Config.StrClubDamageBonus * GetStatMod(str);
                    break;
                case 3: // sword
                    damageMod = Config.ConSwordDamageBonus * GetStatMod(con);
                    break;
            }
            minDamage = (int)Math.Round(minDamage * (1 + damageMod));
            maxDamage = (int)Math.Round(maxDamage * (1 + damageMod));
            critChance *= 1 + GetStatMod(dex) * Config.DexCritChanceBonus;
            critMultiplier *= 1 + GetStatMod(str) * Config.StrCritDamageBonus;
        }
    }
}