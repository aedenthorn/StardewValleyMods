using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;

namespace DinoForm
{
    public partial class ModEntry
    {
        public static void Transform()
        {
            var status = DinoFormStatus(Game1.player);
            if (status == DinoForm.Inactive)
                status = DinoForm.Active;
            else
                status = DinoForm.Inactive;
            Game1.player.modData[DinoFormKey] = status + "";
            ModEntry.buffAPI.UpdateBuffs();
        }
        public static void PlayTransform()
        {
            List<TemporaryAnimatedSprite> sprites = new List<TemporaryAnimatedSprite>();
            if (Game1.random.NextDouble() < 0.5)
                sprites.Add(new TemporaryAnimatedSprite(362, (float)Game1.random.Next(30, 90), 6, 1, new Vector2(Game1.player.getTileLocation().X * 64f, Game1.player.getTileLocation().Y * 64f), false, Game1.random.NextDouble() < 0.5));
            else
                sprites.Add(new TemporaryAnimatedSprite(362, (float)Game1.random.Next(30, 90), 6, 1, new Vector2(Game1.player.getTileLocation().X * 64f, Game1.player.getTileLocation().Y * 64f), false, Game1.random.NextDouble() < 0.5));
            ((Multiplayer)AccessTools.Field(typeof(Game1), "multiplayer").GetValue(null)).broadcastSprites(Game1.player.currentLocation, sprites);
            if(!string.IsNullOrEmpty(Config.TransformSound))
                Game1.player.currentLocation.playSound(Config.TransformSound);
        }

        private void ResetForm()
        {
            Game1.player.modData.Remove(DinoFormKey);
        }

        private static DinoForm DinoFormStatus(Farmer player)
        {
            if (!Config.ModEnabled || !Context.IsWorldReady || !player.modData.TryGetValue(DinoFormKey, out string str))
                return DinoForm.Inactive;
            return Enum.Parse<DinoForm>(str);

        }
        public static Rectangle GetSourceRect(Farmer farmer)
        {
            int index;
            int speed = (int)Math.Ceiling(frameOffset.Value / 12f);
            switch (farmer.FacingDirection)
            {
                case 0:
                    if (breathingFire.Value)
                    {
                        index = 24;
                    }
                    else
                    {
                        index = 8 + speed % 4;
                    }
                    break;
                case 1:
                    if (breathingFire.Value)
                    {
                        index = 20;
                    }
                    else
                    {
                        index = 4 + speed % 4;
                    }
                    break;
                case 2:
                    if (breathingFire.Value)
                    {
                        index = 16;
                    }
                    else
                    {
                        index = speed % 4;
                    }
                    break;
                default:
                    if (breathingFire.Value)
                    {
                        index = 28;
                    }
                    else
                    {
                        index = 12 + speed % 4;
                    }
                    break;
            }
            return new Rectangle(index % 4 * 32, index / 4 * 32, 32, 32);
        }
    }
}