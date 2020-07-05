using Harmony;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using System;

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

            

            SMonitor = Monitor;

            
            ObjectPatches.Initialize(Monitor, helper, config);

            var harmony = HarmonyInstance.Create(this.ModManifest.UniqueID);
            HarmonyInstance.DEBUG = true;

            harmony.Patch(
               original: AccessTools.Method(typeof(StardewValley.Object), nameof(StardewValley.Object.draw), new Type[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) }),
               prefix: new HarmonyMethod(typeof(ObjectPatches), nameof(ObjectPatches.Object_draw_Prefix))
            );

        }
    }
}
