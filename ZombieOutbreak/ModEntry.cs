using Harmony;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.IO;

namespace ZombieOutbreak
{
    public class ModEntry : Mod, IAssetEditor 
    {
        
        public static ModConfig config;
        public static IMonitor SMonitor;
        public static Dictionary<string, Texture2D> zombieTextures = new Dictionary<string, Texture2D>();
        public static Dictionary<string, Texture2D> zombiePortraits = new Dictionary<string, Texture2D>();
        public static Dictionary<long, Texture2D> playerSprites = new Dictionary<long, Texture2D>();
        public static Dictionary<long, Texture2D> playerZombies = new Dictionary<long, Texture2D>();
        internal static List<string> curedNPCs = new List<string>();
        internal static List<long> curedFarmers = new List<long>();
        internal static IEnumerable<string> villagerNames;
        private static IJsonAssetsApi JsonAssets;

        public override void Entry(IModHelper helper)
        {
            config = Helper.ReadConfig<ModConfig>();
            if (!config.EnableMod)
                return;
            ZombiePatches.Initialize(Helper, Monitor, config);
            Utils.Initialize(Helper, Monitor, config);
            
            SMonitor = Monitor;

            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            Helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            Helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
            Helper.Events.GameLoop.OneSecondUpdateTicked += GameLoop_OneSecondUpdateTicked;

            Helper.ConsoleCommands.Add("infect", "Infect an NPC with zombie virus. Usage: infect <npc>", new Action<string, string[]>(Utils.InfectNPC));
            Helper.ConsoleCommands.Add("infectplayer", "Infect local player with zombie virus.", new Action<string, string[]>(Utils.InfectPlayer));


            var harmony = HarmonyInstance.Create(ModManifest.UniqueID); 

            harmony.Patch(
                original: AccessTools.Method(typeof(NPC), nameof(NPC.draw), new Type[] { typeof(SpriteBatch), typeof(float) }),
                prefix: new HarmonyMethod(typeof(ZombiePatches), nameof(ZombiePatches.NPC_draw_prefix))
            );
            
            harmony.Patch(
                original: AccessTools.Method(typeof(Farmer), nameof(Farmer.draw), new Type[] { typeof(SpriteBatch) }),
                prefix: new HarmonyMethod(typeof(ZombiePatches), nameof(ZombiePatches.Farmer_draw_prefix))
            );
            
            harmony.Patch(
                original: AccessTools.Method(typeof(Farmer), nameof(Farmer.eatObject)),
                prefix: new HarmonyMethod(typeof(ZombiePatches), nameof(ZombiePatches.Farmer_eatObject_prefix))
            );
            
           
            harmony.Patch(
                original: AccessTools.Constructor(typeof(DialogueBox), new Type[] { typeof(string), typeof(List<Response>), typeof(int) }),
                prefix: new HarmonyMethod(typeof(ZombiePatches), nameof(ZombiePatches.DialogueBox_complex_prefix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(NPC), nameof(NPC.showTextAboveHead)),
                prefix: new HarmonyMethod(typeof(ZombiePatches), nameof(ZombiePatches.NPC_showTextAboveHead_Prefix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(NPC), nameof(NPC.getHi)),
                postfix: new HarmonyMethod(typeof(ZombiePatches), nameof(ZombiePatches.NPC_getHi_Postfix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(Dialogue), "parseDialogueString"),
                prefix: new HarmonyMethod(typeof(ZombiePatches), nameof(ZombiePatches.Dialogue_prefix))
            );

            harmony.Patch(
                original: AccessTools.Method(typeof(ShopMenu), nameof(ShopMenu.setUpShopOwner)),
                postfix: new HarmonyMethod(typeof(ZombiePatches), nameof(ZombiePatches.setUpShopOwner_postfix))
            );

            harmony.Patch(
                original: AccessTools.Method(typeof(NPC), nameof(NPC.tryToReceiveActiveObject)),
                prefix: new HarmonyMethod(typeof(ZombiePatches), nameof(ZombiePatches.NPC_tryToReceiveActiveObject_prefix))
            );
        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            // load json assets

            JsonAssets = Helper.ModRegistry.GetApi<IJsonAssetsApi>("spacechase0.JsonAssets");
            if (JsonAssets == null)
            {
                Monitor.Log("Can't load Json Assets API for Zombie Outbreak mod");
            }
            else
            {
                JsonAssets.LoadAssets(Path.Combine(Helper.DirectoryPath, "assets/json-assets"));
            }
        }

        private void GameLoop_OneSecondUpdateTicked(object sender, StardewModdingAPI.Events.OneSecondUpdateTickedEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            Utils.CheckForInfection();
        }

        private void GameLoop_DayStarted(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
        {
            curedNPCs.Clear();
            curedFarmers.Clear();
            //Utils.AddZombiePlayer(Game1.player.uniqueMultiplayerID);

            if (Game1.random.NextDouble() < config.DailyZombificationChance)
                Utils.MakeRandomZombie();
            if (zombieTextures.Count > 0 && !Game1.player.mailReceived.Contains("ZombieCure"))
                Game1.mailbox.Add("ZombieCure");
        }

        private void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            villagerNames = Helper.Content.Load<Dictionary<string, string>>("Data/NPCDispositions", ContentSource.GameContent).Keys;

            zombieTextures.Clear();
            playerZombies.Clear();
            Monitor.Log($"Loading zombie textures");
            List<string> zombies = Helper.Data.ReadSaveData<List<string>>("zombies") ?? new List<string>();
            List<long> zombiePlayers = Helper.Data.ReadSaveData<List<long>>("zombiePlayers") ?? new List<long>();
            foreach(string z in zombies)
            {
                Utils.MakeZombieTexture(z);
            }
            foreach(long z in zombiePlayers)
            {
                Utils.MakeZombiePlayer(z);
            }
            Monitor.Log($"Got {zombieTextures.Count} zombie(s)");
            Monitor.Log($"Got {zombiePlayers.Count} zombie player(s)");
        }


        public bool CanEdit<T>(IAssetInfo asset)
        {
            if (!config.EnableMod)
                return false;
            if (asset.AssetNameEquals("Data/mail"))
            {
                return true;
            }
            return false;
        }
        public void Edit<T>(IAssetData asset)
        {
            if (asset.AssetNameEquals("Data/mail"))
                asset.AsDictionary<string, string>().Data["ZombieCure"] = Helper.Translation.Get("zombie-cure-letter");
        }
    }
}
