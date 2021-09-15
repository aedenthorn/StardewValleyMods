using Microsoft.Xna.Framework;
using StardewModdingAPI;
using System;
using System.Collections.Generic;

namespace CustomWarpPoints
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod, IAssetLoader
    {
        public static ModEntry context;
        public static ModConfig config;
        public Dictionary<string, string> WarpDict { get; set; }

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            context = this;
            config = Helper.ReadConfig<ModConfig>();

            if (!config.EnableMod)
                return;

            helper.Events.Player.Warped += Player_Warped;

        }

        private void Player_Warped(object sender, StardewModdingAPI.Events.WarpedEventArgs e)
        {
            if (!config.EnableMod)
                return;

            WarpDict = new Dictionary<string, string>(config.WarpDict);

            Monitor.Log($"{WarpDict.Count} warp entries in mod");

            foreach (var kvp in Helper.Content.Load<Dictionary<string, string>>(config.AdditionalWarpDictFilePath, ContentSource.GameContent) ?? new Dictionary<string, string>())
            {
                try
                {
                    WarpDict.Add(kvp.Key, kvp.Value);
                }
                catch 
                {
                    Monitor.Log($"External warp entry {kvp.Key}: {kvp.Value} syntax error", LogLevel.Error);
                }
            }
            Monitor.Log($"{WarpDict.Count} total warp entries");

            Vector2 oldDest = e.Player.getTileLocation();
            string newDestString = null;
            if (WarpDict.ContainsKey($"{e.OldLocation.name},{e.NewLocation.name},{oldDest.X},{oldDest.Y}"))
                newDestString = WarpDict[$"{e.OldLocation.name},{e.NewLocation.name},{oldDest.X},{oldDest.Y}"];
            else if (WarpDict.ContainsKey($"{e.NewLocation.name},{oldDest.X},{oldDest.Y}"))
                newDestString = WarpDict[$"{e.NewLocation.name},{oldDest.X},{oldDest.Y}"];
            else if (WarpDict.ContainsKey(e.OldLocation.name + "," + e.NewLocation.name))
                newDestString = WarpDict[e.OldLocation.name + "," + e.NewLocation.name];
            if (newDestString == null)
                return;
            try
            {
                var parts = newDestString.Split(',');
                Vector2 newDest = new Vector2(int.Parse(parts[0]), int.Parse(parts[1]));
                Monitor.Log($"Moving warp destination for {e.Player.name} from {oldDest} to {newDest} on warp from {e.OldLocation.name} to {e.NewLocation.name}");

                e.Player.position.Value = newDest * 64;
            }
            catch(Exception ex)
            {
                Monitor.Log($"Error setting destination:\n\n{ex}");
            }
        }

        public bool CanLoad<T>(IAssetInfo asset)
        {
            return asset.AssetNameEquals(config.AdditionalWarpDictFilePath);
        }

        public T Load<T>(IAssetInfo asset)
        {
            return (T)(object)new Dictionary<string, string>();
        }
    }
}