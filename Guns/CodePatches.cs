using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Projectiles;
using StardewValley.Tools;
using System;
using static StardewValley.Projectiles.BasicProjectile;

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
                if (!Config.ModEnabled || __instance.CurrentTool is not MeleeWeapon || __instance.CurrentTool.Name is null || !gunDict.TryGetValue(__instance.CurrentTool.Name, out GunData data))
                    return;

                int row = 0;
                Vector2 pos = __instance.getLocalPosition(Game1.viewport) + data.gunOffsets[__instance.FacingDirection].ToVector2();
                SpriteEffects effects = SpriteEffects.None;
                switch (__instance.FacingDirection)
                {
                    case 0:
                        row = 1;
                        break;
                    case 1:
                        break;
                    case 2:
                        row = 1;
                        effects = SpriteEffects.FlipVertically;
                        break;
                    case 3:
                        effects = SpriteEffects.FlipHorizontally;
                        break;
                }
                if (!SHelper.Input.IsDown(StardewModdingAPI.SButton.MouseLeft) || !__instance.modData.TryGetValue(firingKey, out string str) || !int.TryParse(str, out int fireTicks))
                {
                    if (data.gunAlwaysShow)
                    {
                        b.Draw(data.gunTexture, pos, new Rectangle(0, row * data.gunTileHeight, data.gunTileWidth, data.gunTileHeight), Color.White, 0, Vector2.Zero, 2, effects, (__instance.getStandingY() + 111) / 10000f);
                    }
                    __instance.modData.Remove(firingKey);
                    farmerDict.Remove(__instance.UniqueMultiplayerID);
                    return;
                }

                var firing = (fireTicks % data.fireRate) == 0 ? true : false;
                var altFrame = (fireTicks % data.fireRate) < data.fireTicks ? true : false;
                fireTicks++;
                if (firing)
                {
                    float x = 0;
                    float y = 0;
                    float rotation = data.bulletRotation * (float)Math.PI / 180 + (float)Math.PI / 2f * __instance.FacingDirection;
                    Vector2 start = __instance.Position + data.bulletOffsets[__instance.FacingDirection].ToVector2();
                    
                    switch (__instance.FacingDirection)
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
                    onCollisionBehavior behaviour = data.explosive ? new onCollisionBehavior(explodeOnImpact) : new onCollisionBehavior(dontExplodeOnImpact);
                    Game1.currentLocation.projectiles.Add(new GunProjectile(rotation, data.bulletScale, Game1.random.Next(data.minDamage, data.maxDamage + 1), data.bulletIndex, 0, 0, 0, x * data.bulletVelocity, y * data.bulletVelocity, start, data.collisionSound, data.fireSound, data.explosive, true, __instance.currentLocation, __instance, data.bulletFromSpringObjects, behaviour));
                }
                __instance.modData[firingKey] = fireTicks + "";


                b.Draw(data.gunTexture, pos, new Rectangle(altFrame ? data.gunTileWidth : 0, row * data.gunTileHeight, data.gunTileWidth, data.gunTileHeight), Color.White, 0, Vector2.Zero, 2, effects, (__instance.getStandingY() + 111) / 10000f);
            }
        }

    }
}