using Harmony;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Reflection;

namespace TransparentObjects
{
    public class ModEntry : Mod
    {
        
        public static ModConfig config;
        public static IMonitor SMonitor;


        public override void Entry(IModHelper helper)
        {
            config = Helper.ReadConfig<ModConfig>();
            if (!config.EnableMod)
                return;

            //HarmonyInstance.DEBUG = true;


            SMonitor = Monitor;

            
            ObjectPatches.Initialize(Monitor, helper, config);

            var harmony = HarmonyInstance.Create(ModManifest.UniqueID);

            harmony.Patch(
               original: AccessTools.Method(typeof(StardewValley.Object), nameof(StardewValley.Object.draw), new Type[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) }),
               prefix: new HarmonyMethod(typeof(ObjectPatches), nameof(ObjectPatches.Object_draw_Prefix))
            );

            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            var harmony = HarmonyInstance.Create(ModManifest.UniqueID);

            MethodInfo mi = AccessTools.Method("PyTK.Types.PyDisplayDevice:DrawTile");
            if (mi == null)
            {
                Monitor.Log($"patching PyDisplayDevice");
                harmony.Patch(
                   original: AccessTools.Method("PyTK.Types.PyDisplayDevice:DrawTile"),
                   prefix: new HarmonyMethod(typeof(ObjectPatches), nameof(ObjectPatches.DisplayDevice_DrawTile_Prefix)),
                   postfix: new HarmonyMethod(typeof(ObjectPatches), nameof(ObjectPatches.DisplayDevice_DrawTile_Postfix))
                );
            }
            else
            {
                Monitor.Log($"patching SDisplayDevice");
                harmony.Patch(
                   original: AccessTools.Method("StardewModdingAPI.Framework.Rendering.SDisplayDevice:DrawTile"),
                   prefix: new HarmonyMethod(typeof(ObjectPatches), nameof(ObjectPatches.DisplayDevice_DrawTile_Prefix)),
                   postfix: new HarmonyMethod(typeof(ObjectPatches), nameof(ObjectPatches.DisplayDevice_DrawTile_Postfix))
                );
            }
        }
    }
}
