using Harmony;
using Netcode;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomMonsterFloors
{
	/// <summary>The mod entry point.</summary>
	public class ModEntry : Mod
	{
		public static ModConfig Config;

		/*********
        ** Public methods
        *********/
		/// <summary>The mod entry point, called after the mod is first loaded.</summary>
		/// <param name="helper">Provides simplified APIs for writing mods.</param>
		public override void Entry(IModHelper helper)
		{
			Config = helper.ReadConfig<ModConfig>();

			helper.Events.GameLoop.DayStarted += OnDayStarted;

			var harmony = HarmonyInstance.Create(this.ModManifest.UniqueID);

			harmony.Patch(
			   original: AccessTools.Method(typeof(MineShaft), "loadLevel"),
			   postfix: new HarmonyMethod(typeof(ModEntry),nameof(ModEntry.loadLevel_Postfix))
			);
		}

		private void OnDayStarted(object sender, DayStartedEventArgs e)
		{
			monsterFloors.Clear();
		}
		public static List<int> monsterFloors = new List<int>();

		public static void loadLevel_Postfix(MineShaft __instance, int level, ref NetBool ___netIsMonsterArea, ref NetBool ___netIsSlimeArea, ref NetBool ___netIsDinoArea, NetBool ___netIsQuarryArea, ref NetString ___mapImageSource, bool ___loadedDarkArea, Random ___mineRandom)
		{
			if(__instance.getMineArea(level) == 77377) // not sure about these
			{
				return;
			}

			bool IsMonsterFloor = Game1.random.Next(0, 100) < Config.PercentChanceMonsterFloor;

			if (IsMonsterFloor)
			{

				if (IsBelowMinFloorsApart(level))
				{
					if (___netIsSlimeArea || ___netIsDinoArea)
					{
						RevertMapImageSource(ref ___mapImageSource, ref ___loadedDarkArea, level, __instance.getMineArea(-1), __instance.getMineArea(level), __instance.loadedMapNumber);
					}
					___netIsDinoArea.Value = false;
					___netIsSlimeArea.Value = false;
					___netIsMonsterArea.Value = false;
					return;
				}

				monsterFloors.Add(level);

				if (___netIsQuarryArea)
				{
					___netIsMonsterArea.Value = true;
				}
				else
				{
					int roll = Game1.random.Next(0, 100);
					___netIsMonsterArea.Value = true;
					if (__instance.getMineArea(-1) == 121)
					{
						string[] chances = Config.SlimeDinoMonsterSplitPercents.Split(':');
						int slimeChance = int.Parse(chances[0]);
						int dinoChance = int.Parse(chances[1]);
						if (roll < slimeChance)
						{
							___netIsDinoArea.Value = false;
							___netIsSlimeArea.Value = true;
							___netIsMonsterArea.Value = false;

						}
						else if (roll < dinoChance + slimeChance)
						{
							___netIsDinoArea.Value = true;
							___netIsSlimeArea.Value = false;
							___netIsMonsterArea.Value = false;
							___mapImageSource.Value = "Maps\\Mines\\mine_dino";
						}
						else if (___netIsSlimeArea || ___netIsDinoArea)
						{
							RevertMapImageSource(ref ___mapImageSource, ref ___loadedDarkArea, level, __instance.getMineArea(-1), __instance.getMineArea(level), __instance.loadedMapNumber);
						}
					}
					else
					{
						string[] chances = Config.SlimeMonsterSplitPercents.Split(':');
						if (roll < int.Parse(chances[0]))
						{
							___netIsDinoArea.Value = false;
							___netIsSlimeArea.Value = true;
							___netIsMonsterArea.Value = false;
							___mapImageSource.Value = "Maps\\Mines\\mine_slime";
						}
					}
				}
			}
		}

		private static bool IsBelowMinFloorsApart(int level)
		{
			if (Config.MinFloorsBetweenMonsterFloors <= 0)
				return false;

			foreach(int i in monsterFloors)
			{
				if(Math.Abs(level-i) <= Config.MinFloorsBetweenMonsterFloors){
					return true;
				}
			}
			return false;
		}

		private static void RevertMapImageSource(ref NetString mapImageSource, ref bool loadedDarkArea, int level, int mineAreaNeg, int mineAreaLevel, int mapNumberToLoad)
		{

			if (mineAreaNeg == 0 || mineAreaNeg == 10 || (mineAreaLevel != 0 && mineAreaLevel != 10))
			{
				if (mineAreaLevel == 40)
				{
					mapImageSource.Value = "Maps\\Mines\\mine_frost";
					if (level >= 70)
					{
						NetString netString = mapImageSource;
						netString.Value += "_dark";
						loadedDarkArea = true;
					}
				}
				else if (mineAreaLevel == 80)
				{
					mapImageSource.Value = "Maps\\Mines\\mine_lava";
					if (level >= 110 && level != 120)
					{
						NetString netString2 = mapImageSource;
						netString2.Value += "_dark";
						loadedDarkArea = true;
					}
				}
				else if (mineAreaLevel == 121)
				{
					mapImageSource.Value = "Maps\\Mines\\mine_desert";
					if (mapNumberToLoad % 40 >= 30)
					{
						NetString netString3 = mapImageSource;
						netString3.Value += "_dark";
						loadedDarkArea = true;
					}
				}
				else
				{
					mapImageSource.Value = "Maps\\Mines\\mine";
				}
			}
			else
			{
				mapImageSource.Value = "Maps\\Mines\\mine";
			}
		}
	}
}