using System;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using Object = StardewValley.Object;

namespace BulkAnimalPurchase
{
	public partial class ModEntry
	{
		public static void UpdateMachinesRules()
		{
			if (!Config.EnableMod || !Context.IsWorldReady)
				return;

			foreach (GameLocation location in Game1.locations)
			{
				if (location.IsBuildableLocation())
				{
					foreach (Building building in location.buildings)
					{
						if (building.GetIndoors() is AnimalHouse || building.GetIndoors() is SlimeHutch)
						{
							GameLocation indoors = building.GetIndoors();

							foreach (Object @object in indoors.Objects.Values)
							{
								if (@object.bigCraftable.Value && @object.heldObject.Value is not null)
								{
									if (@object.QualifiedItemId.Equals("(BC)101"))
									{
										@object.MinutesUntilReady = Math.Min(@object.MinutesUntilReady, @object.heldObject.Value.Name switch
										{
											"Egg" => Config.ChickenIncubationTime,
											"Large Egg" => Config.ChickenIncubationTime,
											"Void Egg" => Config.ChickenIncubationTime,
											"Golden Egg" => Config.ChickenIncubationTime,
											"Duck Egg" => Config.DuckIncubationTime,
											"Dinosaur Egg" => Config.DinosaurIncubationTime,
											_ => Config.DefaultIncubatorTime
										});
									}
									else if (@object.QualifiedItemId.Equals("(BC)254"))
									{
										@object.MinutesUntilReady = Math.Min(@object.MinutesUntilReady, @object.heldObject.Value.Name switch
										{
											"Ostrich Egg" => Config.OstrichIncubationTime,
											_ => Config.DefaultOstrichIncubatorTime
										});
									}
									else if (@object.QualifiedItemId.Equals("(BC)156"))
									{
										@object.MinutesUntilReady = Math.Min(@object.MinutesUntilReady, @object.heldObject.Value.Name switch
										{
											"Green Slime Egg" => Config.GreenSlimeIncubationTime,
											"Blue Slime Egg" => Config.BlueSlimeIncubationTime,
											"Red Slime Egg" => Config.RedSlimeIncubationTime,
											"Purple Slime Egg" => Config.PurpleSlimeIncubationTime,
											"Tiger Slime Egg" => Config.TigerSlimeIncubationTime,
											_ => Config.DefaultSlimeIncubatorTime
										});
									}
								}
							}
						}
					}
				}
			}
		}

		public static FarmAnimal ApplyConfiguration(FarmAnimal farmAnimal)
		{
			if (!Config.EnableMod)
				return farmAnimal;

			farmAnimal.friendshipTowardFarmer.Value = Config.InitialFriendship;
			if (Config.AdultAnimals)
			{
				farmAnimal.growFully();
			}
			return farmAnimal;
		}
	}
}
