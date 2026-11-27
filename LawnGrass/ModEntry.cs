using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace LawnGrass
{
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;
        public static ModEntry context;
        public const string lawnKey = "aedenthorn.LawnGrass/lawn";
        public const string maskKey = "aedenthorn.LawnGrass/mask";
        public const string sumsKey = "aedenthorn.LawnGrass/sums";
        public const string posKey = "aedenthorn.LawnGrass/pos";
        public const string lawnPath = "aedenthorn.LawnGrass/lawn_";
        public static PerScreen<int> lastWeedCount = new();
        /// <summary>Lawn tiles that stayed mown in dayUpdate tonight; growWeedGrass must not regrow them.</summary>
        public static readonly HashSet<Grass> heldMownTonight = new(ReferenceEqualityComparer.Instance);

        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();
            SMonitor = Monitor;
            SHelper = helper;

            context = this;

            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.GameLoop.DayStarted += (_, _) => heldMownTonight.Clear();
            helper.Events.Content.AssetRequested += Content_AssetRequested;
            helper.Events.Content.AssetsInvalidated += Content_AssetsInvalidated;
            helper.Events.Display.RenderedStep += Display_RenderedStep;


            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }

        private void Display_RenderedStep(object sender, RenderedStepEventArgs e)
        {
            if (!Config.ModEnabled || !Context.IsWorldReady || Game1.currentLocation?.terrainFeatures is null || e.Step != StardewValley.Mods.RenderSteps.World_Background)
                return;
            for (int y = Game1.viewport.Y / 64 - 1; y < (Game1.viewport.Y + Game1.viewport.Height) / 64 + 7; y++)
            {
                for (int x = Game1.viewport.X / 64 - 1; x < (Game1.viewport.X + Game1.viewport.Width) / 64 + 3; x++)
                {
                    Vector2 tile = new(x, y);
                    if (Game1.currentLocation.terrainFeatures.TryGetValue(tile, out var tf) && tf is Grass grass)
                    {
                        if (!IsLawn(grass) && (!Config.ProtectNonLawn || grass.grassType.Value != 1))
                            continue;
                        if (grass.numberOfWeeds.Value < 0)
                            grass.numberOfWeeds.Value = 0;
                        if (!grass.modData.TryGetValue(posKey, out var str) || !int.TryParse(str, out var pos))
                        {
                            UpdateDrawSums(grass);
                            if (!grass.modData.TryGetValue(posKey, out str) || !int.TryParse(str, out pos))
                            {
                                return;
                            }
                        }
                        var drawPos = Game1.GlobalToLocal(grass.Tile * 64f);
                        e.SpriteBatch.Draw(SHelper.GameContent.Load<Texture2D>(lawnPath + GetWhichSeason(grass.Location)), drawPos, new Rectangle?(new Rectangle(pos % 4 * 16, pos / 4 * 16, 16, 16)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0);
                    }
                }
            }
        }
        private void Content_AssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (!Config.ModEnabled)
                return;
            foreach (Season s in Enum.GetValues(typeof(Season)))
            {
                var season = Utility.getSeasonKey(s);
                if (e.NameWithoutLocale.IsEquivalentTo(lawnPath + season))
                {
                    e.LoadFromModFile<Texture2D>($"assets/lawn_{season}.png", AssetLoadPriority.Exclusive);
                    if (Config.MatchMapGrass)
                        e.Edit(asset => PaintLawnFromMap(asset, season), AssetEditPriority.Late);
                }
            }
        }

        private void Content_AssetsInvalidated(object sender, AssetsInvalidatedEventArgs e)
        {
            if (Config.MatchMapGrass && e.NamesWithoutLocale.Any(n => n.Name.Contains("_outdoorsTileSheet", StringComparison.OrdinalIgnoreCase)))
                InvalidateLawn();
        }

        private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {
        
            var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is not null)
            {
                configMenu.Register(
                    mod: ModManifest,
                    reset: () => Config = new ModConfig(),
                    save: () => { Helper.WriteConfig(Config); InvalidateLawn(); }
                );

                configMenu.AddBoolOption(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("ModEnabled"),
                    getValue: () => Config.ModEnabled,
                    setValue: value => Config.ModEnabled = value
                );
                var props = typeof(ModConfig).GetProperties().ToArray();
                var configMenuExt = Helper.ModRegistry.GetApi<IGMCMOptionsAPI>("jltaylor-us.GMCMOptions");

                foreach (var p in props)
                {
                    if (p.Name == nameof(Config.ModEnabled) || p.Name == nameof(Config.Debug))
                        continue;
                    if (p.PropertyType == typeof(bool))
                    {
                        configMenu.AddBoolOption(
                            mod: ModManifest,
                            name: () => SHelper.Translation.Get(p.Name),
                            tooltip: () => { var t = SHelper.Translation.Get(p.Name + ".Desc"); return t.HasValue() ? t : null; },
                            getValue: () => (bool)p.GetValue(Config),
                            setValue: value => p.SetValue(Config, value)
                        );
                    }
                    else if (p.PropertyType == typeof(int))
                    {
                        configMenu.AddNumberOption(
                            mod: ModManifest,
                            name: () => SHelper.Translation.Get(p.Name),
                            tooltip: () => { var t = SHelper.Translation.Get(p.Name + ".Desc"); return t.HasValue() ? t : null; },
                            getValue: () => (int)p.GetValue(Config),
                            setValue: value => p.SetValue(Config, value)
                        );
                    }
                    else if (p.PropertyType == typeof(float))
                    {
                        configMenu.AddNumberOption(
                            mod: ModManifest,
                            name: () => SHelper.Translation.Get(p.Name),
                            tooltip: () => { var t = SHelper.Translation.Get(p.Name + ".Desc"); return t.HasValue() ? t : null; },
                            getValue: () => (float)p.GetValue(Config),
                            setValue: value => p.SetValue(Config, value)
                        );
                    }
                    else if (p.PropertyType == typeof(double))
                    {
                        configMenu.AddTextOption(
                            mod: ModManifest,
                            name: () => SHelper.Translation.Get(p.Name),
                            tooltip: () => { var t = SHelper.Translation.Get(p.Name + ".Desc"); return t.HasValue() ? t : null; },
                            getValue: () => p.GetValue(Config).ToString(),
                            setValue: value => { if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var d)) { p.SetValue(Config, d); } }
                        );
                    }
                    else if (p.PropertyType == typeof(string))
                    {
                        configMenu.AddTextOption(
                            mod: ModManifest,
                            name: () => SHelper.Translation.Get(p.Name),
                            tooltip: () => { var t = SHelper.Translation.Get(p.Name + ".Desc"); return t.HasValue() ? t : null; },
                            getValue: () => (string)p.GetValue(Config),
                            setValue: value => p.SetValue(Config, value)
                        );
                    }
                    else if (p.PropertyType == typeof(KeybindList))
                    {
                        configMenu.AddKeybindList(
                            mod: ModManifest,
                            name: () => SHelper.Translation.Get(p.Name),
                            tooltip: () => { var t = SHelper.Translation.Get(p.Name + ".Desc"); return t.HasValue() ? t : null; },
                            getValue: () => (KeybindList)p.GetValue(Config),
                            setValue: value => p.SetValue(Config, value)
                        );
                    }
                    else if (p.PropertyType == typeof(SButton))
                    {
                        configMenu.AddKeybind(
                            mod: ModManifest,
                            name: () => SHelper.Translation.Get(p.Name),
                            tooltip: () => { var t = SHelper.Translation.Get(p.Name + ".Desc"); return t.HasValue() ? t : null; },
                            getValue: () => (SButton)p.GetValue(Config),
                            setValue: value => p.SetValue(Config, value)
                        );
                    }
                    else if (p.PropertyType == typeof(Color) && configMenuExt is not null)
                    {
                        configMenuExt.AddColorOption(
                            mod: ModManifest,
                            name: () => SHelper.Translation.Get(p.Name),
                            tooltip: () => { var t = SHelper.Translation.Get(p.Name + ".Desc"); return t.HasValue() ? t : null; },
                            getValue: () => (Color)p.GetValue(Config),
                            setValue: value => p.SetValue(Config, value)
                        );
                    }
                }
            }
        }
    }
}