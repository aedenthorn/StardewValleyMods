using AdvancedMeleeFramework.Integrations;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
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
        public int WeaponAnimationFrame = -1;
        public int WeaponAnimationTicks = 0;
        public int WeaponStartFacingDirection = 0;
        public MeleeWeapon WeaponAnimating;
        public AdvancedMeleeWeapon AdvancedWeaponAnimating;
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
            if (WeaponAnimationFrame < 0 || AdvancedWeaponAnimating is null)
                return;
            MeleeActionFrame frame = AdvancedWeaponAnimating.frames[WeaponAnimationFrame];
            Farmer who = WeaponAnimating.getLastFarmerToUse();

            if (WeaponAnimationFrame == 0 && WeaponAnimationTicks == 0)
                WeaponStartFacingDirection = who.FacingDirection;

            if (who.CurrentTool != WeaponAnimating)
            {
                WeaponAnimating = null;
                WeaponAnimationTicks = 0;
                WeaponAnimationFrame = -1;
                AdvancedWeaponAnimating = null;
                return;
            }

            if (frame.invincible is { } invincible)
                who.temporarilyInvincible = invincible;

            if (WeaponAnimationTicks == 0)
            {
                who.faceDirection((WeaponStartFacingDirection + frame.relativeFacingDirection) % 4);

                if (frame.special is { } special)
                {
                    try
                    {
                        if (!SpecialEffectCallbacks.TryGetValue(special.name, out var callback))
                            throw new($"No special effect found with name {special.name}");
                        callback.Invoke(who, WeaponAnimating, special.parameters);
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
                    WeaponAnimating.setFarmerAnimating(who);
                }
                else if (frame.action == WeaponAction.SPECIAL)
                    WeaponAnimating.animateSpecialMove(who);

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

                    int damage = AdvancedWeaponAnimating.type > 0 ? p.damage * Random.Next(WeaponAnimating.minDamage.Value, WeaponAnimating.maxDamage.Value) : p.damage;

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

            if (++WeaponAnimationTicks >= frame.frameTicks)
            {
                WeaponAnimationFrame++;
                WeaponAnimationTicks = 0;
            }

            if (WeaponAnimationFrame < AdvancedWeaponAnimating.frames.Count)
                return;
            who.completelyStopAnimatingOrDoingAction();
            who.CanMove = true;
            who.UsingTool = false;
            who.setTrajectory(Vector2.Zero);

            if (who.IsLocalPlayer)
            {
                int cd = AdvancedWeaponAnimating.cooldown;
                if (who.professions.Contains(28))
                    cd /= 2;
                if (WeaponAnimating.hasEnchantmentOfType<ArtfulEnchantment>())
                    cd /= 2;

                switch (WeaponAnimating.type.Value)
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

            WeaponAnimationFrame = -1;
            WeaponAnimating = null;
            AdvancedWeaponAnimating = null;
            WeaponAnimationTicks = 0;
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

        public void Heal(Farmer who, MeleeWeapon weapon, Monster? monster, Dictionary<string, string> parameters)
        {
            if (Game1.random.NextDouble() < float.Parse(parameters["chance"]) / 100f)
            {
                int amount = monster is not null ? monster.Health : int.Parse(parameters["amount"]);
                int heal = Math.Max(1, (int)(amount * float.Parse(parameters["amountMult"])));
                who.health = Math.Min(who.maxHealth, Game1.player.health + heal);
                who.currentLocation.debris.Add(new Debris(heal, who.getStandingPosition(), Color.Lime, 1f, who));
                if (parameters.ContainsKey("sound"))
                    Game1.playSound(parameters["sound"]);
            }
        }

        public void Hurt(Farmer who, MeleeWeapon weapon, Monster? monster, Dictionary<string, string> parameters)
        {
            if (Game1.random.NextDouble() < float.Parse(parameters["chance"]) / 100f)
            {
                int amount = monster is not null ? monster.Health : int.Parse(parameters["amount"]);
                int hurt = Math.Max(1, (int)(amount * float.Parse(parameters["amountMult"])));
                who.takeDamage(hurt, true, null);
                if (parameters.ContainsKey("sound"))
                    Game1.playSound(parameters["sound"]);
            }
        }

        public void Coins(Farmer who, MeleeWeapon weapon, Monster? monster, Dictionary<string, string> parameters)
        {
            if (Game1.random.NextDouble() < float.Parse(parameters["chance"]) / 100f)
            {
                int amount = monster is not null ? monster.MaxHealth : int.Parse(parameters["amount"]);
                int coins = (int)Math.Round(amount * float.Parse(parameters["amountMult"]));
                if (parameters.TryGetValue("dropType", out string dropType) && dropType.ToLower() == "wallet")
                {
                    who.Money += coins;
                    if (parameters.TryGetValue("sound", out string sound))
                        Game1.playSound(sound);
                    return;
                }
                Item i = ItemRegistry.Create("(O)GoldCoin");
                i.modData.Add(ModManifest.UniqueID + "/moneyAmount", coins.ToString());
                Game1.createItemDebris(i, monster?.Position ?? Utility.PointToVector2(who.StandingPixel), who.FacingDirection, who.currentLocation);
                if (parameters.ContainsKey("sound"))
                    Game1.playSound(parameters["sound"]);
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
            if (parameters.TryGetValue("damageMult", out var damageMultStr) && float.TryParse(damageMultStr, out float damageMult))
            {
                minDamage = (int)Math.Round(minDamage * damageMult);
                maxDamage = (int)Math.Round(maxDamage * damageMult);
            }
            if (parameters.TryGetValue("minDamage", out var minDamageStr) && parameters.TryGetValue("maxDamage", out var maxDamageStr))
            {
                minDamage = int.Parse(minDamageStr);
                maxDamage = int.Parse(maxDamageStr);
            }

            int radius = int.Parse(parameters["radius"]);
            Farm.LightningStrikeEvent lightningEvent = new()
            {
                bigFlash = true,
                createBolt = true,
            };

            Vector2 offset = Vector2.Zero;
            if (parameters.TryGetValue("offsetX", out var offsetX) && parameters.TryGetValue("offsetY", out var offsetY))
            {
                float x = float.Parse(offsetX);
                float y = float.Parse(offsetY);
                offset = Utils.TranslateVector(new(x, y), who.FacingDirection);
            }
            lightningEvent.boltPosition = who.Position + new Vector2(32f) + offset;
            Game1.flashAlpha = (float)(0.5 + Game1.random.NextDouble());

            if (parameters.TryGetValue("sound", out var sound))
                Game1.playSound(sound);

            Utility.drawLightningBolt(lightningEvent.boltPosition, who.currentLocation);

            who.currentLocation.damageMonster(new((int)Math.Round(who.Position.X - radius), (int)Math.Round(who.Position.Y - radius), radius * 2, radius * 2), minDamage, maxDamage, false, who);
        }

        public void Explosion(Farmer who, MeleeWeapon weapon, Dictionary<string, string> parameters)
        {
            Vector2 tileLocation = who.Tile;
            if (parameters.TryGetValue("tileOffsetX", out var offsetX) && parameters.TryGetValue("tileOffsetY", out var offsetY))
                tileLocation += Utils.TranslateVector(new(float.Parse(offsetX), float.Parse(offsetY)), who.FacingDirection);
            int radius = int.Parse(parameters["radius"]);

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
