using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Enchantments;
using StardewValley.Monsters;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AdvancedMeleeFramework
{
    internal static class AMFPatches
    {
        private static ModEntry ctx;

        public static void Initialize(ModEntry context)
        {
            ctx = context;

            Harmony harmony = new(ctx.ModManifest.UniqueID);

            harmony.Patch(
                original: AccessTools.Constructor(typeof(MeleeWeapon), [typeof(string)]),
                postfix: new(typeof(AMFPatches), nameof(MeleeWeapon_Postfix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(MeleeWeapon), "doAnimateSpecialMove"),
                prefix: new(typeof(AMFPatches), nameof(MeleeWeapon_DoAnimateSpecialMove_Prefix))
            );
            harmony.Patch(
                original: AccessTools.Constructor(typeof(MeleeWeapon), []),
                postfix: new(typeof(AMFPatches), nameof(MeleeWeapon_Postfix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(MeleeWeapon), nameof(MeleeWeapon.drawInMenu), [typeof(SpriteBatch), typeof(Vector2), typeof(float), typeof(float), typeof(float), typeof(StackDrawType), typeof(Color), typeof(bool)]),
                prefix: new(typeof(AMFPatches), nameof(MeleeWeapon_DrawInMenu_Prefix)),
                postfix: new(typeof(AMFPatches), nameof(MeleeWeapon_DrawInMenu_Postfix))
            );

            harmony.Patch(
                original: AccessTools.Method(typeof(BaseEnchantment), nameof(BaseEnchantment.OnDealtDamage)),
                prefix: new(typeof(AMFPatches), nameof(BaseEnchantment_OnDealtDamage_Prefix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(BaseEnchantment), nameof(BaseEnchantment.OnMonsterSlay)),
                prefix: new(typeof(AMFPatches), nameof(BaseEnchantment_OnMonsterSlay_Prefix))
            );

            harmony.Patch(
                original: AccessTools.Method(typeof(Farmer), nameof(Farmer.OnItemReceived)),
                prefix: new(typeof(AMFPatches), nameof(Farmer_OnItemReceived_Prefix))
            );
        }

        private static void MeleeWeapon_Postfix(MeleeWeapon __instance) => Utils.AddEnchantments(__instance);

        private static bool MeleeWeapon_DoAnimateSpecialMove_Prefix(MeleeWeapon __instance, Farmer ___lastUser)
        {
            ctx.Monitor.VerboseLog($"Special move for {__instance.Name}, id {__instance.ItemId}");
            if (ctx.Config.RequireModKey && !ctx.Helper.Input.IsDown(ctx.Config.ModKey))
                return true;
            ctx.AdvancedWeaponAnimating.Value = Utils.GetAdvancedMeleeWeapon(__instance, ___lastUser);
            if (ctx.WeaponAnimationFrame.Value > -1 || ctx.AdvancedWeaponAnimating.Value == null || !ctx.AdvancedWeaponAnimating.Value.frames.Any())
                return true;
            if (___lastUser is null || ___lastUser.CurrentTool != __instance)
                return false;
            ctx.Monitor.VerboseLog($"Animating {__instance.Name}");
            if (___lastUser.isEmoteAnimating)
                ___lastUser.EndEmoteAnimation();
            ctx.WeaponStartFacingDirection.Value = ___lastUser.FacingDirection;
            ctx.WeaponAnimationFrame.Value = 0;
            ctx.WeaponAnimating.Value = __instance;
            return false;
        }

        private static void MeleeWeapon_DrawInMenu_Prefix(MeleeWeapon __instance, ref int __state)
        {
            __state = 0;
            switch (__instance.type.Value)
            {
                case 0:
                case 3:
                    if (MeleeWeapon.defenseCooldown > 1500)
                    {
                        __state = MeleeWeapon.defenseCooldown;
                        MeleeWeapon.defenseCooldown = 1500;
                    }
                    break;
                case 1:
                    if (MeleeWeapon.daggerCooldown > 3000)
                    {
                        __state = MeleeWeapon.daggerCooldown;
                        MeleeWeapon.daggerCooldown = 3000;
                    }
                    break;
                case 2:
                    if (MeleeWeapon.clubCooldown > 6000)
                    {
                        __state = MeleeWeapon.clubCooldown;
                        MeleeWeapon.clubCooldown = 6000;
                    }
                    break;
            }
        }

        private static void MeleeWeapon_DrawInMenu_Postfix(MeleeWeapon __instance, int __state)
        {
            if (__state == 0)
                return;

            switch (__instance.type.Value)
            {
                case 0:
                case 3:
                    MeleeWeapon.defenseCooldown = __state;
                    break;
                case 1:
                    MeleeWeapon.daggerCooldown = __state;
                    break;
                case 2:
                    MeleeWeapon.clubCooldown = __state;
                    break;
            }
        }

        private static bool BaseEnchantment_OnDealtDamage_Prefix(BaseEnchantment __instance, string ____displayName, Monster monster, GameLocation location, Farmer who, int amount)
        {
            if (__instance is not BaseWeaponEnchantment || who is null || who.CurrentTool is not MeleeWeapon mw || string.IsNullOrEmpty(____displayName) || !ctx.AdvancedEnchantments.TryGetValue(____displayName, out var enchantment) || (ctx.EnchantmentTriggers.TryGetValue(who.UniqueMultiplayerID + ____displayName, out int triggerTicks) && triggerTicks == Game1.ticks))
                return true;
            if (!(enchantment.parameters?.TryGetValue("trigger", out string trigger) ?? false))
                return true;
            if (trigger == "damage" || (trigger == "crit" && amount > mw.maxDamage.Value) && !Environment.StackTrace.Contains("OnCalculateDamage"))
            {
                ctx.Monitor.VerboseLog($"Triggered enchantment {enchantment.name} on {trigger}. {mw.Name} did {amount} damage and has {mw.enchantments.Count} enchantments");
                ctx.EnchantmentTriggers[who.UniqueMultiplayerID + ____displayName] = Game1.ticks;
                if (!ctx.AdvancedEnchantmentCallbacks.TryGetValue(enchantment.type, out var callback))
                {
                    ctx.Monitor.Log($"Triggered enchantment {enchantment.type} could not be found", LogLevel.Error);
                    return false;
                }
                Dictionary<string, string> parameters = new(enchantment.parameters)
                {
                    { "amount", amount.ToString() },
                };

                callback?.Invoke(who, mw, monster, parameters);
            }
            return false;
        }

        private static bool BaseEnchantment_OnMonsterSlay_Prefix(BaseEnchantment __instance, string ____displayName, Monster monster, GameLocation location, Farmer who)
        {
            if (__instance is not BaseWeaponEnchantment || who is null || who.CurrentTool is not MeleeWeapon mw || string.IsNullOrEmpty(____displayName) || !ctx.AdvancedEnchantments.TryGetValue(____displayName, out var enchantment) || (ctx.EnchantmentTriggers.TryGetValue(who.UniqueMultiplayerID + ____displayName, out int triggerTicks) && triggerTicks == Game1.ticks))
                return true;
            if (!(enchantment.parameters?.TryGetValue("trigger", out string trigger) ?? false))
                return true;

            if (trigger == "slay")
            {
                ctx.Monitor.VerboseLog($"Triggered enchantment {enchantment.name} on {trigger}. Slayed {monster.Name} with {mw.Name}, which has {mw.enchantments.Count} enchantments");
                ctx.EnchantmentTriggers[who.UniqueMultiplayerID + ____displayName] = Game1.ticks;
                if (!ctx.AdvancedEnchantmentCallbacks.TryGetValue(enchantment.type, out var callback))
                {
                    ctx.Monitor.Log($"Triggered enchantment {enchantment.type} could not be found", LogLevel.Error);
                    return false;
                }
                Dictionary<string, string> parameters = new(enchantment.parameters)
                {
                    { "amount", monster.MaxHealth.ToString() }
                };
                callback?.Invoke(who, mw, monster, parameters);
            }
            return false;
        }

        private static bool Farmer_OnItemReceived_Prefix(Farmer __instance, Item item)
        {
            if (!__instance.IsLocalPlayer || item?.QualifiedItemId != "(O)GoldCoin" || !item.modData.TryGetValue(ctx.ModManifest.UniqueID + "/moneyAmount", out var amountStr))
                return true;
            Game1.playSound("moneyDial");
            int amount = int.Parse(amountStr);
            __instance.Money += amount;
            __instance.removeItemFromInventory(item);
            Game1.dayTimeMoneyBox.gotGoldCoin(amount);
            return false;
        }
    }
}
