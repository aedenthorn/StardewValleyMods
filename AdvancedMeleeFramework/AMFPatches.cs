using Microsoft.Xna.Framework;
using StardewModdingAPI;
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
            //context.Monitor.Log($"created melee weapon {__instance.Name} {__instance.InitialParentTileIndex} {__instance.ParentSheetIndex}");

            AdvancedMeleeWeapon amw = ModEntry.GetAdvancedWeapon(__instance, null);
            if (amw != null)
            {
                int scount = 0;
                foreach (AdvancedEnchantmentData aed in amw.enchantments)
                {
                    switch (aed.type)
                    {
                        case "vampiric":
                            __instance.enchantments.Add(new VampiricEnchantment());
                            break;
                        case "jade":
                            __instance.enchantments.Add(new JadeEnchantment());
                            break;
                        case "aquamarine":
                            __instance.enchantments.Add(new AquamarineEnchantment());
                            break;
                        case "topaz":
                            __instance.enchantments.Add(new TopazEnchantment());
                            break;
                        case "amethyst":
                            __instance.enchantments.Add(new AmethystEnchantment());
                            break;
                        case "ruby":
                            __instance.enchantments.Add(new RubyEnchantment());
                            break;
                        case "emerald":
                            __instance.enchantments.Add(new EmeraldEnchantment());
                            break;
                        case "haymaker":
                            __instance.enchantments.Add(new HaymakerEnchantment());
                            break;
                        case "bugkiller":
                            __instance.enchantments.Add(new BugKillerEnchantment());
                            break;
                        case "crusader":
                            __instance.enchantments.Add(new CrusaderEnchantment());
                            break;
                        case "magic":
                            __instance.enchantments.Add(new MagicEnchantment());
                            break;
                        default:
                            BaseWeaponEnchantment we = new BaseWeaponEnchantment();
                            string key = aed.name;
                            context.Helper.Reflection.GetField<string>(we, "_displayName").SetValue(key);
                            __instance.enchantments.Add(we);
                            break;
                    }
                    scount++;
                    context.Monitor.Log($"added enchantment {aed.type} to {__instance.Name} {__instance.enchantments.Count}");
                }
            }
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
            switch (__instance.type)
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

            switch (__instance.type)
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
            if (!(__instance is BaseWeaponEnchantment) || ____displayName == null || !ModEntry.advancedEnchantments.ContainsKey(____displayName))
                return true;
            AdvancedEnchantmentData enchantment = ModEntry.advancedEnchantments[____displayName];

            if (enchantment.parameters["trigger"] == "damage" || (enchantment.parameters["trigger"] == "crit" && amount > (who.CurrentTool as MeleeWeapon).maxDamage))
            {
                context.Monitor.Log($"Triggered enchantment {enchantment.name} on {enchantment.parameters["trigger"]}");
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
            if (!(__instance is BaseWeaponEnchantment) || !ModEntry.advancedEnchantments.ContainsKey(____displayName))
                return true;
            AdvancedEnchantmentData enchantment = ModEntry.advancedEnchantments[____displayName];
            if (enchantment.parameters["trigger"] == "slay")
            {
                context.Monitor.Log($"Triggered enchantment {enchantment.name} on slay");
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
                        else if (enchantment.parameters.ContainsKey("extraDropItems"))
                        {
                            string[] items = enchantment.parameters["extraDropItems"].Split(',');
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
                        if (enchantment.parameters.ContainsKey("sound"))
                            Game1.playSound(enchantment.parameters["sound"]);
                    }
                }
                else if (enchantment.type == "coins")
                {
                    if (Game1.random.NextDouble() < float.Parse(enchantment.parameters["chance"]) / 100f)
                    {
                        float mult = float.Parse(enchantment.parameters["amountMult"]);
                        int amount = (int)Math.Round(mult * m.maxHealth);
                        who.Money += amount;
                        if (enchantment.parameters.ContainsKey("sound"))
                            Game1.playSound(enchantment.parameters["sound"]);
                    }
                }
            }
            return false;
        }

    }
}