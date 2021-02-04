using Harmony;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Linq;

namespace ProceduralDialogue
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {

        public static ModConfig Config;
        private static string[] maleNames;
        private static IMonitor PMonitor;
        private static Random myRand;
        private static string[] femaleNames;
        private static string lastRandomGender;
        private static string[] neuterNames;

        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            if (!Config.Enabled)
                return;

            PMonitor = Monitor;

            myRand = new Random();

            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;

            var harmony = HarmonyInstance.Create(ModManifest.UniqueID);

            /*
            harmony.Patch(
               original: typeof(NamingMenu).GetConstructor(new[] { typeof(NamingMenu.doneNamingBehavior), typeof(string), typeof(string) }),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(NamingMenu_Prefix))
            );
            */

            harmony.Patch(
               original: AccessTools.Method(typeof(Dialogue), nameof(NPC.checkAction)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(NPC_checkAction_Prefix))
            );
        }


        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {

            foreach (IContentPack contentPack in Helper.ContentPacks.GetOwned())
            {
                try
                {
                    int add = 0;
                    Monitor.Log($"Reading content pack: {contentPack.Manifest.Name} {contentPack.Manifest.Version} from {contentPack.DirectoryPath}");
                    ProceduralDialogueData json = contentPack.ReadJsonFile<ProceduralDialogueData>("content.json");
                    foreach (ProceduralDialogue dialogue in json.data)
                    {
                        if (node.spriteType == "mod")
                        {
                            node.texture = contentPack.LoadAsset<Texture2D>(node.spritePath);

                        }
                        else
                        {
                            node.texture = Helper.Content.Load<Texture2D>(node.spritePath, ContentSource.GameContent);

                        }
                        if (conf.parentSheetIndexes.ContainsKey(add))
                        {
                            node.parentSheetIndex = conf.parentSheetIndexes[add];
                        }
                        else
                        {
                            while (existingPSIs.ContainsKey(id))
                                id++;
                            node.parentSheetIndex = id++;
                        }
                        conf.parentSheetIndexes[add] = node.parentSheetIndex;
                        CustomOreNodes.Add(node);
                        add++;
                    }
                    contentPack.WriteJsonFile("ore_config.json", conf);
                    Monitor.Log($"Got {data.nodes.Count} ores from content pack {contentPack.Manifest.Name}", LogLevel.Debug);
                }
                catch (Exception ex)
                {
                    SMonitor.Log($"Error processing custom_ore_nodes.json in content pack {contentPack.Manifest.Name} {ex}", LogLevel.Error);
                }
            }
            Monitor.Log($"Got {CustomOreNodes.Count} ores total", LogLevel.Debug);
        }


        private static void NPC_checkAction_Prefix(NPC __instance)
        {
            if(__instance.canTalk() && !__instance.CurrentDialogue.Any())
            {
                __instance.CurrentDialogue.Push(GetDialogue(__instance));
            }
        }

        private static Dialogue GetDialogue(NPC instance)
        {
            throw new NotImplementedException();
        }
    }
}