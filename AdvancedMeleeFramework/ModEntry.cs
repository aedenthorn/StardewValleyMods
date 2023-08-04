using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Network;
using StardewValley.Projectiles;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AdvancedMeleeFramework
{
    public class ModEntry : Mod 
    {
        public static ModEntry context;
        public ModConfig Config;
        public static Random myRand;
        public static Dictionary<int, List<AdvancedMeleeWeapon>> advancedMeleeWeapons = new Dictionary<int, List<AdvancedMeleeWeapon>>();
        public static Dictionary<int, List<AdvancedMeleeWeapon>> advancedMeleeWeaponsByType = new Dictionary<int, List<AdvancedMeleeWeapon>>() 
        {
            {1, new List<AdvancedMeleeWeapon>() },
            {2, new List<AdvancedMeleeWeapon>() },
            {3, new List<AdvancedMeleeWeapon>() }
        };
        public static int weaponAnimationFrame = -1;
        public int weaponAnimationTicks;
        public static MeleeWeapon weaponAnimating;
        public static int weaponStartFacingDirection;
        public static AdvancedMeleeWeapon advancedWeaponAnimating = null;
        public static IJsonAssetsApi mJsonAssets;
        public static Dictionary<string, AdvancedEnchantmentData> advancedEnchantments = new Dictionary<string, AdvancedEnchantmentData>();
        public static Dictionary<string, int> EnchantmentTriggers = new Dictionary<string, int>();

        public override void Entry(IModHelper helper)
        {
            context = this;
            Config = Helper.ReadConfig<ModConfig>();
            if (!Config.EnableMod)
                return;

            AMFPatches.Initialize(this);

            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            Helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            Helper.Events.GameLoop.UpdateTicking += GameLoop_UpdateTicking;
            Helper.Events.Input.ButtonPressed += Input_ButtonPressed;
            Helper.Events.Player.InventoryChanged += Player_InventoryChanged;


            myRand = new Random();

            var harmony = new Harmony(ModManifest.UniqueID);

            harmony.Patch(
                original: AccessTools.Method(typeof(MeleeWeapon), "doAnimateSpecialMove"),
                prefix: new HarmonyMethod(typeof(AMFPatches), nameof(AMFPatches.doAnimateSpecialMove_Prefix))
            );
            ConstructorInfo ci = typeof(MeleeWeapon).GetConstructor(new Type[] { typeof(int) });
            harmony.Patch(
               original: ci,
               postfix: new HarmonyMethod(typeof(AMFPatches), nameof(AMFPatches.MeleeWeapon_Postfix))
            );
            ci = typeof(MeleeWeapon).GetConstructor(new Type[] { });
            harmony.Patch(
               original: ci,
               postfix: new HarmonyMethod(typeof(AMFPatches), nameof(AMFPatches.MeleeWeapon_Postfix))
            );
            ci = typeof(MeleeWeapon).GetConstructor(new Type[] { typeof(int), typeof(int) });
            harmony.Patch(
               original: ci,
               postfix: new HarmonyMethod(typeof(AMFPatches), nameof(AMFPatches.MeleeWeapon_Postfix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(MeleeWeapon), nameof(MeleeWeapon.drawInMenu), new Type[] { typeof(SpriteBatch), typeof(Vector2), typeof(float), typeof(float), typeof(float), typeof(StackDrawType), typeof(Color), typeof(bool) }),
                prefix: new HarmonyMethod(typeof(AMFPatches), nameof(AMFPatches.drawInMenu_Prefix)),
                postfix: new HarmonyMethod(typeof(AMFPatches), nameof(AMFPatches.drawInMenu_Postfix))
            );

            harmony.Patch(
                original: AccessTools.Method(typeof(BaseEnchantment), "_OnDealDamage"),
                prefix: new HarmonyMethod(typeof(AMFPatches), nameof(AMFPatches._OnDealDamage_Prefix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(BaseEnchantment), "_OnMonsterSlay"),
                prefix: new HarmonyMethod(typeof(AMFPatches), nameof(AMFPatches._OnMonsterSlay_Prefix))
            );

        }

        private void Player_InventoryChanged(object sender, StardewModdingAPI.Events.InventoryChangedEventArgs e)
        {
            foreach(Item item in e.Player.Items)
            {
                if(item is MeleeWeapon)
                {
                    AddEnchantments(item as MeleeWeapon);
                }
            }
        }


        public void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            mJsonAssets = Helper.ModRegistry.GetApi<IJsonAssetsApi>("spacechase0.JsonAssets");
        }

        public void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            LoadAdvancedMeleeWeapons();
            foreach (Item item in Game1.player.Items)
            {
                if (item is MeleeWeapon)
                {
                    AddEnchantments(item as MeleeWeapon);
                }
            }
        }

        public static void AddEnchantments(MeleeWeapon weapon)
        {
            AdvancedMeleeWeapon amw = GetAdvancedWeapon(weapon, null);
            if (amw != null && amw.enchantments.Any())
            {
                weapon.enchantments.Clear();
                foreach (AdvancedEnchantmentData aed in amw.enchantments)
                {
                    BaseWeaponEnchantment bwe = null;
                    switch (aed.type)
                    {
                        case "vampiric":
                            bwe = new VampiricEnchantment();
                            break;
                        case "jade":
                            bwe = new JadeEnchantment();
                            break;
                        case "aquamarine":
                            bwe = new AquamarineEnchantment();
                            break;
                        case "topaz":
                            bwe = new TopazEnchantment();
                            break;
                        case "amethyst":
                            bwe = new AmethystEnchantment();
                            break;
                        case "ruby":
                            bwe = new RubyEnchantment();
                            break;
                        case "emerald":
                            bwe = new EmeraldEnchantment();
                            break;
                        case "haymaker":
                            bwe = new HaymakerEnchantment();
                            break;
                        case "bugkiller":
                            bwe = new BugKillerEnchantment();
                            break;
                        case "crusader":
                            bwe = new CrusaderEnchantment();
                            break;
                        case "magic":
                            bwe = new MagicEnchantment();
                            break;
                        default:
                            bwe = new BaseWeaponEnchantment();
                            string key = aed.name;
                            context.Helper.Reflection.GetField<string>(bwe, "_displayName").SetValue(key);
                            break;
                    }
                    if(bwe != null)
                    {
                        weapon.enchantments.Add(bwe);
                        //context.Monitor.Log($"added enchantment {aed.type} to {weapon.Name} {weapon.enchantments.Count}");
                    }
                }
            }
        }
        public void LoadAdvancedMeleeWeapons()
        {
            advancedMeleeWeapons.Clear();
            advancedMeleeWeaponsByType[1].Clear();
            advancedMeleeWeaponsByType[2].Clear();
            advancedMeleeWeaponsByType[3].Clear();
            foreach (IContentPack contentPack in Helper.ContentPacks.GetOwned())
            {
                Monitor.Log($"Reading content pack: {contentPack.Manifest.Name} {contentPack.Manifest.Version} from {contentPack.DirectoryPath}", LogLevel.Debug);
                try
                {
                    AdvancedMeleeWeaponData json = contentPack.ReadJsonFile<AdvancedMeleeWeaponData>("content.json") ?? null;
                    WeaponPackConfigData config = contentPack.ReadJsonFile<WeaponPackConfigData>("config.json") ?? new WeaponPackConfigData();

                    if (json != null)
                    {
                        if (json.weapons != null && json.weapons.Count > 0)
                        {
                            foreach (AdvancedMeleeWeapon weapon in json.weapons)
                            {
                                foreach(KeyValuePair<string, string> kvp in weapon.config)
                                {
                                    FieldInfo fi = weapon.GetType().GetField(kvp.Key);

                                    if(fi == null)
                                    {
                                        Monitor.Log($"Error getting field {kvp.Key} in AdvancedMeleeWeapon class.", LogLevel.Error);
                                        continue;
                                    }

                                    if (config.variables.ContainsKey(kvp.Value))
                                    {
                                        var val = config.variables[kvp.Value];
                                        if (val.GetType() == typeof(Int64))
                                            fi.SetValue(weapon, Convert.ToInt32(config.variables[kvp.Value]));
                                        else
                                            fi.SetValue(weapon, config.variables[kvp.Value]);
                                    }
                                    else
                                    {
                                        config.variables.Add(kvp.Value, fi.GetValue(weapon));
                                    }
                                }
                                foreach(MeleeActionFrame frame in weapon.frames)
                                {
                                    foreach (KeyValuePair<string, string> kvp in frame.config)
                                    {
                                        FieldInfo fi = frame.GetType().GetField(kvp.Key);

                                        if (fi == null)
                                        {
                                            Monitor.Log($"Error getting field {kvp.Key} in MeleeActionFrame class.", LogLevel.Error);
                                            continue;
                                        }

                                        if (config.variables.ContainsKey(kvp.Value))
                                        {
                                            fi.SetValue(frame, config.variables[kvp.Value]);
                                        }
                                        else
                                        {
                                            config.variables.Add(kvp.Value, fi.GetValue(frame));
                                        }
                                    }
                                    foreach (AdvancedWeaponProjectile entry in frame.projectiles)
                                    {
                                        foreach (KeyValuePair<string, string> kvp in entry.config)
                                        {
                                            FieldInfo fi = entry.GetType().GetField(kvp.Key);

                                            if (fi == null)
                                            {
                                                Monitor.Log($"Error getting field {kvp.Key} in AdvancedWeaponProjectile class.", LogLevel.Error);
                                                continue;
                                            }

                                            if (config.variables.ContainsKey(kvp.Value))
                                            {
                                                fi.SetValue(entry, config.variables[kvp.Value]);
                                            }
                                            else
                                            {
                                                config.variables.Add(kvp.Value, fi.GetValue(entry));
                                            }
                                        }
                                    }
                                    if(frame.special != null)
                                    {
                                        foreach (KeyValuePair<string, string> kvp in frame.special.config)
                                        {
                                            if (!frame.special.parameters.ContainsKey(kvp.Key))
                                            {
                                                Monitor.Log($"Error getting key {kvp.Key} in SpecialEffects.parameters", LogLevel.Error);
                                                continue;
                                            }
                                            if (config.variables.ContainsKey(kvp.Value))
                                            {
                                                frame.special.parameters[kvp.Key] = config.variables[kvp.Value].ToString();
                                            }
                                            else
                                            {
                                                config.variables.Add(kvp.Value, frame.special.parameters[kvp.Key]);
                                            }

                                        }
                                    }
                                }
                                foreach (AdvancedEnchantmentData entry in weapon.enchantments)
                                {
                                    int count = 0;
                                    foreach (KeyValuePair<string, string> kvp in entry.config)
                                    {
                                        if (!entry.parameters.ContainsKey(kvp.Key))
                                        {
                                            Monitor.Log($"Error getting key {kvp.Key} in AdvancedEnchantmentData.parameters", LogLevel.Error);
                                            continue;
                                        }

                                        if (config.variables.ContainsKey(kvp.Value))
                                        {
                                            entry.parameters[kvp.Key]  = config.variables[kvp.Value].ToString();
                                        }
                                        else
                                        {
                                            config.variables.Add(kvp.Value, entry.parameters[kvp.Key]);
                                        }
                                    }
                                    advancedEnchantments[entry.name] = entry;
                                    count++;
                                }
                                if (config.variables.Any())
                                {
                                    contentPack.WriteJsonFile("config.json", config);
                                }


                                if (weapon.type == 0)
                                {
                                    Monitor.Log($"Adding specific weapon {weapon.id}");
                                    if (!int.TryParse(weapon.id, out int id))
                                    {
                                        Monitor.Log($"Got name instead of id {weapon.id}");
                                        try
                                        {
                                            id = Helper.GameContent.Load<Dictionary<int, string>>("Data/weapons").First(p => p.Value.StartsWith($"{weapon.id}/")).Key;
                                            Monitor.Log($"Got name-based id {id}");
                                        }
                                        catch (Exception ex)
                                        {
                                            if (mJsonAssets != null)
                                            {
                                                id = mJsonAssets.GetWeaponId(weapon.id);
                                                if(id == -1)
                                                {
                                                    //Monitor.Log($"error getting JSON Assets weapon {weapon.id}\n{ex}", LogLevel.Error);
                                                    continue;
                                                }
                                                Monitor.Log($"Added JA weapon {weapon.id}, id {id}");
                                            }
                                            else
                                            {
                                                Monitor.Log($"error getting weapon {weapon.id}\n{ex}", LogLevel.Error);
                                                continue;
                                            }
                                        }
                                        
                                    }
                                    if (!advancedMeleeWeapons.ContainsKey(id))
                                        advancedMeleeWeapons[id] = new List<AdvancedMeleeWeapon>();
                                    advancedMeleeWeapons[id].Add(weapon);
                                }
                                else
                                {
                                    advancedMeleeWeaponsByType[weapon.type].Add(weapon);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Monitor.Log($"error reading content.json file in content pack {contentPack.Manifest.Name}.\r\n{ex}", LogLevel.Error);
                }
            }
            Monitor.Log($"Total advanced melee weapons: {advancedMeleeWeapons.Count}", LogLevel.Debug);
            Monitor.Log($"Total advanced melee dagger attacks: {advancedMeleeWeaponsByType[1].Count}", LogLevel.Debug);
            Monitor.Log($"Total advanced melee club attacks: {advancedMeleeWeaponsByType[2].Count}", LogLevel.Debug);
            Monitor.Log($"Total advanced melee sword attacks: {advancedMeleeWeaponsByType[3].Count}", LogLevel.Debug);
        }

        public override object GetApi()
        {
            return new AdvancedMeleeFrameworkApi();
        }
        public void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            if(e.Button == Config.ReloadButton)
            {
                LoadAdvancedMeleeWeapons();
            }
        }

        public void GameLoop_UpdateTicking(object sender, StardewModdingAPI.Events.UpdateTickingEventArgs e)
        {
            //Monitor.Log($"player sprite frame {Game1.player.Sprite.currentFrame}");
            if (weaponAnimationFrame > -1 && advancedWeaponAnimating != null)
            {
                MeleeActionFrame frame = advancedWeaponAnimating.frames[weaponAnimationFrame];
                Farmer user = weaponAnimating.getLastFarmerToUse();

                if (weaponAnimationFrame == 0 && weaponAnimationTicks == 0)
                {
                    weaponStartFacingDirection = user.facingDirection.Value;
                    //Monitor.Log($"Starting animation, facing {weaponStartFacingDirection}");
                }

                if (user.CurrentTool != weaponAnimating)
                {
                    //Monitor.Log($"Switched tools to {Game1.player.CurrentTool?.DisplayName}");
                    weaponAnimating = null;
                    weaponAnimationFrame = -1; 
                    weaponAnimationTicks = 0;
                    advancedWeaponAnimating = null;
                    return;
                }
                if (frame.invincible != null)
                {
                    //Monitor.Log($"Setting invincible as {frame.invincible}");
                    user.temporarilyInvincible = (bool)frame.invincible;
                }

                if (weaponAnimationTicks == 0)
                {
                    //Monitor.Log($"Starting frame {weaponAnimationFrame}");

                    user.faceDirection((weaponStartFacingDirection + frame.relativeFacingDirection) % 4);
                    //Monitor.Log($"facing {user.getFacingDirection()}, relative {frame.relativeFacingDirection}");

                    if(frame.special != null)
                    {
                        try
                        {
                            switch (frame.special.name)
                            {
                                case "lightning":
                                    LightningStrike(user, weaponAnimating, frame.special.parameters);
                                    break;
                                case "explosion":
                                    Explosion(user, weaponAnimating, frame.special.parameters);
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                            Monitor.Log($"Exception thrown on special effect:\n{ex}", LogLevel.Error);
                        }
                    }

                    if (frame.action == WeaponAction.NORMAL)
                    {
                        //Monitor.Log($"Starting normal attack");
                        user.completelyStopAnimatingOrDoingAction();
                        user.CanMove = false;
                        user.UsingTool = true;
                        user.canReleaseTool = true;
                        weaponAnimating.setFarmerAnimating(user); 
                    }
                    else if (frame.action == WeaponAction.SPECIAL)
                    {
                        //Monitor.Log($"Starting special attack");
                        weaponAnimating.animateSpecialMove(user); 
                    }

                    if(frame.trajectoryX != 0 || frame.trajectoryY != 0)
                    {
                        float trajectoryX = frame.trajectoryX;
                        float trajectoryY = frame.trajectoryY;
                        Vector2 rawTrajectory = TranslateVector(new Vector2(trajectoryX, trajectoryY), user.FacingDirection);
                        user.setTrajectory(new Vector2(rawTrajectory.X, -rawTrajectory.Y)); // game trajectory y is backwards idek
                        //Monitor.Log($"player trajectory {user.xVelocity},{user.yVelocity}");
                    }

                    if (frame.sound != null)
                    {
                        //Monitor.Log($"Playing sound {frame.sound}");
                        user.currentLocation.playSound(frame.sound, NetAudio.SoundContext.Default);
                    }
                    foreach(AdvancedWeaponProjectile p in frame.projectiles)
                    {
                        Vector2 velocity = TranslateVector(new Vector2(p.xVelocity, p.yVelocity), user.FacingDirection);
                        Vector2 startPos = TranslateVector(new Vector2(p.startingPositionX, p.startingPositionY), user.FacingDirection);

                        int damage = advancedWeaponAnimating.type > 0 ? p.damage * myRand.Next(weaponAnimating.minDamage.Value, weaponAnimating.maxDamage.Value) : p.damage;

                        //Monitor.Log($"player position: {user.Position}, start position: { new Vector2(startingPositionX, startingPositionY) }");

                        user.currentLocation.projectiles.Add(new BasicProjectile(damage, p.parentSheetIndex, p.bouncesTillDestruct, p.tailLength, p.rotationVelocity, velocity.X, velocity.Y, user.Position + new Vector2(0, -64) + startPos, p.collisionSound, p.firingSound, p.explode, p.damagesMonsters, user.currentLocation, user, p.spriteFromObjectSheet));
                    }
                }
                if (++weaponAnimationTicks >= frame.frameTicks)
                {
                    weaponAnimationFrame++;
                    weaponAnimationTicks = 0;
                    //Monitor.Log($"Advancing to frame {weaponAnimationFrame}");
                }
                if (weaponAnimationFrame >= advancedWeaponAnimating.frames.Count)
                {
                    //Monitor.Log($"Ending weapon animation");
                    user.completelyStopAnimatingOrDoingAction();
                    user.CanMove = true;
                    user.UsingTool = false;
                    user.setTrajectory(Vector2.Zero);

                    if (user.IsLocalPlayer)
                    {
                        int cd = advancedWeaponAnimating.cooldown;
                        if (user.professions.Contains(28))
                        {
                            cd /= 2;
                        }
                        if (weaponAnimating.hasEnchantmentOfType<ArtfulEnchantment>())
                        {
                            cd /= 2;
                        }

                        switch (weaponAnimating.type.Value)
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
                    weaponAnimationFrame = -1;
                    weaponAnimating = null;
                    advancedWeaponAnimating  = null;
                    weaponAnimationTicks = 0;
                }

                return;
            }
        }

        public Vector2 TranslateVector(Vector2 vector, int facingDirection)
        {

            float outx = vector.X;
            float outy = vector.Y;
            switch (facingDirection)
            {
                case 2:
                    break;
                case 3:
                    outx = -vector.Y;
                    outy = vector.X;
                    break;
                case 0:
                    outx = -vector.X;
                    outy = -vector.Y;
                    break;
                case 1:
                    outx = vector.Y;
                    outy = -vector.X;
                    break;
            }
            return new Vector2(outx, outy);
        }
        public void LightningStrike(Farmer who, MeleeWeapon weapon, Dictionary<string, string> parameters)
        {
            int minDamage = weapon.minDamage.Value;
            int maxDamage = weapon.maxDamage.Value;
            if (parameters.ContainsKey("damageMult"))
            {
                minDamage = (int)Math.Round(weapon.minDamage.Value * float.Parse(parameters["damageMult"]));
                maxDamage = (int)Math.Round(weapon.maxDamage.Value * float.Parse(parameters["damageMult"]));
            }
            else if (parameters.ContainsKey("minDamage") && parameters.ContainsKey("maxDamage"))
            {
                minDamage = int.Parse(parameters["minDamage"]);
                maxDamage = int.Parse(parameters["maxDamage"]);
            }

            int radius = int.Parse(parameters["radius"]);

            Vector2 playerLocation = who.position;
            GameLocation currentLocation = who.currentLocation;
            Farm.LightningStrikeEvent lightningEvent = new Farm.LightningStrikeEvent();
            lightningEvent.bigFlash = true;
            lightningEvent.createBolt = true;

            Vector2 offset = Vector2.Zero;
            if (parameters.ContainsKey("offsetX") && parameters.ContainsKey("offsetY"))
            {
                float x = float.Parse(parameters["offsetX"]);
                float y = float.Parse(parameters["offsetY"]);
                offset = TranslateVector(new Vector2(x, y), who.FacingDirection);
            }
            lightningEvent.boltPosition = playerLocation + new Vector2(32f, 32f) + offset;
            Game1.flashAlpha = (float)(0.5 + Game1.random.NextDouble());
            
            if(parameters.ContainsKey("sound"))
                Game1.playSound(parameters["sound"]);
            
            Utility.drawLightningBolt(lightningEvent.boltPosition, currentLocation);

            currentLocation.damageMonster(new Rectangle((int)Math.Round(playerLocation.X - radius), (int)Math.Round(playerLocation.Y - radius), radius * 2, radius * 2), minDamage, maxDamage, false, who);
        }
        public void Explosion(Farmer user, MeleeWeapon weapon, Dictionary<string, string> parameters)
        {
            Vector2 tileLocation = user.getTileLocation();
            if(parameters.ContainsKey("tileOffsetX") && parameters.ContainsKey("tileOffsetY")) 
                tileLocation += TranslateVector(new Vector2(float.Parse(parameters["tileOffsetX"]), float.Parse(parameters["tileOffsetY"])), user.facingDirection);
            int radius = int.Parse(parameters["radius"]);
            
            int damage;
            if (parameters.ContainsKey("damageMult"))
            {
                damage = (int)Math.Round(Game1.random.Next(weapon.minDamage.Value, weapon.maxDamage.Value + 1) * float.Parse(parameters["damageMult"]));
            }
            else if (parameters.ContainsKey("minDamage") && parameters.ContainsKey("maxDamage"))
            {
                damage = Game1.random.Next(int.Parse(parameters["minDamage"]), int.Parse(parameters["maxDamage"]) + 1);

            }
            else
                damage = Game1.random.Next(weapon.minDamage.Value, weapon.maxDamage.Value + 1);

            user.currentLocation.explode(tileLocation, radius, user, false, damage);
        }

        public static AdvancedMeleeWeapon GetAdvancedWeapon(MeleeWeapon weapon, Farmer user)
        {
            AdvancedMeleeWeapon advancedMeleeWeapon = null;
            if (advancedMeleeWeapons.ContainsKey(weapon.InitialParentTileIndex))
            {
                int skillLevel = -1;
                foreach (AdvancedMeleeWeapon amw in advancedMeleeWeapons[weapon.InitialParentTileIndex])
                {
                    if (user == null || (amw.skillLevel <= user.getEffectiveSkillLevel(4) && amw.skillLevel > skillLevel))
                    {
                        skillLevel = amw.skillLevel;
                        advancedMeleeWeapon = amw;
                    }
                }
            }
            if (advancedMeleeWeapon == null && advancedMeleeWeaponsByType.ContainsKey(weapon.type.Value) && user != null)
            {
                int skillLevel = -1;
                foreach(AdvancedMeleeWeapon amw in advancedMeleeWeaponsByType[weapon.type.Value])
                {
                    if(amw.skillLevel <= user.getEffectiveSkillLevel(4) && amw.skillLevel > skillLevel)
                    {
                        skillLevel = amw.skillLevel;
                        advancedMeleeWeapon = amw;
                    }
                }
            }

            return advancedMeleeWeapon;
        }
    }
}
