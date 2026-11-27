using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Linq;
using Object = StardewValley.Object;

namespace PersisitentGrangeDisplay
{
    public class GrangePatches
    {

        public static void Object_draw_Postfix(Object __instance, SpriteBatch spriteBatch, float layerDepth)
        {
            if (!Game1.player.team.grangeDisplay.Any() || __instance.name != "Grange Display" || !__instance.modData.ContainsKey("spacechase0.BiggerCraftables/BiggerIndex") || __instance.modData["spacechase0.BiggerCraftables/BiggerIndex"] != "9")
                return;

            drawGrangeItems(__instance, spriteBatch, layerDepth);

        }

        public static void Object_draw_Postfix2(Object __instance, SpriteBatch spriteBatch, int x, int y)
        {
            if (!Game1.player.team.grangeDisplay.Any() || __instance.name != "Grange Display" || !__instance.modData.ContainsKey("spacechase0.BiggerCraftables/BiggerIndex") || __instance.modData["spacechase0.BiggerCraftables/BiggerIndex"] != "9")
                return;
            float drawLayer = Math.Max(0f, ((y + 1) * 64 - 24) / 10000f) + x * 1E-05f;

            drawGrangeItems(__instance, spriteBatch, drawLayer);

        }

        public static void drawGrangeItems(Object instance, SpriteBatch spriteBatch, float layerDepth)
        {
            Vector2 start = Game1.GlobalToLocal(Game1.viewport, instance.TileLocation * 64);


            if (ModEntry.Config.ShowCurrentScoreOnGrange)
            {
                int score = ModEntry.GetGrangeScore();
                spriteBatch.DrawString(Game1.smallFont, score.ToString(), new Vector2(start.X + 24 * 4 - score.ToString().Length * 8, start.Y + 51 * 4), ModEntry.GetPointsColor(score), 0f, Vector2.Zero, 1f, SpriteEffects.None, layerDepth + 0.0202f);
            }

            start.X += 4f;
            int xCutoff = (int)start.X + 168;
            start.Y += 8f;

            for (int j = 0; j < Game1.player.team.grangeDisplay.Count; j++)
            {
                if (Game1.player.team.grangeDisplay[j] != null)
                {
                    start.Y += 42f;
                    start.X += 4f;
                    spriteBatch.Draw(Game1.shadowTexture, start, new Microsoft.Xna.Framework.Rectangle?(Game1.shadowTexture.Bounds), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth + 0.02f);
                    start.Y -= 42f;
                    start.X -= 4f;
                    Game1.player.team.grangeDisplay[j].drawInMenu(spriteBatch, start, 1f, 1f, layerDepth + 0.0201f + j / 10000f, StackDrawType.Hide);
                }
                start.X += 60f;
                if (start.X >= xCutoff)
                {
                    start.X = xCutoff - 168;
                    start.Y += 64f;
                }
            }
        }

        public static bool Object_isPassable_Prefix(Object __instance, ref bool __result)
        {
            if (__instance.bigCraftable && __instance.Name == "Grange Display" && __instance.modData.ContainsKey("spacechase0.BiggerCraftables/BiggerIndex") && int.Parse(__instance.modData["spacechase0.BiggerCraftables/BiggerIndex"]) >= 9)
            {
                __result = true;
                return false;
            }
            return true;
        }

        public static void Object_checkForAction_Postfix(Object __instance, bool justCheckingForActivity, ref bool __result)
        {
            if (!__result && __instance.bigCraftable && __instance.bigCraftable && __instance.Name == "Grange Display" && !justCheckingForActivity)
            {
                Game1.player.team.grangeMutex.RequestLock(delegate
                {
                    while (Game1.player.team.grangeDisplay.Count < 9)
                    {
                        Game1.player.team.grangeDisplay.Add(null);
                    }
                    ModEntry.isGrangeMenu = true;
                    Game1.activeClickableMenu = new StorageContainer(Game1.player.team.grangeDisplay.ToList(), 9, 3, new StorageContainer.behaviorOnItemChange(onGrangeChange), new InventoryMenu.highlightThisItem(Utility.highlightSmallObjects));
                }, null);
                __result = true;
            }
        }

        public static void StorageContainer_draw_Postfix(StorageContainer __instance, SpriteBatch b)
        {
            if (ModEntry.isGrangeMenu && ModEntry.Config.ShowCurrentScoreInMenu)
            {
                int score = ModEntry.GetGrangeScore();
                b.DrawString(Game1.smallFont, score.ToString(), new Vector2(__instance.ItemsToGrabMenu.xPositionOnScreen + 24 * 4 - score.ToString().Length * 8, __instance.ItemsToGrabMenu.yPositionOnScreen + 48 * 4), ModEntry.GetPointsColor(score), 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.001f);
            }
        }
        
        public static void Game1_exitActiveMenu_Postfix()
        {
            ModEntry.isGrangeMenu = false;
        }

        public static void FarmerTeam_NewDay_Prefix(FarmerTeam __instance, ref NetCollection<Item> __state)
        {
            __state = new NetCollection<Item>(__instance.grangeDisplay);
            ModEntry.context.Monitor.Log($"new day Grange display items: {__instance.grangeDisplay.Count} {__state.Count}");
            __instance.grangeDisplay.Clear();
        }
        public static void FarmerTeam_NewDay_Postfix(FarmerTeam __instance, NetCollection<Item> __state)
        {
            __instance.grangeDisplay.Clear();
            foreach (Item item in __state)
            {
                __instance.grangeDisplay.Add(item);
            }
        }

        public static void addItemToGrangeDisplay(Item i, int position, bool force)
        {
            while (Game1.player.team.grangeDisplay.Count < 9)
            {
                Game1.player.team.grangeDisplay.Add(null);
            }
            if (position < 0 || position >= Game1.player.team.grangeDisplay.Count || (Game1.player.team.grangeDisplay[position] != null && !force))
            {
                return;
            }
            Game1.player.team.grangeDisplay[position] = i;
        }

        public static bool onGrangeChange(Item i, int position, Item old, StorageContainer container, bool onRemoval)
        {
            if (!onRemoval)
            {
                if (i.Stack > 1 || (i.Stack == 1 && old != null && old.Stack == 1 && i.canStackWith(old)))
                {
                    if (old != null && i != null && old.canStackWith(i))
                    {
                        container.ItemsToGrabMenu.actualInventory[position].Stack = 1;
                        container.heldItem = old;
                        return false;
                    }
                    if (old != null)
                    {
                        Utility.addItemToInventory(old, position, container.ItemsToGrabMenu.actualInventory, null);
                        container.heldItem = i;
                        return false;
                    }
                    int allButOne = i.Stack - 1;
                    Item reject = i.getOne();
                    reject.Stack = allButOne;
                    container.heldItem = reject;
                    i.Stack = 1;
                }
            }
            else if (old != null && old.Stack > 1 && !old.Equals(i))
            {
                return false;
            }
            addItemToGrangeDisplay((onRemoval && (old == null || old.Equals(i))) ? null : i, position, true);
            return true;
        }
    }
}