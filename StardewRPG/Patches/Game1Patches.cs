using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Menus;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using Object = StardewValley.Object;

namespace StardewRPG
{
    public partial class ModEntry
    {

		private static void Game1_drawHUD_Prefix(ref float[] __state)
        {
            if (!Config.EnableMod)
                return;
			__state = new float[] { Game1.player.health, Game1.player.maxHealth, Game1.player.stamina, Game1.player.MaxStamina };
			float health = 100 * Game1.player.health / Game1.player.maxHealth;
			float stamina = 270 * Game1.player.stamina / Game1.player.MaxStamina;
			Game1.player.health = (int)health;
			Game1.player.maxHealth = 100;
			Game1.player.stamina = stamina;
			Game1.player.MaxStamina = 270;
		}
		
		private static void Game1_drawHUD_Postfix(float[] __state)
        {
            if (!Config.EnableMod)
                return;
			Game1.player.health = (int)__state[0];
			Game1.player.maxHealth = (int)__state[1];
			Game1.player.stamina = __state[2];
			Game1.player.MaxStamina = (int)__state[3];

		}

		private static bool Game1_updatePause_Prefix()
        {
            if (!Config.EnableMod || !Game1.killScreen || !Config.PermaDeath)
                return true;
            return false;
        }
        
    }
}