using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Objects;
using Object = StardewValley.Object;

namespace PushPull
{
	/// <summary>The mod entry point.</summary>
	public partial class ModEntry : Mod
	{
		internal static IMonitor SMonitor;
		internal static IModHelper SHelper;
		internal static IManifest SModManifest;
		internal static ModConfig Config;
		internal static ModEntry context;
		internal static Dictionary<Object, MovementData> movingObjects = new();
		internal static PerScreen<Vector2> PushingTile = new(() => new());
		internal static PerScreen<int> PushingTicks = new(() => new());
		internal static PerScreen<Vector2> PullingTile = new(() => new());
		internal static PerScreen<int> PullingTicks = new(() => new());
		internal static PerScreen<int> PullingFace = new(() => new());

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
            helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;

			// Load Harmony patches
			try
			{
				Harmony harmony = new(ModManifest.UniqueID);

                harmony.Patch(
                    original: AccessTools.Method(typeof(Farmer), nameof(Farmer.MovePosition)),
                    prefix: new(typeof(ModEntry), nameof(Farmer_MovePosition_Prefix)),
                    postfix: new(typeof(ModEntry), nameof(Farmer_MovePosition_Postfix))
                );
                harmony.Patch(
                    original: AccessTools.DeclaredPropertySetter(typeof(Farmer), nameof(Farmer.FacingDirection)),
                    prefix: new(typeof(ModEntry), nameof(Farmer_FacingDirection_Prefix))
                );
                harmony.Patch(
                    original: AccessTools.Method(typeof(Object), nameof(Object.draw), [typeof(SpriteBatch), typeof(int), typeof(int) , typeof(float)]),
                    prefix: new(typeof(ModEntry), nameof(Object_draw_Prefix))
                );
                harmony.Patch(
                    original: AccessTools.Method(typeof(Chest), nameof(Chest.draw), [typeof(SpriteBatch), typeof(int), typeof(int) , typeof(float)]),
                    transpiler: new(typeof(ModEntry), nameof(Chest_draw_Transpiler))
                );
            }
			catch (Exception e)
			{
				Monitor.Log($"Issue with Harmony patching: {e}", LogLevel.Error);
				return;
			}
		}


        private void GameLoop_UpdateTicked(object? sender, StardewModdingAPI.Events.UpdateTickedEventArgs e)
        {
            if (!Config.ModEnabled)
                return;

            foreach (var obj in movingObjects.Keys.ToArray())
            {
                var d = movingObjects[obj];
                d.position += Config.Speed;
                if (d.position >= 64)
                {
					if(d.location.objects.ContainsKey(obj.TileLocation))
					{
                        d.location.objects.Remove(obj.TileLocation);
                        d.location.objects[d.destination] = obj;
                    }
                    movingObjects.Remove(obj);
                }
                else
                {
                    movingObjects[obj] = d;
                }
            }
        }


        private void GameLoop_GameLaunched(object? sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
		{
			// get Generic Mod Config Menu's API (if it's installed)
			var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
			if (configMenu is null)
				return;

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
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => SHelper.Translation.Get("GMCM.Speed.Name"),
                getValue: () => Config.Speed,
                setValue: value => Config.Speed = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => SHelper.Translation.Get("GMCM.Delay.Name"),
                getValue: () => Config.Delay,
                setValue: value => Config.Delay = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => SHelper.Translation.Get("GMCM.Pull.Name"),
                getValue: () => Config.Pull,
                setValue: value => Config.Pull = value
            );
            configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.Rocks.Name"),
				getValue: () => Config.Rocks,
				setValue: value => Config.Rocks = value
			);
			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.Clumps.Name"),
				getValue: () => Config.Clumps,
				setValue: value => Config.Clumps = value
			);
			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.Sticks.Name"),
				getValue: () => Config.Sticks,
				setValue: value => Config.Sticks = value
			);
			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.Constructs.Name"),
				getValue: () => Config.Constructs,
				setValue: value => Config.Constructs = value
			);
		}
	}
}
