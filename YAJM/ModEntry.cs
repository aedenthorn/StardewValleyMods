using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Characters;
using System;
using System.Collections.Generic;
using System.IO;

namespace YAJM
{
    public class ModEntry : Mod, IAssetEditor
    {
        public static ModEntry context;

        public static ModConfig Config;
        private float lastYJumpVelocity;
        private float velX;
        private float velY;
        private  Texture2D horseShadow;
        private  Texture2D horse;
        private  bool playerJumpingWithHorse;
        private static bool blockedJump;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            context = this;
            Config = Helper.ReadConfig<ModConfig>();
            if (!Config.EnableMod)
                return;

            Helper.Events.Input.ButtonPressed += Input_ButtonPressed;
            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;

            Harmony harmony = new Harmony(Helper.ModRegistry.ModID);

            harmony.Patch(
               original: AccessTools.Method(typeof(Farmer), nameof(Farmer.getDrawLayer)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Farmer_getDrawLayer_prefix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(NPC), nameof(NPC.draw), new Type[] { typeof(SpriteBatch), typeof(float) }),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.NPC_draw_prefix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.isCollidingPosition), new Type[] { typeof(Microsoft.Xna.Framework.Rectangle), typeof(xTile.Dimensions.Rectangle), typeof(bool), typeof(int), typeof(bool), typeof(Character), typeof(bool), typeof(bool), typeof(bool) }),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.GameLocation_isCollidingPosition_prefix))
            );

            horseShadow = Helper.ModContent.Load<Texture2D>(Path.Combine("assets", "horse_shadow.png"));
            horse = Helper.ModContent.Load<Texture2D>(Path.Combine("assets", "horse.png"));
            
        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu != null)
            {
                // register mod
                configMenu.Register(
                    mod: ModManifest,
                    reset: () => Config = new ModConfig(),
                    save: () => Helper.WriteConfig(Config)
                );

                configMenu.AddBoolOption(
                    mod: ModManifest,
                    name: () => "Mod Enabled?",
                    getValue: () => Config.EnableMod,
                    setValue: value => Config.EnableMod = value
                );
                configMenu.AddBoolOption(
                    mod: ModManifest,
                    name: () => "Multi Jump?",
                    getValue: () => Config.EnableMultiJump,
                    setValue: value => Config.EnableMultiJump = value
                );
                configMenu.AddKeybind(
                    mod: ModManifest,
                    name: () => "Jump Button",
                    getValue: () => Config.JumpButton,
                    setValue: value => Config.JumpButton = value
                );
                configMenu.AddNumberOption(
                    mod: ModManifest,
                    name: () => "Ordinary Jump Height",
                    getValue: () => (int)Config.OrdinaryJumpHeight,
                    setValue: value => Config.OrdinaryJumpHeight = value
                );
                configMenu.AddNumberOption(
                    mod: ModManifest,
                    name: () => "Max Jump Distance",
                    getValue: () => Config.MaxJumpDistance,
                    setValue: value => Config.MaxJumpDistance = value
                );
                configMenu.AddBoolOption(
                    mod: ModManifest,
                    name: () => "Play Jump Sound",
                    getValue: () => Config.PlayJumpSound,
                    setValue: value => Config.PlayJumpSound = value
                );
                configMenu.AddTextOption(
                    mod: ModManifest,
                    name: () => "Jump Sound",
                    getValue: () => Config.JumpSound,
                    setValue: value => Config.JumpSound = value
                );
                configMenu.AddBoolOption(
                    mod: ModManifest,
                    name: () => "Custom Horse Texture?",
                    getValue: () => Config.CustomHorseTexture,
                    setValue: value => Config.CustomHorseTexture = value
                );
            }
        }

        private static bool Farmer_getDrawLayer_prefix(ref Farmer __instance, ref float __result)
        {
            if(__instance.UniqueMultiplayerID == Game1.player.UniqueMultiplayerID && context.playerJumpingWithHorse)
            {
                __result = 0.992f;
                return false;
            }
            return true;
        }

        private static bool GameLocation_isCollidingPosition_prefix(GameLocation __instance, bool isFarmer, ref bool __result)
        {
            if (isFarmer && context.playerJumpingWithHorse && !blockedJump)
            {
                __result = false;
                return false;
            }
            return true;
        }

        private static void NPC_draw_prefix(ref NPC __instance, SpriteBatch b, float alpha)
        {
            if (__instance is Horse)
            {
                b.Draw(context.horseShadow, __instance.getLocalPosition(Game1.viewport) + Config.HorseShadowOffset + new Vector2((float)(__instance.Sprite.SpriteWidth * 4 / 2), (float)(__instance.GetBoundingBox().Height / 2)), new Microsoft.Xna.Framework.Rectangle?(__instance.Sprite.SourceRect), Color.White * alpha, __instance.rotation, new Vector2((float)(__instance.Sprite.SpriteWidth / 2), (float)__instance.Sprite.SpriteHeight * 3f / 4f), Math.Max(0.2f, __instance.Scale) * 4f, (__instance.flip || (__instance.Sprite.CurrentAnimation != null && __instance.Sprite.CurrentAnimation[__instance.Sprite.currentAnimationIndex].flip)) ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0);
                if ((__instance as Horse).rider != null)
                {
                    if (context.playerJumpingWithHorse)
                    {
                        __instance.Position += new Vector2(0, (__instance as Horse).rider.yJumpOffset * 2);
                        __instance.drawOnTop = true;
                    }
                    else
                    {
                        __instance.drawOnTop = false;
                    }
                }
            }
        }

        private void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            if (e.Button == Config.JumpButton && Context.IsPlayerFree && !Game1.player.IsSitting() && !Game1.player.swimming.Value && Game1.currentMinigame == null && Game1.player.yJumpVelocity == 0)
            {
                playerJumpingWithHorse = false;
                blockedJump = false;

                if (Config.PlayJumpSound && Config.JumpSound.Length > 0)
                    Game1.playSound(Config.JumpSound);
                velX = 0;
                velY = 0;
                int ox = 0;
                int oy = 0;
                switch (Game1.player.facingDirection.Value)
                {
                    case 0:
                        oy = -1;
                        break;
                    case 1:
                        ox = 1;
                        break;
                    case 2:
                        oy = 1;
                        break;
                    case 3:
                        ox = -1;
                        break;
                }

                int maxJumpDistance = Math.Max(2, Config.MaxJumpDistance);
                GameLocation l = Game1.player.currentLocation;
                List<bool> collisions = new List<bool>();
                for (int i = 0; i < maxJumpDistance; i++)
                {
                    Microsoft.Xna.Framework.Rectangle box = Game1.player.GetBoundingBox();
                    if (Game1.player.isRidingHorse())
                    {
                        box.X += ox * Game1.tileSize / 2;
                        box.Y += oy * Game1.tileSize / 2;
                    }
                    box.X += ox * Game1.tileSize * i;
                    box.Y += oy * Game1.tileSize * i;
                    collisions.Add(
                        l.isCollidingPosition(box, Game1.viewport, true, 0, false, Game1.player) 
                        || box.X >= l.map.Layers[0].LayerWidth * Game1.tileSize 
                        || box.Y >= l.map.Layers[0].LayerHeight * Game1.tileSize 
                        || box.X < 0 
                        || box.Y < 0
                        || (l.waterTiles?.waterTiles[box.X / Game1.tileSize, box.Y / Game1.tileSize].isWater == true && !Helper.ModRegistry.IsLoaded("aedenthorn.Swim"))
                    );
                }

                playerJumpingWithHorse = Game1.player.isRidingHorse();
                if (!collisions[0] && !collisions[1])
                {
                    PlayerJump(Config.OrdinaryJumpHeight);
                    return;
                }

                for (int i = 1; i < collisions.Count; i++)
                {
                    if (!collisions[i])
                    {
                        velX = ox * (float)Math.Sqrt(i * 16);
                        velY = oy * (float)Math.Sqrt(i * 16);
                        lastYJumpVelocity = 0;
                        Game1.player.CanMove = false;
                        PlayerJump((float)Math.Sqrt(i * 16));
                        return;
                    }
                }
                blockedJump = true;
                PlayerJump(Config.OrdinaryJumpHeight);
            }
        }

        private void PlayerJump(float v)
        {
            Game1.player.synchronizedJump(v);
            Helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;
        }

        private void GameLoop_UpdateTicked(object sender, StardewModdingAPI.Events.UpdateTickedEventArgs e)
        {
            if (Game1.player.yJumpVelocity == 0f && lastYJumpVelocity < 0f)
            {
                playerJumpingWithHorse = false;
                blockedJump = false;
                Game1.player.canMove = true;
                Helper.Events.GameLoop.UpdateTicked -= GameLoop_UpdateTicked;
                return;
            }
            Game1.player.position.X += velX;
            Game1.player.position.Y += velY;
            lastYJumpVelocity = Game1.player.yJumpVelocity;
        }


        public bool CanEdit<T>(IAssetInfo asset)
        {
            if (!Config.EnableMod)
                return false;
            if (asset.AssetNameEquals("Animals/Horse"))
            {
                return true;
            }

            return false;
        }
        /// <summary>Edit a matched asset.</summary>
        /// <param name="asset">A helper which encapsulates metadata about an asset and enables changes to it.</param>
        public void Edit<T>(IAssetData asset)
        {
            //Texture2D customTexture = this.Helper.Content.Load<Texture2D>(Path.Combine("assets","horse.png");
            if(!Config.CustomHorseTexture)
                asset.AsImage().ReplaceWith(horse);
        }
    }
}
