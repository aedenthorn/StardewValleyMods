using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.GameData.Crops;
using StardewValley.GameData.Machines;
using StardewValley.GameData.Objects;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Menus;
using StardewValley.TerrainFeatures;
using System.Collections.Generic;
using Object = StardewValley.Object;

namespace SeedInfo
{
    public partial class ModEntry
    {
        public static Season[] seasons = new Season[] {Season.Spring, Season.Summer, Season.Fall, Season.Winter};

        public static ObjectDataDefinition preserveItemFactory = new ObjectDataDefinition();

        public static ObjectInfo GetCrop(Object seed, int quality)
        {
            if (!tryGetCropData(seed.ItemId, out CropData data))
                return null;
            try
            {
                Object obj = ItemRegistry.Create<Object>("(O)" + data.HarvestItemId, 1, quality, false); //  Add unique identifier because it seems to prefer BigCraftables when casting to Object (and crops aren't BigCraftables)
                //SMonitor.Log($"{seed.DisplayName}'s crop is {obj.DisplayName}.");
                return new ObjectInfo(obj);
            }
            catch
            {
            }
            return null;
        }

        public static ObjectInfo GetPickle(Object crop, int quality)
        {
            if (crop is null)
                return null;
            Object obj;
            if (crop.Category == Object.flowersCategory)
            {
                obj = preserveItemFactory.CreateFlavoredHoney(crop);
            }
            else if (crop.Category == Object.FruitsCategory)
            {
                obj = preserveItemFactory.CreateFlavoredJelly(crop);
            }
            else
            {
                obj = preserveItemFactory.CreateFlavoredPickle(crop);
            }
            obj.Quality = quality;
            //SMonitor.Log($"{crop.DisplayName} pickles to {obj?.DisplayName}");
            return new ObjectInfo(obj);
        }

        public static ObjectInfo GetKeg(Object crop, int quality)
        {
            if (crop is null)
                return null;

            if(crop.Category == Object.flowersCategory && Config.DisplayMead)
            {
                Object pickle = GetPickle(crop, quality).obj;
                if(pickle.ItemId == "340") // If it is honey
                {
                    return GetKeg(pickle, quality);
                }
            }

            Dictionary<string, MachineData> machineData = DataLoader.Machines(Game1.content);
            MachineData kegData = machineData["(BC)12"]; // Keg
            Object machine = ItemRegistry.Create<Object>("(BC)12");
            Item cropItem = ItemRegistry.Create<Item>(crop.QualifiedItemId);

            if (!MachineDataUtility.TryGetMachineOutputRule(machine, kegData, MachineOutputTrigger.ItemPlacedInMachine, cropItem, Game1.player, Game1.getFarm(), out MachineOutputRule rule, out MachineOutputTriggerRule triggerRule, out MachineOutputRule ruleIgnoringCount, out MachineOutputTriggerRule triggerIgnoringCount))
                return null;

            MachineItemOutput mio = MachineDataUtility.GetOutputData(machine, kegData, rule, cropItem, Game1.player, Game1.getFarm());

            if(mio == null)
                return null;

            Item output = MachineDataUtility.GetOutputItem(machine, mio, cropItem, Game1.player, true, out int? _);

            Object outputObject = ItemRegistry.Create<Object>(output.QualifiedItemId, 1, quality);

            if (outputObject.preserve.Value is Object.PreserveType type)
            {
                //SMonitor.Log($"Using the factory for {outputObject.DisplayName} with price {outputObject.salePrice()}");
                outputObject = preserveItemFactory.CreateFlavoredItem(type, crop);
                outputObject.Quality = quality;
            }else if(outputObject.ItemId == "350") // Juice
            {
                //SMonitor.Log($"Using the factory for {outputObject.DisplayName} with price {outputObject.salePrice()}");
                outputObject = preserveItemFactory.CreateFlavoredJuice(crop);
                outputObject.Quality = quality;
            }else if(outputObject.ItemId == "348") // Wine
            {
                //SMonitor.Log($"Using the factory for {outputObject.DisplayName} with price {outputObject.salePrice()}");
                outputObject = preserveItemFactory.CreateFlavoredWine(crop);
                outputObject.Quality = quality;
            }

            //SMonitor.Log($"{crop.DisplayName} kegs to {outputObject?.DisplayName} with price {outputObject.salePrice()}");

            return new ObjectInfo(outputObject);
        }

