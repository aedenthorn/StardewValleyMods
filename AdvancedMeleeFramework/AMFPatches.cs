using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Monsters;
using StardewValley.Tools;
using System;
using System.Linq;
using Object = StardewValley.Object;

namespace AdvancedMeleeFramework
{
    public class AMFPatches
    {
        private static ModEntry context;

        public static void Initialize(ModEntry modEntry)
        {
            context = modEntry;
        }
        public static void MeleeWeapon_Postfix(MeleeWeapon __instance)
        {
            //context.Monitor.Log($"0 created melee weapon {__instance.Name} {__instance.InitialParentTileIndex} {__instance.ParentSheetIndex} {Environment.StackTrace}");

            ModEntry.AddEnchantments(__instance);
        }


        public static bool doAnimateSpecialMove_Prefix(MeleeWeapon __instance, Farmer ___lastUser)
        {
            context.Monitor.Log($"Special move for {__instance.Name}, id {__instance.InitialParentTileIndex}");

            if (context.Config.RequireModKey && !context.Helper.Input.IsDown(context.Config.ModKey))
                return true;

            ModEntry.advancedWeaponAnimating = ModEntry.GetAdvancedWeapon(__instance, ___lastUser);

            if (ModEntry.weaponAnimationFrame > -1 || ModEntry.advancedWeaponAnimating == null || !ModEntry.advancedWeaponAnimating.frames.Any())
                return true;

            if (___lastUser == null || ___lastUser.CurrentTool != __instance)
            {
                return false;
            }

            context.Monitor.Log($"Animating {__instance.DisplayName}");

            if (___lastUser.isEmoteAnimating)
            {
                ___lastUser.EndEmoteAnimation();
            }
            ModEntry.weaponStartFacingDirection = ___lastUser.getFacingDirection();
            ModEntry.weaponAnimationFrame = 0;
            ModEntry.weaponAnimating = __instance;
            return false;
        }
        public static void drawInMenu_Prefix(MeleeWeapon __instance, ref int __state)
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
        public static void drawInMenu_Postfix(MeleeWeapon __instance, int __state)
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

