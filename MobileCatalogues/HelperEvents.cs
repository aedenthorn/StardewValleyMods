using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System;
using System.IO;
using System.Linq;

namespace MobileCatalogues
{
    public class HelperEvents
    {
        private static IMonitor Monitor;
        private static IModHelper Helper;
        private static ModConfig Config;
        private static IMobilePhoneApi api;

        // call this method from your Entry class
        public static void Initialize(IModHelper helper, IMonitor monitor, ModConfig config)
        {
            Monitor = monitor;
            Helper = helper;
            Config = config;
        }
        public static void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {
            ModEntry.api = Helper.ModRegistry.GetApi<IMobilePhoneApi>("aedenthorn.MobilePhone");
            if (ModEntry.api != null)
            {
                api = ModEntry.api;
                Texture2D appIcon;
                bool success;

                appIcon = Helper.ModContent.Load<Texture2D>(Path.Combine("assets", "app_icon.png"));
                success = ModEntry.api.AddApp(Helper.ModRegistry.ModID + "Catalogues", Helper.Translation.Get("catalogues"), CataloguesApp.OpenCatalogueApp, appIcon);
                Monitor.Log($"loaded catalogues app successfully: {success}", LogLevel.Debug);

                Visuals.MakeTextures();
            }
        }
        public static void GameLoop_SaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            var shops = DataLoader.Shops(Game1.content);
            var keys = shops.Keys;
            if (Config.AllCatalogues.Count != keys.Count)
            {
                Config.AllCatalogues.Clear();
                Config.AllCatalogues.AddRange(keys);
                Helper.WriteConfig(Config);
            }
            CataloguesApp.catalogueList = new();
            CataloguesApp.catalogueList.AddRange(keys.Where(k => (!Config.ShownCatalogues.Any() || Config.ShownCatalogues.Contains(k)) && (!Config.HiddenCatalogues.Any() || !Config.HiddenCatalogues.Contains(k))));
        }

        public static void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (api.IsCallingNPC() || api.GetRunningApp() != Helper.ModRegistry.ModID)
                return;

            if (e.Button == SButton.MouseLeft)
            {
                Point mousePos = Game1.getMousePosition();
                if (!api.GetScreenRectangle().Contains(mousePos))
                {
                    return;
                }

                Helper.Input.Suppress(SButton.MouseLeft);
                Vector2 screenPos = api.GetScreenPosition();
                Vector2 screenSize = api.GetScreenSize();
                if (!CataloguesApp.opening && new Rectangle((int)(screenPos.X + screenSize.X - Config.AppHeaderHeight), (int)(screenPos.Y), (int)Config.AppHeaderHeight, (int)Config.AppHeaderHeight).Contains(mousePos))
                {
                    Monitor.Log($"Closing app");
                    CataloguesApp.CloseApp();
                    return;
                }
                CataloguesApp.opening = false;
                Visuals.clicking = true;
                Visuals.lastMousePosition = mousePos;
            }
        }

        public static void Input_MouseWheelScrolled(object sender, MouseWheelScrolledEventArgs e)
        {

            Visuals.offsetY += Math.Sign(e.Delta) * (Config.AppRowHeight + Config.MarginY);
            //Monitor.Log($"offsetY {offsetY} max {screenSize.Y - Config.MarginY + (Config.MarginY + Game1.dialogueFont.LineSpacing * 0.9f) * audio.Length}");
            Visuals.offsetY = Math.Min(0, Math.Max(Visuals.offsetY, (int)(api.GetScreenSize().Y - (Config.AppHeaderHeight + Config.MarginY + (Config.MarginY + Config.AppRowHeight) * CataloguesApp.catalogueList.Count))));
        }
    }
}
