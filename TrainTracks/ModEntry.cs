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

        private static readonly string trainKey = "aedenthorn.TrainTracks/Train";
        private static readonly string trackKey = "aedenthorn.TrainTracks/index";

        public static ModEntry context;
        private static Texture2D trackTexture;

        private static float currentSpeed = 1;
        private static bool placingTracks;
        private static int currentTrackIndex;
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
            helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;
            helper.Events.Display.RenderedWorld += Display_RenderedWorld;
            helper.Events.Input.ButtonPressed += Input_ButtonPressed;


            var harmony = new Harmony(ModManifest.UniqueID);

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
                    currentTrackIndex = (currentTrackIndex + 1) % 12;
                    if (Config.SwitchSound.Length > 0)
                        Game1.player.currentLocation.playSound(Config.SwitchSound);
                    Helper.Input.Suppress(e.Button);
                    return;
                }
                if (e.Button == Config.PrevTrackKey)
                {
                    currentTrackIndex = currentTrackIndex == 0 ? 11 : currentTrackIndex - 1;
                    if (Config.SwitchSound.Length > 0)
                        Game1.player.currentLocation.playSound(Config.SwitchSound);
                    Helper.Input.Suppress(e.Button);
                    return;
                }
                if (e.Button == Config.PlaceTrackKey)
                {
                    if(!Game1.player.currentLocation.terrainFeatures.TryGetValue(Game1.currentCursorTile, out TerrainFeature oldFeature) || (oldFeature is Flooring && oldFeature.modData.ContainsKey(trackKey)))
                    {
                        Flooring f = new Flooring(-42) { modData = new ModDataDictionary() { { trackKey, currentTrackIndex + "" } } };
                        Game1.player.currentLocation.terrainFeatures[Game1.currentCursorTile] = f;
                        if(Config.PlaceTrackSound.Length > 0)
                            Game1.player.currentLocation.playSound(Config.PlaceTrackSound);
                    }
                    Helper.Input.Suppress(e.Button);
                    return;
                }
                if (e.Button == Config.RemoveTrackKey)
                {
                    if(Game1.player.currentLocation.terrainFeatures.TryGetValue(Game1.currentCursorTile, out TerrainFeature oldFeature) && oldFeature is Flooring && oldFeature.modData.ContainsKey(trackKey))
                    {
                        Game1.player.currentLocation.terrainFeatures.Remove(Game1.currentCursorTile);
                        if(Config.PlaceTrackSound.Length > 0)
                            Game1.player.currentLocation.playSound(Config.RemoveSound);
                    }
                    Helper.Input.Suppress(e.Button);
                    return;
                }
            }
            if (e.Button == Config.SpeedUpKey && Game1.player.mount?.modData.ContainsKey(trainKey) == true)
            {
                if (currentSpeed < 16)
                {
                    if (Config.SpeedSound.Length > 0)
                        Game1.player.currentLocation.playSound(Config.SpeedSound);
                    currentSpeed = Math.Min(16, currentSpeed + 1);
                    Monitor.Log($"Current speed {currentSpeed}");
                }
                Helper.Input.Suppress(e.Button);
                return;
            }
            if (e.Button == Config.SlowDownKey && Game1.player.mount?.modData.ContainsKey(trainKey) == true)
            {
                if(currentSpeed > 0)
                {
                    currentSpeed = Math.Max(0, currentSpeed - 1);
                    Monitor.Log($"Current speed {currentSpeed}");
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
            if (e.Button == SButton.Enter && Game1.player.mount == null && Game1.player.currentLocation.terrainFeatures.TryGetValue(Game1.currentCursorTile, out TerrainFeature feature) && feature is Flooring && feature.modData.TryGetValue(trackKey, out string indexString) && int.TryParse(indexString, out int index))
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
            try
            {
                trackTexture = Game1.content.Load<Texture2D>("Mods/aedenthorn.TrainTracks/Tracks");
            }
            catch
            {
                trackTexture = Helper.Content.Load<Texture2D>("assets/tracks.png");
            }

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
                name: () => "Speed Up",
                getValue: () => Config.SlowDownKey,
                setValue: value => Config.SlowDownKey = value
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
        private static Vector2 lastTilePos;

        private void GameLoop_UpdateTicked(object sender, StardewModdingAPI.Events.UpdateTickedEventArgs e)
        {
            if (placingTracks && Game1.activeClickableMenu != null)
                placingTracks = false;
            if (!Config.EnableMod || Game1.player.mount == null || !Game1.player.mount.modData.ContainsKey(trainKey))
                return;
            Game1.player.canMove = false;

            Vector2 offset = GetOffset(Game1.player.FacingDirection);
            var tilePos = new Vector2((int)((Game1.player.Position.X + offset.X) / 64), (int)((Game1.player.Position.Y + offset.Y) / 64));
            if (Game1.player.currentLocation.terrainFeatures.TryGetValue(tilePos, out TerrainFeature feature) && feature is Flooring && feature.modData.TryGetValue(trackKey, out string indexString) && int.TryParse(indexString, out int index))
            {
                int facing = Game1.player.FacingDirection;
                Vector2 pos = new Vector2((float)Math.Round(Game1.player.Position.X), (float)Math.Round(Game1.player.Position.Y));
                Vector2 tPos = feature.currentTileLocation * 64;
                int dir = -1;

                switch(index)
                {
                    case 0:
                    case 2:
                        if (facing == 0)
                        {
                            pos.X = tPos.X - offset.X;
                            dir = 0;
                        }
                        else if (facing == 1)
                        {
                            pos.X = tPos.X - GetOffset(2).X;
                            facing = 2;
                            dir = 2;
                        }
                        else if (facing == 2)
                        {
                            pos.X = tPos.X - offset.X;
                            dir = 2;
                        }
                        else if (facing == 3)
                        {
                            pos.X = tPos.X - GetOffset(0).X;
                            facing = 0;
                            dir = 0;
                        }
                        break;
                    case 1:
                    case 3:
                        if (facing == 0)
                        {
                            pos.Y = tPos.Y - GetOffset(1).Y;
                            facing = 1;
                            dir = 1;
                        }
                        else if (facing == 1)
                        {
                            pos.Y = tPos.Y - offset.Y;
                            dir = 1;
                        }
                        else if (facing == 2)
                        {
                            pos.Y = tPos.Y - GetOffset(3).Y;
                            facing = 3;
                            dir = 3;
                        }
                        else if (facing == 3)
                        {
                            pos.Y = tPos.Y - offset.Y;
                            dir = 3;
                        }
                        break;
                    case 4:
                        if (facing == 0)
                        {
                            if (pos.Y > tPos.Y - GetOffset(1).Y)
                            {
                                pos.X = tPos.X - offset.X;
                                dir = 0;
                            }
                            else
                            {
                                Monitor.Log($"turning right, pos {pos}, tPos {tPos}");
                                pos.Y = tPos.Y - GetOffset(1).Y;
                                facing = 1;
                                dir = 1;
                            }
                        }
                        else if (facing == 1)
                        {
                            pos.Y = tPos.Y - offset.Y;
                            dir = 1;
                        }
                        else if (facing == 2)
                        {
                            pos.X = tPos.X - offset.X;
                            dir = 2;
                        }
                        else if (facing == 3)
                        {
                            if (pos.X > tPos.X)
                            {
                                pos.Y = tPos.Y - offset.Y;
                                dir = 3;
                            }
                            else
                            {
                                Monitor.Log($"turning down, pos {pos}, tPos {tPos}");

                                pos.X = tPos.X + - GetOffset(2).X;
                                facing = 2;
                                dir = 2;
                            }
                        }
                        break;
                    case 5:
                        if (facing == 0)
                        {
                            if (pos.Y > tPos.Y - GetOffset(3).Y)
                            {
                                pos.X = tPos.X - offset.X;
                                dir = 0;
                            }
                            else
                            {
                                Monitor.Log($"turning left, pos {pos}, tPos {tPos}");
                                pos.Y = tPos.Y - GetOffset(3).Y;
                                facing = 3;
                                dir = 3;
                            }
                        }
                        else if (facing == 1)
                        {
                            Monitor.Log($"turning down, pos {pos}, tPos {tPos}");
                            pos.X = tPos.X - GetOffset(2).X;
                            facing = 2;
                            dir = 2;
                        }
                        else if (facing == 2)
                        {
                            pos.X = tPos.X - offset.X;
                            dir = 2;
                        }
                        else if (facing == 3)
                        {
                            pos.Y = tPos.Y - offset.Y;
                            dir = 3;
                        }
                        break;
                    case 6:
                        if (facing == 0)
                        {
                            pos.X = tPos.X - offset.X;
                            dir = 0;
                        }
                        else if (facing == 1)
                        {
                            Monitor.Log($"turning up, pos {pos}, tPos {tPos}");
                            pos.X = tPos.X - GetOffset(0).X;
                            facing = 0;
                            dir = 0;
                        }
                        else if (facing == 2)
                        {
                            Monitor.Log($"turning left, pos {pos}, tPos {tPos}");
                            pos.Y = tPos.Y - GetOffset(3).Y;
                            facing = 3;
                            dir = 3;
                        }
                        else if (facing == 3)
                        {
                            pos.Y = tPos.Y - offset.Y;
                            dir = 3;
                        }
                        break;
                    case 7:
                        if (facing == 0)
                        {
                            pos.X = tPos.X - offset.X;
                            dir = 0;
                        }
                        else if (facing == 1)
                        {
                            pos.Y = tPos.Y - offset.Y;
                            dir = 1;
                        }
                        else if (facing == 2)
                        {
                            Monitor.Log($"turning right, pos {pos}, tPos {tPos}");
                            pos.Y = tPos.Y - GetOffset(1).Y;
                            facing = 1;
                            dir = 1;
                        }
                        else if (facing == 3)
                        {
                            if (pos.X > tPos.X)
                            {
                                pos.Y = tPos.Y - offset.Y;
                                dir = 3;
                            }
                            else
                            {
                                Monitor.Log($"turning up, pos {pos}, tPos {tPos}");
                                pos.X = tPos.X - GetOffset(0).X;
                                facing = 0;
                                dir = 0;
                            }
                        }
                        break;
                    case 8:
                        if (facing == 0)
                        {
                            pos.X = tPos.X - offset.X;
                            if (pos.Y > tPos.Y + 4)
                            {
                                dir = 0;
                            }
                        }
                        else if (facing == 1)
                        {
                            pos.X = tPos.X - GetOffset(2).X;
                            facing = 2;
                            dir = 2;
                        }
                        else if (facing == 2)
                        {
                            pos.X = tPos.X - offset.X;
                            dir = 2;
                        }
                        else if (facing == 3)
                        {
                            pos.X = tPos.X - GetOffset(0).X;
                            facing = 0;
                            if (pos.Y > tPos.Y + 4)
                            {
                                dir = 0;
                            }
                        }
                        break;
                    case 9:
                        if (facing == 0)
                        {
                            pos.X = tPos.X - offset.X;
                            dir = 0;
                        }
                        else if (facing == 1)
                        {
                            pos.X = tPos.X - GetOffset(2).X;
                            facing = 2;
                            if (pos.Y > tPos.Y - 4)
                            {
                                dir = 2;
                            }
                        }
                        else if (facing == 2)
                        {
                            pos.X = tPos.X - offset.X;
                            if (pos.Y < tPos.Y + 16)
                            {
                                dir = 2;
                            }
                        }
                        else if (facing == 3)
                        {
                            pos.X = tPos.X - GetOffset(0).X;
                            facing = 0;
                            dir = 0;
                        }
                        break;
                    case 10:
                        if (facing == 0)
                        {
                            pos.Y = tPos.Y - GetOffset(1).Y;
                            facing = 1;
                            if (pos.X < tPos.X - 4)
                            {
                                dir = 1;
                            }
                        }
                        else if (facing == 1)
                        {
                            pos.Y = tPos.Y - offset.Y;
                            if (pos.X < tPos.X - 4)
                            {
                                dir = 1;
                            }
                        }
                        else if (facing == 2)
                        {
                            pos.Y = tPos.Y - GetOffset(3).Y;
                            facing = 3;
                            dir = 3;
                        }
                        else if (facing == 3)
                        {
                            pos.Y = tPos.Y - offset.Y;
                            dir = 3;
                        }
                        break;
                    case 11:
                        if (facing == 0)
                        {
                            pos.Y = tPos.Y - GetOffset(1).Y;
                            facing = 1;
                            dir = 1;
                        }
                        else if (facing == 1)
                        {
                            pos.Y = tPos.Y - offset.Y;
                            dir = 1;
                        }
                        else if (facing == 2)
                        {
                            pos.Y = tPos.Y - GetOffset(3).Y;
                            facing = 3;
                            if (pos.X < tPos.X + 4)
                            {
                                dir = 3;
                            }
                        }
                        else if (facing == 3)
                        {
                            pos.Y = tPos.Y - offset.Y;
                            if (pos.X > tPos.X - 16)
                            {
                                dir = 3;
                            }
                        }
                        break;

                }
                Vector2 move = Vector2.Zero;
                switch (dir)
                {
                    case 0:
                        move = new Vector2(0, -1);
                        break;
                    case 1:
                        move = new Vector2(1, 0);
                        break;
                    case 2:
                        move = new Vector2(0, 1);
                        break;
                    case 3:
                        move = new Vector2(-1, 0);
                        break;
                }
                Game1.player.Position = pos + move * currentSpeed;
                Game1.player.FacingDirection = facing;
            }
            if (false && tilePos != lastTilePos)
            {
                if(feature is Flooring && (feature as Flooring).whichFloor.Value < 12)
                {
                    Monitor.Log($"floor {(feature as Flooring).whichFloor.Value}, pos {Game1.player.Position}, old tile {tilePos}, new tile { new Vector2((int)((Game1.player.Position.X) / 64), (int)((Game1.player.Position.Y) / 64))}, offset {Game1.player.Position - new Vector2((int)((Game1.player.Position.X) / 64) * 64, (int)((Game1.player.Position.Y) / 64) * 64)}, floor tile {feature.currentTileLocation}, pos {feature.currentTileLocation * 64}");
                }
                else
                {
                    Monitor.Log($"pos {Game1.player.Position}, old tile {lastTilePos}, new tile { new Vector2((int)((Game1.player.Position.X) / 64), (int)((Game1.player.Position.Y) / 64))}, offset {Game1.player.Position - new Vector2((int)((Game1.player.Position.X) / 64) * 64, (int)((Game1.player.Position.Y) / 64) * 64)}");
                }
                lastTilePos = tilePos;
            }
        }

        private static Vector2 GetOffset(int facingDirection)
        {
            var offset = Vector2.Zero;
            switch (facingDirection)
            {
                case 0:
                    offset = new Vector2(16, 16);
                    break;
                case 1:
                    offset = new Vector2(16, -16);
                    break;
                case 2:
                    offset = new Vector2(16, 0);
                    break;
                case 3:
                    offset = new Vector2(16, -16);
                    break;
            }
            return offset;
        }


        /// <summary>Get whether this instance can load the initial version of the given asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public bool CanLoad<T>(IAssetInfo asset)
        {
            if (!Config.EnableMod)
                return false;

            return asset.AssetNameEquals("Mods/aedenthorn.TrainTracks/TrainInternal");
        }

        /// <summary>Load a matched asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public T Load<T>(IAssetInfo asset)
        {
            Texture2D tex;
            try
            {
                tex = Game1.content.Load<Texture2D>("Mods/aedenthorn.TrainTracks/Train");
            }
            catch
            {
                tex = Helper.Content.Load<Texture2D>("assets/horse.png");
            }

            return (T)(object)tex;
        }
    }
}