        public static bool _OnDealDamage_Prefix(BaseEnchantment __instance, string ____displayName, Monster monster, GameLocation location, Farmer who, ref int amount)
        {
            if (__instance is not BaseWeaponEnchantment || who is null || who.CurrentTool is not MeleeWeapon || ____displayName == null  || ____displayName == "" || !ModEntry.advancedEnchantments.ContainsKey(____displayName) || (ModEntry.EnchantmentTriggers.TryGetValue(who.UniqueMultiplayerID + ____displayName, out int trigger) && trigger == Game1.ticks))
                return true;
            AdvancedEnchantmentData enchantment = ModEntry.advancedEnchantments[____displayName];

            if (enchantment?.parameters?.TryGetValue("trigger", out string triggerString) != true)
                return true;

            if (triggerString == "damage" || (triggerString == "crit" && amount > (who.CurrentTool as MeleeWeapon).maxDamage.Value) && !Environment.StackTrace.Contains("OnCalculateDamage"))
            {
                context.Monitor.Log($"Triggered enchantment {enchantment.name} on {triggerString} {amount} {(who.CurrentTool as MeleeWeapon).enchantments.Count}");
                ModEntry.EnchantmentTriggers[who.UniqueMultiplayerID + ____displayName] = Game1.ticks;
                if (enchantment.type == "heal")
                {
                    if (Game1.random.NextDouble() < float.Parse(enchantment.parameters["chance"]) / 100f)
                    {
                        int heal = Math.Max(1, (int)(amount * float.Parse(enchantment.parameters["amountMult"])));
                        who.health = Math.Min(who.maxHealth, Game1.player.health + heal);
                        location.debris.Add(new Debris(heal, new Vector2((float)Game1.player.getStandingX(), (float)Game1.player.getStandingY()), Color.Lime, 1f, who));
                        if (enchantment.parameters.ContainsKey("sound"))
                            Game1.playSound(enchantment.parameters["sound"]);
                    }
                }
                else if (enchantment.type == "hurt")
                {
                    if (Game1.random.NextDouble() < float.Parse(enchantment.parameters["chance"]) / 100f)
                    {
                        int hurt = Math.Max(1, (int)(amount * float.Parse(enchantment.parameters["amountMult"])));
                        who.takeDamage(hurt, true, null);
                        if (enchantment.parameters.ContainsKey("sound"))
                            Game1.playSound(enchantment.parameters["sound"]);
                    }
                }
                else if (enchantment.type == "coins")
                {
                    if (Game1.random.NextDouble() < float.Parse(enchantment.parameters["chance"]) / 100f)
                    {
                        float mult = float.Parse(enchantment.parameters["amountMult"]);
                        int coins = (int)Math.Round(mult * amount);
                        who.Money += coins;
                        if (enchantment.parameters.ContainsKey("sound"))
                            Game1.playSound(enchantment.parameters["sound"]);
                    }
                } 
            }
            return false;
        }
        public static bool _OnMonsterSlay_Prefix(BaseEnchantment __instance, string ____displayName, Monster m, GameLocation location, Farmer who)
        {
            if (!(__instance is BaseWeaponEnchantment) || ____displayName == null || who is null || !ModEntry.advancedEnchantments.TryGetValue(____displayName, out AdvancedEnchantmentData enchantment) || enchantment is null || (ModEntry.EnchantmentTriggers.ContainsKey(who.UniqueMultiplayerID + ____displayName) && ModEntry.EnchantmentTriggers[who.UniqueMultiplayerID + ____displayName] == Game1.ticks))
                return true;
            
            if (enchantment.parameters?.TryGetValue("trigger", out string trigger) == true && trigger == "slay")
            {
                context.Monitor.Log($"Triggered enchantment {enchantment.name} on slay");
                ModEntry.EnchantmentTriggers[who.UniqueMultiplayerID + ____displayName] = Game1.ticks;
                if (enchantment.type == "heal")
                {
                    if (Game1.random.NextDouble() < float.Parse(enchantment.parameters["chance"]) / 100f)
                    {
                        int heal = Math.Max(1, (int)(m.Health * float.Parse(enchantment.parameters["amountMult"])));
                        who.health = Math.Min(who.maxHealth, Game1.player.health + heal);
                        location.debris.Add(new Debris(heal, new Vector2((float)Game1.player.getStandingX(), (float)Game1.player.getStandingY()), Color.Lime, 1f, who));
                        if (enchantment.parameters.ContainsKey("sound"))
                            Game1.playSound(enchantment.parameters["sound"]);
                    }
                }
                else if (enchantment.type == "hurt")
                {
                    if (Game1.random.NextDouble() < float.Parse(enchantment.parameters["chance"]) / 100f)
                    {
                        int hurt = Math.Max(1, (int)(m.Health * float.Parse(enchantment.parameters["amountMult"])));
                        who.takeDamage(hurt, true, null);
                        if (enchantment.parameters.ContainsKey("sound"))
                            Game1.playSound(enchantment.parameters["sound"]);
                    }
                }
                else if (enchantment.type == "loot")
                {
                    if (Game1.random.NextDouble() < float.Parse(enchantment.parameters["chance"]) / 100f)
                    {
                        if (enchantment.parameters.ContainsKey("extraDropChecks"))
                        {
                            int extraChecks = Math.Max(1, int.Parse(enchantment.parameters["extraDropChecks"]));
                            for (int i = 0; i < extraChecks; i++)
                            {
                                location.monsterDrop(m, m.GetBoundingBox().Center.X, m.GetBoundingBox().Center.Y, who);
                            }
                        }
                        else if (enchantment.parameters.TryGetValue("extraDropItems", out string extra))
                        {
                            string[] items = extra.Split(',');
                            foreach (string item in items)
                            {
                                string[] ic = item.Split('_');
                                if (ic.Length == 1)
                                    Game1.createItemDebris(new Object(int.Parse(item), 1, false, -1, 0), m.Position, Game1.random.Next(4), m.currentLocation, -1);
                                else if (ic.Length == 2)
                                {
                                    float chance = int.Parse(ic[1]) / 100f;
                                    if (Game1.random.NextDouble() < chance)
                                        Game1.createItemDebris(new Object(int.Parse(ic[0]), 1, false, -1, 0), m.Position, Game1.random.Next(4), m.currentLocation, -1);
                                }
                                else if (ic.Length == 4)
                                {
                                    float chance = int.Parse(ic[3]) / 100f;
                                    if (Game1.random.NextDouble() < chance)
                                        Game1.createItemDebris(new Object(int.Parse(ic[0]), Game1.random.Next(int.Parse(ic[1]), int.Parse(ic[2]))), m.Position, Game1.random.Next(4), m.currentLocation, -1);
                                }
                            }
                        }
                        if (enchantment.parameters.TryGetValue("sound", out string sound))
                            Game1.playSound(sound);
                    }
                }
                else if (enchantment.type == "coins")
                {
                    if (Game1.random.NextDouble() < float.Parse(enchantment.parameters["chance"]) / 100f)
                    {
                        float mult = float.Parse(enchantment.parameters["amountMult"]);
                        int amount = (int)Math.Round(mult * m.MaxHealth);
                        who.Money += amount;
                        if (enchantment.parameters.TryGetValue("sound", out string sound))
                            Game1.playSound(sound);
                    }
                }
            }
            return false;
        }
    }
}