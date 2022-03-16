using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System.IO;

namespace Email
{
    public partial class ModEntry
    {
        public void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {
            if (!Config.EnableMod)
                return;
            api = Helper.ModRegistry.GetApi<IMobilePhoneApi>("aedenthorn.MobilePhone");
            if (api != null)
            {
                Texture2D appIcon;
                bool success;

                appIcon = Helper.Content.Load<Texture2D>(Path.Combine("assets", "app_icon.png"));
                success = api.AddApp(Helper.ModRegistry.ModID, Helper.Translation.Get("email"), OpenEmailApp, appIcon);
                Monitor.Log($"loaded email app successfully: {success}", LogLevel.Debug);

                MakeTextures();
            }
        }
        public void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
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
                if (!opening && new Rectangle((int)(screenPos.X + screenSize.X - Config.AppHeaderHeight), (int)(screenPos.Y), (int)Config.AppHeaderHeight, (int)Config.AppHeaderHeight).Contains(mousePos))
                {
                    Monitor.Log($"Closing app");
                    CloseApp();
                    return;
                }
                opening = false;
                clicking = true;
                lastMousePosition = mousePos;
            }
        }
    }
}
