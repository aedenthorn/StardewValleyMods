using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Object = StardewValley.Object;

namespace MobileCatalogues
{
    public class Catalogues
    {
        private static IMonitor Monitor;
        private static IModHelper Helper;
        private static ModConfig Config;

        // call this method from your Entry class
        public static void Initialize(IModHelper helper, IMonitor monitor, ModConfig config)
        {
            Monitor = monitor;
            Helper = helper;
            Config = config;
        }
        internal static async void OpenCatalogue(string id)
        {
            await Task.Delay(100);
            Monitor.Log("Really opening catalogue");
            Utility.TryOpenShopMenu(id, null, false);
        }

        /*

        private static Dictionary<ISalable, int[]> GetAllWallpapersAndFloors()
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

        private static Dictionary<ISalable, int[]> GetAllFurnitures()
        {
            Dictionary<ISalable, int[]> decors = new Dictionary<ISalable, int[]>();
            Furniture f;
            foreach (KeyValuePair<int, string> v in Game1.content.Load<Dictionary<int, string>>("Data\\Furniture"))
            {
                if(v.Value.Split('/')[1] == "fishtank")
                    f = new FishTankFurniture(v.Key, Vector2.Zero);
                else if(v.Value.Split('/')[1] == "bed")
                    f = new BedFurniture(v.Key, Vector2.Zero);
                else if(v.Value.Split('/')[0].EndsWith("TV"))
                    f = new TV(v.Key, Vector2.Zero);
                else
                    f = new Furniture(v.Key, Vector2.Zero);
                decors.Add(f, new int[]
                {
                    Config.FreeFurnitureCatalogue ? 0 : (int)Math.Round(f.salePrice() * Config.PriceMult),
                    int.MaxValue
                });
            }
            return decors;
        }

        private static Dictionary<ISalable, int[]> GetAllSeeds()
        {
            Dictionary<ISalable, int[]> items = new Dictionary<ISalable, int[]>();
            Dictionary<int, string> cropData = Helper.GameContent.Load<Dictionary<int, string>>("Data\\Crops");
            Dictionary<int, string> fruitTreeData = Helper.GameContent.Load<Dictionary<int, string>>("Data\\fruitTrees");

            Dictionary<int, int> seedProducts = new Dictionary<int, int>();

            foreach (KeyValuePair<int, string> kvp in cropData)
            {
                string[] values = kvp.Value.Split('/');
                if (!int.TryParse(values[3], out int product))
                    continue;
                seedProducts.Add(kvp.Key, product);
            }
            foreach (KeyValuePair<int, string> kvp in fruitTreeData)
            {
                string[] values = kvp.Value.Split('/');
                if (!int.TryParse(values[2], out int product))
                    continue;
                seedProducts.Add(kvp.Key, product);
            }

            foreach (KeyValuePair<int, int> crop in seedProducts)
            {
                bool include = true;
                if(Config.SeedsToInclude.ToLower() == "shipped")
                {
                    include = Game1.player.basicShipped.ContainsKey(crop.Value);
                }
                else if (Config.SeedsToInclude.ToLower() == "season")
                {
                    include = new Crop(crop.Key, 0, 0, null).seasonsToGrowIn.Contains(Game1.currentSeason);
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

        private static Dictionary<ISalable, int[]> GetAllClothing()
        {
            Dictionary<ISalable, int[]> stock = new Dictionary<ISalable, int[]>();
            foreach (KeyValuePair<int, string> v in Game1.content.Load<Dictionary<int, string>>("Data\\ClothingInformation"))
            {
                Clothing c = new Clothing(v.Key);
                stock.Add(c, new int[]
                {
                    Config.FreeClothingCatalogue ? 0 : (int)Math.Round(c.salePrice() * Config.PriceMult),
                    int.MaxValue
                });
            }
            return stock;
        }

        private static void AdjustPrices(ref Dictionary<ISalable, int[]> dict, bool free)
        {
            ISalable[] keys = dict.Keys.ToArray();
            for (int i = 0; i < keys.Length; i++)
            {
                dict[keys[i]][0] = free ? 0 : (int)Math.Round(dict[keys[i]][0] * Config.PriceMult);
            }
        }
        public static bool boughtTraderItem(ISalable s, Farmer arg2, int arg3)
        {
            if (s.Name == "Magic Rock Candy")
            {
                Desert.boughtMagicRockCandy = true;
            }
            return false;
        }
        */
    }
}
