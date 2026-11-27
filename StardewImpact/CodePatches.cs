using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Tools;
using System;
using Object = StardewValley.Object;

namespace StardewImpact
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(Farmer), nameof(Farmer.draw))]
        public class Farmer_draw_Patch
        {
            public static bool Prefix(Farmer __instance, SpriteBatch b)
            {
                if (!Config.ModEnabled || !Context.IsPlayerFree || __instance.IsSitting() || __instance.isRidingHorse() || __instance.swimming.Value || __instance.CurrentTool is not MeleeWeapon || !__instance.modData.TryGetValue(currentSlotKey, out string key) || !__instance.modData.TryGetValue(slotPrefix + key, out string name) || !GetAvailableCharacters().TryGetValue(name, out CharacterData data) || data.Sprite is null)
                    return true;
                int yOffset = 0;
                bool walking = __instance.movementDirections.Contains(__instance.FacingDirection);
                int xAxis = 0;
                switch (__instance.FacingDirection)
                {
                    case 0:
                        yOffset = 64;
                        xAxis = walking ? (int)__instance.Position.Y : 0;
                        break;
                    case 1:
                        yOffset = 32;
                        xAxis = walking ? (int)__instance.Position.X : 0;
                        break;
                    case 2:
                        yOffset = 0;
                        xAxis = walking ? (int)__instance.Position.Y : 0;
                        break;
                    case 3:
                        yOffset = 96;
                        xAxis = walking ? (int)__instance.Position.X : 0;
                        break;
                }
                int xOffset = (xAxis % 256) / 64 * 16;
                Rectangle sourceRect = new Rectangle(xOffset, yOffset, 16, 32);
                b.Draw(data.Sprite, __instance.getLocalPosition(Game1.viewport) - new Vector2(0, 24) * 4f, sourceRect, Color.White, 0, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0f, __instance.getStandingY() / 10000f));
                return false;
            }
        }
    }
}