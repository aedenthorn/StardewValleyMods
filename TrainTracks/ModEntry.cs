using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Characters;
using StardewValley.TerrainFeatures;
using System;

namespace TrainTracks
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod, IAssetLoader
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static readonly string trainKey = "aedenthorn.TrainTracks/Train";
        public static readonly string trackKey = "aedenthorn.TrainTracks/index";
        public static readonly string switchDataKey = "aedenthorn.TrainTracks/switchData";
        public static readonly string speedDataKey = "aedenthorn.TrainTracks/speedData";
        public static readonly string currentSwitchKey = "aedenthorn.TrainTracks/switch";

        public static ModEntry context;
        public static Texture2D trackTexture;
        public static Texture2D frontTexture;

        public static float currentSpeed = 1;
        public static bool placingTracks;
        public static int currentTrackIndex;
        public static int trackLength = 17;
        public static Vector2 lastTile = new Vector2(-1, -1);
        public static Vector2 lastPlacementTile = new Vector2(-1, -1);
        public static string lastLocation = "";
        public static bool canWarp = true;
        public static bool turnedThisTile;

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
            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;
            helper.Events.Display.RenderedWorld += Display_RenderedWorld;
            helper.Events.Input.ButtonPressed += Input_ButtonPressed;


            var harmony = new Harmony(ModManifest.UniqueID);

            harmony.Patch(
               original: AccessTools.Method(typeof(Horse), nameof(Horse.draw), new Type[] { typeof(SpriteBatch)}),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Horse_draw_Postfix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(Flooring), nameof(Flooring.draw), new Type[] { typeof(SpriteBatch), typeof(Vector2) }),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Flooring_draw_Prefix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(Flooring), nameof(Flooring.performToolAction)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Flooring_performToolAction_Prefix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(Horse), nameof(Horse.checkAction)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Horse_checkAction_Prefix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(Horse), nameof(Horse.dismount)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Horse_dismount_Prefix))
            );

        }
        public override object GetApi()
        {
            return new TrainTracksApi();
        }

        private void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            try
            {
                trackTexture = Game1.content.Load<Texture2D>("Mods/aedenthorn.TrainTracks/Tracks");
            }
            catch
            {
                trackTexture = Helper.Content.Load<Texture2D>("assets/tracks.png");
            }

            try
            {
                frontTexture = Game1.content.Load<Texture2D>("Mods/aedenthorn.TrainTracks/TrainFront");
            }
            catch
            {
                frontTexture = Helper.Content.Load<Texture2D>("assets/horse_front.png");
            }
        }

        public void Display_RenderedWorld(object sender, StardewModdingAPI.Events.RenderedWorldEventArgs e)
        {
            if (!Config.EnableMod || !placingTracks || Game1.activeClickableMenu != null)
                return;

            if (placingTracks)
            {
                float layerDepth = (Game1.currentCursorTile.Y * (16 * Game1.pixelZoom) + 16 * Game1.pixelZoom) / 10000f;

                e.SpriteBatch.Draw(trackTexture, Game1.GlobalToLocal(Game1.viewport, Game1.currentCursorTile * 64), new Rectangle(currentTrackIndex * 16, 0, 16, 16), Color.White, 0, Vector2.Zero, 4, SpriteEffects.None, layerDepth);
            }
        }


        private void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            if (!Config.EnableMod || !Context.IsPlayerFree)
                return;
            if (e.Button == Config.TogglePlacingKey)
            {
                placingTracks = !placingTracks;
                Monitor.Log($"Placing tracks: {placingTracks}");
                Helper.Input.Suppress(e.Button);
                return;
            }
            if (placingTracks)
            {
                if (e.Button == Config.NextTrackKey)
                {
                    currentTrackIndex = (currentTrackIndex + 1) % trackLength;
                    if (Config.SwitchSound.Length > 0)
                        Game1.player.currentLocation.playSound(Config.SwitchSound);
                    Helper.Input.Suppress(e.Button);
                    return;
                }
                if (e.Button == Config.PrevTrackKey)
                {
                    currentTrackIndex = currentTrackIndex == 0 ? trackLength - 1 : currentTrackIndex - 1;
                    if (Config.SwitchSound.Length > 0)
                        Game1.player.currentLocation.playSound(Config.SwitchSound);
                    Helper.Input.Suppress(e.Button);
                    return;
                }
                if (e.Button == Config.PlaceTrackKey)
                {
                    lastPlacementTile = new Vector2(-1, -1);
                    Helper.Input.Suppress(e.Button);
                    return;
                }
                if (e.Button == Config.RemoveTrackKey)
                {
                    if(Game1.player.currentLocation.terrainFeatures.TryGetValue(Game1.currentCursorTile, out TerrainFeature oldFeature) && oldFeature is Flooring && oldFeature.modData.ContainsKey(trackKey))
                    {
                        if ((oldFeature as Flooring).whichFloor.Value != -42)
                        {
                            Game1.player.currentLocation.terrainFeatures[Game1.currentCursorTile].modData.Remove(trackKey);
                        }
                        else
                        {
                            Game1.player.currentLocation.terrainFeatures.Remove(Game1.currentCursorTile);
                        }
                        if(Config.PlaceTrackSound.Length > 0)
                            Game1.player.currentLocation.playSound(Config.RemoveSound);
                    }
                    Helper.Input.Suppress(e.Button);
                    return;
                }
            }
            if (e.Button == Config.SpeedUpKey && Game1.player.mount?.modData.ContainsKey(trainKey) == true)
            {
                if(currentSpeed < Config.MaxSpeed)
                {
                    if (Config.SpeedSound.Length > 0)
                        Game1.player.currentLocation.playSound(Config.SpeedSound);
                    currentSpeed = Math.Min(Config.MaxSpeed, currentSpeed + 0.5f);
                }
                Helper.Input.Suppress(e.Button);
                return;
            }
            if (e.Button == Config.SlowDownKey && Game1.player.mount?.modData.ContainsKey(trainKey) == true)
            {
                if(currentSpeed > 0)
                {
                    currentSpeed = Math.Max(0, currentSpeed - 0.5f);
                    if (Config.SpeedSound.Length > 0)
                        Game1.player.currentLocation.playSound(Config.SpeedSound);
                }
                Helper.Input.Suppress(e.Button);
                return;
            }
            if (e.Button == Config.ReverseKey && Game1.player.mount?.modData.ContainsKey(trainKey) == true)
            {
                Game1.player.FacingDirection = (Game1.player.FacingDirection + 2) % 4;
                Game1.player.mount.FacingDirection = Game1.player.FacingDirection;
                Monitor.Log($"Reversing");
                if (Config.ReverseSound.Length > 0)
                    Game1.player.currentLocation.playSound(Config.ReverseSound);

                Helper.Input.Suppress(e.Button);
                return;
            }
            if (e.Button == Config.PlaceTrainKey && Game1.player.mount == null && Game1.player.currentLocation.terrainFeatures.TryGetValue(Game1.currentCursorTile, out TerrainFeature feature) && feature is Flooring && feature.modData.TryGetValue(trackKey, out string indexString) && int.TryParse(indexString, out int index))
            {
                currentSpeed = Config.DefaultSpeed;

                if (Config.PlaceTrainSound.Length > 0)
                    Game1.player.currentLocation.playSound(Config.PlaceTrainSound);
                Monitor.Log($"Spawning cart on track {index}");
                for (int i = Game1.currentLocation.characters.Count - 1; i >= 0; i--)
                {
                    if (Game1.currentLocation.characters[i] is Horse && Game1.currentLocation.characters[i].modData.ContainsKey(trainKey))
                    {
                        Game1.currentLocation.characters.RemoveAt(i);
                    }
                }
                Horse horse = new Horse(Guid.NewGuid(), (int)feature.currentTileLocation.X, (int)feature.currentTileLocation.Y) { Name = "Mods/TrainTracks/Train", modData = new ModDataDictionary() { { trainKey, "true" } } };
                horse.Sprite = new AnimatedSprite("Mods\\aedenthorn.TrainTracks\\TrainInternal", 0, 32, 32);
                int facing;
                switch (index)
                {
                    case 0:
                    case 2:
                    case 8:
                    case 9:
                        facing = 2;
                        break;
                    default:
                        facing = 1;
                        break;
                }
                horse.FacingDirection  = facing;
                Game1.currentLocation.characters.Add(horse);
                horse.faceDirection(facing);
                horse.Position -= GetOffset(facing);
                Helper.Input.Suppress(e.Button);
                return;
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
                name: () => "Mod Enabled?",
                getValue: () => Config.EnableMod,
                setValue: value => Config.EnableMod = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Default Speed",
                getValue: () => Config.DefaultSpeed,
                setValue: value => Config.DefaultSpeed = value,
                min: 0,
                max: 16
            );
            configMenu.AddSectionTitle(
                mod: ModManifest,
                text: () => "KeyBinds"
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Prev Track",
                getValue: () => Config.PrevTrackKey,
                setValue: value => Config.PrevTrackKey = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Next Track",
                getValue: () => Config.NextTrackKey,
                setValue: value => Config.NextTrackKey = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Place Track",
                getValue: () => Config.PlaceTrackKey,
                setValue: value => Config.PlaceTrackKey = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Remove Track",
                getValue: () => Config.RemoveTrackKey,
                setValue: value => Config.RemoveTrackKey = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Toggle Placing",
                getValue: () => Config.TogglePlacingKey,
                setValue: value => Config.TogglePlacingKey = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Place Train",
                getValue: () => Config.PlaceTrainKey,
                setValue: value => Config.PlaceTrainKey = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Speed Up",
                getValue: () => Config.SpeedUpKey,
                setValue: value => Config.SpeedUpKey = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Slow Down",
                getValue: () => Config.SlowDownKey,
                setValue: value => Config.SlowDownKey = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Turn Right",
                getValue: () => Config.TurnRightKey,
                setValue: value => Config.TurnRightKey = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Turn Left",
                getValue: () => Config.TurnLeftKey,
                setValue: value => Config.TurnLeftKey = value
            );
            configMenu.AddSectionTitle(
                mod: ModManifest,
                text: () => "Sounds"
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Place Track",
                getValue: () => Config.PlaceTrackSound,
                setValue: value => Config.PlaceTrackSound = value
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Remove Track",
                getValue: () => Config.RemoveSound,
                setValue: value => Config.RemoveSound = value
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Switch Track",
                getValue: () => Config.SwitchSound,
                setValue: value => Config.SwitchSound = value
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Place Train",
                getValue: () => Config.PlaceTrainSound,
                setValue: value => Config.PlaceTrainSound = value
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Reverse",
                getValue: () => Config.PlaceTrackSound,
                setValue: value => Config.PlaceTrackSound = value
            );
        }

        private void GameLoop_UpdateTicked(object sender, StardewModdingAPI.Events.UpdateTickedEventArgs e)
        {
            if (placingTracks && Game1.activeClickableMenu != null)
                placingTracks = false;
            if (!Config.EnableMod)
                return;
            if (placingTracks && (Helper.Input.IsDown(Config.PlaceTrackKey) || Helper.Input.IsSuppressed(Config.PlaceTrackKey)) && Game1.currentCursorTile != lastPlacementTile)
            {
                lastPlacementTile = Game1.currentCursorTile;
                TryPlaceTrack(Game1.player.currentLocation, Game1.currentCursorTile, currentTrackIndex);
            }
            if (Game1.player.mount != null && Game1.player.mount.modData.ContainsKey(trainKey) && !Game1.isWarping)
                MoveOnTrack();
        }

        /// <summary>Get whether this instance can load the initial version of the given asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public bool CanLoad<T>(IAssetInfo asset)
        {
            if (!Config.EnableMod)
                return false;

            return asset.AssetNameEquals("Mods/aedenthorn.TrainTracks/TrainInternal") || asset.AssetNameEquals("Mods/aedenthorn.TrainTracks/TrainFrontInternal");
        }

        /// <summary>Load a matched asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public T Load<T>(IAssetInfo asset)
        {
            Texture2D tex;
            if (asset.AssetNameEquals("Mods/aedenthorn.TrainTracks/TrainInternal"))
            {
                try
                {
                    tex = Game1.content.Load<Texture2D>("Mods/aedenthorn.TrainTracks/Train");
                }
                catch
                {
                    tex = Helper.Content.Load<Texture2D>("assets/horse.png");
                }
            }
            else
            {
                try
                {
                    tex = Game1.content.Load<Texture2D>("Mods/aedenthorn.TrainTracks/TrainFront");
                }
                catch
                {
                    tex = Helper.Content.Load<Texture2D>("assets/horse_front.png");
                }
            }
            return (T)(object)tex;
        }
    }
}
