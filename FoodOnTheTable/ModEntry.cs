using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Globalization;
using Object = StardewValley.Object;

namespace FoodOnTheTable
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;

        public static IFurnitureDisplayFrameworkAPI fdfAPI;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.Patch(
               original: AccessTools.Method(typeof(NPC), nameof(NPC.performTenMinuteUpdate)),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(NPC_performTenMinuteUpdate_Postfix))
            );
           harmony.Patch(
               original: AccessTools.Method(typeof(NPC), nameof(NPC.dayUpdate)),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(NPC_dayUpdate_Postfix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(FarmHouse), nameof(FarmHouse.updateEvenIfFarmerIsntHere)),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(FarmHouse_updateEvenIfFarmerIsntHere_Postfix))
            );
        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
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
                name: () => "Mod Enabled?",
                getValue: () => Config.EnableMod,
                setValue: value => Config.EnableMod = value
            );

            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Minutes To Hungry",
				tooltip: () => "Minutes since last meal",
				getValue: () => Config.MinutesToHungry,
                setValue: value => Config.MinutesToHungry = value
            );
			configMenu.AddTextOption(
				mod: ModManifest,
				name: () => "Points Mult",
				tooltip: () => "Friendship point multiplier for spouses and roommates",
				getValue: () => "" + Config.PointsMult,
				setValue: delegate (string value) { try { Config.PointsMult = float.Parse(value, CultureInfo.InvariantCulture); } catch { } }
			);
			configMenu.AddTextOption(
				mod: ModManifest,
				name: () => "Move To Food % Chance",
				tooltip: () => "Percent chance per tick to move to food if hungry",
				getValue: () => "" + Config.MoveToFoodChance,
				setValue: delegate (string value) { try { Config.MoveToFoodChance = float.Parse(value, CultureInfo.InvariantCulture); } catch { } }
			);
			configMenu.AddTextOption(
				mod: ModManifest,
				name: () => "Max Distance to Eat",
				tooltip: () => "Max distance in tiles from food to eat it",
				getValue: () => "" + Config.MaxDistanceToEat,
				setValue: delegate (string value) { try { Config.MaxDistanceToEat = float.Parse(value, CultureInfo.InvariantCulture); } catch { } }
			);
			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => "Count as Fed Spouse",
				tooltip: () => "For Another Hunger Mod",
				getValue: () => Config.CountAsFedSpouse,
				setValue: value => Config.CountAsFedSpouse = value
			);
			try
			{
                fdfAPI = SHelper.ModRegistry.GetApi<IFurnitureDisplayFrameworkAPI>("aedenthorn.FurnitureDisplayFramework");
            }
			catch { }
        }

        private void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            if (!Config.EnableMod)
                return;
        }


		private static bool TryToEatFood(NPC __instance, PlacedFoodData food)
		{
			if (food != null && Vector2.Distance(food.foodTile, __instance.getTileLocation()) < Config.MaxDistanceToEat)
			{
				SMonitor.Log($"eating {food.foodObject.Name} at {food.foodTile}");
				using (IEnumerator<Furniture> enumerator = __instance.currentLocation.furniture.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						if (enumerator.Current.boundingBox.Value != food.furniture.boundingBox.Value)
							continue;
						if (food.slot > -1)
						{
							enumerator.Current.modData.Remove("aedenthorn.FurnitureDisplayFramework/" + food.slot);
							SMonitor.Log($"ate food at slot {food.slot} in {enumerator.Current.Name}");
						}
						else
						{
							enumerator.Current.heldObject.Value = null;
							SMonitor.Log($"ate held food in {enumerator.Current.Name}");
						}

						if (__instance.currentLocation is FarmHouse)
						{
							Farmer owner = (__instance.currentLocation as FarmHouse).owner;

							if (owner.friendshipData.ContainsKey(__instance.Name) && (owner.friendshipData[__instance.Name].IsMarried() || owner.friendshipData[__instance.Name].IsRoommate()))
							{
								int points = 80;
								switch (food.value)
								{
									case 1:
										points = 45;
										break;
									case 2:
										points = 20;
										break;
									default:
										__instance.doEmote(20);
										break;
								}
								owner.friendshipData[__instance.Name].Points += (int)(points * Config.PointsMult);
                                if (Config.CountAsFedSpouse && SHelper.ModRegistry.IsLoaded("spacechase0.AnotherHungerMod"))
								{
									owner.modData["spacechase0.AnotherHungerMod/FedSpouse"] = "true";
                                }
								SMonitor.Log($"Friendship with {owner.Name} increased by {(int)(points * Config.PointsMult)} points!");
							}
						}
						__instance.modData["aedenthorn.FoodOnTheTable/LastFood"] = Game1.timeOfDay.ToString();
						return true;
					}
				}
			}
			return false;
		}
		private static PlacedFoodData GetClosestFood(NPC npc, GameLocation location)
		{

			List<PlacedFoodData> foodList = new List<PlacedFoodData>();
			foreach (var f in location.furniture)
			{
				if (f.heldObject.Value != null && f.heldObject.Value.Edibility > 0)
				{
					for (int x = f.boundingBox.X / 64; x < (f.boundingBox.X + f.boundingBox.Width) / 64; x++)
					{
						for (int y = f.boundingBox.Y / 64; y < (f.boundingBox.Y + f.boundingBox.Height) / 64; y++)
						{
							foodList.Add(new PlacedFoodData(f, new Vector2(x, y), f.heldObject.Value, -1));
						}
					}
				}
				if (fdfAPI != null)
				{
                    List<Object> objList = fdfAPI.GetSlotObjects(f);
					if (objList is null || objList.Count == 0)
						continue;
					for (int i = 0; i < objList.Count; i++)
					{
                        if (objList[i] is not null && objList[i].Edibility > 0)
                        {
                            var slotRect = fdfAPI.GetSlotRect(f, i);
                            if (slotRect != null)
                                foodList.Add(new PlacedFoodData(f, new Vector2((f.boundingBox.X + slotRect.Value.X) / 64, (f.boundingBox.Y + slotRect.Value.Y) / 64), objList[i], i));
                        }
                    }
				}
			}
			if (foodList.Count == 0)
			{
				//SMonitor.Log("Got no food");
				return null;
			}
			List<string> favList = new List<string>(Game1.NPCGiftTastes["Universal_Love"].Split(' '));
			List<string> likeList = new List<string>(Game1.NPCGiftTastes["Universal_Like"].Split(' '));
			List<string> okayList = new List<string>(Game1.NPCGiftTastes["Universal_Neutral"].Split(' '));

			if (Game1.NPCGiftTastes.TryGetValue(npc.Name, out string NPCLikes) && NPCLikes != null)
			{
				favList.AddRange(NPCLikes.Split('/')[1].Split(' '));
				likeList.AddRange(NPCLikes.Split('/')[3].Split(' '));
				okayList.AddRange(NPCLikes.Split('/')[5].Split(' '));
			}
			for (int i = foodList.Count - 1; i >= 0; i--)
			{
				if (favList.Contains(foodList[i].foodObject.ParentSheetIndex + ""))
				{
					foodList[i].value = 3;
				}
				else
				{
					if (likeList.Contains(foodList[i].foodObject.ParentSheetIndex + ""))
					{
						foodList[i].value = 2;
					}
					else
					{
						if (okayList.Contains(foodList[i].foodObject.ParentSheetIndex + ""))
						{
							foodList[i].value = 1;
						}
						else
							foodList.RemoveAt(i);
					}
				}
			}
			if (foodList.Count == 0)
			{
				//SMonitor.Log("Got no food");
				return null;
			}

			foodList.Sort(delegate (PlacedFoodData a, PlacedFoodData b)
			{
				var compare = b.value.CompareTo(a.value);
				if (compare != 0)
					return compare;
				return (Vector2.Distance(a.foodTile, npc.getTileLocation()).CompareTo(Vector2.Distance(b.foodTile, npc.getTileLocation())));
			});

			SMonitor.Log($"Got {foodList.Count} possible food for {npc.Name}; best: {foodList[0].foodObject.Name} at {foodList[0].foodTile}, value {foodList[0].value}");
			return foodList[0];
		}
		private static Object GetObjectFromID(string id, int amount, int quality)
		{
			if (int.TryParse(id, out int index))
			{
				//SMonitor.Log($"Spawning object with index {id}");
				return new Object(index, amount, false, -1, quality);
			}
			foreach (var kvp in Game1.objectInformation)
			{
				if (kvp.Value.StartsWith(id + "/"))
					return new Object(kvp.Key, amount, false, -1, quality);
			}
			return null;
		}
        private static bool WantsToEat(NPC spouse)
		{
			if (!spouse.modData.ContainsKey("aedenthorn.FoodOnTheTable/LastFood") || spouse.modData["aedenthorn.FoodOnTheTable/LastFood"].Length == 0)
			{
				return true;
			}

			return GetMinutes(Game1.timeOfDay) - GetMinutes(int.Parse(spouse.modData["aedenthorn.FoodOnTheTable/LastFood"])) > Config.MinutesToHungry;
		}

		private static int GetMinutes(int timeOfDay)
		{
			return timeOfDay % 100 + timeOfDay / 100 * 60;
		}
	}

}