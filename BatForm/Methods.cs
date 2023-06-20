using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Object = StardewValley.Object;

namespace BatForm
{
    public partial class ModEntry
    {
        public static void TransformBat()
        {
            var status = BatFormStatus(Game1.player);
            if (status == BatForm.Inactive || status == BatForm.SwitchingFrom)
                status = BatForm.SwitchingTo;
            else
                status = BatForm.SwitchingFrom;
            Game1.player.modData[batFormKey] = status + "";
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
        private void ResetBat()
        {
            Game1.player.modData.Remove(batFormKey);
            height.Value = 0;
            Game1.forceSnapOnNextViewportUpdate = true;
            Game1.game1.refreshWindowSettings();
            if(Game1.player is not null)
            {
                Game1.player.ignoreCollisions = false;
            }
        }

        private static BatForm BatFormStatus(Farmer player)
        {
            if (!Config.ModEnabled || !player.modData.TryGetValue(batFormKey, out string str))
                return BatForm.Inactive;
            return Enum.Parse<BatForm>(str);

        }

        private static void EnforceMapBounds()
        {
            if (Game1.isWarping || Game1.currentLocation == null)
                return;

            xTile.Dimensions.Location tileLocation = Game1.player.nextPositionTile();
            int nextTilePositionX = tileLocation.X;
            int nextTilePositionY = tileLocation.Y;
            xTile.Dimensions.Location location = Game1.player.nextPositionPoint();
            int nextPositionX = location.X;
            int nextPositionY = location.Y;

            if (nextTilePositionX == Game1.player.getTileX() && nextTilePositionY == Game1.player.getTileY())
            {
                if (nextPositionX < 0)
                    nextTilePositionX--;
                if (nextPositionY < 0)
                    nextTilePositionY--;
            }

            foreach (Warp warp in Game1.currentLocation.warps)
            {
                if (warp.X == nextTilePositionX && warp.Y == nextTilePositionY)
                    return;
            }

            const int offsetX = 0;
            const int offsetY = Game1.tileSize / 4;
            int maxX = Game1.currentLocation.map.DisplaySize.Width - Game1.tileSize;
            int maxY = Game1.currentLocation.map.DisplaySize.Height - Game1.tileSize + offsetY;

            if (Game1.player.position.X < offsetX)
                Game1.player.position.X = offsetX;
            if (Game1.player.position.X > maxX)
                Game1.player.position.X = maxX;
            if (Game1.player.position.Y < offsetY)
                Game1.player.position.Y = offsetY;
            if (Game1.player.position.Y > maxY)
                Game1.player.position.Y = maxY;
        }
    }
}