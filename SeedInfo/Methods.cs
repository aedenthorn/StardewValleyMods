using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using Object = StardewValley.Object;

namespace SeedInfo
{
    public partial class ModEntry
    {
        public static string[] seasons = new string[] { "spring", "summer", "fall", "winter" };

        public static ObjectInfo GetCrop(Object seed, int quality)
        {
            Dictionary<int, string> cropData = Game1.content.Load<Dictionary<int, string>>("Data\\Crops");
            if (!cropData.TryGetValue(seed.ParentSheetIndex, out string data))
                return null;
            try
            {
                int index = int.Parse(data.Split('/')[3]);
                var obj = new Object(index, 1, false, -1, quality);
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
            if(crop.Category == Object.flowersCategory)
            {
                int honeyPriceAddition = Convert.ToInt32(Game1.objectInformation[crop.ParentSheetIndex].Split('/', StringSplitOptions.None)[1]) * 2;
                
                obj = new Object(Vector2.Zero, 340, crop.Name + " Honey", false, true, false, false)
                {
                    Price = Convert.ToInt32(Game1.objectInformation[340].Split('/', StringSplitOptions.None)[1]) + honeyPriceAddition,
                    Quality = quality,
                };
                obj.preservedParentSheetIndex.Value = crop.ParentSheetIndex;
            }
            else if (crop.Category == Object.FruitsCategory)
            {
                obj = new Object(Vector2.Zero, 344, crop.Name + " Jelly", false, true, false, false)
                {
                    Price = 50 + crop.Price * 2,
                    Quality = quality
                };
                obj.preserve.Value = new Object.PreserveType?(Object.PreserveType.Jelly);
                obj.preservedParentSheetIndex.Value = crop.ParentSheetIndex;
            }
            else
            {
                obj = new Object(Vector2.Zero, 342, "Pickled " + crop.Name, false, true, false, false)
                {
                    Price = 50 + crop.Price * 2,
                    Quality = quality
                };
                obj.preserve.Value = new Object.PreserveType?(Object.PreserveType.Pickle);
                obj.preservedParentSheetIndex.Value = crop.ParentSheetIndex;
            }
            return new ObjectInfo(obj);
        }

        public static ObjectInfo GetKeg(Object crop, int quality)
        {
            if (crop is null)
                return null;

            Object obj = null;

            switch (crop.ParentSheetIndex)
            {
                case 262:
                    obj = new Object(346, 1, false, -1, quality);
                    break;
                case 304:
                    obj = new Object(303, 1, false, -1, quality);
                    break;
                case 433:
                    obj = new Object(395, 1, false, -1, quality);
                    break;
                case 815:
                    obj = new Object(614, 1, false, -1, quality);
                    break;
            }
            if(obj is null)
            {
                switch (crop.Category)
                {
                    case -80:
                        obj = new Object(459, 1, false, -1, quality);
                        break;
                    case -79:
                        obj = new Object(Vector2.Zero, 348, crop.Name + " Wine", false, true, false, false)
                        {
                            Price = crop.Price * 3,
                            Quality = quality
                        };
                        obj.preserve.Value = new Object.PreserveType?(Object.PreserveType.Wine);
                        obj.preservedParentSheetIndex.Value = crop.ParentSheetIndex;
                        break;
                    case -75:
                        obj = new Object(Vector2.Zero, 350, crop.Name + " Juice", false, true, false, false)
                        {
                            Price = (int)((double)crop.Price * 2.25),
                            Quality = quality
                        };
                        obj.preserve.Value = new Object.PreserveType?(Object.PreserveType.Juice);
                        obj.preservedParentSheetIndex.Value = crop.ParentSheetIndex;
                        break;
                }
            }
            if (obj is null)
                return null;
            return new ObjectInfo(obj);
        }

        public static int NeedFertilizer(Object seed)
        {
            Crop c = new Crop(seed.ParentSheetIndex, 0, 0);

            if (c.seasonsToGrowIn.Contains(seasons[(Utility.getSeasonNumber(Game1.currentSeason) + 1) % 4]))
                return 0;
            if (HasEnoughDaysLeft(c, 0))
                return 0;
            if(HasEnoughDaysLeft(c, 465))
                return 465;
            if(HasEnoughDaysLeft(c, 466))
                return 466;
            if(HasEnoughDaysLeft(c, 918))
                return 918;
            return -1;
        }

        public static bool HasEnoughDaysLeft(Crop c, int fertilizer)
        {
            HoeDirt d = new HoeDirt(1, c);
            d.currentLocation = Game1.getFarm();
            d.currentTileLocation = new Vector2(0,0);
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
                if (shopMenu.forSale[shopMenu.currentItemIndex + i] != shopMenu.hoveredItem || shopMenu.forSale[shopMenu.currentItemIndex + i] is not Object || !shopDict.TryGetValue(((Object)shopMenu.forSale[shopMenu.currentItemIndex + i]).ParentSheetIndex, out var info))
                    continue;
                var mousePos = Game1.getMousePosition();
                var pos = GetIconPosition(shopMenu, i, 0);
                if (info.needFertilizer == 0)
                {
                    b.Draw(Game1.mouseCursors, pos, new Rectangle(145, 273, 32, 32), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1);
                }
                else if (info.needFertilizer < 0)
                {
                    b.Draw(Game1.mouseCursors, pos, new Rectangle(209, 273, 32, 32), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1);
                }
                else
                {
                    b.Draw(Game1.objectSpriteSheet, pos, Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, info.needFertilizer, 16, 16), Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 1);
                }
                if(new Rectangle((int)pos.X, (int)pos.Y, 32, 32).Contains(mousePos))
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
                    IClickableMenu.drawToolTip(b, hoverObj.desc, hoverObj.name, hoverObj.obj, false, -1, shopMenu.currency, -1, -1, null, hoverObj.price);
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
            b.Draw(Game1.objectSpriteSheet, pos + new Vector2(j * 96, 0), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, info.obj.ParentSheetIndex, 16, 16), Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 1);
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
    }
}