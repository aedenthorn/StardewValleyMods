using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Network;
using System;
using System.IO;
using System.Linq;
using System.Text;
using Color = Microsoft.Xna.Framework.Color;

namespace OpenFolder
{
    public partial class ModEntry
    {
        private static OptionsButton gmcmButton;

        [HarmonyPatch(typeof(OptionsPage), new Type[] {typeof(int), typeof(int), typeof(int), typeof(int) })]
        [HarmonyPatch(MethodType.Constructor)]
        public class OptionsPage_Patch
        {
            public static void Postfix(OptionsPage __instance)
            {
                if(!Config.ModEnabled) 
                    return;
                if (Config.AddGameFolderButton)
                {
                    __instance.options.Add(new OptionsButton(SHelper.Translation.Get("game-folder"), delegate ()
                    {
                        TryOpenFolder(Path.GetDirectoryName(SHelper.GetType().Assembly.Location));
                    }));
                }
                if (Config.AddModsFolderButton)
                {
                    __instance.options.Add(new OptionsButton(SHelper.Translation.Get("mods-folder"), delegate ()
                    {
                        TryOpenFolder(Path.Combine(Path.GetDirectoryName(SHelper.GetType().Assembly.Location), "Mods"));
                    }));
                }
            }
        }
        /*
        public static void GMCM_AddDefaultLabels_Postfix(object __instance)
        {
            gmcmButton = null;
            if (!Config.ModEnabled || !Config.AddGMCMModsFolderButton)
                return;
            var mc = AccessTools.Field(__instance.GetType(), "ModConfig").GetValue(__instance);
            Vector2 leftPosition = new Vector2(Game1.uiViewport.Width / 2 - 450, Game1.uiViewport.Height - 50 - 36);
            gmcmButton = new OptionsButton(SHelper.Translation.Get("mod-folder"), delegate ()
            {
                var mod = SHelper.ModRegistry.GetAll().First(m =>
                    m.Manifest.UniqueID == ((IManifest)AccessTools.Field(mc.GetType(), "ModManifest")).UniqueID);
                TryOpenFolder((string)AccessTools.Field(mod.GetType(), "DirectoryPath").GetValue(mod));

            });
        }
        public static void GMCM_draw_Prefix(IClickableMenu __instance, SpriteBatch b)
        {
            if (!Config.ModEnabled || !Config.AddGMCMModsFolderButton || gmcmButton is null)
                return;
            gmcmButton.draw(b, Game1.viewport.Width - gmcmButton.bounds.Width, Game1.viewport.Height - gmcmButton.bounds.Height);
        }
        public static bool GMCM_receiveLeftClick_Prefix(IClickableMenu __instance, int x, int y)
        {
            if (!Config.ModEnabled || !Config.AddGMCMModsFolderButton || gmcmButton is null)
                return true;
            var bx = x - (Game1.viewport.Width - gmcmButton.bounds.Width);
            var by = y - (Game1.viewport.Height - gmcmButton.bounds.Height);
            if (gmcmButton.bounds.Contains(bx, by))
            {
                gmcmButton.receiveLeftClick(bx, by);
                return false;
            }
            return true;
        }
        */
    }
}