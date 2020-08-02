using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Minigames;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MobileCatalogues
{
    public class ModEntry : Mod
    {
        public static ModEntry context;

        internal static ModConfig Config;

        private IMobilePhoneApi api;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            context = this;
            Config = Helper.ReadConfig<ModConfig>();
            if (!Config.EnableMod)
                return;

            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
        }

        private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {
            api = Helper.ModRegistry.GetApi<IMobilePhoneApi>("aedenthorn.MobilePhone");
            if (api != null)
            {
                Texture2D appIcon;
                bool success;
                if (Config.EnableCatalogue)
                {
                    appIcon = Helper.Content.Load<Texture2D>(Path.Combine("assets", "app_icon_catalogue.png"));
                    success = api.AddApp(Helper.ModRegistry.ModID + "Catalogue", Helper.Translation.Get("catalogue"), OpenCatalogue, appIcon);
                    Monitor.Log($"loaded catalogue app successfully: {success}", LogLevel.Debug);
                }
                if (Config.EnableFurnitureCatalogue)
                {
                    appIcon = Helper.Content.Load<Texture2D>(Path.Combine("assets", "app_icon_furniture_catalogue.png"));
                    success = api.AddApp(Helper.ModRegistry.ModID + "FurnitureCatalogue", Helper.Translation.Get("furniture-catalogue"), OpenFurnitureCatalogue, appIcon);
                    Monitor.Log($"loaded furniture catalogue app successfully: {success}", LogLevel.Debug);
                }
                if (Config.EnableSeedCatalogue)
                {
                    appIcon = Helper.Content.Load<Texture2D>(Path.Combine("assets", "app_icon_seed_catalogue.png"));
                    success = api.AddApp(Helper.ModRegistry.ModID + "SeedCatalogue", Helper.Translation.Get("seed-catalogue"), OpenSeedCatalogue, appIcon);
                    Monitor.Log($"loaded furniture catalogue app successfully: {success}", LogLevel.Debug);
                }
            }
        }

        private void OpenCatalogue()
        {
            Monitor.Log("Opening catalogue");
            DelayedOpen(new ShopMenu(GetAllWallpapersAndFloors(), 0, null, null, null, "Catalogue"));
            
        }

        private void OpenFurnitureCatalogue()
        {
            Monitor.Log("Opening furniture catalogue");
            DelayedOpen(new ShopMenu(GetAllFurnitures(), 0, null, null, null, "Furniture Catalogue"));
        }

        private void OpenSeedCatalogue()
        {
            Monitor.Log("Opening seed catalogue");
            DelayedOpen(new ShopMenu(GetAllSeeds(), 0, null));
        }

        private async void DelayedOpen(ShopMenu menu)
        {
            await Task.Delay(100);
            Monitor.Log("Really opening catalogue");
            Game1.activeClickableMenu = menu;
        }

        private Dictionary<ISalable, int[]> GetAllWallpapersAndFloors()
        {
            Dictionary<ISalable, int[]> decors = new Dictionary<ISalable, int[]>();
            Wallpaper f;
            for (int i = 0; i < 112; i++)
            {
                f = new Wallpaper(i, false);
                decors.Add(new Wallpaper(i, false)
                {
                    Stack = int.MaxValue
                }, new int[]
                {
                    Config.FreeCatalogue  ? 0 : f.Price,
                    int.MaxValue
                });
            }
            for (int j = 0; j < 56; j++)
            {
                f = new Wallpaper(j, false);
                decors.Add(new Wallpaper(j, true)
                {
                    Stack = int.MaxValue
                }, new int[]
                {
                    Config.FreeCatalogue  ? 0 : f.Price,
                    int.MaxValue
                });
            }
            return decors;
        }

        private Dictionary<ISalable, int[]> GetAllFurnitures()
        {
            Dictionary<ISalable, int[]> decors = new Dictionary<ISalable, int[]>();
            Furniture f;
            foreach (KeyValuePair<int, string> v in Game1.content.Load<Dictionary<int, string>>("Data\\Furniture"))
            {
                if (true)
                {
                    f = new Furniture(v.Key, Vector2.Zero);
                    decors.Add(f, new int[]
                    {
                        Config.FreeFurnitureCatalogue ? 0 : f.Price,
                        int.MaxValue
                    });
                }
            }
            f = new Furniture(1402, Vector2.Zero);
            decors.Add(f, new int[]
            {
                Config.FreeFurnitureCatalogue  ? 0 : f.Price,
                int.MaxValue
            });
            f = new TV(1680, Vector2.Zero);
            decors.Add(f, new int[]
            {
                Config.FreeFurnitureCatalogue  ? 0 : f.Price,
                int.MaxValue
            });
            f = new TV(1466, Vector2.Zero);
            decors.Add(f, new int[]
            {
                Config.FreeFurnitureCatalogue  ? 0 : f.Price,
                int.MaxValue
            });
            f = new TV(1468, Vector2.Zero);
            decors.Add(f, new int[]
            {
                Config.FreeFurnitureCatalogue  ? 0 : f.Price,
                int.MaxValue
            });
            return decors;
        }
        private Dictionary<ISalable, int[]> GetAllSeeds()
        {
            Dictionary<ISalable, int[]> items = new Dictionary<ISalable, int[]>();
            Dictionary<int, string> cropData = base.Helper.Content.Load<Dictionary<int, string>>("Data\\Crops", 0);
            foreach (KeyValuePair<int, string> crop in cropData)
            {
                string[] values = crop.Value.Split('/');
                if (!int.TryParse(values[3], out int product))
                    continue;
                bool include = false;
                switch (Config.SeedsToInclude.ToLower())
                {
                    case "all":
                        include = true;
                        break;
                    case "shipped":
                        include = Game1.player.basicShipped.ContainsKey(product);
                        break;
                    case "season":
                        include = Utility.possibleCropsAtThisTime(Game1.currentSeason, Game1.dayOfMonth <= 7).Contains(product);
                        break;
                }
                if (include)
                {
                    StardewValley.Object item = new StardewValley.Object(crop.Key, int.MaxValue, false, -1, 0);
                    if (!item.bigCraftable.Value && item.ParentSheetIndex == 745)
                    {
                        item.Price = 50;
                    }
                    items.Add(item, new int[]
                    {
                        Config.FreeSeedCatalogue ? 0 : item.Price,
                        int.MaxValue
                    });
                }
            }
            return items;
        }
    }
}
