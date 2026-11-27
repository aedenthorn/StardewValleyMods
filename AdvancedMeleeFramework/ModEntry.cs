using AdvancedMeleeFramework.Integrations;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Enchantments;
using StardewValley.Monsters;
using StardewValley.Projectiles;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace AdvancedMeleeFramework
{
    public class ModEntry : Mod
    {
        public static ModEntry Instance;
        public ModConfig Config;
        public Random Random;
        public Dictionary<string, List<AdvancedMeleeWeapon>> AdvancedMeleeWeapons = [];
        public Dictionary<int, List<AdvancedMeleeWeapon>> AdvancedMeleeWeaponsByType = new()
        {
            { 1, [] },
            { 2, [] },
            { 3, [] },
        };
        public Dictionary<string, AdvancedEnchantmentData> AdvancedEnchantments = [];
        public Dictionary<string, int> EnchantmentTriggers = [];
        public Dictionary<string, Action<Farmer, MeleeWeapon, Monster?, Dictionary<string, string>>> AdvancedEnchantmentCallbacks = [];
        public Dictionary<string, Action<Farmer, MeleeWeapon, Dictionary<string, string>>> SpecialEffectCallbacks = [];
        public PerScreen<int> WeaponAnimationFrame = new(() => -1);
        public PerScreen<int> WeaponAnimationTicks = new(() => 0);
        public PerScreen<int> WeaponStartFacingDirection = new(() => 0);
        public PerScreen<MeleeWeapon> WeaponAnimating = new(() => null);
        public PerScreen<AdvancedMeleeWeapon> AdvancedWeaponAnimating = new(() => null);
        public IJsonAssetsApi? JsonAssetsApi;

        public override void Entry(IModHelper helper)
        {
            Instance = this;
            Config = Helper.ReadConfig<ModConfig>();

            AMFPatches.Initialize(this);
            Utils.Initialize(this);

            Random = new();

            Helper.Events.Player.InventoryChanged += onInventoryChanged;

            Helper.Events.GameLoop.GameLaunched += onGameLaunched;
            Helper.Events.GameLoop.SaveLoaded += onSaveLoaded;
            Helper.Events.GameLoop.UpdateTicking += onUpdateTicking;

            Helper.Events.Input.ButtonPressed += onButtonPressed;

            registerDefaultEnchantments();
            registerDefaultSpecialEffects();
        }

        public override object GetApi(IModInfo mod) => new AdvancedMeleeFrameworkApi(mod, this);

        private void onInventoryChanged(object sender, InventoryChangedEventArgs e)
        {
            foreach (var item in e.Player.Items)
                if (item is MeleeWeapon mw)
                    Utils.AddEnchantments(mw);
        }

        private void onGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            JsonAssetsApi = Helper.ModRegistry.GetApi<IJsonAssetsApi>("spacechase0.JsonAssets");

            var gmcm = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (gmcm is null)
                return;

            gmcm.Register(ModManifest, () => Config = new(), () => Helper.WriteConfig(Config));
            gmcm.AddBoolOption(ModManifest, () => Config.EnableMod, v => Config.EnableMod = v, () => "Enabled");
            gmcm.AddKeybind(ModManifest, () => Config.ReloadButton, v => Config.ReloadButton = v, () => "Reload Button");
            gmcm.AddBoolOption(ModManifest, () => Config.RequireModKey, v => Config.RequireModKey = v, () => "Require Activate Button");
            gmcm.AddKeybind(ModManifest, () => Config.ModKey, v => Config.ModKey = v, () => "Activate Button");
        }

        private void onSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            Utils.LoadAdvancedMeleeWeapons();
            foreach (var item in Game1.player.Items)
                if (item is MeleeWeapon mw)
                    Utils.AddEnchantments(mw);
        }

        private void onUpdateTicking(object sender, UpdateTickingEventArgs e)
        {
            if (WeaponAnimationFrame.Value < 0 || AdvancedWeaponAnimating.Value is null)
                return;
            MeleeActionFrame frame = AdvancedWeaponAnimating.Value.frames[WeaponAnimationFrame.Value];
            Farmer who = WeaponAnimating.Value.getLastFarmerToUse();

            if (WeaponAnimationFrame.Value == 0 && WeaponAnimationTicks.Value == 0)
                WeaponStartFacingDirection.Value = who.FacingDirection;

            if (who.CurrentTool != WeaponAnimating.Value)
            {
                WeaponAnimating.Value = null;
                WeaponAnimationTicks.Value = 0;
                WeaponAnimationFrame.Value = -1;
                AdvancedWeaponAnimating.Value = null;
                return;
            }

            if (frame.invincible is { } invincible)
                who.temporarilyInvincible = invincible;

            if (WeaponAnimationTicks.Value == 0)
            {
                who.faceDirection((WeaponStartFacingDirection.Value + frame.relativeFacingDirection) % 4);

                if (frame.special is { } special)
                {
                    try
                    {
                        if (!SpecialEffectCallbacks.TryGetValue(special.name, out var callback))
                            throw new($"No special effect found with name {special.name}");
                        callback.Invoke(who, WeaponAnimating.Value, special.parameters);
                    }
                    catch (Exception ex)
                    {
                        Monitor.Log($"Exception thrown on special effect:\n{ex}", LogLevel.Error);
                    }
                }

                if (frame.action == WeaponAction.NORMAL)
                {
                    who.completelyStopAnimatingOrDoingAction();
                    who.CanMove = false;
                    who.UsingTool = true;
                    who.canReleaseTool = true;
                    WeaponAnimating.Value.setFarmerAnimating(who);
                }
                else if (frame.action == WeaponAction.SPECIAL)
                    WeaponAnimating.Value.animateSpecialMove(who);

                if (frame.trajectoryX != 0 || frame.trajectoryY != 0)
                {
                    Vector2 rawTrajectory = Utils.TranslateVector(new(frame.trajectoryX, frame.trajectoryY), who.FacingDirection);
                    who.setTrajectory(new(rawTrajectory.X, -rawTrajectory.Y));
                }

                if (frame.sound is not null)
                    who.currentLocation.playSound(frame.sound);

                foreach (AdvancedWeaponProjectile p in frame.projectiles)
                {
                    Vector2 velocity = Utils.TranslateVector(new(p.xVelocity, p.yVelocity), who.FacingDirection);
                    Vector2 startPos = Utils.TranslateVector(new(p.startingPositionX, p.startingPositionY), who.FacingDirection);

                    int damage = AdvancedWeaponAnimating.Value.type > 0 ? p.damage * Random.Next(WeaponAnimating.Value.minDamage.Value, WeaponAnimating.Value.maxDamage.Value) : p.damage;

                    who.currentLocation.projectiles.Add(new BasicProjectile(damage,
                                                                            p.parentSheetIndex,
                                                                            p.bouncesTillDestruct,
                                                                            p.tailLength,
                                                                            p.rotationVelocity,
                                                                            velocity.X,
                                                                            velocity.Y,
                                                                            who.Position + new Vector2(0, -64) + startPos,
                                                                            p.collisionSound,
                                                                            p.bounceSound,
                                                                            p.firingSound,
                                                                            p.explode,
                                                                            p.damagesMonsters,
                                                                            who.currentLocation,
                                                                            who,
                                                                            null,
                                                                            p.shotItemId));
                }
            }

            if (++WeaponAnimationTicks.Value >= frame.frameTicks)
            {
                WeaponAnimationFrame.Value++;
                WeaponAnimationTicks.Value = 0;
            }

            if (WeaponAnimationFrame.Value < AdvancedWeaponAnimating.Value.frames.Count)
                return;
            who.completelyStopAnimatingOrDoingAction();
            who.CanMove = true;
            who.UsingTool = false;
            who.setTrajectory(Vector2.Zero);

            if (who.IsLocalPlayer)
            {
                int cd = AdvancedWeaponAnimating.Value.cooldown;
                if (who.professions.Contains(28))
                    cd /= 2;
                if (WeaponAnimating.Value.hasEnchantmentOfType<ArtfulEnchantment>())
                    cd /= 2;

                switch (WeaponAnimating.Value.type.Value)
                {
                    case 1:
                        MeleeWeapon.daggerCooldown = cd;
                        break;
                    case 2:
                        MeleeWeapon.clubCooldown = cd;
                        break;
                    case 3:
                        MeleeWeapon.defenseCooldown = cd;
                        break;
                }
            }

            WeaponAnimationFrame.Value = -1;
            WeaponAnimating.Value = null;
            AdvancedWeaponAnimating.Value = null;
            WeaponAnimationTicks.Value = 0;
        }

        private void onButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (e.Button == Config.ReloadButton)
                Utils.LoadAdvancedMeleeWeapons();
        }

        private void registerDefaultEnchantments()
        {
            AdvancedEnchantmentCallbacks.Add("heal", Heal);
            AdvancedEnchantmentCallbacks.Add("hurt", Hurt);
            AdvancedEnchantmentCallbacks.Add("coins", Coins);
            AdvancedEnchantmentCallbacks.Add("loot", Loot);
        }

        private void registerDefaultSpecialEffects()
        {
            SpecialEffectCallbacks.Add("lightning", LightningStrike);
            SpecialEffectCallbacks.Add("explosion", Explosion);
        }

        private float defaultMultFromTrigger(string trigger) => trigger == "slay" ? .1f : 1f;

        public void Heal(Farmer who, MeleeWeapon weapon, Monster? monster, Dictionary<string, string> parameters)
        {
            if (Game1.random.NextDouble() < float.Parse(parameters["chance"]) / 100f)
            {
                float mult = parameters.TryGetValue("amountMult", out var multStr) ? float.Parse(multStr) : defaultMultFromTrigger(parameters["trigger"]);
                if (!int.TryParse(parameters["amount"], out int amount))
                    amount = monster?.MaxHealth ?? 1;
                int heal = Math.Max(1, (int)(amount * mult));
                who.health = Math.Min(who.maxHealth, Game1.player.health + heal);
                who.currentLocation.debris.Add(new Debris(heal, who.getStandingPosition(), Color.Lime, 1f, who));
                if (parameters.TryGetValue("sound", out var sound))
                    Game1.playSound(sound);
            }
        }

        public void Hurt(Farmer who, MeleeWeapon weapon, Monster? monster, Dictionary<string, string> parameters)
        {
            if (Game1.random.NextDouble() < float.Parse(parameters["chance"]) / 100f)
            {
                float mult = parameters.TryGetValue("amountMult", out var multStr) ? float.Parse(multStr) : defaultMultFromTrigger(parameters["trigger"]);
                if (!int.TryParse(parameters["amount"], out int amount))
                    amount = monster?.MaxHealth ?? 1;
                int hurt = Math.Max(1, (int)(amount * mult));
                who.takeDamage(hurt, true, null);
                if (parameters.TryGetValue("sound", out var sound))
                    Game1.playSound(sound);
            }
        }

        public void Coins(Farmer who, MeleeWeapon weapon, Monster? monster, Dictionary<string, string> parameters)
        {
            if (Game1.random.NextDouble() < float.Parse(parameters["chance"]) / 100f)
            {
                float mult = parameters.TryGetValue("amountMult", out var multStr) ? float.Parse(multStr) : defaultMultFromTrigger(parameters["trigger"]);
                if (!int.TryParse(parameters["amount"], out int amount))
                    amount = monster?.MaxHealth ?? 1;
                int coins = (int)Math.Round(amount * mult);
                if (parameters.TryGetValue("dropType", out string dropType) && dropType.ToLower() == "wallet")
                {
                    who.Money += coins;
                    if (parameters.TryGetValue("sound", out string sound2))
                        Game1.playSound(sound2);
                    return;
                }
                Item i = ItemRegistry.Create("(O)GoldCoin");
                i.modData.Add(ModManifest.UniqueID + "/moneyAmount", coins.ToString());
                Game1.createItemDebris(i, monster?.Position ?? Utility.PointToVector2(who.StandingPixel), who.FacingDirection, who.currentLocation);
                if (parameters.TryGetValue("sound", out var sound))
                    Game1.playSound(sound);
            }
        }

        public void Loot(Farmer who, MeleeWeapon weapon, Monster? monster, Dictionary<string, string> parameters)
        {
            if (monster is null)
                return;
            if (Game1.random.NextDouble() < float.Parse(parameters["chance"]) / 100f)
            {
                Vector2 position = monster.Position;
                if (parameters.ContainsKey("extraDropChecks"))
                {
                    int extraChecks = Math.Max(1, int.Parse(parameters["extraDropChecks"]));
                    for (int i = 0; i < extraChecks; i++)
                        who.currentLocation.monsterDrop(monster, monster.GetBoundingBox().Center.X, monster.GetBoundingBox().Center.Y, who);
                }
                else if (parameters.TryGetValue("extraDropItems", out string extraDrops))
                {
                    string[] items = extraDrops.Split(',');
                    foreach (var item in items)
                    {
                        string[] itemData = item.Split('_');
                        if (itemData.Length == 1)
                            Game1.createItemDebris(ItemRegistry.Create(item), position, Game1.random.Next(4), who.currentLocation);
                        else if (itemData.Length == 2)
                        {
                            float chance = int.Parse(itemData[1]) / 100f;
                            if (Game1.random.NextDouble() < chance)
                                Game1.createItemDebris(ItemRegistry.Create(itemData[0]), position, Game1.random.Next(4), who.currentLocation);
                        }
                        else if (itemData.Length == 4)
                        {
                            float chance = int.Parse(itemData[3]) / 100f;
                            if (Game1.random.NextDouble() < chance)
                                Game1.createItemDebris(ItemRegistry.Create(itemData[0], Game1.random.Next(int.Parse(itemData[1]), int.Parse(itemData[2]))), position, Game1.random.Next(4), who.currentLocation);
                        }
                    }
                }
                if (parameters.TryGetValue("sound", out var sound))
                    Game1.playSound(sound);
            }
        }

        public void LightningStrike(Farmer who, MeleeWeapon weapon, Dictionary<string, string> parameters)
        {
            int minDamage = weapon.minDamage.Value;
            int maxDamage = weapon.maxDamage.Value;
            if (parameters.TryGetValue("minDamage", out var minDamageStr))
                minDamage = int.Parse(minDamageStr);
            if (parameters.TryGetValue("maxDamage", out var maxDamageStr))
                maxDamage = int.Parse(maxDamageStr);
            if (parameters.TryGetValue("damageMult", out var damageMultStr) && float.TryParse(damageMultStr, out float damageMult))
            {
                minDamage = (int)Math.Round(minDamage * damageMult);
                maxDamage = (int)Math.Round(maxDamage * damageMult);
            }

            if (!int.TryParse(parameters["radius"], out int radius))
                radius = 3;
            Farm.LightningStrikeEvent lightningEvent = new()
            {
                bigFlash = true,
                createBolt = true,
            };

            Vector2 offset = Vector2.Zero;
            if (!parameters.TryGetValue("offsetX", out var offsetX) || !float.TryParse(offsetX, out var x))
                x = 0;
            if (!parameters.TryGetValue("offsetY", out var offsetY) || !float.TryParse(offsetY, out var y))
                y = 0;
            if (x != 0 || y != 0)
                offset = Utils.TranslateVector(new(x, y), who.FacingDirection);
            lightningEvent.boltPosition = who.Position + new Vector2(32f, 0f) + (offset * 64f);
            Game1.flashAlpha = (float)(0.5 + Game1.random.NextDouble());

            if (parameters.TryGetValue("sound", out var sound))
                Game1.playSound(sound);

            Utility.drawLightningBolt(lightningEvent.boltPosition, who.currentLocation);

            who.currentLocation.damageMonster(new((int)Math.Round(lightningEvent.boltPosition.X / 64 - radius) * 64, (int)Math.Round(lightningEvent.boltPosition.Y / 64 - radius) * 64, (radius * 2 + 1) * 64, (radius * 2 + 1) * 64), minDamage, maxDamage, false, who);
        }

        public void Explosion(Farmer who, MeleeWeapon weapon, Dictionary<string, string> parameters)
        {
            Vector2 tileLocation = who.Tile;
            if (parameters.TryGetValue("offsetX", out var offsetX) && parameters.TryGetValue("offsetY", out var offsetY))
                tileLocation += Utils.TranslateVector(new(float.Parse(offsetX), float.Parse(offsetY)), who.FacingDirection);

            if (!int.TryParse(parameters["radius"], out int radius))
                radius = 3;

            int damage = -1;
            if (parameters.TryGetValue("damageMult", out var damageMultStr) && float.TryParse(damageMultStr, out float damageMult))
                damage = (int)Math.Round(Game1.random.Next(weapon.minDamage.Value, weapon.maxDamage.Value + 1) * damageMult);
            if (parameters.TryGetValue("minDamage", out var minDamage) && parameters.TryGetValue("maxDamage", out var maxDamage))
                damage = Game1.random.Next(int.Parse(minDamage), int.Parse(maxDamage));
            if (damage < 0)
                damage = Game1.random.Next(weapon.minDamage.Value, weapon.maxDamage.Value + 1);

            who.currentLocation.explode(tileLocation, radius, who, false, damage);
        }
    }
}
