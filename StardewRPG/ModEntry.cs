using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Projectiles;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Globalization;
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

            // Buff patches

            harmony.Patch(
               original: AccessTools.Method(typeof(BuffsDisplay), nameof(BuffsDisplay.addOtherBuff)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.BuffsDisplay_addOtherBuff_Prefix))
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
            harmony.Patch(
               original: AccessTools.Method(typeof(CraftingPage), "clickCraftingRecipe"),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.CraftingPage_clickCraftingRecipe_Prefix))
            );

            // Crop Patches

            harmony.Patch(
               original: AccessTools.Method(typeof(Crop), nameof(Crop.harvest)),
               transpiler: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Crop_harvest_Transpiler))
            );


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

            harmony.Patch(
               original: AccessTools.Method(typeof(Farmer), nameof(Farmer.CanBeDamaged)),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Farmer_CanBeDamaged_Postfix))
            );


            harmony.Patch(
               original: AccessTools.Method(typeof(Farmer), nameof(Farmer.changeFriendship)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Farmer_changeFriendship_Prefix))
            );


            harmony.Patch(
               original: AccessTools.Method(typeof(Farmer), nameof(Farmer.performTenMinuteUpdate)),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Farmer_performTenMinuteUpdate_Postfix))
            );


            // Fishing patches

            harmony.Patch(
               original: AccessTools.Constructor(typeof(BobberBar), new Type[] { typeof(int), typeof(float), typeof(bool), typeof(int) }),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.BobberBar_Postfix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(BobberBar), nameof(BobberBar.update)),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.BobberBar_update_Postfix))
            );


            // GameLocation patches

            harmony.Patch(
               original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.damageMonster), new Type[] { typeof(Rectangle), typeof(int), typeof(int), typeof(bool), typeof(float), typeof(int), typeof(float), typeof(float), typeof(bool), typeof(Farmer) }),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.GameLocation_damageMonster_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.draw), new Type[] { typeof(SpriteBatch) }),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.GameLocation_draw_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.spawnObjects)),
               transpiler: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.GameLocation_spawnObjects_Transpiler))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.performOrePanTenMinuteUpdate)),
               transpiler: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.GameLocation_performOrePanTenMinuteUpdate_Transpiler))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.performTenMinuteUpdate)),
               transpiler: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.GameLocation_performTenMinuteUpdate_Transpiler))
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


            harmony.Patch(
               original: AccessTools.Method(typeof(Game1), nameof(Game1.UpdateOther)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Game1_UpdateOther_Prefix)),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Game1_UpdateOther_Postfix))
            );


            // NPC patches

            harmony.Patch(
               original: AccessTools.Method(typeof(NPC), nameof(NPC.tryToReceiveActiveObject)),
               transpiler: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.NPC_tryToReceiveActiveObject_Transpiler))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(NPC), "engagementResponse"),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.NPC_engagementResponse_Prefix))
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

            harmony.Patch(
               original: AccessTools.Method(typeof(Object), nameof(Object.salePrice)),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Object_salePrice_Postfix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(Object), nameof(Object.sellToStorePrice)),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Object_sellToStorePrice_Postfix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(Object), nameof(Object.draw), new Type[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) }),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Object_draw_Prefix))
            );


            // Tool patches

            harmony.Patch(
               original: AccessTools.Method(typeof(MeleeWeapon), nameof(MeleeWeapon.setFarmerAnimating)),
               transpiler: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.MeleeWeapon_setFarmerAnimating_Transpiler))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(Pickaxe), nameof(Pickaxe.DoFunction)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Pickaxe_DoFunction_Prefix)),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Pickaxe_DoFunction_Postfix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(Axe), nameof(Axe.DoFunction)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Axe_DoFunction_Prefix)),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Axe_DoFunction_Postfix))
            );

            harmony.Patch(
               original: AccessTools.Constructor(typeof(BasicProjectile), new Type[] { typeof(int), typeof(int), typeof(int), typeof(int), typeof(float), typeof(float), typeof(float), typeof(Vector2), typeof(string), typeof(string), typeof(bool), typeof(bool), typeof(GameLocation), typeof(Character), typeof(bool), typeof(BasicProjectile.onCollisionBehavior) }),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.BasicProjectile_Postfix))
            );


            
            // UI Patches

            harmony.Patch(
               original: AccessTools.Constructor(typeof(SkillsPage), new Type[] { typeof(int), typeof(int), typeof(int), typeof(int) }),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.SkillsPage_Postfix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(SkillsPage), nameof(SkillsPage.draw), new Type[] { typeof(SpriteBatch) }),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.SkillsPage_draw_Postfix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(SkillsPage), nameof(SkillsPage.receiveLeftClick)),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.SkillsPage_receiveLeftClick_Postfix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(SkillsPage), nameof(SkillsPage.performHoverAction))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(CharacterCustomization), nameof(CharacterCustomization.performHoverAction)),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.CharacterCustomization_performHoverAction_Postfix))
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
            Monitor.Log("Mod loaded");
        }

        public override object GetApi()
        {
            return new StardewRPGApi();
        }

        private static void Display_RenderedWorld(object sender, StardewModdingAPI.Events.RenderedWorldEventArgs e)
        {
            if(Config.EnableMod && Config.PermaDeath && Game1.killScreen)
            {
                deathScreenTimer++;
                if (deathScreenTimer > Config.PermaDeathScreenTicks)
                {
                    deathScreenTimer = 0;
                    Game1.killScreen = false;
                    SHelper.Events.Display.RenderedWorld -= Display_RenderedWorld;
                    Game1.ExitToTitle();
                }
                else
                {
                    e.SpriteBatch.Draw(blackTexture, new Rectangle(0, 0, Game1.viewport.Width, Game1.viewport.Height), Color.White);
                    e.SpriteBatch.DrawString(Game1.dialogueFont, "YOU DIED", new Vector2(Game1.viewport.Width / 2 - Game1.dialogueFont.MeasureString("YOU DIED").X * 4, Game1.viewport.Height / 2 - Game1.dialogueFont.MeasureString("YOU DIED").Y * 4), Color.Red * Math.Min(1, deathScreenTimer / (Config.PermaDeathScreenTicks / 2f)), 0, Vector2.Zero, 8, SpriteEffects.None, 1);
                }
            }
        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {

            var cpapi = Helper.ModRegistry.GetApi<IContentPatcherAPI>("Pathoschild.ContentPatcher");
            if (cpapi != null)
            {
                cpapi.RegisterToken(ModManifest, "PlayerStr", () =>
                {
                    // save is loaded
                    if (!Context.IsWorldReady)
                        return null;

                    return new[] { GetStatValue(Game1.player, "str", Config.BaseStatValue) +"" };
                });
                cpapi.RegisterToken(ModManifest, "PlayerCon", () =>
                {
                    // save is loaded
                    if (!Context.IsWorldReady)
                        return null;

                    return new[] { GetStatValue(Game1.player, "con", Config.BaseStatValue) +"" };
                });
                cpapi.RegisterToken(ModManifest, "PlayerDex", () =>
                {
                    // save is loaded
                    if (!Context.IsWorldReady)
                        return null;

                    return new[] { GetStatValue(Game1.player, "dex", Config.BaseStatValue) +"" };
                });
                cpapi.RegisterToken(ModManifest, "PlayerInt", () =>
                {
                    // save is loaded
                    if (!Context.IsWorldReady)
                        return null;

                    return new[] { GetStatValue(Game1.player, "int", Config.BaseStatValue) +"" };
                });
                cpapi.RegisterToken(ModManifest, "PlayerWis", () =>
                {
                    // save is loaded
                    if (!Context.IsWorldReady)
                        return null;

                    return new[] { GetStatValue(Game1.player, "wis", Config.BaseStatValue) +"" };
                });
                cpapi.RegisterToken(ModManifest, "PlayerCha", () =>
                {
                    // save is loaded
                    if (!Context.IsWorldReady)
                        return null;

                    return new[] { GetStatValue(Game1.player, "cha", Config.BaseStatValue) +"" };
                });
            }

            // get Generic Mod Config Menu's API (if it's installed)
            var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is not null)
            {

                // register mod
                configMenu.Register(
                    mod: ModManifest,
                    reset: () => Config = new ModConfig(),
                    save: () => Helper.WriteConfig(Config)
                );
                configMenu.AddBoolOption(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("mod-enabled"),
                    getValue: () => Config.EnableMod,
                    setValue: value => Config.EnableMod = value
                );
                configMenu.AddNumberOption(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("health-regen"),
                    tooltip: () => Helper.Translation.Get("every-ten-minutes"),
                    getValue: () => Config.HealthRegen,
                    setValue: value => Config.HealthRegen = value
                );
                configMenu.AddTextOption(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("stamina-regen"),
                    tooltip: () => Helper.Translation.Get("every-ten-minutes"),
                    getValue: () => Config.StaminaRegen + "",
                    setValue: delegate (string value) { try { Config.StaminaRegen = float.Parse(value, CultureInfo.InvariantCulture); } catch { } }
                );

                configMenu.AddPageLink(
                    mod: ModManifest,
                    pageId: "leveling",
                    text: () => Helper.Translation.Get("leveling")
                );
                configMenu.AddPageLink(
                    mod: ModManifest,
                    pageId: "stats",
                    text: () => Helper.Translation.Get("stats")
                );
                configMenu.AddPageLink(
                    mod: ModManifest,
                    pageId: "bonuses",
                    text: () => Helper.Translation.Get("bonuses")
                );
                configMenu.AddPageLink(
                    mod: ModManifest,
                    pageId: "death",
                    text: () => Helper.Translation.Get("death")
                );
                configMenu.AddPageLink(
                    mod: ModManifest,
                    pageId: "tools",
                    text: () => Helper.Translation.Get("tools")
                );

                configMenu.AddPage(
                    mod: ModManifest,
                    pageId: "leveling",
                    pageTitle: () => Helper.Translation.Get("leveling")
                );
                configMenu.AddBoolOption(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("manual-skills"),
                    tooltip: () => Helper.Translation.Get("manual-skills-desc"),
                    getValue: () => Config.ManualSkillUpgrades,
                    setValue: value => Config.ManualSkillUpgrades = value
                );
                configMenu.AddBoolOption(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("level-notify"),
                    tooltip: () => Helper.Translation.Get("level-notify-desc"),
                    getValue: () => Config.NotifyOnLevelUp,
                    setValue: value => Config.NotifyOnLevelUp = value
                );
                configMenu.AddTextOption(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("level-exp-mult"),
                    tooltip: () => Helper.Translation.Get("level-exp-mult-desc"),
                    getValue: () => Config.LevelIncrementExpMult + "",
                    setValue: delegate (string value) { try { Config.LevelIncrementExpMult = float.Parse(value, CultureInfo.InvariantCulture); } catch { } }
                );
                configMenu.AddNumberOption(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("health-per-level"),
                    tooltip: () => Helper.Translation.Get("health-per-level-desc"),
                    getValue: () => Config.BaseHealthPerLevel,
                    setValue: value => Config.BaseHealthPerLevel = value
                );
                configMenu.AddNumberOption(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("stamina-per-level"),
                    tooltip: () => Helper.Translation.Get("stamina-per-level-desc"),
                    getValue: () => Config.BaseStaminaPerLevel,
                    setValue: value => Config.BaseStaminaPerLevel = value
                );

                configMenu.AddPage(
                    mod: ModManifest,
                    pageId: "stats",
                    pageTitle: () => Helper.Translation.Get("stats")
                );
                configMenu.AddNumberOption(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("base-value"),
                    tooltip: () => Helper.Translation.Get("base-value-desc"),
                    getValue: () => Config.BaseStatValue,
                    setValue: value => Config.BaseStatValue = value,
                    min: Config.MinStatValue,
                    max: Config.MaxStatValue
                );
                configMenu.AddNumberOption(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("min-value"),
                    tooltip: () => Helper.Translation.Get("min-value-desc"),
                    getValue: () => Config.MinStatValue,
                    setValue: value => Config.MinStatValue = value
                );
                configMenu.AddNumberOption(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("max-value"),
                    tooltip: () => Helper.Translation.Get("max-value-desc"),
                    getValue: () => Config.MaxStatValue,
                    setValue: value => Config.MaxStatValue = value
                );
                configMenu.AddTextOption(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("bonus-levels"),
                    tooltip: () => Helper.Translation.Get("bonus-levels-desc"),
                    getValue: () => Config.StatBonusLevels,
                    setValue: value => Config.StatBonusLevels = value
                );
                configMenu.AddTextOption(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("bonus-levels"),
                    tooltip: () => Helper.Translation.Get("bonus-levels-desc"),
                    getValue: () => Config.StatPenaltyLevels,
                    setValue: value => Config.StatPenaltyLevels = value
                );
                configMenu.AddNumberOption(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("start-points"),
                    tooltip: () => Helper.Translation.Get("start-points-desc"),
                    getValue: () => Config.StartStatExtraPoints,
                    setValue: value => Config.StartStatExtraPoints = value
                );
                configMenu.AddNumberOption(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("points-per-stardrop"),
                    tooltip: () => Helper.Translation.Get("points-per-stardrop-desc"),
                    getValue: () => Config.StatPointsPerStardrop,
                    setValue: value => Config.StatPointsPerStardrop = value
                );

                configMenu.AddPage(
                    mod: ModManifest,
                    pageId: "bonuses",
                    pageTitle: () => Helper.Translation.Get("bonuses")
                );

                configMenu.AddSectionTitle(
                    mod: ModManifest,
                    text: () => Helper.Translation.Get("str-full")
                );
                configMenu.AddTextOption(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("club-damage"),
                    tooltip: () => Helper.Translation.Get("stat-bonus-desc"),
                    getValue: () => Config.StrClubDamageBonus + "",
                    setValue: delegate (string value) { try { Config.StrClubDamageBonus = float.Parse(value, CultureInfo.InvariantCulture); } catch { } }
                );
                configMenu.AddNumberOption(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("club-speed"),
                    tooltip: () => Helper.Translation.Get("stat-bonus-desc"),
                    getValue: () => Config.StrClubSpeedBonus,
                    setValue: value => Config.StrClubSpeedBonus = value
                );
                configMenu.AddNumberOption(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("pickaxe-damage"),
                    tooltip: () => Helper.Translation.Get("stat-bonus-desc"),
                    getValue: () => Config.StrPickaxeDamageBonus,
                    setValue: value => Config.StrPickaxeDamageBonus = value
                );
                configMenu.AddNumberOption(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("axe-damage"),
                    tooltip: () => Helper.Translation.Get("stat-bonus-desc"),
                    getValue: () => Config.StrAxeDamageBonus,
                    setValue: value => Config.StrAxeDamageBonus = value
                );
                configMenu.AddTextOption(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("crit-damage"),
                    tooltip: () => Helper.Translation.Get("stat-bonus-desc"),
                    getValue: () => Config.StrCritDamageBonus + "",
                    setValue: delegate (string value) { try { Config.StrCritDamageBonus = float.Parse(value, CultureInfo.InvariantCulture); } catch { } }
                );
                configMenu.AddTextOption(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("fish-reel-speed"),
                    tooltip: () => Helper.Translation.Get("stat-bonus-desc"),
                    getValue: () => Config.StrFishingReelSpeedBonus + "",
                    setValue: delegate (string value) { try { Config.StrFishingReelSpeedBonus = float.Parse(value, CultureInfo.InvariantCulture); } catch { } }
                );
                configMenu.AddTextOption(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("fish-treasure-speed"),
                    tooltip: () => Helper.Translation.Get("stat-bonus-desc"),
                    getValue: () => Config.StrFishingTreasureSpeedBonus + "",
                    setValue: delegate (string value) { try { Config.StrFishingTreasureSpeedBonus = float.Parse(value, CultureInfo.InvariantCulture); } catch { } }
                );

                configMenu.AddSectionTitle(
                    mod: ModManifest,
                    text: () => Helper.Translation.Get("con-full")
                );
                configMenu.AddTextOption(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("sword-damage"),
                    tooltip: () => Helper.Translation.Get("stat-bonus-desc"),
                    getValue: () => Config.ConSwordDamageBonus + "",
                    setValue: delegate (string value) { try { Config.ConSwordDamageBonus = float.Parse(value, CultureInfo.InvariantCulture); } catch { } }
                );
                configMenu.AddNumberOption(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("sword-speed"),
                    tooltip: () => Helper.Translation.Get("stat-bonus-desc"),
                    getValue: () => Config.ConSwordSpeedBonus,
                    setValue: value => Config.ConSwordSpeedBonus = value
                );
                configMenu.AddNumberOption(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("defense-bonus"),
                    tooltip: () => Helper.Translation.Get("stat-bonus-desc"),
                    getValue: () => Config.ConDefenseBonus,
                    setValue: value => Config.ConDefenseBonus = value
                );
                configMenu.AddTextOption(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("health-bonus"),
                    tooltip: () => Helper.Translation.Get("stat-bonus-desc"),
                    getValue: () => Config.ConHealthBonus + "",
                    setValue: delegate (string value) { try { Config.ConHealthBonus = float.Parse(value, CultureInfo.InvariantCulture); } catch { } }
                );
                configMenu.AddTextOption(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("stamina-bonus"),
                    tooltip: () => Helper.Translation.Get("stat-bonus-desc"),
                    getValue: () => Config.ConStaminaBonus + "",
                    setValue: delegate (string value) { try { Config.ConStaminaBonus = float.Parse(value, CultureInfo.InvariantCulture); } catch { } }
                );
                configMenu.AddNumberOption(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("health-regen-bonus"),
                    tooltip: () => Helper.Translation.Get("stat-bonus-desc"),
                    getValue: () => Config.ConHealthRegenBonus,
                    setValue: value => Config.ConHealthRegenBonus = value
                );
                configMenu.AddTextOption(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("stamina-regen-bonus"),
                    tooltip: () => Helper.Translation.Get("stat-bonus-desc"),
                    getValue: () => Config.ConStaminaRegenBonus + "",
                    setValue: delegate (string value) { try { Config.ConStaminaRegenBonus = float.Parse(value, CultureInfo.InvariantCulture); } catch { } }
                );
                configMenu.AddBoolOption(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("resist-buff-roll"),
                    tooltip: () => Helper.Translation.Get("resist-buff-roll-desc"),
                    getValue: () => Config.ConRollToResistDebuff,
                    setValue: value => Config.ConRollToResistDebuff = value
                );
                configMenu.AddTextOption(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("debuff-dur"),
                    tooltip: () => Helper.Translation.Get("stat-bonus-desc"),
                    getValue: () => Config.ConDebuffDurationBonus + "",
                    setValue: delegate (string value) { try { Config.ConDebuffDurationBonus = float.Parse(value, CultureInfo.InvariantCulture); } catch { } }
                );

                configMenu.AddSectionTitle(
                    mod: ModManifest,
                    text: () => Helper.Translation.Get("dex-full")
                );
                configMenu.AddTextOption(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("dagger-damage"),
                    tooltip: () => Helper.Translation.Get("stat-bonus-desc"),
                    getValue: () => Config.DexDaggerDamageBonus + "",
                    setValue: delegate (string value) { try { Config.DexDaggerDamageBonus = float.Parse(value, CultureInfo.InvariantCulture); } catch { } }
                );
                configMenu.AddTextOption(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("ranged-damage"),
                    tooltip: () => Helper.Translation.Get("stat-bonus-desc"),
                    getValue: () => Config.DexRangedDamageBonus + "",
                    setValue: delegate (string value) { try { Config.DexRangedDamageBonus = float.Parse(value, CultureInfo.InvariantCulture); } catch { } }
                );
                configMenu.AddNumberOption(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("dagger-speed"),
                    tooltip: () => Helper.Translation.Get("stat-bonus-desc"),
                    getValue: () => Config.DexDaggerSpeedBonus,
                    setValue: value => Config.DexDaggerSpeedBonus = value
                );
                configMenu.AddTextOption(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("crit-chance"),
                    tooltip: () => Helper.Translation.Get("stat-bonus-desc"),
                    getValue: () => Config.DexCritChanceBonus + "",
                    setValue: delegate (string value) { try { Config.DexCritChanceBonus = float.Parse(value, CultureInfo.InvariantCulture); } catch { } }
                );
                configMenu.AddNumberOption(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("bobber-size"),
                    tooltip: () => Helper.Translation.Get("stat-bonus-desc"),
                    getValue: () => Config.DexFishingBobberSizeBonus,
                    setValue: value => Config.DexFishingBobberSizeBonus = value
                );
                configMenu.AddBoolOption(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("dodge-roll"),
                    tooltip: () => Helper.Translation.Get("dodge-roll-desc"),
                    getValue: () => Config.DexRollForMiss,
                    setValue: value => Config.DexRollForMiss = value
                );

                configMenu.AddSectionTitle(
                    mod: ModManifest,
                    text: () => Helper.Translation.Get("int-full")
                );
                configMenu.AddTextOption(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("skill-bonus"),
                    tooltip: () => Helper.Translation.Get("stat-bonus-desc"),
                    getValue: () => Config.IntSkillLevelsBonus + "",
                    setValue: delegate (string value) { try { Config.IntSkillLevelsBonus = float.Parse(value, CultureInfo.InvariantCulture); } catch { } }
                );
                configMenu.AddTextOption(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("crop-quality"),
                    tooltip: () => Helper.Translation.Get("stat-bonus-desc"),
                    getValue: () => Config.IntCropQualityBonus + "",
                    setValue: delegate (string value) { try { Config.IntCropQualityBonus = float.Parse(value, CultureInfo.InvariantCulture); } catch { } }
                );
                configMenu.AddTextOption(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("artifact-chance"),
                    tooltip: () => Helper.Translation.Get("stat-bonus-desc"),
                    getValue: () => Config.IntArtifactSpotChanceBonus + "",
                    setValue: delegate (string value) { try { Config.IntArtifactSpotChanceBonus = float.Parse(value, CultureInfo.InvariantCulture); } catch { } }
                );
                configMenu.AddTextOption(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("pan-chance"),
                    tooltip: () => Helper.Translation.Get("stat-bonus-desc"),
                    getValue: () => Config.IntPanSpotChanceBonus + "",
                    setValue: delegate (string value) { try { Config.IntPanSpotChanceBonus = float.Parse(value, CultureInfo.InvariantCulture); } catch { } }
                );
                configMenu.AddTextOption(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("fish-spot-chance"),
                    tooltip: () => Helper.Translation.Get("stat-bonus-desc"),
                    getValue: () => Config.IntFishSpotChanceBonus + "",
                    setValue: delegate (string value) { try { Config.IntFishSpotChanceBonus = float.Parse(value, CultureInfo.InvariantCulture); } catch { } }
                );
                configMenu.AddNumberOption(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("forage-chance"),
                    tooltip: () => Helper.Translation.Get("stat-bonus-desc"),
                    getValue: () => Config.IntForagingSpotChanceBonus,
                    setValue: value => Config.IntForagingSpotChanceBonus = value
                );
                configMenu.AddBoolOption(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("craft-roll"),
                    tooltip: () => Helper.Translation.Get("craft-roll-desc"),
                    getValue: () => Config.IntRollCraftingChance,
                    setValue: value => Config.IntRollCraftingChance = value
                );

                configMenu.AddSectionTitle(
                    mod: ModManifest,
                    text: () => Helper.Translation.Get("wis-full")
                );
                configMenu.AddTextOption(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("craft-resource-bonus"),
                    tooltip: () => Helper.Translation.Get("stat-bonus-desc"),
                    getValue: () => Config.WisCraftResourceReqBonus + "",
                    setValue: delegate (string value) { try { Config.WisCraftResourceReqBonus = float.Parse(value, CultureInfo.InvariantCulture); } catch { } }
                );
                configMenu.AddTextOption(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("craft-time-bonus"),
                    tooltip: () => Helper.Translation.Get("stat-bonus-desc"),
                    getValue: () => Config.WisCraftTimeBonus + "",
                    setValue: delegate (string value) { try { Config.WisCraftTimeBonus = float.Parse(value, CultureInfo.InvariantCulture); } catch { } }
                );
                configMenu.AddTextOption(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("exp-bonus"),
                    tooltip: () => Helper.Translation.Get("stat-bonus-desc"),
                    getValue: () => Config.WisExpBonus + "",
                    setValue: delegate (string value) { try { Config.WisExpBonus = float.Parse(value, CultureInfo.InvariantCulture); } catch { } }
                );
                configMenu.AddTextOption(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("spot-visibility-bonus"),
                    tooltip: () => Helper.Translation.Get("stat-bonus-desc"),
                    getValue: () => Config.WisSpotVisibility + "",
                    setValue: delegate (string value) { try { Config.WisSpotVisibility = float.Parse(value, CultureInfo.InvariantCulture); } catch { } }
                );

                configMenu.AddSectionTitle(
                    mod: ModManifest,
                    text: () => Helper.Translation.Get("cha-full")
                );
                configMenu.AddTextOption(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("friend-bonus"),
                    tooltip: () => Helper.Translation.Get("stat-bonus-desc"),
                    getValue: () => Config.ChaFriendshipBonus + "",
                    setValue: delegate (string value) { try { Config.ChaFriendshipBonus = float.Parse(value, CultureInfo.InvariantCulture); } catch { } }
                );
                configMenu.AddTextOption(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("price-bonus"),
                    tooltip: () => Helper.Translation.Get("stat-bonus-desc"),
                    getValue: () => Config.ChaPriceBonus + "",
                    setValue: delegate (string value) { try { Config.ChaPriceBonus = float.Parse(value, CultureInfo.InvariantCulture); } catch { } }
                );
                configMenu.AddBoolOption(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("romance-roll"),
                    tooltip: () => Helper.Translation.Get("romance-roll-desc"),
                    getValue: () => Config.ChaRollRomanceChance,
                    setValue: value => Config.ChaRollRomanceChance = value
                );



                configMenu.AddPage(
                    mod: ModManifest,
                    pageId: "death",
                    pageTitle: () => Helper.Translation.Get("death")
                );
                configMenu.AddBoolOption(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("perma-death"),
                    tooltip: () => Helper.Translation.Get("perma-death-desc"),
                    getValue: () => Config.PermaDeath,
                    setValue: value => Config.PermaDeath = value
                ); ;
                configMenu.AddNumberOption(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("perma-death-ticks"),
                    tooltip: () => Helper.Translation.Get("perma-death-ticks-desc"),
                    getValue: () => Config.PermaDeathScreenTicks,
                    setValue: value => Config.PermaDeathScreenTicks = value
                );
                configMenu.AddTextOption(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("exp-lost-on-death"),
                    tooltip: () => Helper.Translation.Get("exp-lost-on-death-desc"),
                    getValue: () => Config.ExperienceLossPercentOnDeath + "",
                    setValue: delegate (string value) { try { Config.ExperienceLossPercentOnDeath = float.Parse(value, CultureInfo.InvariantCulture); } catch { } }
                );

                configMenu.AddPage(
                    mod: ModManifest,
                    pageId: "tools",
                    pageTitle: () => Helper.Translation.Get("tools")
                );
                configMenu.AddTextOption(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("tool-level-req"),
                    tooltip: () => Helper.Translation.Get("tool-level-req-desc"),
                    getValue: () => Config.ToolLevelReqMult + "",
                    setValue: delegate (string value) { try { Config.ToolLevelReqMult = float.Parse(value, CultureInfo.InvariantCulture); } catch { } }
                );
                configMenu.AddTextOption(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("weapon-level-req"),
                    tooltip: () => Helper.Translation.Get("weapon-level-req-desc"),
                    getValue: () => Config.WeaponLevelReqMult + "",
                    setValue: delegate (string value) { try { Config.WeaponLevelReqMult = float.Parse(value, CultureInfo.InvariantCulture); } catch { } }
                );
            }

        }
    }
}