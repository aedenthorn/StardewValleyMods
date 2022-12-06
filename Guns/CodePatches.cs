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
        public class Farmer_BeginUsingTool_Patch
        {
            public static bool Prefix(Farmer __instance)
            {
                if (!Config.ModEnabled || __instance.CurrentTool is not MeleeWeapon)
                    return true;
                isFiring = true;
                return false;
            }
        }
        [HarmonyPatch(typeof(Tool), nameof(Tool.beginUsing))]
        public class MeleeWeapon_beginUsing_Patch
        {
            public static bool Prefix(MeleeWeapon __instance)
            {
                if (!Config.ModEnabled)
                    return true;
                isFiring = true;
                return false;
            }
        }
       [HarmonyPatch(typeof(MeleeWeapon), nameof(MeleeWeapon.leftClick))]
        public class MeleeWeapon_leftClick_Patch
        {
            public static bool Prefix(MeleeWeapon __instance)
            {
                if (!Config.ModEnabled)
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
                int row = 0;
                Vector2 pos = __instance.getLocalPosition(Game1.viewport); 
                SpriteEffects effects = SpriteEffects.None;
                switch (Game1.player.FacingDirection)
                {
                    case 0:
                        pos += new Vector2(24, -56);
                        row = 1;
                        break;
                    case 1:
                        pos += new Vector2(8, -40);
                        break;
                    case 2:
                        row = 1;
                        pos += new Vector2(-24, -40);
                        effects = SpriteEffects.FlipVertically;
                        break;
                    case 3:
                        pos += new Vector2(-8, -40);
                        effects = SpriteEffects.FlipHorizontally;
                        break;
                }
                b.Draw(gunTexture, pos, new Rectangle(altFrame * 32, row * 32, 32, 32), Color.White, 0, Vector2.Zero, 2, effects, (__instance.getStandingY() + 111) / 10000f);
            }
        }
    }
}