using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Tools;
using System;

namespace Guns
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(Farmer), nameof(Farmer.BeginUsingTool))]
        public class Farmer_BeginUsingTool_Patch
        {
            public static bool Prefix(Farmer __instance)
            {
                if (!Config.ModEnabled || __instance.CurrentTool is not MeleeWeapon || !gunDict.ContainsKey(__instance.CurrentTool.Name))
                    return true;
                __instance.modData[firingKey] = "0";
                farmerDict[__instance.UniqueMultiplayerID] = __instance.CurrentTool.Name;
                return false;
            }
        }
        [HarmonyPatch(typeof(Tool), nameof(Tool.beginUsing))]
        public class MeleeWeapon_beginUsing_Patch
        {
            public static bool Prefix(Tool __instance, Farmer who)
            {
                if (!Config.ModEnabled || __instance is not MeleeWeapon || !gunDict.ContainsKey(__instance.Name))
                    return true;
                __instance.modData[firingKey] = "0";
                farmerDict[who.UniqueMultiplayerID] = __instance.Name;
                return false;
            }
        }
       [HarmonyPatch(typeof(MeleeWeapon), nameof(MeleeWeapon.leftClick))]
        public class MeleeWeapon_leftClick_Patch
        {
            public static bool Prefix(MeleeWeapon __instance, Farmer who)
            {
                if (!Config.ModEnabled || !gunDict.ContainsKey(__instance.Name))
                    return true;
                __instance.modData[firingKey] = "0";
                farmerDict[who.UniqueMultiplayerID] = __instance.Name;
                return false;
            }
        }
        [HarmonyPatch(typeof(Farmer), nameof(Farmer.draw))]
        public class Farmer_draw_Patch
        {
            public static void Postfix(Farmer __instance, SpriteBatch b)
            {
                if (!SHelper.Input.IsDown(StardewModdingAPI.SButton.MouseLeft))
                {
                    __instance.modData.Remove(firingKey);
                    return;
                }
                if (!Config.ModEnabled || !__instance.modData.TryGetValue(firingKey, out string str) || !int.TryParse(str, out int fireTicks) || !farmerDict.TryGetValue(__instance.UniqueMultiplayerID, out string dataKey))
                    return;
                GunData data = gunDict[dataKey];
                fireTicks++;
                altFrame = (fireTicks / 5 % 2);
                if (fireTicks % 5 == 0)
                {
                    float x = 0;
                    float y = 0;
                    float rotation = data.bulletRotation + (float)Math.PI / 2f * Game1.player.FacingDirection;
                    Vector2 start = Game1.player.Position + data.bulletOffsets[Game1.player.FacingDirection].ToVector2();
                    
                    switch (Game1.player.FacingDirection)
                    {
                        case 0:
                            y = -1;
                            break;
                        case 1:
                            x = 1;
                            break;
                        case 2:
                            y = 1;
                            break;
                        case 3:
                            x = -1;
                            break;
                    }
                    if(data.fireSound != null)
                        Game1.playSound(data.fireSound);
                    Game1.currentLocation.projectiles.Add(new GunProjectile(rotation, data.bulletScale, Game1.random.Next(data.minDamage, data.maxDamage + 1), data.bulletIndex, 0, 0, 0, x * data.bulletVelocity, y * data.bulletVelocity, start, "", "", false, true, Game1.player.currentLocation, Game1.player, true, null));
                }
                __instance.modData[firingKey] = fireTicks + "";

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
                b.Draw(data.texture, pos, new Rectangle(altFrame * 32, row * 32, 32, 32), Color.White, 0, Vector2.Zero, 2, effects, (__instance.getStandingY() + 111) / 10000f);
            }
        }
    }
}