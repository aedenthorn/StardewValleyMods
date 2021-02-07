using Harmony;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Monsters;
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
        private static ModConfig Config;
        private static Random myRand;
        private static IMonitor SMonitor;
        private static IModHelper SHelper;
        private static Dictionary<int, List<AdvancedMeleeWeapon>> advancedMeleeWeapons = new Dictionary<int, List<AdvancedMeleeWeapon>>();
        private static Dictionary<int, List<AdvancedMeleeWeapon>> advancedMeleeWeaponsByType = new Dictionary<int, List<AdvancedMeleeWeapon>>() 
        {
            {1, new List<AdvancedMeleeWeapon>() },
            {2, new List<AdvancedMeleeWeapon>() },
            {3, new List<AdvancedMeleeWeapon>() }
        };
        private static int weaponAnimationFrame = -1;
        private int weaponAnimationTicks;
        private static MeleeWeapon weaponAnimating;
        private static int weaponStartFacingDirection;
        private static AdvancedMeleeWeapon advancedWeaponAnimating = null;
        private static IJsonAssetsApi mJsonAssets;

        public override void Entry(IModHelper helper)
        {
            context = this;
            Config = Helper.ReadConfig<ModConfig>();
            if (!Config.EnableMod)
                return;

            SMonitor = Monitor;
            SHelper = Helper;

            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            Helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            Helper.Events.GameLoop.UpdateTicking += GameLoop_UpdateTicking;
            Helper.Events.Input.ButtonPressed += Input_ButtonPressed;

            myRand = new Random();

            var harmony = HarmonyInstance.Create(ModManifest.UniqueID);

            harmony.Patch(
                original: AccessTools.Method(typeof(MeleeWeapon), "doAnimateSpecialMove"),
                prefix: new HarmonyMethod(typeof(ModEntry), nameof(doAnimateSpecialMove_Prefix))
            );

            ConstructorInfo ci = typeof(MeleeWeapon).GetConstructor(new Type[] { typeof(int) });
            harmony.Patch(
               original: ci,
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.MeleeWeapon_Postfix))
            );
            ci = typeof(MeleeWeapon).GetConstructor(new Type[] { });
            harmony.Patch(
               original: ci,
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.MeleeWeapon_Postfix))
            );
            
            harmony.Patch(
                original: AccessTools.Method(typeof(MeleeWeapon), nameof(MeleeWeapon.drawInMenu), new Type[] { typeof(SpriteBatch), typeof(Vector2), typeof(float), typeof(float), typeof(float), typeof(StackDrawType), typeof(Color), typeof(bool) }),
                prefix: new HarmonyMethod(typeof(ModEntry), nameof(drawInMenu_Prefix)),
                postfix: new HarmonyMethod(typeof(ModEntry), nameof(drawInMenu_Postfix))
            );

        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            mJsonAssets = Helper.ModRegistry.GetApi<IJsonAssetsApi>("spacechase0.JsonAssets");

        }

        private void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            LoadAdvancedMeleeWeapons();
        }

        private void LoadAdvancedMeleeWeapons()
        {
            advancedMeleeWeapons.Clear();
            advancedMeleeWeaponsByType[1].Clear();
            advancedMeleeWeaponsByType[2].Clear();
            advancedMeleeWeaponsByType[3].Clear();
            foreach (IContentPack contentPack in SHelper.ContentPacks.GetOwned())
            {
                SMonitor.Log($"Reading content pack: {contentPack.Manifest.Name} {contentPack.Manifest.Version} from {contentPack.DirectoryPath}", LogLevel.Debug);
                try
                {
                    AdvancedMeleeWeaponData json = contentPack.ReadJsonFile<AdvancedMeleeWeaponData>("content.json") ?? null;

                    if (json != null)
                    {
                        if (json.weapons != null && json.weapons.Count > 0)
                        {
                            foreach (AdvancedMeleeWeapon weapon in json.weapons)
                            {
                                if (weapon.type == 0)
                                {
                                    SMonitor.Log($"Adding specific weapon {weapon.id}");
                                    if (!int.TryParse(weapon.id, out int id))
                                    {
                                        SMonitor.Log($"Got name instead of id {weapon.id}");
                                        try
                                        {
                                            id = Helper.Content.Load<Dictionary<int, string>>("Data/weapons", ContentSource.GameContent).First(p => p.Value.StartsWith($"{weapon.id}/")).Key;
                                            SMonitor.Log($"Got name-based id {id}");
                                        }
                                        catch (Exception ex)
                                        {
                                            if (mJsonAssets != null)
                                            {
                                                id = mJsonAssets.GetWeaponId(weapon.id);
                                                if(id == -1)
                                                {
                                                    SMonitor.Log($"error getting JSON Assets weapon {weapon.id}\n{ex}", LogLevel.Error);
                                                    continue;
                                                }
                                                SMonitor.Log($"Added JA weapon {weapon.id}, id {id}");
                                            }
                                            else
                                            {
                                                SMonitor.Log($"error getting weapon {weapon.id}\n{ex}", LogLevel.Error);
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
                    SMonitor.Log($"error reading content.json file in content pack {contentPack.Manifest.Name}.\r\n{ex}", LogLevel.Error);
                }
            }
            SMonitor.Log($"Total advanced melee weapons: {advancedMeleeWeapons.Count}", LogLevel.Debug);
            SMonitor.Log($"Total advanced melee dagger attacks: {advancedMeleeWeaponsByType[1].Count}", LogLevel.Debug);
            SMonitor.Log($"Total advanced melee club attacks: {advancedMeleeWeaponsByType[2].Count}", LogLevel.Debug);
            SMonitor.Log($"Total advanced melee sword attacks: {advancedMeleeWeaponsByType[3].Count}", LogLevel.Debug);
        }

        public override object GetApi()
        {
            return new AdvancedMeleeFrameworkApi();
        }
        private void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            if(e.Button == Config.ReloadButton)
            {
                LoadAdvancedMeleeWeapons();
            }
        }

        private void GameLoop_UpdateTicking(object sender, StardewModdingAPI.Events.UpdateTickingEventArgs e)
        {
            //SMonitor.Log($"player sprite frame {Game1.player.Sprite.currentFrame}");
            if (weaponAnimationFrame > -1 && advancedWeaponAnimating != null)
            {
                MeleeActionFrame frame = advancedWeaponAnimating.frames[weaponAnimationFrame];
                Farmer user = weaponAnimating.getLastFarmerToUse();

                if (weaponAnimationFrame == 0 && weaponAnimationTicks == 0)
                {
                    weaponStartFacingDirection = user.facingDirection.Value;
                    //SMonitor.Log($"Starting animation, facing {weaponStartFacingDirection}");
                }

                if (user.CurrentTool != weaponAnimating)
                {
                    //SMonitor.Log($"Switched tools to {Game1.player.CurrentTool?.DisplayName}");
                    weaponAnimating = null;
                    weaponAnimationFrame = -1; 
                    weaponAnimationTicks = 0;
                    advancedWeaponAnimating = null;
                    return;
                }
                if (frame.invincible != null)
                {
                    //SMonitor.Log($"Setting invincible as {frame.invincible}");
                    user.temporarilyInvincible = (bool)frame.invincible;
                }

                if (weaponAnimationTicks == 0)
                {
                    //SMonitor.Log($"Starting frame {weaponAnimationFrame}");

                    user.faceDirection((weaponStartFacingDirection + frame.relativeFacingDirection) % 4);
                    //SMonitor.Log($"facing {user.getFacingDirection()}, relative {frame.relativeFacingDirection}");

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
                        //SMonitor.Log($"Starting normal attack");
                        user.completelyStopAnimatingOrDoingAction();
                        user.CanMove = false;
                        user.UsingTool = true;
                        user.canReleaseTool = true;
                        weaponAnimating.setFarmerAnimating(user); 
                    }
                    else if (frame.action == WeaponAction.SPECIAL)
                    {
                        //SMonitor.Log($"Starting special attack");
                        weaponAnimating.animateSpecialMove(user); 
                    }

                    if(frame.trajectoryX != 0 || frame.trajectoryY != 0)
                    {
                        float trajectoryX = frame.trajectoryX;
                        float trajectoryY = frame.trajectoryY;
                        Vector2 rawTrajectory = TranslateVector(new Vector2(trajectoryX, trajectoryY), user.FacingDirection);
                        user.setTrajectory(new Vector2(rawTrajectory.X, -rawTrajectory.Y)); // game trajectory y is backwards idek
                        //SMonitor.Log($"player trajectory {user.xVelocity},{user.yVelocity}");
                    }

                    if (frame.sound != null)
                    {
                        //SMonitor.Log($"Playing sound {frame.sound}");
                        user.currentLocation.playSound(frame.sound, NetAudio.SoundContext.Default);
                    }
                    foreach(WeaponProjectile p in frame.projectiles)
                    {
                        Vector2 velocity = TranslateVector(new Vector2(p.xVelocity, p.yVelocity), user.FacingDirection);
                        Vector2 startPos = TranslateVector(new Vector2(p.startingPositionX, p.startingPositionY), user.FacingDirection);

                        int damage = advancedWeaponAnimating.type > 0 ? p.damage * myRand.Next(weaponAnimating.minDamage, weaponAnimating.maxDamage) : p.damage;

                        //SMonitor.Log($"player position: {user.Position}, start position: { new Vector2(startingPositionX, startingPositionY) }");

                        user.currentLocation.projectiles.Add(new BasicProjectile(damage, p.parentSheetIndex, p.bouncesTillDestruct, p.tailLength, p.rotationVelocity, velocity.X, velocity.Y, user.Position + new Vector2(0, -64) + startPos, p.collisionSound, p.firingSound, p.explode, p.damagesMonsters, user.currentLocation, user, p.spriteFromObjectSheet));
                    }
                }
                if (++weaponAnimationTicks >= frame.frameTicks)
                {
                    weaponAnimationFrame++;
                    weaponAnimationTicks = 0;
                    //SMonitor.Log($"Advancing to frame {weaponAnimationFrame}");
                }
                if (weaponAnimationFrame >= advancedWeaponAnimating.frames.Count)
                {
                    //SMonitor.Log($"Ending weapon animation");
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

        private static void MeleeWeapon_Postfix(MeleeWeapon __instance)
        {
            //context.Monitor.Log($"created melee weapon {__instance.Name} {__instance.InitialParentTileIndex} {__instance.ParentSheetIndex}");

            AdvancedMeleeWeapon amw = GetAdvancedWeapon(__instance, null);
            if(amw != null && amw.enchantments != null)
            {
                foreach(AdvancedEnchantmentData aed in amw.enchantments)
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
                            AdvancedEnchantment ae = new AdvancedEnchantment(__instance, amw, aed);
                            __instance.enchantments.Add(ae);
                            break;
                    }
                    context.Monitor.Log($"added enchantment {aed.type} to {__instance.Name} {__instance.enchantments.Count}");
                }
            }
        }
        
        private static bool doAnimateSpecialMove_Prefix(MeleeWeapon __instance, Farmer ___lastUser)
        {
            SMonitor.Log($"Special move for {__instance.Name}, id {__instance.InitialParentTileIndex}");

            if (Config.RequireModKey && !SHelper.Input.IsDown(Config.ModKey))
                return true;

            advancedWeaponAnimating = GetAdvancedWeapon(__instance, ___lastUser);

            if (weaponAnimationFrame > -1 || advancedWeaponAnimating == null)
                return true;

            if (___lastUser == null || ___lastUser.CurrentTool != __instance)
            {
                return false;
            }

            SMonitor.Log($"Animating {__instance.DisplayName}");

            if (___lastUser.isEmoteAnimating)
            {
                ___lastUser.EndEmoteAnimation();
            }
            weaponStartFacingDirection = ___lastUser.getFacingDirection();
            weaponAnimationFrame = 0;
            weaponAnimating = __instance;
            return false;
        }
        private static void drawInMenu_Prefix(MeleeWeapon __instance, ref int __state)
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
        private static void drawInMenu_Postfix(MeleeWeapon __instance, int __state)
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

        private Vector2 TranslateVector(Vector2 vector, int facingDirection)
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
        private void LightningStrike(Farmer who, MeleeWeapon weapon, Dictionary<string, string> parameters)
        {
            int minDamage = weapon.minDamage;
            int maxDamage = weapon.maxDamage;
            if (parameters.ContainsKey("damageMult"))
            {
                minDamage = (int)Math.Round(weapon.minDamage * float.Parse(parameters["damageMult"]));
                maxDamage = (int)Math.Round(weapon.maxDamage * float.Parse(parameters["damageMult"]));
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
        private void Explosion(Farmer user, MeleeWeapon weapon, Dictionary<string, string> parameters)
        {
            Vector2 tileLocation = user.getTileLocation();
            if(parameters.ContainsKey("tileOffsetX") && parameters.ContainsKey("tileOffsetY")) 
                tileLocation += TranslateVector(new Vector2(float.Parse(parameters["tileOffsetX"]), float.Parse(parameters["tileOffsetY"])), user.facingDirection);
            int radius = int.Parse(parameters["radius"]);
            
            int damage;
            if (parameters.ContainsKey("damageMult"))
            {
                damage = (int)Math.Round(Game1.random.Next(weapon.minDamage, weapon.maxDamage + 1) * float.Parse(parameters["damageMult"]));
            }
            else if (parameters.ContainsKey("minDamage") && parameters.ContainsKey("maxDamage"))
            {
                damage = Game1.random.Next(int.Parse(parameters["minDamage"]), int.Parse(parameters["maxDamage"]) + 1);

            }
            else
                damage = Game1.random.Next(weapon.minDamage, weapon.maxDamage + 1);

            user.currentLocation.explode(tileLocation, radius, user, false, damage);
        }

        private static AdvancedMeleeWeapon GetAdvancedWeapon(MeleeWeapon weapon, Farmer user)
        {
            AdvancedMeleeWeapon advancedMeleeWeapon = null;
            if (advancedMeleeWeapons.ContainsKey(weapon.InitialParentTileIndex))
            {
                int skillLevel = -1;
                foreach (AdvancedMeleeWeapon amw in advancedMeleeWeapons[weapon.initialParentTileIndex])
                {
                    if (user == null || (amw.skillLevel <= user.getEffectiveSkillLevel(4) && amw.skillLevel > skillLevel))
                    {
                        skillLevel = amw.skillLevel;
                        advancedMeleeWeapon = amw;
                    }
                }
            }
            if (advancedMeleeWeapon == null && advancedMeleeWeaponsByType.ContainsKey(weapon.type) && user != null)
            {
                int skillLevel = -1;
                foreach(AdvancedMeleeWeapon amw in advancedMeleeWeaponsByType[weapon.type])
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
