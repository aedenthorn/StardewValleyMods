using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Tools;

namespace Guns
{
    public partial class ModEntry
    {
       [HarmonyPatch(typeof(Farmer), nameof(Farmer.BeginUsingTool))]
        public class MeleeWeapon_leftClick_Patch
        {
            public static bool Prefix(Farmer __instance)
            {
                if (!Config.ModEnabled || __instance.CurrentTool is not MeleeWeapon)
                    return true;
                isFiring = true;
                return false;
            }
        }
        [HarmonyPatch(typeof(Farmer), nameof(Farmer.draw))]
        public class Farmer_draw_Patch
        {
            public static void Postfix(Farmer __instance, SpriteBatch b)
            {
                if (!Config.ModEnabled || !isFiring)
                    return;


                if(!SHelper.Input.IsDown(StardewModdingAPI.SButton.MouseLeft))
                {
                    isFiring = false;
                    return;
                }
                b.Draw(gunTexture, new Vector2(__instance.getLocalPosition(Game1.viewport).X + (Game1.player.FacingDirection == 3 ? - 12 :12f), __instance.getLocalPosition(Game1.viewport).Y - 24f), new Rectangle(altFrame * 28, 0, 28, 18), Color.White, 0, Vector2.Zero, 2, Game1.player.FacingDirection == 3 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 1f);
            }
        }
    }
}