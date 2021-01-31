using Harmony;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.Projectiles;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using Object = StardewValley.Object;

namespace AdvancedMeleeFramework
{
    public class ModEntry : Mod 
    {
        public static ModEntry context;
        private static ModConfig Config;
        private static Random myRand;
        private static IMonitor SMonitor;
        private static IModHelper SHelper;
        private static Dictionary<int,AdvancedMeleeWeapon> advancedMeleeWeapons = new Dictionary<int, AdvancedMeleeWeapon>();
        private static int weaponAnimationFrame = -1;
        private int weaponAnimationTicks;
        private static MeleeWeapon weaponAnimating;
        private static int weaponStartFacingDirection;

        public override void Entry(IModHelper helper)
        {
            context = this;
            Config = Helper.ReadConfig<ModConfig>();
            if (!Config.EnableMod)
                return;

            SMonitor = Monitor;
            SHelper = Helper;

            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            Helper.Events.GameLoop.UpdateTicking += GameLoop_UpdateTicking;
            Helper.Events.Input.ButtonPressed += Input_ButtonPressed;

            myRand = new Random();

            var harmony = HarmonyInstance.Create(ModManifest.UniqueID);

            harmony.Patch(
                original: AccessTools.Method(typeof(MeleeWeapon), "doAnimateSpecialMove"),
                prefix: new HarmonyMethod(typeof(ModEntry), nameof(doAnimateSpecialMove_Prefix))
            );

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
            if (weaponAnimationFrame > -1)
            {
                AdvancedMeleeWeapon weapon = advancedMeleeWeapons[weaponAnimating.InitialParentTileIndex];
                MeleeActionFrame frame = weapon.frames[weaponAnimationFrame];
                Farmer user = weaponAnimating.getLastFarmerToUse();

                if (weaponAnimationFrame == 0 && weaponAnimationTicks == 0)
                {
                    weaponStartFacingDirection = user.facingDirection.Value;
                    SMonitor.Log($"Starting animation, facing {weaponStartFacingDirection}");
                }

                if (false && user.CurrentTool != weaponAnimating)
                {
                    SMonitor.Log($"Switched tools to {Game1.player.CurrentTool?.DisplayName}");
                    weaponAnimating = null;
                    weaponAnimationFrame = -1; 
                    weaponAnimationTicks = 0;
                    return;
                }
                
                if (weaponAnimationTicks == 0)
                {
                    SMonitor.Log($"Starting frame {weaponAnimationFrame}");

                    user.faceDirection((weaponStartFacingDirection + frame.relativeFacingDirection) % 4);
                    SMonitor.Log($"facing {user.getFacingDirection()}, relative {frame.relativeFacingDirection}");

                    if (frame.action == WeaponAction.NORMAL)
                    {
                        SMonitor.Log($"Starting normal attack");
                        user.completelyStopAnimatingOrDoingAction();
                        user.CanMove = false;
                        user.UsingTool = true;
                        user.canReleaseTool = true;
                        weaponAnimating.setFarmerAnimating(user); 
                    }
                    else if (frame.action == WeaponAction.SPECIAL)
                    {
                        SMonitor.Log($"Starting special attack");
                        weaponAnimating.animateSpecialMove(user); 
                    }

                    if(frame.trajectoryX != 0 || frame.trajectoryY != 0)
                    {
                        float trajectoryX = frame.trajectoryX;
                        float trajectoryY = frame.trajectoryY;
                        TranslateVector(ref trajectoryX, ref trajectoryY, user.FacingDirection);
                        user.setTrajectory(new Vector2(trajectoryX, -trajectoryY));
                    }

                    if (frame.sound != null)
                    {
                        SMonitor.Log($"Playing sound {frame.sound}");
                        user.currentLocation.playSound(frame.sound, NetAudio.SoundContext.Default);
                    }
                    foreach(WeaponProjectile p in frame.projectiles)
                    {
                        float xVelocity = p.xVelocity;
                        float yVelocity = p.yVelocity;
                        float startingPositionX = p.startingPositionX;
                        float startingPositionY = p.startingPositionY;
                        TranslateVector(ref xVelocity, ref yVelocity, user.FacingDirection);
                        TranslateVector(ref startingPositionX, ref startingPositionY, user.FacingDirection);

                        SMonitor.Log($"player position: {user.Position}, start position: { new Vector2(startingPositionX, startingPositionY) }");

                        user.currentLocation.projectiles.Add(new BasicProjectile(p.damage, p.parentSheetIndex, p.bouncesTillDestruct, p.tailLength, p.rotationVelocity, xVelocity, yVelocity, user.Position + new Vector2(0, -64) + new Vector2(startingPositionX,startingPositionY), p.collisionSound, p.firingSound, p.explode, p.damagesMonsters, user.currentLocation, user, p.spriteFromObjectSheet));
                    }
                }
                if (++weaponAnimationTicks >= frame.frameTicks)
                {
                    weaponAnimationFrame++;
                    weaponAnimationTicks = 0;
                    SMonitor.Log($"Advancing to frame {weaponAnimationFrame}");
                }
                if (weaponAnimationFrame >= weapon.frames.Count)
                {
                    SMonitor.Log($"Ending weapon animation");
                    user.completelyStopAnimatingOrDoingAction();
                    user.CanMove = true;
                    user.UsingTool = false;

                    if (user.IsLocalPlayer)
                    {
                        int cd = weapon.cooldown;
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
                                break;
                                MeleeWeapon.clubCooldown = cd;
                            case 3:
                                MeleeWeapon.defenseCooldown = cd;
                                break;
                        }
                    }
                    weaponAnimationFrame = -1;
                    weaponAnimating = null;
                    weaponAnimationTicks = 0;
                }

                return;
            }
        }

        private void TranslateVector(ref float x, ref float y, int facingDirection)
        {

            float outx = x;
            float outy = y;
            switch (facingDirection)
            {
                case 2:
                    break;
                case 3:
                    outx = -y;
                    outy = x;
                    break;
                case 0:
                    outx = -x;
                    outy = -y;
                    break;
                case 1:
                    outx = y;
                    outy = -x;
                    break;
            }
            x = outx;
            y = outy;
        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            LoadAdvancedMeleeWeapons();
        }

        private void LoadAdvancedMeleeWeapons()
        {
            advancedMeleeWeapons.Clear();
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
                                if (!int.TryParse(weapon.id, out int id))
                                {
                                    try
                                    {
                                        id = Helper.Content.Load<Dictionary<int, string>>("Data/weapons", ContentSource.GameContent).First(p => p.Value.StartsWith($"{weapon.id}/")).Key;
                                        advancedMeleeWeapons.Add(id, weapon);
                                    }
                                    catch
                                    {
                                        SMonitor.Log($"error getting weapon {weapon.id}", LogLevel.Error);
                                        continue;
                                    }
                                }
                                advancedMeleeWeapons.Add(id, weapon);
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
        }

        public override object GetApi()
        {
            return new AdvancedMeleeFrameworkApi();
        }
        private static bool doAnimateSpecialMove_Prefix(MeleeWeapon __instance, Farmer ___lastUser)
        {
            SMonitor.Log($"Special move for {__instance.Name}, id {__instance.InitialParentTileIndex}");

            if (!advancedMeleeWeapons.ContainsKey(__instance.InitialParentTileIndex) || weaponAnimationFrame > -1 || (Config.RequireModKey && !SHelper.Input.IsDown(Config.ModKey)))
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

    }
}
