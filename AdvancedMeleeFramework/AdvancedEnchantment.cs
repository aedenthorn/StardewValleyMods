using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Monsters;
using StardewValley.Tools;
using System;
using Object = StardewValley.Object;

namespace AdvancedMeleeFramework
{
    public class AdvancedEnchantment: BaseWeaponEnchantment
    {
        private AdvancedMeleeWeapon advancedWeapon;
        private MeleeWeapon weapon;
        private AdvancedEnchantmentData enchantment;

        public AdvancedEnchantment()
        {
        }
        public AdvancedEnchantment(MeleeWeapon w, AdvancedMeleeWeapon amw, AdvancedEnchantmentData aed)
        {
            weapon = w;
            advancedWeapon = amw;
            enchantment = aed;
        }

        public override bool CanApplyTo(Item item)
        {
            return true;
        }
        protected override void _OnDealDamage(Monster monster, GameLocation location, Farmer who, ref int amount)
        {
            if (enchantment.parameters["trigger"] == "damage" || (enchantment.parameters["trigger"] == "crit" && amount > weapon.maxDamage.Value))
            {
                //ModEntry.context.Monitor.Log($"Triggered enchantment {enchantment.type} on {enchantment.parameters["trigger"]} for {weapon.Name}");
                if (enchantment.type == "heal")
                {
                    if (Game1.random.NextDouble() < float.Parse(enchantment.parameters["chance"]) / 100f)
                    {
                        int heal = Math.Max(1, (int)(amount * float.Parse(enchantment.parameters["amountMult"])));
                        who.health = Math.Min(who.maxHealth, Game1.player.health + heal);
                        location.debris.Add(new Debris(heal, new Vector2((float)Game1.player.getStandingX(), (float)Game1.player.getStandingY()), Color.Lime, 1f, who));
                        if(enchantment.parameters.ContainsKey("sound"))
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
        }
        protected override void _OnEquip(Farmer who)
        {
            base._OnEquip(who);
        }
        protected override void _OnUnequip(Farmer who)
        {
            base._OnUnequip(who);
        }
        protected override void _OnMonsterSlay(Monster m, GameLocation location, Farmer who)
        {
            if (enchantment.parameters["trigger"] == "slay")
            {
                //ModEntry.context.Monitor.Log($"Triggered enchantment {enchantment.type} on slay for {weapon.Name}");
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
                    return;
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
                                else if(ic.Length == 2)
                                {
                                    float chance = int.Parse(ic[1]) / 100f;
                                    if (Game1.random.NextDouble() < chance)
                                        Game1.createItemDebris(new Object(int.Parse(ic[0]), 1, false, -1, 0), m.Position, Game1.random.Next(4), m.currentLocation, -1);
                                }
                                else if(ic.Length == 4)
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
        }
    }
}