        // Returns "" if it needs no fertilizer and null if no fertilizer could save it.
        public static string NeedFertilizer(Object seed)
        {
            if (!tryGetCropData(seed.ItemId, out CropData data))
                return "";

            Crop crop = new Crop(seed.ItemId, 0, 0, Game1.getFarm());            

            if (data.Seasons.Contains(seasons[(Utility.getSeasonNumber(Game1.currentSeason) + 1) % 4]))
                return "";
            if (HasEnoughDaysLeft(crop, null))
                return "";
            if (HasEnoughDaysLeft(crop, "(O)465"))
                return "465";
            if (HasEnoughDaysLeft(crop, "(O)466"))
                return "466";
            if (HasEnoughDaysLeft(crop, "(O)918"))
                return "918";
            return null;
        }

        public static bool HasEnoughDaysLeft(Crop c, string fertilizer)
        {
            HoeDirt d = new HoeDirt(1, c);
            d.Location = Game1.getFarm();
            d.Tile = new Vector2(0, 0);
            
            d.fertilizer.Value = fertilizer;
            AccessTools.Method(typeof(HoeDirt), "applySpeedIncreases").Invoke(d, new object[] { Game1.player });
            c = d.crop;

            int days = 0;
            for (int i = 0; i < c.phaseDays.Count - 1; i++)
            {
                days += c.phaseDays[i];
            }
            return Config.DaysPerMonth - Game1.dayOfMonth >= days;

        }

        public static void DrawAllInfo(SpriteBatch b)
        {
            if (!Config.ModEnabled || Game1.activeClickableMenu is not ShopMenu)
                return;

            ShopMenu shopMenu = (ShopMenu)Game1.activeClickableMenu;

            for (int i = 0; i < shopMenu.forSaleButtons.Count; i++)
            {
                if (shopMenu.currentItemIndex + i > shopMenu.forSale.Count - 1)
                    break;
                if (shopMenu.forSale[shopMenu.currentItemIndex + i] != shopMenu.hoveredItem || shopMenu.forSale[shopMenu.currentItemIndex + i] is not Object || !shopDict.TryGetValue(((Object)shopMenu.forSale[shopMenu.currentItemIndex + i]).QualifiedItemId, out var info))
                    continue;
                var mousePos = Game1.getMousePosition();
                var pos = GetIconPosition(shopMenu, i, 0);
                if (info.needFertilizer == "") // Needs no fertilizer
                {
                    b.Draw(Game1.mouseCursors, pos, new Rectangle(145, 273, 32, 32), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1);
                }
                else if (info.needFertilizer == null) // Too late no matter what
                {
                    b.Draw(Game1.mouseCursors, pos, new Rectangle(209, 273, 32, 32), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1);
                }
                else
                {
                    b.Draw(Game1.objectSpriteSheet, pos, Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, Game1.objectData[info.needFertilizer].SpriteIndex, 16, 16), Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 1);
                }
                if (new Rectangle((int)pos.X, (int)pos.Y, 32, 32).Contains(mousePos))
                {
                    AccessTools.FieldRefAccess<ShopMenu, string>(shopMenu, "hoverText") = "";
                    IClickableMenu.drawToolTip(b, SHelper.Translation.Get("need-fertilizer-" + info.needFertilizer), null, null);
                }
                ObjectInfo hoverObj = null;

                for (int j = 0; j < 4; j++)
                {
                    if (info.info.Count < j)
                        break;
                    var quality = qualities[j];
                    Rectangle qr = quality < 4 ? new Rectangle(338 + (quality - 1) * 8, 400, 8, 8) : new Rectangle(346, 392, 8, 8);

                    Vector2 offset = Vector2.Zero;
                    int num = 0;
                    if (info.info[j].crop is not null)
                        num++;
                    if (info.info[j].pickle is not null)
                        num++;
                    if (info.info[j].keg is not null)
                        num++;
                    if (num == 0)
                        continue;
                    if (num < 2)
                        offset = new Vector2(0, 28);
                    else if (num < 2)
                        offset = new Vector2(0, 14);
                    if (info.info[j].crop is not null)
                    {
                        pos = GetIconPosition(shopMenu, i, 1) + offset;
                        if (DrawInfo(mousePos, b, pos, j, info.info[j].crop, quality, qr))
                        {
                            hoverObj = info.info[j].crop;
                        }
                    }
                    if (info.info[j].pickle is not null)
                    {
                        pos = GetIconPosition(shopMenu, i, 2) + offset;
                        if (DrawInfo(mousePos, b, pos, j, info.info[j].pickle, quality, qr))
                        {
                            hoverObj = info.info[j].pickle;
                        }
                    }
                    if (info.info[j].keg is not null)
                    {
                        pos = GetIconPosition(shopMenu, i, 3) + offset;
                        if (DrawInfo(mousePos, b, pos, j, info.info[j].keg, quality, qr))
                        {
                            hoverObj = info.info[j].keg;
                        }
                    }
                }
                if (hoverObj is not null)
                {
                    AccessTools.FieldRefAccess<ShopMenu, string>(shopMenu, "hoverText") = "";
                    IClickableMenu.drawToolTip(b, hoverObj.desc, hoverObj.name, hoverObj.obj, false, -1, shopMenu.currency, null, -1, null, hoverObj.price);
                }
            }
        }

