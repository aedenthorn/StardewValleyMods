using HarmonyLib;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace StardewRPG
{
    public partial class ModEntry
    {

		private static void Game1_drawHUD_Prefix(ref float[] __state)
        {
            if (!Config.EnableMod)
                return;
            Game1.player.maxHealth = (int)Math.Max(1, Math.Round(GetExperienceLevel(Game1.player) * Config.BaseHealthPerLevel * (1 + Config.ConHealthBonus * GetStatMod(GetStatValue(Game1.player, "con", Config.BaseStatValue)))));
            float healthFraction  = Math.Min(1, (float)Game1.player.health / Game1.player.maxHealth);
            Game1.player.health = (int)Math.Round(Game1.player.maxHealth * healthFraction); 
            float health = 100 * healthFraction;
			float stamina = 270 * Game1.player.stamina / Game1.player.MaxStamina;
            __state = new float[] { Game1.player.health, Game1.player.maxHealth, Game1.player.stamina, Game1.player.MaxStamina };
			Game1.player.health = (int)Math.Round(health);
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
		private static void Game1_UpdateOther_Prefix(ref float[] __state)
        {
            if (!Config.EnableMod)
                return;
            Game1.player.maxHealth = (int)Math.Max(1, Math.Round(GetExperienceLevel(Game1.player) * Config.BaseHealthPerLevel * (1 + Config.ConHealthBonus * GetStatMod(GetStatValue(Game1.player, "con", Config.BaseStatValue)))));
            float healthFraction  = Math.Min(1, (float)Game1.player.health / Game1.player.maxHealth);
            Game1.player.health = (int)Math.Round(Game1.player.maxHealth * healthFraction); 
            float health = 100 * healthFraction;
			float stamina = 270 * Game1.player.stamina / Game1.player.MaxStamina;
            __state = new float[] { Game1.player.health, Game1.player.maxHealth, Game1.player.stamina, Game1.player.MaxStamina };
			Game1.player.health = (int)Math.Round(health);
			Game1.player.maxHealth = 100;
			Game1.player.stamina = stamina;
			Game1.player.MaxStamina = 270;
		}

        private static void Game1_UpdateOther_Postfix(float[] __state)
        {
            if (!Config.EnableMod)
                return;
			Game1.player.health = (int)__state[0];
			Game1.player.maxHealth = (int)__state[1];
			Game1.player.stamina = __state[2];
			Game1.player.MaxStamina = (int)__state[3];

		}

        private static bool Game1_updatePause_Prefix(ref bool __state)
        {
            if (!Config.EnableMod || !Game1.killScreen || !Config.PermaDeath)
            {
                __state = Game1.killScreen;
                return true;
            }
            SHelper.Events.Display.RenderedWorld += Display_RenderedWorld;
            return false;
        }
        
		private static void Game1_updatePause_Postfix(bool __state)
        {
            if (!Config.EnableMod || !__state || Config.PermaDeath)
                return;
            if (!Game1.killScreen)
            {
                SMonitor.Log("Kill screen finished");
                var currentExp = GetStatValue(Game1.player, "exp");
                var level = GetExperienceLevel(Game1.player);
                var extraExp = currentExp;
                if (level > 1)
                {
                    extraExp -= GetExperienceLevels()[level - 2];
                }
                var expToLose = extraExp * Config.ExperienceLossPercentOnDeath / 100;

                SetModData(Game1.player, "exp", currentExp - expToLose);
                SMonitor.Log($"Exp lost {expToLose}; remaining {currentExp - expToLose}");
            }
        }
        
    }
}