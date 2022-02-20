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
		private static void BobberBar_Postfix(ref int ___bobberBarHeight, ref float ___bobberBarPos)
		{
			if (!Config.EnableMod)
				return;
			___bobberBarHeight += GetStatMod(GetStatValue(Game1.player, "dex", Config.DefaultStatValue)) * Config.DexDaggerSpeedBonus;
			___bobberBarPos = 568 - ___bobberBarHeight;
		}
		private static void BobberBar_update_Postfix(bool ___bobberInBar, bool ___treasure, bool ___treasureCaught, float ___treasurePosition, int ___bobberBarHeight, float ___bobberBarPos, ref float ___treasureCatchLevel, ref float ___distanceFromCatching)
		{
			if (!Config.EnableMod || (!___bobberInBar && (!___treasure || ___treasureCaught)))
				return;
            if (___bobberInBar)
            {
				___distanceFromCatching += GetStatMod(GetStatValue(Game1.player, "str", Config.DefaultStatValue)) * Config.StrFishingReelSpeedBonus;
			}
            else if (___treasurePosition + 12f <= ___bobberBarPos - 32f + ___bobberBarHeight && ___treasurePosition - 16f >= ___bobberBarPos - 32f)
            {
				___treasureCatchLevel += GetStatMod(GetStatValue(Game1.player, "str", Config.DefaultStatValue)) * Config.StrFishingTreasureSpeedBonus;
			}
		}
	}
}