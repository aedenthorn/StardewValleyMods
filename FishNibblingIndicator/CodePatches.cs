using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Minigames;
using StardewValley.Tools;
using System;

namespace FishNibblingIndicator
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(TemporaryAnimatedSpriteList), nameof(TemporaryAnimatedSpriteList.Add))]
        public static class GameLocation_performAction_Patch
        {
            public static void Prefix(ref TemporaryAnimatedSprite item)
            {
                if (!Config.ModEnabled || Game1.player.CurrentTool is not FishingRod f || !f.isNibbling || item.textureName != "LooseSprites\\Cursors" || item.sourceRect != new Rectangle(395, 497, 3, 8))
                    return;
                SMonitor.Log("Changing nibble sprite");
                item.textureName = Config.SourceTexture;
                AccessTools.Method(typeof(TemporaryAnimatedSprite), "loadTexture").Invoke(item, Array.Empty<object>());
                item.sourceRect = new Rectangle(Config.SourceX, Config.SourceY, Config.SourceW, Config.SourceH);
                item.sourceRectStartingPos = new Vector2((float)Config.SourceX, (float)Config.SourceY);
                item.scale = Config.Scale;
                item.alphaFade = Config.AlphaFade;
                item.scaleChange = Config.ScaleChange;
                item.motion = new(Config.MotionX, Config.MotionY);
                item.position += new Vector2(Config.OffsetX, Config.OffsetY);
                item.initialPosition = item.position;
            }
        }
    }
}