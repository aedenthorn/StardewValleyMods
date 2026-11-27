using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Menus;
using System.Reflection;
using Object = StardewValley.Object;

namespace CustomMounts
{
	/// <summary>The mod entry point.</summary>
	public partial class ModEntry : Mod
	{
		public static IMonitor SMonitor;
		public static IModHelper SHelper;
		public static IManifest SModManifest;
		public static ModConfig Config;
		public static ModEntry context;
        public static string modKey = "aedenthorn.CustomMounts";
        public static string nameKey = "aedenthorn.CustomMounts/name";
        public static string animKey = "aedenthorn.CustomMounts/animation";
        public static string dictPath = "aedenthorn.CustomMounts/dict";
        public static Dictionary<string, MountData> MountDict {
            get
            {
                return SHelper.GameContent.Load<Dictionary<string, MountData>>(dictPath);
            }
        }

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
		{
			Config = Helper.ReadConfig<ModConfig>();

			context = this;
			SMonitor = Monitor;
			SHelper = helper;
			SModManifest = ModManifest;

			helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            helper.Events.Content.AssetRequested += Content_AssetRequested;
            helper.Events.Input.ButtonPressed += Input_ButtonPressed;
			
            Harmony harmony = new(ModManifest.UniqueID);
            foreach (var list in (from type in typeof(Horse).Assembly.GetTypes()
                               where type.FullName.StartsWith("StardewValley.Characters.Horse")
                                 select type.GetMethods(
                                 BindingFlags.Public | BindingFlags.NonPublic |
                                 BindingFlags.Instance | BindingFlags.Static)))
            {
                foreach(var m in list)
                {
                    if (m.Name.Contains("<checkAction>"))
                    {
                        harmony.Patch(
                            original: m,
                            transpiler: new(typeof(ModEntry), nameof(Horse_checkAction2_Transpiler))
                        );
                    }
                }
            }

            harmony.Patch(
                original: AccessTools.Constructor(typeof(Horse), [typeof(Guid), typeof(int), typeof(int)] ),
                postfix: new(typeof(ModEntry), nameof(Horse_Postfix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(Horse), nameof(Horse.ChooseAppearance)),
                prefix: new(typeof(ModEntry), nameof(Horse_ChooseAppearance_Prefix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(Horse), nameof(Horse.GetBoundingBox)),
                postfix: new(typeof(ModEntry), nameof(Horse_GetBoundingBox_Postfix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(Horse), nameof(Horse.dayUpdate)),
                postfix: new(typeof(ModEntry), nameof(Horse_dayUpdate_Postfix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(Horse), nameof(Horse.PerformDefaultHorseFootstep)),
                transpiler: new(typeof(ModEntry), nameof(Horse_PerformDefaultHorseFootstep_Transpiler))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(Horse), nameof(Horse.draw), [typeof(SpriteBatch)]),
                prefix: new(typeof(ModEntry), nameof(Horse_draw_Prefix)),
                postfix: new(typeof(ModEntry), nameof(Horse_draw_Postfix)),
                transpiler: new(typeof(ModEntry), nameof(Horse_draw_Transpiler))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(Horse), nameof(Horse.checkAction)),
                prefix: new(typeof(ModEntry), nameof(Horse_checkAction_Prefix)),
                transpiler: new(typeof(ModEntry), nameof(Horse_checkAction_Transpiler))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(Horse), nameof(Horse.update), [typeof(GameTime), typeof(GameLocation)]),
                prefix: new(typeof(ModEntry), nameof(Horse_update_Prefix)),
                transpiler: new(typeof(ModEntry), nameof(Horse_update_Transpiler))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(Horse), nameof(Horse.SyncPositionToRider)),
                postfix: new(typeof(ModEntry), nameof(Horse_SyncPositionToRider_Postfix))
            );
            

            harmony.Patch(
                original: AccessTools.Method(typeof(Building), nameof(Building.OnUpgraded)),
                postfix: new(typeof(ModEntry), nameof(Building_OnUpgraded_Postfix))
            );

            harmony.Patch(
                original: AccessTools.Method(typeof(Stable), nameof(Stable.GetDefaultHorseTile)),
                prefix: new(typeof(ModEntry), nameof(Stable_GetDefaultHorseTile_Prefix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(Stable), nameof(Stable.updateHorseOwnership)),
                transpiler: new(typeof(ModEntry), nameof(Stable_updateHorseOwnership_Transpiler))
            );

            /*
            harmony.Patch(
                original: AccessTools.Method(typeof(Character), nameof(Character.faceDirection)),
                prefix: new(typeof(ModEntry), nameof(Character_faceDirection_Prefix))
            );
            */

            harmony.Patch(
                original: AccessTools.Method(typeof(NPC), nameof(NPC.behaviorOnFarmerLocationEntry)),
                prefix: new(typeof(ModEntry), nameof(NPC_behaviorOnFarmerLocationEntry_Prefix))
            );
            

            //harmony.Patch(
            //    original: AccessTools.Method(typeof(NPC), nameof(NPC.draw), [typeof(SpriteBatch), typeof(float)]),
            //    transpiler: new(typeof(ModEntry), nameof(NPC_draw_Transpiler))
            //);

            harmony.Patch(
                original: AccessTools.Method(typeof(Object), nameof(Object.performUseAction)),
                transpiler: new(typeof(ModEntry), nameof(Object_performUseAction_Transpiler))
            );

            harmony.Patch(
                original: AccessTools.Method(typeof(Utility), nameof(Utility.GetHorseWarpRestrictionsForFarmer)),
                postfix: new(typeof(ModEntry), nameof(Utility_GetHorseWarpRestrictionsForFarmer_Postfix))
            );

            harmony.Patch(
                original: AccessTools.Method(typeof(Farmer), nameof(Farmer.Update), [typeof(GameTime), typeof(GameLocation)]),
                prefix: new(typeof(ModEntry), nameof(Farmer_Update_Prefix)),
                postfix: new(typeof(ModEntry), nameof(Farmer_Update_Postfix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(Farmer), nameof(Farmer.updateMovementAnimation)),
                transpiler: new(typeof(ModEntry), nameof(Farmer_updateMovementAnimation_Transpiler))
            );

            harmony.Patch(
                original: AccessTools.Method(typeof(Farmer), nameof(Farmer.getMovementSpeed)),
                transpiler: new(typeof(ModEntry), nameof(Farmer_getMovementSpeed_Transpiler))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(FarmerTeam), nameof(FarmerTeam.OnRequestHorseWarp)),
                transpiler: new(typeof(ModEntry), nameof(FarmerTeam_OnRequestHorseWarp_Transpiler))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(Game1), nameof(Game1.UpdateHorseOwnership)),
                transpiler: new(typeof(ModEntry), nameof(Game1_UpdateHorseOwnership_Transpiler))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(AnimalPage), nameof(AnimalPage.CreateSpriteComponent)),
                postfix: new(typeof(ModEntry), nameof(AnimalPage_CreateSpriteComponent_Postfix))
            );
        }

