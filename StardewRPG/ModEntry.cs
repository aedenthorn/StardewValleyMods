using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using Object = StardewValley.Object;

namespace StardewRPG
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;
        public static int deathScreenTimer;
        public static Texture2D blackTexture;
        public static readonly string modDataKey = "aedenthorn.StardewRPG/";

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            if (!Config.EnableMod)
                return;

            context = this;

            SMonitor = Monitor;
            SHelper = helper;
            blackTexture = new Texture2D(Game1.graphics.GraphicsDevice, 1, 1);
            blackTexture.SetData(new Color[] { Color.Black });

            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;

            var harmony = new Harmony(ModManifest.UniqueID);


            // Farmer Patches

            harmony.Patch(
               original: AccessTools.Constructor(typeof(Farmer), new Type[] { typeof(FarmerSprite), typeof(Vector2), typeof(int), typeof(string), typeof(List<Item>), typeof(bool) }),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Farmer_Postfix))
            );
            
            harmony.Patch(
               original: AccessTools.Method(typeof(Farmer), nameof(Farmer.doneEating)),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Farmer_doneEating_Postfix))
            );
            
            harmony.Patch(
               original: AccessTools.Method(typeof(Farmer), nameof(Farmer.gainExperience)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Farmer_gainExperience_Prefix))
            );
            
            harmony.Patch(
               original: AccessTools.PropertyGetter(typeof(Farmer), nameof(Farmer.Level)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Farmer_Level_Prefix))
            );
            
            harmony.Patch(
               original: AccessTools.Method(typeof(Farmer), "performBeginUsingTool"),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Farmer_performBeginUsingTool_Prefix))
            );
            

            // Tool patches

            harmony.Patch(
               original: AccessTools.Method(typeof(MeleeWeapon), nameof(MeleeWeapon.setFarmerAnimating)),
               transpiler: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.MeleeWeapon_setFarmerAnimating_Transpiler))
            );
            
            
            // Crafting patches

            harmony.Patch(
               original: AccessTools.Method(typeof(CraftingRecipe), nameof(CraftingRecipe.consumeIngredients)),
               transpiler: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.CraftingRecipe_consumeIngredients_Transpiler))
            );
            
            harmony.Patch(
               original: AccessTools.Method(typeof(CraftingRecipe), nameof(CraftingRecipe.doesFarmerHaveIngredientsInInventory)),
               transpiler: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.CraftingRecipe_doesFarmerHaveIngredientsInInventory_Transpiler))
            );


            // Object patches

            harmony.Patch(
               original: AccessTools.Method(typeof(Object), nameof(Object.performObjectDropInAction)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Object_performObjectDropInAction_Prefix)),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Object_performObjectDropInAction_Postfix))
            );
            
            harmony.Patch(
               original: AccessTools.Method(typeof(Object), nameof(Object.performDropDownAction)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Object_performObjectDropInAction_Prefix)),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Object_performObjectDropInAction_Postfix))
            );


            // Fishing patches

            harmony.Patch(
               original: AccessTools.Constructor(typeof(BobberBar),new Type[] { typeof(int), typeof(float), typeof(bool), typeof(int) }),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.BobberBar_Postfix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(BobberBar), nameof(BobberBar.update)),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.BobberBar_update_Postfix))
            );


            // GameLocation patches

            harmony.Patch(
               original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.damageMonster)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.GameLocation_damageMonster_Prefix))
            );
            
            
            // Game1 patches

            harmony.Patch(
               original: AccessTools.Method(typeof(Game1), nameof(Game1.updatePause)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Game1_updatePause_Prefix))
            );

            
            harmony.Patch(
               original: AccessTools.Method(typeof(Game1), "drawHUD"),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Game1_drawHUD_Prefix)),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Game1_drawHUD_Postfix))
            );


            // UI Patches

            harmony.Patch(
               original: AccessTools.Method(typeof(SkillsPage), nameof(SkillsPage.draw), new Type[] { typeof(SpriteBatch) }),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.SkillsPage_draw_Postfix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(CharacterCustomization), nameof(CharacterCustomization.draw), new Type[] { typeof(SpriteBatch) }),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.CharacterCustomization_draw_Prefix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(CharacterCustomization), "setUpPositions"),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.CharacterCustomization_setUpPositions_Postfix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(CharacterCustomization), "selectionClick"),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.CharacterCustomization_selectionClick_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(ChatBox), "runCommand"),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.ChatBox_runCommand_Prefix))
            );
        }

        private void Display_RenderedWorld(object sender, StardewModdingAPI.Events.RenderedWorldEventArgs e)
        {
            if(Config.EnableMod && Config.PermaDeath && Game1.killScreen)
            {
                deathScreenTimer++;
                if (deathScreenTimer > Config.PermaDeathScreenTicks)
                {
                    deathScreenTimer = 0;
                    Game1.killScreen = false;
                    Game1.ExitToTitle();
                }
                else
                {
                    e.SpriteBatch.Draw(blackTexture, new Rectangle(0, 0, Game1.viewport.Width, Game1.viewport.Height), Color.White);
                    e.SpriteBatch.DrawString(Game1.dialogueFont, "YOU DIED", new Vector2(Game1.viewport.Width / 2 - SpriteText.getWidthOfString("YOU DIED") * 8, Game1.viewport.Height / 2), Color.Red, 0, Vector2.Zero, 16, SpriteEffects.None, 1);
                    e.SpriteBatch.Draw(blackTexture, new Rectangle(0, 0, Game1.viewport.Width, Game1.viewport.Height), Color.White * Math.Min(0, 1 - deathScreenTimer / (Config.PermaDeathScreenTicks / 4)));
                }
            }
        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            // get Generic Mod Config Menu's API (if it's installed)
            var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
                return;

            // register mod
            configMenu.Register(
                mod: ModManifest,
                reset: () => Config = new ModConfig(),
                save: () => Helper.WriteConfig(Config)
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Mod Enabled",
                getValue: () => Config.EnableMod,
                setValue: value => Config.EnableMod = value
            );
        }
    }
}