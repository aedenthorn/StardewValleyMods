using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Network;
using StardewValley.Projectiles;
using System;

namespace FireBreath
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {

        public static ModConfig Config;
        private bool firing = false;

        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            if (!Config.Enabled)
                return;

            Helper.Events.Input.ButtonPressed += Input_ButtonPressed;
            Helper.Events.Input.ButtonReleased += Input_ButtonReleased;
            Helper.Events.GameLoop.UpdateTicking += GameLoop_UpdateTicking;
        }

        private int ticks;

        private void GameLoop_UpdateTicking(object sender, StardewModdingAPI.Events.UpdateTickingEventArgs e)
        {
            if (Game1.player?.currentLocation != null && firing)
            {
                if(ticks % 120 == 0 && Config.FireSound != "")
                {
                    Game1.player.currentLocation.playSound(Config.FireSound, NetAudio.SoundContext.Default);
                }
                if(ticks++ % 3 != 0)
                {
                    return;
                }
                float fire_angle = 0f;
                float mouthOffset = 16f;
                Vector2 shot_origin = new Vector2((float)Game1.player.GetBoundingBox().Center.X - 32f, (float)Game1.player.GetBoundingBox().Center.Y - 80f);
                switch (Game1.player.facingDirection.Value)
                {
                    case 0:
                        shot_origin.Y -= mouthOffset;
                        fire_angle = 90f;
                        break;
                    case 1:
                        shot_origin.X += mouthOffset;
                        fire_angle = 0f;
                        break;
                    case 2:
                        fire_angle = 270f;
                        shot_origin.Y += mouthOffset;
                        break;
                    case 3:
                        shot_origin.X -= mouthOffset;
                        fire_angle = 180f;
                        break;
                }
                fire_angle += (float)Math.Sin((double)((float)ticks * 16 / 1000f * 180f) * 3.1415926535897931 / 180.0) * 25f;
                Vector2 shot_velocity = new Vector2((float)Math.Cos((double)fire_angle * 3.1415926535897931 / 180.0), -(float)Math.Sin((double)fire_angle * 3.1415926535897931 / 180.0));
                shot_velocity *= 10f;
                BasicProjectile projectile = new BasicProjectile((int)Math.Round(Config.FireDamage * (Config.ScaleWithSkill ? Game1.player.getEffectiveSkillLevel(4) / 10f : 1)), 10, 0, 1, 0.196349546f, shot_velocity.X, shot_velocity.Y, shot_origin, "", "", false, true, Game1.player.currentLocation, Game1.player, false, null);
                projectile.ignoreTravelGracePeriod.Value = true;
                projectile.maxTravelDistance.Value = (int)Math.Round(Config.FireDistance * (Config.ScaleWithSkill ? Game1.player.getEffectiveSkillLevel(4) / 10f : 1));
                Game1.player.currentLocation.projectiles.Add(projectile);
            }
            else
            {
                ticks = 0;
            }
        }

        private void Input_ButtonReleased(object sender, StardewModdingAPI.Events.ButtonReleasedEventArgs e)
        {
            if(e.Button == Config.FireButton)
            {
                Monitor.Log("End fire breath");
                firing = false;
            }
        }

        private void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            if (e.Button == Config.FireButton)
            {
                Monitor.Log($"Begin fire breath, skill level {Game1.player.getEffectiveSkillLevel(4)}");
                firing = true;
            }
        }

    }
}