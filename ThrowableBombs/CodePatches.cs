using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using Object = StardewValley.Object;

namespace ThrowableBombs
{
    public partial class ModEntry
    {
        public static string[] bombIndexes = { "(O)286", "(O)287", "(O)288" };

        public static Dictionary<int, BombData> bombDict = new Dictionary<int, BombData>();

        [HarmonyPatch(typeof(Game1), nameof(Game1.pressUseToolButton))]
        public class Game1_pressUseToolButton_Patch
        {
            public static bool Prefix(ref bool __result)
            {
                if (!Config.ModEnabled || !Context.IsPlayerFree || Game1.player.ActiveObject is not Object obj || !obj.HasContextTag("bomb_item"))
                    return true;
                Farmer who = Game1.player;
                var cursorTile = Game1.currentCursorTile;
                var distance = Vector2.Distance(who.Tile, cursorTile);
                if(Config.MaxDistance > 0 && distance > Config.MaxDistance)
                {
                    cursorTile = Vector2.Lerp(who.Tile, cursorTile, Config.MaxDistance / distance);
                    cursorTile.X = (float)Math.Round(cursorTile.X);
                    cursorTile.Y = (float)Math.Round(cursorTile.Y);
                }

                Vector2 cursorLoc = cursorTile * 64;
                Vector2 playerLoc = who.Position + new Vector2(0, -128);
                var location = who.currentLocation;
                var parentSheetIndex = who.ActiveObject.ParentSheetIndex;
                float y = playerLoc.Y / 64f;
                int idNum = Game1.random.Next();
                while (bombDict.ContainsKey(idNum))
                    idNum = Game1.random.Next();

                location.playSound("dwoop");
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

                var data = new BombData() { location = location.NameOrUniqueName, startPos = playerLoc + offset, currentPos = playerLoc + offset, endPos = cursorLoc, id = idNum };
                bombDict.Add(idNum, data);
                SHelper.Multiplayer.SendMessage(data, "PlacedBomb");
                Game1.Multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite[]
                {
                    new TemporaryAnimatedSprite(parentSheetIndex, 100f, 1, 24, playerLoc + offset, true, false, location, who)
                    {
                        shakeIntensity = 0.5f,
                        shakeIntensityChange = 0.002f,
                        extraInfoForEndBehavior = idNum,
                        endFunction = new TemporaryAnimatedSprite.endBehavior(location.removeTemporarySpritesWithID),
                    }
                });
                Game1.Multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite[]
                {
                    new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(598, 1279, 3, 4), 53f, 5, 9, playerLoc + offset + new Vector2(5f, 3f) * 4f, true, false, (float)(y + 7) / 10000f, 0f, Color.Yellow, 4f, 0f, 0f, 0f, false)
                    {
                        id = idNum
                    }
                });
                Game1.Multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite[]
                {
                    new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(598, 1279, 3, 4), 53f, 5, 9, playerLoc + offset + new Vector2(5f, 3f) * 4f, true, true, (float)(y + 7) / 10000f, 0f, Color.Orange, 4f, 0f, 0f, 0f, false)
                    {
                        delayBeforeAnimationStart = 100,
                        id = idNum
                    }
                });
                Game1.Multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite[]
                {
                    new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(598, 1279, 3, 4), 53f, 5, 9, playerLoc + offset + new Vector2(5f, 3f) * 4f, true, false, (float)(y + 7) / 10000f, 0f, Color.White, 3f, 0f, 0f, 0f, false)
                    {
                        delayBeforeAnimationStart = 200,
                        id = idNum
                    }
                });
                who.reduceActiveItemByOne();
                who.faceGeneralDirection(cursorLoc);

                switch (who.FacingDirection)
                {
                    case 0:
                        ((FarmerSprite)who.Sprite).animateOnce(295, 35f, 8, new AnimatedSprite.endOfAnimationBehavior(FishingRod.endOfAnimationBehavior));
                        break;
                    case 1:
                        ((FarmerSprite)who.Sprite).animateOnce(296, 35f, 8, new AnimatedSprite.endOfAnimationBehavior(FishingRod.endOfAnimationBehavior));
                        break;
                    case 2:
                        ((FarmerSprite)who.Sprite).animateOnce(297, 35f, 8, new AnimatedSprite.endOfAnimationBehavior(FishingRod.endOfAnimationBehavior));
                        break;
                    case 3:
                        ((FarmerSprite)who.Sprite).animateOnce(298, 35f, 8, new AnimatedSprite.endOfAnimationBehavior(FishingRod.endOfAnimationBehavior));
                        break;
                }
                __result = true;
                return false;
            }
        }
    }
}