using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
using xTile.Dimensions;
using xTile.Display;
using xTile.Layers;

namespace ExtraMapLayers
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {

        public static IMonitor PMonitor;
        public static IModHelper PHelper;
        private Harmony harmony;
        public static ModEntry context;
        public static ModConfig config;
        public static Regex numberRx = new Regex("[0-9]$", RegexOptions.Compiled);

        public static PerScreen<int> thisLayerDepth = new PerScreen<int>();
        public static PerScreen<int> lastLayerDepth = new PerScreen<int>();

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            context = this;
            config = Helper.ReadConfig<ModConfig>();

            if (!config.EnableMod)
                return;

            PMonitor = Monitor;
            PHelper = helper;

            harmony = new Harmony(ModManifest.UniqueID);
            Harmony.DEBUG = false;

            harmony.Patch(
               original: AccessTools.Method(typeof(Layer), nameof(Layer.Draw)),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Layer_Draw_Postfix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(Layer), "DrawNormal"),
               transpiler: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Layer_DrawNormal_Transpiler))
            );
            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            Monitor.Log($"device type {Game1.mapDisplayDevice?.GetType()}");

            var pytkapi = Helper.ModRegistry.GetApi("Platonymous.Toolkit");
            if(pytkapi != null)
            {
                Monitor.Log($"patching pytk");
                harmony.Patch(
                   original: AccessTools.Method(pytkapi.GetType().Assembly.GetType("PyTK.Extensions.PyMaps"), "drawLayer", new Type[] { typeof(Layer), typeof(IDisplayDevice), typeof(Rectangle), typeof(int), typeof(Location), typeof(bool) }),
                   prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.PyTK_drawLayer_Prefix))
                );
                harmony.Patch(
                   original: AccessTools.Method(pytkapi.GetType().Assembly.GetType("PyTK.Types.PyDisplayDevice"), "DrawTile"),
                   prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.DrawTile_Prefix))
                );
            }
            harmony.Patch(
               original: AccessTools.Method(Game1.mapDisplayDevice.GetType(), "DrawTile"),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.DrawTile_Prefix))
            );
        }

        public static IEnumerable<CodeInstruction> Layer_DrawNormal_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            PMonitor.Log("Transpiling Layer_DrawNormal");
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (i > 0&& codes[i].opcode == OpCodes.Callvirt && (MethodInfo)codes[i].operand == AccessTools.Method("String:Equals", new Type[]{typeof(string)}) && codes[i - 1].opcode == OpCodes.Ldstr && (string)codes[i - 1].operand == "Front")
                {
                    PMonitor.Log("switching equals to startswith for layer id");
                    codes[i].operand = AccessTools.Method("String:StartsWith", new Type[] { typeof(string) });
                    break;
                }
            }
            return codes.AsEnumerable();
        }

        private static int GetLayerOffset(string layerName)
        {
            return layerName.StartsWith("Front") ? 16 : 0;
        }
        public static void Layer_Draw_Postfix(Layer __instance, IDisplayDevice displayDevice, Rectangle mapViewport, Location displayOffset, bool wrapAround, int pixelZoom)
        {
            if (!config.EnableMod || numberRx.IsMatch(__instance.Id))
                return;

            foreach (Layer layer in __instance.Map.Layers)
            {
                if (layer.Id.StartsWith(__instance.Id) && int.TryParse(layer.Id.Substring(__instance.Id.Length), out int layerIndex))
                {
                    thisLayerDepth.Value = layerIndex;
                    layer.Draw(displayDevice, mapViewport, displayOffset, wrapAround, pixelZoom);
                    thisLayerDepth.Value = 0;
                }
            }
        }
        public static void DrawTile_Prefix(ref float layerDepth)
        {
            if (!config.EnableMod || thisLayerDepth.Value == 0)
                return;
            if(lastLayerDepth.Value != thisLayerDepth.Value)
            {
                lastLayerDepth.Value = thisLayerDepth.Value;
            }
            layerDepth += thisLayerDepth.Value / 10000f;
        }
        public static bool PyTK_drawLayer_Prefix(Layer layer)
        {
            return (!config.EnableMod || !Regex.IsMatch(layer.Id, "[0-9]$"));
        }
    }
}