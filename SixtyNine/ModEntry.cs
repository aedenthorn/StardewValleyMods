using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SixtyNine
{
    public class ModEntry : Mod
    {
        public static ModEntry context;

        public static ModConfig Config;
        public static IJsonAssetsApi JsonAssets;
        private List<string> niceNPCs = new List<string>();

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            context = this;
            Config = Helper.ReadConfig<ModConfig>();
            if (!Config.EnableMod)
                return;
            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.GameLoop.OneSecondUpdateTicked += GameLoop_OneSecondUpdateTicked;

            
        }

        private void GameLoop_OneSecondUpdateTicked(object sender, OneSecondUpdateTickedEventArgs e)
        {
            if (!Context.IsPlayerFree)
                return;

            bool wearing = JsonAssets.GetClothingId("Sixty-Nine Shirt") != -1 && Game1.player.shirtItem?.Value?.parentSheetIndex?.Value == JsonAssets.GetClothingId("Sixty-Nine Shirt");

            if (!wearing)
            {
                return;
            }

            //Monitor.Log("Wearing shirt... nice.");

            foreach (NPC npc in Game1.player.currentLocation.characters.Where(n => n.isVillager()))
            {
                if(Vector2.Distance(Game1.player.position, npc.position) < Config.MaxDistanceNice)
                {
                    if (!niceNPCs.Contains(npc.name))
                    {
                        npc.showTextAboveHead(Helper.Translation.Get("nice"));
                        niceNPCs.Add(npc.name);
                    }
                }
                else
                {
                    if(niceNPCs.Contains(npc.name))
                        niceNPCs.Remove(npc.name);
                }
            }
        }

        public void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {
            JsonAssets = Helper.ModRegistry.GetApi<IJsonAssetsApi>("spacechase0.JsonAssets");
            if (JsonAssets == null)
            {
                Monitor.Log("Can't load Json Assets API for SixtyNine shirt");
            }
            else
            {
                JsonAssets.LoadAssets(Path.Combine(Helper.DirectoryPath, "assets/json-assets"));
            }
        }
    }
}
