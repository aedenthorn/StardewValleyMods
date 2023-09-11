using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Network;
using Object = StardewValley.Object;

namespace DinoForm
{
    public partial class ModEntry
    {
        public static int buffId = 3885251;
        public static int consumeBuffId = 3885252;

        [HarmonyPatch(typeof(Farmer), nameof(Farmer.draw))]
        public class Farmer_draw_Patch
        {
            public static bool Prefix(Farmer __instance, SpriteBatch b)
            {
                if (!Config.ModEnabled || __instance != Game1.player || DinoFormStatus(Game1.player) == DinoForm.Inactive)
                    return true;
                var drawLayer = __instance.getDrawLayer();
                Texture2D texture = SHelper.GameContent.Load<Texture2D>("Characters\\Monsters\\Pepper Rex");
                b.Draw(texture, Game1.player.getLocalPosition(Game1.viewport) + new Vector2(28, 0), new Rectangle?(GetSourceRect(Game1.player)), Color.White, 0f, new Vector2(16f, 16f), 4f, SpriteEffects.None, drawLayer + 0.001f);
                var chance = Game1.random.NextDouble();
                if (chance < 0.001 && Game1.soundBank != null && (dinoSound.Value is null || !dinoSound.Value.IsPlaying) && Game1.player.currentLocation == Game1.currentLocation)
                {
                    dinoSound.Value = Game1.soundBank.GetCue("croak");
                    dinoSound.Value.Play();
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(FarmerSprite), "checkForFootstep")]
        public class FarmerSprite_checkForFootstep_Patch
        {
            public static bool Prefix(FarmerSprite __instance, Farmer ___owner)
            {
                if (!Config.ModEnabled || DinoFormStatus(Game1.player) == DinoForm.Inactive || ___owner != Game1.player)
                    return true;
                return false;
            }
        }

        [HarmonyPatch(typeof(Farmer), nameof(Farmer.GetBoundingBox))]
        public class Farmer_GetBoundingBox_Patch
        {
            public static void Postfix(Farmer __instance, ref Rectangle __result)
            {
                if (!Config.ModEnabled || __instance.mount is not null || __instance != Game1.player || DinoFormStatus(Game1.player) == DinoForm.Inactive)
                    return;
                __result.Inflate(16, 0);
            }
        }

        [HarmonyPatch(typeof(Farmer), nameof(Farmer.MovePosition))]
        public class Farmer_MovePosition_Patch
        {
            public static bool Prefix(Farmer __instance, GameLocation currentLocation)
            {
                if (!Config.ModEnabled || __instance.mount is not null || __instance != Game1.player || DinoFormStatus(Game1.player) == DinoForm.Inactive)
                    return true;

                if (breathingFire.Value)
                    return false;
                if(__instance.movementDirections.Count == 0)
                {
                    frameOffset.Value = 0;
                }
                else
                {
                    frameOffset.Value++;
                }
                foreach(var d in __instance.movementDirections)
                {
                    var nextPosition = __instance.nextPosition(d);
                    foreach (var kvp in currentLocation.Objects.Pairs)
                    {
                        if (nextPosition.Intersects(kvp.Value.getBoundingBox(kvp.Key))) 
                        {
                            if (kvp.Value.Name.Equals("Weeds"))
                            {
                                AccessTools.Method(typeof(Object), "cutWeed").Invoke(kvp.Value, new object[] { __instance, currentLocation });
                                currentLocation.Objects.Remove(kvp.Key);
                            }
                            else if(kvp.Value.Name.Contains("Stone") && !kvp.Value.bigCraftable.Value && kvp.Value is not Fence)
                            {
                                currentLocation.playSound("hammer", NetAudio.SoundContext.Default);
                                currentLocation.destroyObject(kvp.Key, __instance);
                            }
                            else if(kvp.Value.Name.Contains("Twig"))
                            {
                                kvp.Value.Fragility = 2;
                                currentLocation.playSound("axchop", NetAudio.SoundContext.Default);
                                Game1.createRadialDebris(currentLocation, 12, (int)kvp.Value.TileLocation.X, (int)kvp.Value.TileLocation.Y, Game1.random.Next(4, 10), false, -1, false, -1);
                                currentLocation.debris.Add(new Debris(new Object(388, 1, false, -1, 0), kvp.Value.TileLocation * 64f + new Vector2(32f, 32f)));
                                kvp.Value.performRemoveAction(kvp.Value.TileLocation, Game1.currentLocation);
                                Game1.currentLocation.Objects.Remove(kvp.Key);
                            }
                        } 
                    }
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(Game1), nameof(Game1.pressUseToolButton))]
        public class Game1_pressUseToolButton_Patch
        {
            public static bool Prefix()
            {
                return (!Config.ModEnabled || DinoFormStatus(Game1.player) == DinoForm.Inactive);
            }
        }
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.isCollidingWithWarp))]
        public class GameLocation_isCollidingWithWarp_Patch
        {
            public static void Prefix(ref Rectangle position, Character character)
            {
                if (!Config.ModEnabled || character != Game1.player || DinoFormStatus(Game1.player) == DinoForm.Inactive)
                    return;
                position.Inflate(-16, 0);
            }
        }
    }
}