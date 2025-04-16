using StardewModdingAPI;
using StardewValley.Tools;
using StardewValley;
using System;
using System.Collections.Generic;
using StardewValley.Monsters;

namespace AdvancedMeleeFramework
{
    public interface IAdvancedMeleeFrameworkApi
    {
        /// <summary>
        /// Add a custom enchantment for contentpacks to use
        /// </summary>
        /// <param name="typeId">The id of the enchantment, used by contentpacks to use the enchantment</param>
        /// <param name="callback">The method to call when the enchantment is triggered</param>
        /// <returns>true if the enchantment was successfully added, otherwise false</returns>
        bool RegisterEnchantment(string typeId, Action<Farmer, MeleeWeapon, Monster?, Dictionary<string, string>> callback);

        /// <summary>
        /// Remove a registered custom enchantment
        /// </summary>
        /// <param name="typeId">The id of the enchantment</param>
        /// <returns>true if the enchantment was successfully removed, otherwise false</returns>
        bool UnRegisterEnchantment(string typeId);

        /// <summary>
        /// Add a custom special effect for contentpacks to use
        /// </summary>
        /// <param name="name">The name of the effect, used by contentpacks to use the effect</param>
        /// <param name="callback">The method to call when the effect is triggered</param>
        /// <returns>true if the effect was successfully added, otherwise false</returns>
        bool RegisterSpecialEffect(string name, Action<Farmer, MeleeWeapon, Dictionary<string, string>> callback);

        /// <summary>
        /// Remove a registered special effect
        /// </summary>
        /// <param name="name">The name of the special effect</param>
        /// <returns>true if the effect was successfully removed, otherwise false</returns>
        bool UnRegisterSpecialEffect(string name);
    }

    public class AdvancedMeleeFrameworkApi : IAdvancedMeleeFrameworkApi
    {
        private ModEntry ctx;
        private IModInfo mod;

        public AdvancedMeleeFrameworkApi(IModInfo accessor, ModEntry context)
        {
            ctx = context;
            mod = accessor;
        }

        public bool RegisterEnchantment(string typeId, Action<Farmer, MeleeWeapon, Monster?, Dictionary<string, string>> callback)
        {
            if (ctx.AdvancedEnchantmentCallbacks.ContainsKey(typeId))
            {
                ctx.Monitor.Log($"{mod.Manifest.Name} tried to add enchantment {typeId} but it already exists");
                return false;
            }
            ctx.AdvancedEnchantmentCallbacks.Add(typeId, callback);
            ctx.Monitor.Log($"{mod.Manifest.Name} added enchantment {typeId}");
            return true;
        }

        public bool RegisterSpecialEffect(string name, Action<Farmer, MeleeWeapon, Dictionary<string, string>> callback)
        {
            if (ctx.SpecialEffectCallbacks.ContainsKey(name))
            {
                ctx.Monitor.Log($"{mod.Manifest.Name} tried to add special effect {name} but it already exists");
                return false;
            }
            ctx.SpecialEffectCallbacks.Add(name, callback);
            ctx.Monitor.Log($"{mod.Manifest.Name} added special effect {name}");
            return true;
        }

        public bool UnRegisterEnchantment(string typeId)
        {
            if (ctx.AdvancedEnchantmentCallbacks.Remove(typeId))
            {
                ctx.Monitor.Log($"{mod.Manifest.Name} removed enchantment {typeId}");
                return true;
            }
            ctx.Monitor.Log($"{mod.Manifest.Name} tried to remove enchantment {typeId} but it didn't exist");
            return false;
        }

        public bool UnRegisterSpecialEffect(string name)
        {
            if (ctx.SpecialEffectCallbacks.Remove(name))
            {
                ctx.Monitor.Log($"{mod.Manifest.Name} removed special effect {name}");
                return true;
            }
            ctx.Monitor.Log($"{mod.Manifest.Name} tried to remove special effect {name} but it didn't exist");
            return false;
        }
    }
}