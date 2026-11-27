using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Monsters;
using StardewValley.TerrainFeatures;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using xTile.Tiles;

namespace AreaOfEffect
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;
        public static ModEntry context;
        public static bool loadingTutorials;

        public const string toolsPath = "aedenthorn.AreaOfEffect/tools";
        public const string effectsPath = "aedenthorn.AreaOfEffect/effects";
        public const string spellsPath = "aedenthorn.AreaOfEffect/spells";
        public const string copperWand = "aedenthorn.AreaOfEffect/copperWand";
        public const string silverWand = "aedenthorn.AreaOfEffect/silverWand";
        public const string goldWand = "aedenthorn.AreaOfEffect/goldWand";
        public const string fireballWand = "aedenthorn.AreaOfEffect/fireballWand";
        public const string chargesKey = "aedenthorn.AreaOfEffect/charges";
        public const string effectKey = "aedenthorn.AreaOfEffect/effect";
        public const string lightKey = "aedenthorn.AreaOfEffect/light";
        public const string texturePrefix = "aedenthorn.AreaOfEffect/textures/";
        public const string internalPrefix = "aedenthorn.AreaOfEffect/internal/";

        public static Dictionary<string, Texture2D> TextureDict { get; set; } = new();
        public static Dictionary<string, SpellLightData> LightDict { get; set; } = new();
        public static Dictionary<Monster, MonsterBuffManager> BuffDict { get; set; } = new();
        public static Dictionary<object, DelayedEffectData> EOTObjDict { get; set; } = new();
        public static List<DelayedEffectData> DelayTileList { get; set; } = new();
        public static List<DelayedEffectData> EOTTileList { get; set; } = new();
        public static Dictionary<TemporaryAnimatedSprite, MovingSpriteData> MovingSpriteDict { get; set; } = new();
        public static Dictionary<SpellProjectile, LinearProjectileInstance> ProjectileDict { get; set; } = new();

        public static Dictionary<string, SpellToolData> ToolDict
        {
            get
            {
                return SHelper.GameContent.Load<Dictionary<string, SpellToolData>>(toolsPath);
            }
        }

        public static Dictionary<string, SpellData> SpellDict
        {
            get
            {
                return SHelper.GameContent.Load<Dictionary<string, SpellData>>(spellsPath);
            }
        }

        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();
            SMonitor = Monitor;
            SHelper = helper;

            context = this;

            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;
            helper.Events.Content.AssetRequested += Content_AssetRequested;
            helper.Events.Input.ButtonPressed += Input_ButtonPressed;
            helper.Events.Input.ButtonsChanged += Input_ButtonsChanged;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();

        }

        private void GameLoop_UpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (loadingTutorials)
            {
                CreateTutorials();
                loadingTutorials = false;
            }
            if (Game1.shouldTimePass())
            {
                if (BuffDict.Any())
                {
                    if (!Context.IsWorldReady)
                    {
                        BuffDict.Clear();
                        return;
                    }
                    foreach (var key in BuffDict.Keys.ToArray())
                    {
                        var m = BuffDict[key];
                        if (m.Update(Game1.currentGameTime))
                        {
                            BuffDict.Remove(key);
                        }
                    }
                }
                if (LightDict.Any())
                {
                    foreach (var key in LightDict.Keys.ToArray())
                    {
                        var m = LightDict[key];
                        if (m.Update(Game1.currentGameTime))
                        {
                            LightDict.Remove(key);
                        }
                    }
                }
                if (MovingSpriteDict.Any())
                {
                    foreach (var key in MovingSpriteDict.Keys.ToArray())
                    {
                        var m = MovingSpriteDict[key];
                        var moved = m.Parent.Position - m.LastPos;
                        key.Position += moved;
                        m.LastPos = key.Position;
                        if (!m.Location.temporarySprites.Contains(key))
                        {
                            MovingSpriteDict.Remove(key);
                        }
                    }
                }
                if (DelayTileList.Any())
                {
                    for (int i = DelayTileList.Count - 1; i >= 0; i--)
                    {
                        var d = DelayTileList[i];
                        d.Milliseconds -= Game1.currentGameTime.ElapsedGameTime.Milliseconds;
                        if (d.Milliseconds > 0)
                            continue;
                        if (!d.Effect.PerTile)
                        {
                            ApplyCenteredEffect(d.Location, d.Who, d.Tile, d.Effect, d.Radius);
                            if (d.Effect.Seconds > 0 && d.Effect.ReapplyToTile)
                            {
                                EOTTileList.Add(new(d.Location, d.Who, d.Tiles, d.Effect, d.Effect.Seconds * 1000 - 1));
                            }
                        }
                        else
                        {
                            List<object> applied = new();
                            foreach (var tile in d.Tiles)
                            {
                                ApplyEffectToTile(d.Location, d.Who, tile, d.Effect, applied);
                            }
                            if (d.Effect.Seconds > 0 && d.Effect.ReapplyToTile)
                            {
                                EOTTileList.Add(new(d.Location, d.Who, d.Tiles, d.Effect, d.Effect.Seconds * 1000 - 1));
                            }
                        }
                        DelayTileList.RemoveAt(i);
                    }
                }
                if (EOTTileList.Any())
                {
                    for (int i = EOTTileList.Count - 1; i >= 0; i--)
                    {
                        var d = EOTTileList[i];
                        int seconds = d.Milliseconds / 1000;
                        d.Milliseconds -= Game1.currentGameTime.ElapsedGameTime.Milliseconds;
                        if (d.Milliseconds / 1000 == seconds && d.Milliseconds > 0)
                            continue;
                        if (!d.Effect.PerTile)
                        {
                            ApplyCenteredEffect(d.Location, d.Who, d.Tile, d.Effect, d.Radius);
                        }
                        else
                        {
                            List<object> applied = new();
                            foreach (var tile in d.Tiles)
                            {
                                ApplyEffectToTile(d.Location, d.Who, tile, d.Effect, applied);
                            }
                        }
                        if (d.Milliseconds <= 0)
                        {
                            EOTTileList.RemoveAt(i);
                        }
                    }
                }
                if (EOTObjDict.Any())
                {
                    foreach (var obj in EOTObjDict.Keys.ToArray())
                    {
                        var d = EOTObjDict[obj];
                        int seconds = d.Milliseconds / 1000;
                        d.Milliseconds -= Game1.currentGameTime.ElapsedGameTime.Milliseconds;
                        if (d.Milliseconds / 1000 == seconds && d.Milliseconds > 0)
                            continue;
                        if (obj is Monster m)
                        {
                            ApplyMonsterEffect(d.Location, d.Who, m, d.Effect);
                        }
                        else if (obj is Farmer f)
                        {
                            ApplyFarmerEffect(d.Location, f, d.Effect);
                        }
                        else if (obj is Object o)
                        {
                            ApplyObjectEffect(d.Location, d.Who, d.Tile, o, d.Effect);
                        }
                        else if (obj is TerrainFeature tf)
                        {
                            ApplyTerrainFeatureEffect(d.Location, d.Who, d.Tile, tf, d.Effect);
                        }
                        else if (obj is ResourceClump rc)
                        {
                            ApplyResourceClumpEffect(d.Location, d.Who, d.Tile, rc, d.Effect);
                        }
                        else if (obj is Crop c)
                        {
                            ApplyCropEffect(d.Location, d.Who, d.Tile, c.Dirt, d.Effect);
                        }
                        else if (obj is Horse h)
                        {
                            ApplyHorseEffect(d.Location, d.Who, h, d.Effect);
                        }
                        else if (obj is Pet p)
                        {
                            ApplyPetEffect(d.Location, d.Who, p, d.Effect);
                        }
                        else if (obj is NPC n && n.IsVillager)
                        {
                            ApplyNPCEffect(d.Location, d.Who, n, d.Effect);
                        }
                        else if (obj is FarmAnimal a)
                        {
                            ApplyAnimalEffect(d.Location, d.Who, a, d.Effect);
                        }
                        else if (obj is Building b)
                        {
                            ApplyBuildingEffect(d.Location, d.Who, b, d.Effect);
                        }
                        if(d.Milliseconds <= 0)
                        {
                            EOTObjDict.Remove(obj);
                        }
                    }
                }
            }
            if (ProjectileDict.Any())
            {
                foreach (var p in ProjectileDict.Keys.ToArray())
                {
                    var data = ProjectileDict[p];
                    if(!data.location.projectiles.Contains(p))
                    {
                        ProjectileDict.Remove(p);
                        continue;
                    }
                    foreach(var tile in GetRoundedTiles(p.position.Value / 64))
                    {
                        if (Vector2.Distance(data.firer.Tile, tile) < 2)
                            continue;
                        if (!data.affectedTiles.Contains(tile))
                        {
                            foreach (var effect in data.spell.Effects)
                            {
                                if (effect.PerTile)
                                {
                                    List<object> applied = new();
                                    ApplyEffectToTile(data.location, data.firer, tile, effect, applied);
                                }
                            }
                            foreach (var s in data.spell.Sprites)
                            {
                                if (s.PerTile)
                                {
                                    ApplySpriteToTile(data.location, tile, s, Vector2.Distance(data.target, tile));
                                }
                            }
                            data.affectedTiles.Add(tile);
                        }
                    }
                }
            }
        }

        private void Input_ButtonsChanged(object sender, ButtonsChangedEventArgs e)
        {
            if (!Config.ModEnabled)
                return;
            if (Context.IsPlayerFree)
            {
                if (Config.CastButton.JustPressed() && Game1.player.CurrentTool is Tool t && TryGetTool(t, out var data) && data.Spells?.Count != 1)
                {
                    Game1.activeClickableMenu = new CastSpellMenu(t, data.Spells);
                    foreach(var k in Config.CastButton.Keybinds)
                    {
                        foreach (var b in k.Buttons)
                        {
                            SHelper.Input.Suppress(b);
                        }
                    }
                }
            }
        }

        public override object GetApi()
        {
            return new AOEAPI();
        }

        private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Config.ModEnabled)
                return;
            if (Config.Debug)
            {
                if (Context.IsWorldReady)
                {
                    //SHelper.GameContent.InvalidateCache("Data/Shops");

                    //if(Game1.player.CursorSlotItem is Tool t)
                    //{
                    //    var x = t.QualifiedItemId;
                    //    ParsedItemData data = ItemRegistry.GetDataOrErrorItem(x);
                    //    var y = 2;
                    //}
                    //SHelper.GameContent.InvalidateCache(spellsPath);
                    //Game1.player.mailReceived.Add("hasPickedUpMagicInk");
                    if (e.Button == SButton.NumPad4)
                    {
                        //foreach(var k in TextureDict.Keys)
                        //{
                        //    SHelper.GameContent.InvalidateCache(k);
                        //}
                        //CreateTutorials();
                        //File.WriteAllText(Path.Combine(SHelper.DirectoryPath, "test.json"), JsonConvert.SerializeObject(SpellDict, Formatting.Indented));
                    }
                }
            }
        }

        private void Content_AssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (!Config.ModEnabled)
                return;
            if (e.NameWithoutLocale.StartsWith(texturePrefix))
            {
                if (TextureDict.TryGetValue(e.NameWithoutLocale.ToString(), out Texture2D tex))
                {
                    e.LoadFrom(() => tex, Priority.Last);
                }
            }
            else if (e.NameWithoutLocale.StartsWith(internalPrefix))
            {
                e.LoadFromModFile<Texture2D>(e.NameWithoutLocale.ToString().Replace(internalPrefix, "assets/"), AssetLoadPriority.Exclusive);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo(spellsPath))
            {
                e.LoadFrom(() => new Dictionary<string, SpellData>(), AssetLoadPriority.Exclusive);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo(toolsPath))
            {
                e.LoadFrom(() => new Dictionary<string, SpellToolData>(), AssetLoadPriority.Exclusive);
            }
        }

        private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {
            if (Helper.ModRegistry.IsLoaded("aedenthorn.Tutorials"))
            {
                loadingTutorials = true;
            }
            if (Config.Debug)
            {
                //var qCPF = Helper.ModRegistry.GetApi<IQCPFAPI>("aedenthorn.QCPF");
                //qCPF.StartPack();
                //qCPF.AddEditData(toolsPath, new Dictionary<string, object>(ToolDict.Select(kvp => new KeyValuePair<string, object>(kvp.Key, kvp.Value))));
                //qCPF.WritePack(Path.Combine(SHelper.DirectoryPath, "tools.json"));
            }
            var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is not null)
            {
                configMenu.Register(
                    mod: ModManifest,
                    reset: () => Config = new ModConfig(),
                    save: () => Helper.WriteConfig(Config)
                );

                var exclude = new List<string>()
                {
                    "Debug"
                };
                var props = typeof(ModConfig).GetProperties().ToArray();
                var configMenuExt = Helper.ModRegistry.GetApi<IGMCMOptionsAPI>("jltaylor-us.GMCMOptions");


                foreach (var p in props)
                {
                    if (exclude.Contains(p.Name))
                        continue;
                    if (p.PropertyType == typeof(bool))
                    {
                        configMenu.AddBoolOption(
                            mod: ModManifest,
                            name: () => { var t = Helper.Translation.Get(p.Name); return t.HasValue() ? t : AddSpaces(p.Name); },
                            tooltip: () => { var t = Helper.Translation.Get(p.Name + ".Desc"); return t.HasValue() ? t : null; },
                            getValue: () => (bool)p.GetValue(Config),
                            setValue: value => p.SetValue(Config, value)
                        );
                    }
                    else if (p.PropertyType == typeof(int))
                    {
                        configMenu.AddNumberOption(
                            mod: ModManifest,
                            name: () => { var t = Helper.Translation.Get(p.Name); return t.HasValue() ? t : AddSpaces(p.Name); },
                            tooltip: () => { var t = Helper.Translation.Get(p.Name + ".Desc"); return t.HasValue() ? t : null; },
                            getValue: () => (int)p.GetValue(Config),
                            setValue: value => p.SetValue(Config, value)
                        );
                    }
                    else if (p.PropertyType == typeof(float))
                    {
                        configMenu.AddNumberOption(
                            mod: ModManifest,
                            name: () => { var t = Helper.Translation.Get(p.Name); return t.HasValue() ? t : AddSpaces(p.Name); },
                            tooltip: () => { var t = Helper.Translation.Get(p.Name + ".Desc"); return t.HasValue() ? t : null; },
                            getValue: () => (float)p.GetValue(Config),
                            setValue: value => p.SetValue(Config, value)
                        );
                    }
                    else if (p.PropertyType == typeof(double))
                    {
                        configMenu.AddTextOption(
                            mod: ModManifest,
                            name: () => { var t = Helper.Translation.Get(p.Name); return t.HasValue() ? t : AddSpaces(p.Name); },
                            tooltip: () => { var t = Helper.Translation.Get(p.Name + ".Desc"); return t.HasValue() ? t : null; },
                            getValue: () => p.GetValue(Config).ToString(),
                            setValue: value => { if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var d)) { p.SetValue(Config, d); } }
                        );
                    }
                    else if (p.PropertyType == typeof(string))
                    {
                        configMenu.AddTextOption(
                            mod: ModManifest,
                            name: () => { var t = Helper.Translation.Get(p.Name); return t.HasValue() ? t : AddSpaces(p.Name); },
                            tooltip: () => { var t = Helper.Translation.Get(p.Name + ".Desc"); return t.HasValue() ? t : null; },
                            getValue: () => (string)p.GetValue(Config),
                            setValue: value => p.SetValue(Config, value)
                        );
                    }
                    else if (p.PropertyType == typeof(KeybindList))
                    {
                        configMenu.AddKeybindList(
                            mod: ModManifest,
                            name: () => { var t = Helper.Translation.Get(p.Name); return t.HasValue() ? t : AddSpaces(p.Name); },
                            tooltip: () => { var t = Helper.Translation.Get(p.Name + ".Desc"); return t.HasValue() ? t : null; },
                            getValue: () => (KeybindList)p.GetValue(Config),
                            setValue: value => p.SetValue(Config, value)
                        );
                    }
                    else if (p.PropertyType == typeof(SButton))
                    {
                        configMenu.AddKeybind(
                            mod: ModManifest,
                            name: () => { var t = Helper.Translation.Get(p.Name); return t.HasValue() ? t : AddSpaces(p.Name); },
                            tooltip: () => { var t = Helper.Translation.Get(p.Name + ".Desc"); return t.HasValue() ? t : null; },
                            getValue: () => (SButton)p.GetValue(Config),
                            setValue: value => p.SetValue(Config, value)
                        );
                    }
                    else if (p.PropertyType == typeof(Color) && configMenuExt is not null)
                    {
                        configMenuExt.AddColorOption(
                            mod: ModManifest,
                            name: () => { var t = Helper.Translation.Get(p.Name); return t.HasValue() ? t : AddSpaces(p.Name); },
                            tooltip: () => { var t = Helper.Translation.Get(p.Name + ".Desc"); return t.HasValue() ? t : null; },
                            getValue: () => (Color)p.GetValue(Config),
                            setValue: value => p.SetValue(Config, value)
                        );
                    }
                }
            }
        }

        public static string AddSpaces(string str)
        {
            string newStr = "";
            foreach (var c in str)
            {
                if (c >= 'A' && c <= 'Z' && newStr.Length > 0)
                {
                    newStr += " ";
                }
                newStr += c;
            }
            return newStr;
        }
    }
}