        private static Vector2 GetIconPosition(ShopMenu menu, int i, int which)
        {
            Vector2 offset = new Vector2(64, 64);
            if (which > 0)
                offset = new Vector2(512, 12);
            if (which > 1)
                offset = new Vector2(512, 40);
            if (which > 2)
                offset = new Vector2(512, 68);
            return menu.forSaleButtons[i].bounds.Location.ToVector2() + offset;
        }

        private static bool DrawInfo(Point mousePos, SpriteBatch b, Vector2 pos, int j, ObjectInfo info, int quality, Rectangle qr)
        {
            ObjectData data = Game1.objectData[info.obj.ItemId];
            Texture2D texture;
            if (data != null)
            {
                string textureSheet = data.Texture;
                texture = Game1.content.Load<Texture2D>(textureSheet == null ? "Maps/springobjects" : textureSheet);
            }
            else
            {
                texture = Game1.objectSpriteSheet;
            }
            

            b.Draw(texture, pos + new Vector2(j * 96, 0), Game1.getSourceRectForStandardTileSheet(texture, info.obj.ParentSheetIndex, 16, 16), Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 1);
            if (quality > 0)
            {
                b.Draw(Game1.mouseCursors, pos + new Vector2(j * 96, 16), qr, Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 1);
            }
            b.DrawString(Game1.smallFont, Utility.getNumberWithCommas(info.price), pos + new Vector2(32 + 96 * j, 4), Config.PriceColor);
            if (new Rectangle(Utility.Vector2ToPoint(pos + new Vector2(j * 96, 0)), new Point(96, 28)).Contains(mousePos))
            {
                return true;
            }
            return false;
        }

        public static bool tryGetCropData(string seedId, out CropData data)
        {
            Dictionary<string, CropData> cropData = Game1.content.Load<Dictionary<string, CropData>>("Data\\Crops");
            if(!cropData.TryGetValue(seedId, out data))
            {
                return false;
            }
            return true;
        }
    }
}