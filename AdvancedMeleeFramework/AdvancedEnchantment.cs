using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Monsters;
using StardewValley.Tools;
using System;

namespace AdvancedMeleeFramework
{
    public class AdvancedEnchantment: BaseWeaponEnchantment
    {
        private AdvancedMeleeWeapon advancedWeapon;
        private MeleeWeapon weapon;
        private AdvancedEnchantmentData enchantment;

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
            if (enchantment.parameters["trigger"] == "damage" || (enchantment.parameters["trigger"] == "crit" && amount > weapon.maxDamage))
            {
                ModEntry.context.Monitor.Log($"Triggered enchantment {enchantment.type} on hit for {weapon.Name}");
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
                    return;
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
                ModEntry.context.Monitor.Log($"Triggered enchantment {enchantment.type} on slay for {weapon.Name}");
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
            }
        }
    }
}