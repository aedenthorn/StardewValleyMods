using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using xTile.Dimensions;

namespace ThrowableBombs
{
    public partial class ModEntry
    {
        public static int[] bombIndexes = { 286, 287, 288 };

        public static Dictionary<int, BombData> bombDict = new Dictionary<int, BombData>();

        [HarmonyPatch(typeof(Game1), nameof(Game1.pressActionButton))]
        public class Game1_pressActionButton_Patch
        {
            public static bool Prefix(ref bool __result)
            {
                if (!Config.ModEnabled || !Context.IsPlayerFree || Game1.player.ActiveObject is null || Game1.player.ActiveObject.bigCraftable.Value || !bombIndexes.Contains(Game1.player.ActiveObject.ParentSheetIndex))
                    return true;
                Vector2 cursorLoc = new Vector2(Game1.getOldMouseX() + Game1.viewport.X, Game1.getOldMouseY() + Game1.viewport.Y);
                Vector2 playerLoc = Game1.player.Position + new Vector2(0, -128);
                Vector2 dir = cursorLoc - playerLoc;
                float speed = 0.05f;
                Vector2 decel = new Vector2(-0.0013f * dir.X, -0.0013f * dir.Y);
                var multiplayer = AccessTools.StaticFieldRefAccess<Game1, Multiplayer>("multiplayer");
                var location = Game1.player.currentLocation;
                var parentSheetIndex = Game1.player.ActiveObject.ParentSheetIndex;
                float y = playerLoc.Y / 64f;
                int idNum = Game1.random.Next();
                while (bombDict.ContainsKey(idNum))
                    idNum = Game1.random.Next();


                location.playSound("thudStep", NetAudio.SoundContext.Default);
                location.netAudio.StartPlaying("fuse");
                Vector2 offset = Vector2.Zero;
                switch (parentSheetIndex)
                {
                    case 286:
                        {
                            offset = new Vector2(5f, 3f) * 4f;
                            break;
                        }
                    case 287:
                        {
                            break;
                        }
                    case 288:
                        {
                            offset = new Vector2(5f, 0f) * 4f;
                            break;
                        }
                }

                bombDict.Add(idNum, new BombData() { location = location, startPos = playerLoc + offset, currentPos = playerLoc + offset, endPos = cursorLoc });

                multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite[]
                {
                    new TemporaryAnimatedSprite(parentSheetIndex, 100f, 1, 24, playerLoc, true, false, location, Game1.player)
                    {
                        shakeIntensity = 0.5f,
                        shakeIntensityChange = 0.002f,
                        extraInfoForEndBehavior = idNum,
                        endFunction = new TemporaryAnimatedSprite.endBehavior(location.removeTemporarySpritesWithID)
                    }
                });
                multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite[]
                {
                    new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(598, 1279, 3, 4), 53f, 5, 9, playerLoc + offset, true, false, (float)(y + 7) / 10000f, 0f, Color.Yellow, 4f, 0f, 0f, 0f, false)
                    {
                        id = (float)idNum
                    }
                });
                multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite[]
                {
                    new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(598, 1279, 3, 4), 53f, 5, 9, playerLoc + new Vector2(5f, 3f) * 4f, true, true, (float)(y + 7) / 10000f, 0f, Color.Orange, 4f, 0f, 0f, 0f, false)
                    {
                        delayBeforeAnimationStart = 100,
                        id = (float)idNum
                    }
                });
                multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite[]
                {
                    new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(598, 1279, 3, 4), 53f, 5, 9, playerLoc + new Vector2(5f, 3f) * 4f, true, false, (float)(y + 7) / 10000f, 0f, Color.White, 3f, 0f, 0f, 0f, false)
                    {
                        delayBeforeAnimationStart = 200,
                        id = (float)idNum
                    }
                });
                Game1.player.reduceActiveItemByOne();
                __result = true;
                return false;
            }
        }
    }
}