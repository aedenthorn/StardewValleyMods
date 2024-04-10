using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.GameData.Crops;
using StardewValley.GameData.Machines;
using StardewValley.GameData.Objects;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Menus;
using StardewValley.Objects;
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

            Item output = GetMachineOutput(crop, "(BC)12"); // (BC)12 is keg

            if (output is null)
                return null;

            Object outputObject = ItemRegistry.Create<Object>(output.QualifiedItemId, 1, quality);

            if (outputObject.preserve.Value is Object.PreserveType type) // preserve.Value seems to never be set.
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

        public static ObjectInfo GetDehydrator(Object crop, int quality)
        {
            if (crop is null)
                return null;

            Item output = GetMachineOutput(crop, "(BC)Dehydrator"); // (BC)Dehydrator is keg

            if (output is null)
                return null;

            Object outputObject = ItemRegistry.Create<Object>(output.QualifiedItemId, 1, quality);

            if (outputObject.preserve.Value is Object.PreserveType type) // preserve.Value seems to never be set.
            {
                outputObject = preserveItemFactory.CreateFlavoredItem(type, crop);
                outputObject.Quality = quality;
            }
            else if (outputObject.ItemId == "DriedFruit") // Dried fruit
            {
                outputObject = preserveItemFactory.CreateFlavoredDriedFruit(crop);
                outputObject.Quality = quality;
            }
            else if (outputObject.ItemId == "DriedMushrooms") // Dried mushrooms
            {
                outputObject = preserveItemFactory.CreateFlavoredDriedMushroom(crop);
                outputObject.Quality = quality;
            }

            SMonitor.Log($"{crop.DisplayName} dehydrates to {outputObject?.DisplayName} with price {outputObject.salePrice()}");

            return new ObjectInfo(outputObject);
        }

        public static Item GetMachineOutput(Object input, string machineQualifiedId)
        {
            Dictionary<string, MachineData> machineData = DataLoader.Machines(Game1.content);
            if(!machineData.TryGetValue(machineQualifiedId, out MachineData data))
            {
                SMonitor.Log("No data for " + machineQualifiedId);
                return null;
            }

            Object machine = ItemRegistry.Create<Object>(machineQualifiedId);
            Item inputItem = ItemRegistry.Create<Item>(input.QualifiedItemId);

            MachineOutputRule rule;

            if (!MachineDataUtility.TryGetMachineOutputRule(machine, data, MachineOutputTrigger.ItemPlacedInMachine, inputItem, Game1.player, Game1.getFarm(), out MachineOutputRule baseRule, out MachineOutputTriggerRule triggerRule, out MachineOutputRule ruleIgnoringCount, out MachineOutputTriggerRule triggerIgnoringCount))
            {
                if(ruleIgnoringCount != null) // ruleIgnoringCount is a rule that would have applied except that there weren't enough items.
                {
                    inputItem.Stack = triggerIgnoringCount.RequiredCount;
                    rule = ruleIgnoringCount;
                }
                else
                {
                    return null;
                }
            }else
            {
                rule = baseRule;
            }

            MachineItemOutput mio = MachineDataUtility.GetOutputData(machine, data, rule, inputItem, Game1.player, Game1.getFarm());

            if (mio == null)
                return null;

            return MachineDataUtility.GetOutputItem(machine, mio, inputItem, Game1.player, true, out int? _);
        }

        // Returns "" if it needs no fertilizer and null if no fertilizer could save it.
        public static string NeedFertilizer(Object seed)
        {
            if (!tryGetCropData(seed.ItemId, out CropData data))
                return "0";

            Crop crop = new Crop(seed.ItemId, 0, 0, Game1.getFarm());            

            // For compativility with the i18n, needs to return 0 when there is enough time and -1 when there is not enough time no matter what
            if (data.Seasons.Contains(seasons[(Utility.getSeasonNumber(Game1.currentSeason) + 1) % 4]))
                return "0";
            if (HasEnoughDaysLeft(crop, null))
                return "0";
            if (HasEnoughDaysLeft(crop, HoeDirt.speedGroQID))
                return HoeDirt.speedGroID;
            if (HasEnoughDaysLeft(crop, HoeDirt.superSpeedGroQID))
                return HoeDirt.superSpeedGroID;
            if (HasEnoughDaysLeft(crop, HoeDirt.hyperSpeedGroQID))
                return HoeDirt.hyperSpeedGroID;
            return "-1";
        }

        public static bool HasEnoughDaysLeft(Crop c, string fertilizer)
        {
            HoeDirt d = new HoeDirt(1, c);
            d.Location = Game1.getFarm();
            d.Tile = new Vector2(0, 0);
            
            d.fertilizer.Value = fertilizer;

            d.applySpeedIncreases(Game1.player);
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
                var pos = shopMenu.forSaleButtons[i].bounds.Location.ToVector2() + new Vector2(64, 64);
                if (info.needFertilizer == "0") // Needs no fertilizer
                {
                    b.Draw(Game1.mouseCursors, pos, new Rectangle(145, 273, 32, 32), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1);
                }
                else if (info.needFertilizer == "-1") // Too late no matter what
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
                    
                    int num = 0;
                    bool showCrop = (info.info[j].crop is not null) && Config.DisplayCrop;
                    bool showPickle = (info.info[j].pickle is not null) && Config.DisplayPickle;
                    bool showKeg = (info.info[j].keg is not null) && Config.DisplayKeg;
                    bool showDehydrate = (info.info[j].dehydrate is not null) && Config.DisplayDehydrator;

                    if (showCrop)
                        num++;
                    if (showPickle)
                        num++;
                    if (showKeg)
                        num++;
                    if (showDehydrate)
                        num++;

                    if (num == 0)
                        continue;

                    int current = 0;

                    float scale = num <= 3 ? 2f : 1.5f;

                    if (showCrop)
                    {
                        pos = GetIconPosition(shopMenu, i, current++, scale);
                        if (DrawInfo(mousePos, b, pos, j, info.info[j].crop, quality, qr, scale))
                        {
                            hoverObj = info.info[j].crop;
                        }
                    }
                    if (showPickle)
                    {
                        pos = GetIconPosition(shopMenu, i, current++, scale);
                        if (DrawInfo(mousePos, b, pos, j, info.info[j].pickle, quality, qr, scale))
                        {
                            hoverObj = info.info[j].pickle;
                        }
                    }
                    if (showKeg)
                    {
                        pos = GetIconPosition(shopMenu, i, current++, scale);
                        if (DrawInfo(mousePos, b, pos, j, info.info[j].keg, quality, qr, scale))
                        {
                            hoverObj = info.info[j].keg;
                        }
                    }
                    if (showDehydrate)
                    {
                        pos = GetIconPosition(shopMenu, i, current++, scale);
                        if (DrawInfo(mousePos, b, pos, j, info.info[j].dehydrate, quality, qr, scale))
                        {
                            hoverObj = info.info[j].dehydrate;
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

        private static Vector2 GetIconPosition(ShopMenu menu, int i, int which, float scale)
        {
            Vector2 offset = new Vector2(512, (12 + 28 * which) * scale/2);
            return menu.forSaleButtons[i].bounds.Location.ToVector2() + offset;
        }

        private static Rectangle GetIconRectangle(ShopMenu menu, int i, int which, int totalIcons, int qualityNumber)
        {
            if(totalIcons < 4)
            {
                return new Rectangle(menu.forSaleButtons[i].bounds.Location.X + 512 + (96 * qualityNumber), menu.forSaleButtons[i].bounds.Location.Y + 12 + 28 * which, 28, 28);
            }
            else
            {
                return new Rectangle(menu.forSaleButtons[i].bounds.Location.X + 512 + (96 * qualityNumber), menu.forSaleButtons[i].bounds.Location.Y + 8 + 14 * which, 3, 3);
            }
        }

        private static bool DrawInfo(Point mousePos, SpriteBatch b, Vector2 pos, int j, ObjectInfo info, int quality, Rectangle qr, float scale)
        {
            ParsedItemData data = ItemRegistry.GetDataOrErrorItem(info.obj.ItemId);

            if (data.IsErrorItem)
            {
                SMonitor.Log($"{info.name} is an error item!");
                return false;
            }

            Texture2D texture = data.GetTexture();

            b.Draw(texture, pos + new Vector2(j * 96, 0), data.GetSourceRect(0, info.obj.ParentSheetIndex), Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 1);

            if (info.obj is ColoredObject coloredObj)
            {
                Rectangle coloredSourceRect = data.GetSourceRect(1, coloredObj.ParentSheetIndex);
                b.Draw(texture, pos + new Vector2(j * 96, 0), coloredSourceRect, coloredObj.color.Value, 0f, Vector2.Zero, scale, SpriteEffects.None, 1);
            }

            if (quality > 0)
            {
                b.Draw(Game1.mouseCursors, pos + new Vector2(j*96, 16), qr, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 1);
            }
            b.DrawString(Game1.smallFont, Utility.getNumberWithCommas(info.price), pos + new Vector2(32 + 96 * j, 4), Config.PriceColor, 0f, Vector2.Zero, scale/2f, SpriteEffects.None, 1);
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