        private void Input_ButtonPressed(object? sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            return;
            if(e.Button == SButton.O)
            {
                SHelper.GameContent.InvalidateCache(dictPath);
                var cc = Game1.getFarm().characters;
                for (int i = cc.Count - 1; i >= 0; i--)
                {
                    if (cc[i] is Horse horse)
                    {
                        cc.RemoveAt(i);
                    }
                }
            }
        }

        private void GameLoop_SaveLoaded(object? sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
        }

        private void Content_AssetRequested(object? sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo(dictPath))
            {
                e.LoadFrom(() => new Dictionary<string, MountData>(), StardewModdingAPI.Events.AssetLoadPriority.Exclusive);
            }
        }

        private void GameLoop_GameLaunched(object? sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {

            // get Generic Mod Config Menu's API (if it's installed)
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
                    name: () => ModEntry.SHelper.Translation.Get("GMCM.ModEnabled.Name"),
                    getValue: () => Config.ModEnabled,
                    setValue: value => Config.ModEnabled = value
                );
                configMenu.AddBoolOption(
                    mod: ModManifest,
                    name: () => ModEntry.SHelper.Translation.Get("GMCM.AllowMultipleMounts.Name"),
                    getValue: () => Config.AllowMultipleMounts,
                    setValue: value => Config.AllowMultipleMounts = value
                );
                configMenu.AddKeybind(
                    mod: ModManifest,
                    name: () => ModEntry.SHelper.Translation.Get("GMCM.RenameModKey.Name"),
                    getValue: () => Config.RenameModKey,
                    setValue: value => Config.RenameModKey = value
                );
            }
		}
	}
}
