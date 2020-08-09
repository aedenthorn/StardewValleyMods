using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Object = StardewValley.Object;

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
                    Monitor.Log($"loaded seed catalogue app successfully: {success}", LogLevel.Debug);
                }
                if (Config.EnableDesertCatalogue)
                {
                    appIcon = Helper.Content.Load<Texture2D>(Path.Combine("assets", "app_icon_desert_catalogue.png"));
                    success = api.AddApp(Helper.ModRegistry.ModID + "DesertCatalogue", Helper.Translation.Get("desert-catalogue"), OpenDesertCatalogue, appIcon);
                    Monitor.Log($"loaded desert catalogue app successfully: {success}", LogLevel.Debug);
                }
                if (Config.EnableTravelingCatalogue)
                {
                    appIcon = Helper.Content.Load<Texture2D>(Path.Combine("assets", "app_icon_travel_catalogue.png"));
                    success = api.AddApp(Helper.ModRegistry.ModID + "TravelCatalogue", Helper.Translation.Get("travel-catalogue"), OpenTravelingCatalogue, appIcon);
                    Monitor.Log($"loaded travelling catalogue app successfully: {success}", LogLevel.Debug);
                }
                if (Config.EnableHatCatalogue)
                {
                    appIcon = Helper.Content.Load<Texture2D>(Path.Combine("assets", "app_icon_hat_catalogue.png"));
                    success = api.AddApp(Helper.ModRegistry.ModID + "HatCatalogue", Helper.Translation.Get("hat-catalogue"), OpenHatMouseCatalogue, appIcon);
                    Monitor.Log($"loaded hat catalogue app successfully: {success}", LogLevel.Debug);
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
            DelayedOpen(new ShopMenu(GetAllSeeds(), 0, null, null, null, "Seed Catalogue"));
        }

        private void OpenTravelingCatalogue()
        {
            if (Config.LimitTravelingCatalogToInTown && Game1.getLocationFromName("Forest") != null && !(Game1.getLocationFromName("Forest") as Forest).travelingMerchantDay)
            {
                Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("traveling-merchant-not-here"));
                return;
            }

            var dict = Utility.getTravelingMerchantStock((int)(Game1.uniqueIDForThisGame + (ulong)Game1.stats.DaysPlayed));

            AdjustPrices(ref dict);

            Monitor.Log("Opening traveling catalogue");
            DelayedOpen(new ShopMenu(dict, 0, "Traveler", new Func<ISalable, Farmer, int, bool>(Utility.onTravelingMerchantShopPurchase), null, null));
        }

        private void OpenDesertCatalogue()
        {
            if (Config.LimitDesertCatalogToBusFixed && !Game1.player.mailReceived.Contains("ccVault"))
            {
                Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("desert-merchant-cannot-ship"));
                return;
            }
            var dict = Desert.getDesertMerchantTradeStock(Game1.player);
            AdjustPrices(ref dict);

            Monitor.Log("Opening desert catalogue");
            DelayedOpen(new ShopMenu(dict, 0, "DesertTrade", new Func<ISalable, Farmer, int, bool>(boughtTraderItem), null, null));
        }

        private bool boughtTraderItem(ISalable s, Farmer arg2, int arg3)
        {
            if (s.Name == "Magic Rock Candy")
            {
                Desert.boughtMagicRockCandy = true;
            }
            return false;
        }

        private void OpenHatMouseCatalogue()
        {
            if (Game1.player.achievements.Count == 0)
            {
                Game1.activeClickableMenu = new DialogueBox(Helper.Translation.Get("catalog-not-available"));
                return;
            }
            var dict = Utility.getHatStock();
            AdjustPrices(ref dict);

            Monitor.Log("Opening hat catalogue");
            DelayedOpen(new ShopMenu(dict, 0, "HatMouse", null, null, null));
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
                    Config.FreeCatalogue  ? 0 : (int)Math.Round(f.salePrice() * Config.PriceMult),
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
                    Config.FreeCatalogue  ? 0 : (int)Math.Round(f.salePrice() * Config.PriceMult),
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
                        Config.FreeFurnitureCatalogue ? 0 : (int)Math.Round(f.salePrice() * Config.PriceMult),
                        int.MaxValue
                    });
                }
            }
            f = new Furniture(1402, Vector2.Zero);
            decors.Add(f, new int[]
            {
                Config.FreeFurnitureCatalogue  ? 0 :  (int)Math.Round(f.salePrice() * Config.PriceMult),
                int.MaxValue
            });
            f = new TV(1680, Vector2.Zero);
            decors.Add(f, new int[]
            {
                Config.FreeFurnitureCatalogue  ? 0 : (int)Math.Round(f.salePrice() * Config.PriceMult),
                int.MaxValue
            });
            f = new TV(1466, Vector2.Zero);
            decors.Add(f, new int[]
            {
                Config.FreeFurnitureCatalogue  ? 0 :  (int)Math.Round(f.salePrice() * Config.PriceMult),
                int.MaxValue
            });
            f = new TV(1468, Vector2.Zero);
            decors.Add(f, new int[]
            {
                Config.FreeFurnitureCatalogue  ? 0 :  (int)Math.Round(f.salePrice() * Config.PriceMult),
                int.MaxValue
            });
            return decors;
        }
        private Dictionary<ISalable, int[]> GetAllSeeds()
        {
            Dictionary<ISalable, int[]> items = new Dictionary<ISalable, int[]>();
            Dictionary<int, string> cropData = base.Helper.Content.Load<Dictionary<int, string>>("Data\\Crops", 0);
            List<int> shopSeeds = new List<int>();
            if (Config.SeedsToInclude.ToLower() == "season")
            {
                foreach (var item in (Game1.getLocationFromName("SeedShop") as SeedShop).shopStock().Keys)
                {
                    int idx = (item as Object).ParentSheetIndex;
                    Monitor.Log($"{item.DisplayName} {idx}");
                    shopSeeds.Add(idx);
                }
            }
            foreach (KeyValuePair<int, string> crop in cropData)
            {
                bool include = true;
                if(Config.SeedsToInclude.ToLower() == "shipped")
                {
                    string[] values = crop.Value.Split('/');
                    if (!int.TryParse(values[3], out int product))
                        continue;
                    include = Game1.player.basicShipped.ContainsKey(product);

                }
                else if(Config.SeedsToInclude.ToLower() == "season")
                {
                    include = shopSeeds.Contains(crop.Key);

                }
                if (include)
                {
                    Object item = new Object(crop.Key, int.MaxValue, false, -1, 0);
                    if (!item.bigCraftable.Value && item.ParentSheetIndex == 745)
                    {
                        item.Price = (int)Math.Round(50 * Config.PriceMult);
                    }
                    items.Add(item, new int[]
                    {
                        Config.FreeSeedCatalogue ? 0 :  (int)Math.Round(item.salePrice() * Config.PriceMult),
                        int.MaxValue
                    });
                }
            }
            return items;
        }

        private void AdjustPrices(ref Dictionary<ISalable, int[]> dict)
        {
            ISalable[] keys = dict.Keys.ToArray();
            for (int i = 0; i < keys.Length; i++)
            {
                dict[keys[i]][0] = (int)Math.Round(dict[keys[i]][0] * Config.PriceMult);
            }
        }
    }
}
