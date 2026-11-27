using Harmony;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;

namespace CustomTextColours
{
    public class ModEntry : Mod 
    {
        public static ModEntry context;

        private static ModConfig Config;
        private static IMonitor SMonitor;
        private static IModHelper SHelper;

        public override void Entry(IModHelper helper)
        {
            context = this;
            Config = Helper.ReadConfig<ModConfig>();
            if (!Config.EnableMod)
                return;

            SMonitor = Monitor;
            SHelper = Helper;

            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;

            var harmony = HarmonyInstance.Create(this.ModManifest.UniqueID);

            harmony.Patch(
                original: AccessTools.Method(typeof(SpriteText), nameof(SpriteText.drawString)),
                prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.drawString_Prefix))
            );

            harmony.Patch(
                original: AccessTools.Method(typeof(SpriteText), nameof(SpriteText.getColorFromIndex)),
                prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.getColorFromIndex_Prefix))
            );

        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            Game1.textColor = Config.DefaultColor;
            Game1.textShadowColor = Config.ShadowColor;
        }

        private static void drawString_Prefix(ref int color)
        {
            if (color == -1)
                color = -99;
        }
        private static bool getColorFromIndex_Prefix(ref Color __result, int index)
        {
            if(index == -1)
                index = -99;
            switch (index)
            {
                case -99:
                    __result = Config.Color0;
                    break;
                case 1:
                    __result = Config.Color1;
                    break;
                case 2:
                    __result = Config.Color2;
                    break;
                case 3:
                    __result = Config.Color3;
                    break;
                case 4:
                    __result = Config.Color4;
                    break;
                case 5:
                    __result = Config.Color5;
                    break;
                case 6:
                    __result = Config.Color6;
                    break;
                case 7:
                    __result = Config.Color7;
                    break;
                case 8:
                    __result = Config.Color8;
                    break;
                default:
                    __result = Config.Color9;
                    break;

            }
            return false;
        }
    }
}
