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
    }
}