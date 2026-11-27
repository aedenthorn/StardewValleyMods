using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData.Objects;
using System;
using System.Collections.Generic;

namespace CraftableBalloons
{
	public partial class ModEntry : Mod
	{
		public static IMonitor SMonitor;
		public static IModHelper SHelper;
		public static IManifest SModManifest;
		public static ModConfig Config;
		public static ModEntry context;
        public static List<Point> graveTiles = new List<Point>();
        public static string balloonKey = "aedenthorn.CraftableBalloons_balloon";
        public static string balloonPath = "aedenthorn.CraftableBalloons/balloon";
        public static string dictPath = "aedenthorn.CraftableBalloons/dict";
        public static string modKey = "aedenthorn.CraftableBalloons/color";
        public static Dictionary<Character, (Vector2 pos, Point vel)> characterMovement = new Dictionary<Character, (Vector2 pos, Point vel)>();
        public override void Entry(IModHelper helper)
		{
			Config = Helper.ReadConfig<ModConfig>();

			context = this;
			SMonitor = Monitor;
			SHelper = helper;
			SModManifest = ModManifest;

			helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.Content.AssetRequested += Content_AssetRequested;
            helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;

            Harmony harmony = new Harmony(ModManifest.UniqueID);
			harmony.PatchAll();
        }


        private void GameLoop_UpdateTicked(object sender, StardewModdingAPI.Events.UpdateTickedEventArgs e)
        {
            if (!Config.ModEnabled || !Context.IsWorldReady)
                return;
            foreach(var farmer in Game1.getAllFarmers())
            {
                if(farmer.currentLocation != Game1.currentLocation || !TryGetBalloonFromCharacter(farmer, out _, out _))
                {
                    characterMovement.Remove(farmer);
                    continue;
                }
                if (!characterMovement.TryGetValue(farmer, out var data))
                {
                    characterMovement[farmer] = (farmer.Position, Point.Zero);
                }
                else
                {
                    int max = 40;
                    Vector2 change = farmer.Position - data.pos;
                    Point newVel = change.ToPoint();
                    newVel = data.vel + newVel;
                    newVel = new Point(MathHelper.Clamp(newVel.X, -max, max), MathHelper.Clamp(newVel.Y, -max, max));
                    newVel -= new Point(Math.Sign(newVel.X) * 1, Math.Sign(newVel.Y) * 1);
                    characterMovement[farmer] = (farmer.Position, newVel);
                }
            }
            foreach(var npc in Game1.currentLocation.characters)
            {
                if(!TryGetBalloonFromCharacter(npc, out _, out _))
                {
                    characterMovement.Remove(npc);
                    continue;
                }
                if (!characterMovement.TryGetValue(npc, out var data))
                {
                    characterMovement[npc] = (npc.Position, Point.Zero);
                }
                else
                {
                    int max = 40;
                    Vector2 change = npc.Position - data.pos;
                    Point newVel = change.ToPoint();
                    newVel = data.vel + newVel;
                    newVel = new Point(MathHelper.Clamp(newVel.X, -max, max), MathHelper.Clamp(newVel.Y, -max, max));
                    newVel -= new Point(Math.Sign(newVel.X) * 1, Math.Sign(newVel.Y) * 1);
                    characterMovement[npc] = (npc.Position, newVel);
                }
            }
        }

        private void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
        {
            if (!Config.ModEnabled)
                return;
            if (e.NameWithoutLocale.IsEquivalentTo("Data/Objects"))
            {
				e.Edit(asset =>
				{
					var dict = asset.AsDictionary<string, ObjectData>().Data;
					dict[balloonKey] = new ObjectData()
					{
                        Name = balloonKey,
                        DisplayName = SHelper.Translation.Get("balloon"),
                        Description = SHelper.Translation.Get("balloon-desc"),
                        Type = "Equipment",
                        Category = -29,
                        Price = 100,
                        Texture = balloonPath,
                        SpriteIndex = 0,
                        ColorOverlayFromNextIndex = false,
                        Edibility = -300,
                        IsDrink = false,
                        Buffs = null,
                        GeodeDropsDefaultItems = false,
                        GeodeDrops = null,
                        ArtifactSpotChances = null,
                        CanBeGivenAsGift = true,
                        CanBeTrashed = true,
                        ExcludeFromFishingCollection = false,
                        ExcludeFromShippingCollection = false,
                        ExcludeFromRandomSale = false,
                        ContextTags = null,
                        CustomFields = null
                        
                    };
                });
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Data/NPCGiftTastes"))
            {
				e.Edit(asset =>
				{
					var dict = asset.AsDictionary<string, string>().Data;
					dict["Universal_Like"] = $"{dict["Universal_Like"]} {balloonKey}";
					dict["Jas"] = dict["Jas"].Replace("FairyBox", $"FairyBox {balloonKey}");
					dict["Vincent"] = dict["Vincent"].Replace("FrogEgg", $"FrogEgg {balloonKey}");
					dict["Leo"] = dict["Leo"].Replace("ParrotEgg", $"ParrotEgg {balloonKey}");
                });
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Data/CraftingRecipes"))
            {
                e.Edit(asset =>
                {
                    var dict = asset.AsDictionary<string, string>();
                    dict.Data[balloonKey] = $"766 5 768 1 769 1/Home/{balloonKey}/false/default/";
                });
            }
            else if (e.NameWithoutLocale.IsEquivalentTo(balloonPath))
            {
                e.LoadFromModFile<Texture2D>("assets/Balloon.png", StardewModdingAPI.Events.AssetLoadPriority.Exclusive);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo(dictPath))
            {
                e.LoadFrom(() => new Dictionary<string, string>(), StardewModdingAPI.Events.AssetLoadPriority.Exclusive);
            }
        }


        public void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
		{
			// Get Generic Mod Config Menu's API
			var gmcm = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");

            if (gmcm is not null)
			{
				// Register mod
				gmcm.Register(
					mod: ModManifest,
					reset: () => Config = new ModConfig(),
                    save: () => Helper.WriteConfig(Config)
                );
                // Main section
                gmcm.AddBoolOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.ModEnabled.Name"),
					getValue: () => Config.ModEnabled,
					setValue: value => Config.ModEnabled = value
				);
                gmcm.AddKeybind(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.ModKey.Name"),
					getValue: () => Config.ModKey,
					setValue: value => Config.ModKey = value
				);
                gmcm.AddNumberOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.PrismaticChance.Name"),
					getValue: () => Config.PrismaticChance,
					setValue: value => Config.PrismaticChance = value
				);
            }
		}
	}

}
