using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Objects;
using xTile.Dimensions;
using Object = StardewValley.Object;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace CustomMounts
{
    public partial class ModEntry
    {
        private static void MakeCustomMount(Horse __instance)
        {
            var stable = __instance.TryFindStable();
            if (stable is null)
                return;
            var bd = stable.GetData();
            if (bd is null)
                return;
            foreach (var kvp in MountDict)
            {
                if (bd.Name == kvp.Value.Stable || stable.buildingType.Value == kvp.Value.Stable)
                {
                    __instance.modData[modKey] = kvp.Key;
                    __instance.Name = kvp.Value.Name;
                    SetSprite(__instance, kvp.Value);
                    return;
                }
            }
        }

        private static void DrawHat(Hat hat, SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, int direction, bool useAnimalTexture, Horse horse)
        {
            if (CheckModData(horse, out var data))
            {
                scaleSize = (int)Math.Round(scaleSize * data.HatScale);

                if (horse.FacingDirection == 3)
                {
                    if (data.HatFramesFlipped != null && data.HatFramesFlipped.TryGetValue(horse.Sprite.CurrentFrame, out var frame))
                    {
                        location += frame.Offset;
                        HatDraw(hat, frame.Rotation * (float)(Math.PI / 180), spriteBatch, location, scaleSize, transparency, layerDepth, direction, useAnimalTexture);
                        return;
                    }
                }
                else
                {
                    if (data.HatFrames != null && data.HatFrames.TryGetValue(horse.Sprite.CurrentFrame, out var frame))
                    {
                        location += frame.Offset;
                        HatDraw(hat, frame.Rotation * (float)(Math.PI / 180), spriteBatch, location, scaleSize, transparency, layerDepth, direction, useAnimalTexture);
                        return;
                    }
                }
                location += data.HatOffsets[horse.FacingDirection] - new Vector2(0, 20);
                hat.draw(spriteBatch, location, scaleSize, transparency, layerDepth, direction, useAnimalTexture);
            }
            else
            {
                hat.draw(spriteBatch, location, scaleSize, transparency, layerDepth, direction, useAnimalTexture);
            }
        }
        private static void HatDraw(Hat hat, float rotation, SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, int direction, bool useAnimalTexture = false)
        {
            ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(hat.QualifiedItemId);
            int spriteIndex = itemData.SpriteIndex;
            Texture2D texture;
            if (useAnimalTexture)
            {
                string textureName = itemData.GetTextureName();
                if (Game1.content.DoesAssetExist<Texture2D>(textureName + "_animals"))
                {
                    textureName += "_animals";
                }
                texture = Game1.content.Load<Texture2D>(textureName);
            }
            else
            {
                texture = itemData.GetTexture();
            }
            switch (direction)
            {
                case 0:
                    direction = 3;
                    break;
                case 2:
                    direction = 0;
                    break;
                case 3:
                    direction = 2;
                    break;
            }
            Rectangle drawnSourceRect = ((!itemData.IsErrorItem) ? new Rectangle(spriteIndex * 20 % texture.Width, spriteIndex * 20 / texture.Width * 20 * 4 + direction * 20, 20, 20) : itemData.GetSourceRect(0, null));
            spriteBatch.Draw(texture, location + new Vector2(10f, 10f), new Rectangle?(drawnSourceRect), hat.isPrismatic.Value ? (Utility.GetPrismaticColor(0, 1f) * transparency) : (Color.White * transparency), rotation, new Vector2(3f, 3f), 3f * scaleSize, SpriteEffects.None, layerDepth);
        }

        public static int toggle;
        private static void DrawHorse(SpriteBatch spriteBatch, Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth, Horse horse)
        {
            if (!Config.ModEnabled || horse.rider is null || !CheckModData(horse, out var data))
            {

            }
            else
            {
                float xScale = horse.Sprite.SpriteWidth / 32f;
                float yScale = horse.Sprite.SpriteHeight / 32f;
                position = horse.getLocalPosition(Game1.viewport) + new Vector2(48f * xScale, -24f * yScale - horse.rider.yOffset - (horse.Sprite.SpriteHeight - 32) / 2);
                sourceRectangle = new Rectangle((int)Math.Round(160 * xScale), (int)Math.Round(96 * yScale), (int)Math.Round(9 * xScale), (int)Math.Round(15 * yScale));
            }
            spriteBatch.Draw(texture, position, sourceRectangle, color, rotation, origin, scale, effects, layerDepth);

        }


        private static Vector2 GetDrawPosition(Vector2 position, Character __instance)
        {
            if (!Config.ModEnabled || __instance is not Horse || __instance.Sprite.SpriteWidth == 32)
                return position;
            return position + new Vector2(32 - __instance.Sprite.SpriteWidth, 0) * 1.5f;
        }


        private static void SetSprite(Horse horse, MountData data)
        {
            horse.Sprite = new AnimatedSprite(data.TexturePath, 0, data.FrameWidth, data.FrameHeight);
            horse.Sprite.textureUsesFlippedRightForLeft = true;
            horse.Sprite.loop = true;
            horse.spriteOverridden = true;
        }

        private static int SetSpeed(int speed, Farmer farmer)
        {

            if (!Config.ModEnabled || !farmer.mount.modData.TryGetValue(modKey, out var key) || !MountDict.TryGetValue(key, out var data))
                return speed;
            return data.Speed;
        }
        private static float SetSpeedBonus(float speedBonus, Farmer farmer)
        {

            if (!Config.ModEnabled || farmer.mount is null || !farmer.mount.modData.TryGetValue(modKey, out var key) || !MountDict.TryGetValue(key, out var data))
                return speedBonus;
            return data.EatSpeedBonus;
        }

        private static string SetStepSound(string which, Horse horse)
        {
            if (!CheckModData(horse, out var data))
                return which;
            switch (which)
            {
                case "thudStep":
                    return string.IsNullOrEmpty(data.FootstepSound) ? which : data.FootstepSound;
                case "woodyStep":
                    return string.IsNullOrEmpty(data.FootstepSoundWood) ? which : data.FootstepSoundWood;
                case "stoneStep":
                    return string.IsNullOrEmpty(data.FootstepSoundStone) ? which : data.FootstepSoundStone;
                default:
                    return which;
            }
        }
        private static string SetCarrotItem(string item, Horse horse)
        {
            if (!CheckModData(horse, out var data))
                return item;
            return data.EatItem;
        }
        private static string SetEatSound(string sound, Horse horse)
        {
            if (!CheckModData(horse, out var data))
                return sound;
            return data.EatSound;
        }
        private static string SetNameYourHorse(string value, Horse horse)
        {
            if (!CheckModData(horse, out var data))
                return value;
            return string.Format(SHelper.Translation.Get("NameYourX"), data.Name);
        }
        private static string SetDefaultHorseName(string value, Horse horse)
        {
            if (!CheckModData(horse, out var data))
                return value;
            return data.Name;
        }
        private static string SetFluteItem(string value, Object obj)
        {
            if (!Config.ModEnabled)
                return value;
            Utility.ForEachBuilding<Stable>(delegate (Stable stable)
            {
                Horse curHorse = stable.getStableHorse();
                var owner = curHorse?.getOwner();
                var ownerId = curHorse?.ownerId;
                if (owner == Game1.player)
                {
                    if(curHorse.modData.TryGetValue(modKey, out var key) && MountDict.TryGetValue(key, out var data))
                    {
                        if(obj.QualifiedItemId == data.FluteItem)
                        {
                            value = data.FluteItem;
                            return false;
                        }
                    }
                }
                return true;
            }, true);
            return value;
        }
        private static string SetFluteSound(string value, Object obj)
        {
            if (!Config.ModEnabled)
                return value;
            Utility.ForEachBuilding<Stable>(delegate (Stable stable)
            {
                Horse curHorse = stable.getStableHorse();
                if (curHorse?.getOwner() == Game1.player)
                {
                    if (curHorse.modData.TryGetValue(modKey, out var key) && MountDict.TryGetValue(key, out var data))
                    {
                        if (obj.QualifiedItemId == data.FluteItem)
                        {
                            value = data.FluteSound;
                            return false;
                        }
                    }
                }
                return true;
            }, true);
            return value;
        }
        private static Horse? GetHorseForFlute(long uid)
        {
            Farmer? farmer = Game1.GetPlayer(uid, false);
            if (farmer.Items.Count <= farmer.CurrentToolIndex || farmer.Items[farmer.CurrentToolIndex] is null)
                return null;
            Item obj = farmer.Items[farmer.CurrentToolIndex];
            Horse? horse = null;
            Utility.ForEachBuilding<Stable>(delegate (Stable stable)
            {
                Horse curHorse = stable.getStableHorse();
                if (curHorse != null && curHorse.getOwner() == farmer && (!Config.ModEnabled || IsFluteFor(curHorse, obj)))
                {
                    horse = curHorse;
                    return false;
                }
                return true;
            }, true);
            return horse;
        }

        private static bool IsFluteFor(Horse horse, Item obj)
        {
            if (!CheckModData(horse, out var data))
                return obj.QualifiedItemId == "(O)911";
            return obj.QualifiedItemId == data.FluteItem;
        }

        private static void PreventOwnershipErasure(NetLong l, long v)
        {
            if (!Config.ModEnabled || !Config.AllowMultipleMounts)
                l.Value = v;
        }
        private static Farmer? PreventNameErasure(Farmer? result)
        {
            if (!Config.ModEnabled || !Config.AllowMultipleMounts)
                return result;
            return null;
        }
        private static double OverrideRandomAnimationChance(double v, Horse horse)
        {
            if (!CheckModData(horse, out var data) || data.CustomAnimations == null)
            {
                return 0.002;
            }
            else
            {
                var roll = Game1.random.NextDouble();
                double totalChance = 0;
                foreach (var l in data.CustomAnimations)
                {
                    if (l.Value.FacingDirection != horse.FacingDirection)
                        continue;
                    totalChance += l.Value.Chance;
                    if (roll < totalChance)
                    {
                        horse.Sprite.loop = false;
                        List<FarmerSprite.AnimationFrame> frames = new();
                        foreach (var f in l.Value.Frames)
                        {
                            frames.Add(new FarmerSprite.AnimationFrame(f.Frame, Game1.random.Next(f.MinLength, f.MaxLength), false, f.Flip, null, false));
                        }
                        horse.modData[animKey] = l.Key;
                        horse.Sprite.setCurrentAnimation(frames);
                        break;
                    }
                }
                return 0;
            }
        }
        private static bool CheckModData(Horse horse, out MountData data)
        {
            if (!Config.ModEnabled || !horse.modData.TryGetValue(modKey, out var key) || !MountDict.TryGetValue(key, out var d))
            {
                data = null;
                return false;
            }
            data = d;
            return true;
        }
    }
}