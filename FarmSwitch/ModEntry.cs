using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using System.Linq;
using System.Reflection;

namespace FarmSwitch
{
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;
        public static ModEntry context;

        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            Helper.Events.Input.ButtonPressed += Input_ButtonPressed;
            
            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }


        private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Config.EnableMod)
                return;
            if (Game1.activeClickableMenu != null && Game1.player?.currentLocation?.lastQuestionKey?.Equals("FarmSwitch_Which") == true)
            {

                IClickableMenu menu = Game1.activeClickableMenu;
                if (menu == null || menu.GetType() != typeof(DialogueBox))
                    return;

                DialogueBox db = menu as DialogueBox;
                int resp = db.selectedResponse;
                Response[] resps = db.responses;

                if (resp < 0 || resps == null || resp >= resps.Length || resps[resp] == null || resps[resp].responseKey == "cancel")
                    return;

                string name = resps[resp].responseKey;
                var location = Game1.getLocationFromName(name);
                if (location == null)
                {
                    SMonitor.Log($"Location {name} not found.", LogLevel.Warn);
                    return;
                }

                Monitor.Log($"Answered {Game1.player.currentLocation.lastQuestionKey} with {name}");

                db.closeDialogue();
                var oldFarm = Game1._locationLookup["Farm"];
                string path = oldFarm.mapPath.Value.Substring("Maps\\".Length);
                oldFarm.name.Value = "FarmSwitch_" + path;

                Monitor.Log($"old farm path {path}, new name {oldFarm.Name}");
                Monitor.Log($"new farm path {location.Name}");

                location.name.Value = "Farm";


                Game1._locationLookup["Farm"] = location;
                Game1._locationLookup["FarmSwitch_" + path] = oldFarm;
                Game1._locationLookup.Remove(name);
                var totem = ItemRegistry.Create("(O)688");
                foreach (var f in oldFarm.farmers)
                {
                    AccessTools.Method(totem.GetType(), "totemWarp").Invoke(totem, new object[] { f });
                }
            }
        }
        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is not null)
            {

                // register mod
                configMenu.Register(
                    mod: ModManifest,
                    reset: () => Config = new ModConfig(),
                    save: () => Helper.WriteConfig(Config)
                );

                configMenu.AddBoolOption(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("Config.EnableMod"),
                    getValue: () => Config.EnableMod,
                    setValue: value => Config.EnableMod = value
                );
            }
        }
    }
}
