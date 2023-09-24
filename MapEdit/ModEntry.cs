using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using System.Collections.Generic;
using xTile;
using xTile.Layers;
using xTile.Tiles;

namespace MapEdit
{
    public partial class ModEntry : Mod
    {
        public static ModEntry context;

        public static ModConfig Config;
        public static IModHelper SHelper;
        public static IMonitor SMonitor;

        public static int modNumber = 189017541;

        public static Texture2D existsTexture;
        public static Texture2D activeTexture;
        public static Texture2D copiedTexture;

        public static List<string> cleanMaps = new List<string>();
        public static MapCollectionData mapCollectionData = new MapCollectionData();

        public static PerScreen<bool> modActive = new PerScreen<bool>();
        public static PerScreen<Vector2> copiedTileLoc = new PerScreen<Vector2>();
        public static PerScreen<Vector2> pastedTileLoc = new PerScreen<Vector2>();
        public static PerScreen<Dictionary<string, Tile>> currentTileDict = new PerScreen<Dictionary<string, Tile>>();
        public static PerScreen<string> currentLayer = new PerScreen<string>();
        private static PerScreen<TileSelectMenu> tileMenu = new();

        public override void Entry(IModHelper helper)
        {
            context = this;
            Config = Helper.ReadConfig<ModConfig>();

            SHelper = Helper;
            SMonitor = Monitor;

            currentTileDict.Value = new Dictionary<string, Tile>();

            CreateTextures();

            SHelper.Events.Display.RenderedWorld += Display_RenderedWorld;
            SHelper.Events.Display.RenderedHud += Display_RenderedHud;
            SHelper.Events.Input.ButtonsChanged += Input_ButtonsChanged;
            SHelper.Events.Input.ButtonPressed += Input_ButtonPressed;
            SHelper.Events.Input.MouseWheelScrolled += Input_MouseWheelScrolled;
            SHelper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            SHelper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;
            SHelper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            SHelper.Events.GameLoop.ReturnedToTitle += GameLoop_ReturnedToTitle;
            SHelper.Events.Player.Warped += Player_Warped;

            SHelper.Events.Content.AssetRequested += Content_AssetRequested;

            var harmony = new Harmony(ModManifest.UniqueID);

            harmony.PatchAll();
        }

    }
}
