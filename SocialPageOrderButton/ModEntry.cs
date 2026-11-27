using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace SocialPageOrderMenu
{
    public class ModEntry : Mod
    {
        public static ModConfig Config;
        public static Dictionary<string, object> outdoorAreas = new Dictionary<string, object>();
        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static int xOffset = 16;
        public static MyOptionsDropDown dropDown;
        public static bool wasSorted = false;


        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();
            if (!Config.EnableMod)
                return;
            SMonitor = Monitor;
            SHelper = Helper;

            helper.Events.Input.ButtonPressed += Input_ButtonPressed;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }

        private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Config.EnableMod || Game1.activeClickableMenu is not GameMenu || (Game1.activeClickableMenu as GameMenu).GetCurrentPage() is not SocialPage)
                return;
            if (e.Button == Config.prevButton)
            {
                int sort = Config.CurrentSort;
                sort--;
                if (sort < 0)
                    sort = 3;
                Config.CurrentSort = sort;
            }
            else if (e.Button == Config.nextButton)
            {
                int sort = Config.CurrentSort;
                sort++;
                sort %= 4;
                Config.CurrentSort = sort;
            }
            else
                return;
            dropDown.selectedOption = Config.CurrentSort;
            Helper.WriteConfig(Config);
            ResortSocialList();
        }

        [HarmonyPatch(typeof(SocialPage), new Type[] { typeof(int), typeof(int), typeof(int), typeof(int) })]
        [HarmonyPatch(MethodType.Constructor)]
        public class SocialPage_Patch
        {
            public static void Postfix(int x, int y, int width, int height)
            {
                if (!Config.EnableMod)
                    return;
                dropDown = new MyOptionsDropDown("", 0);
                for(int i = 0; i < 4; i++)
                {
                    dropDown.dropDownDisplayOptions.Add(SHelper.Translation.Get($"sort-{i}"));
                    dropDown.dropDownOptions.Add(SHelper.Translation.Get($"sort-{i}"));
                }
                dropDown.RecalculateBounds();
                dropDown.selectedOption = Config.CurrentSort;
                wasSorted = false;
            }
        }
        [HarmonyPatch(typeof(SocialPage), nameof(SocialPage.draw), new Type[] { typeof(SpriteBatch) })]
        public class SocialPage_drawTextureBox_Patch
        {
            public static void Prefix(SocialPage __instance, SpriteBatch b)
            {
                if (!Config.EnableMod)
                    return;
                if (!wasSorted)
                {
                    wasSorted = true;
                    ResortSocialList();
                }
            }
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log("Transpiling SocialPage.draw");
                var codes = new List<CodeInstruction>(instructions);
                int index = codes.FindLastIndex(ci => ci.opcode == OpCodes.Call && (MethodInfo)ci.operand == AccessTools.Method(typeof(IClickableMenu), nameof(IClickableMenu.drawTextureBox), new Type[] { typeof(SpriteBatch), typeof(Texture2D), typeof(Rectangle), typeof(int), typeof(int), typeof(int), typeof(int), typeof(Color), typeof(float), typeof(bool), typeof(float) }));
                if(index > -1)
                {
                    SMonitor.Log("Inserting dropdown draw method");
                    codes.Insert(index + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(SocialPage_drawTextureBox_Patch), nameof(DrawDropDown))));
                    codes.Insert(index + 1, new CodeInstruction(OpCodes.Ldarg_1));
                    codes.Insert(index + 1, new CodeInstruction(OpCodes.Ldarg_0));

                }
                return codes.AsEnumerable();
            }
            public static void DrawDropDown(SocialPage page, SpriteBatch b)
            {
                if (!Config.EnableMod)
                    return;
                dropDown.draw(b, page.xPositionOnScreen + page.width / 2 - dropDown.bounds.Width / 2, page.yPositionOnScreen + page.height);
                if (SHelper.Input.IsDown(SButton.MouseLeft) && AccessTools.FieldRefAccess<OptionsDropDown, bool>(dropDown, "clicked") && dropDown.dropDownBounds.Contains(Game1.getMouseX() - (page.xPositionOnScreen + page.width / 2 - dropDown.bounds.Width / 2), Game1.getMouseY() - (page.yPositionOnScreen + page.height)))
                {
                    dropDown.selectedOption = (int)Math.Max(Math.Min((float)(Game1.getMouseY() - page.yPositionOnScreen - page.height - dropDown.dropDownBounds.Y) / (float)dropDown.bounds.Height, (float)(dropDown.dropDownOptions.Count - 1)), 0f);
                }
            }
        }
        [HarmonyPatch(typeof(SocialPage), nameof(SocialPage.receiveLeftClick))]
        public class SocialPage_receiveLeftClick_Patch
        {
            public static bool Prefix(SocialPage __instance, int x, int y)
            {
                if (!Config.EnableMod)
                    return true;
                if (dropDown.bounds.Contains(x - (__instance.xPositionOnScreen + __instance.width / 2 - dropDown.bounds.Width / 2), y - __instance.yPositionOnScreen - __instance.height))
                {
                    dropDown.receiveLeftClick(x - (__instance.xPositionOnScreen + __instance.width / 2 - dropDown.bounds.Width / 2), y - __instance.yPositionOnScreen - __instance.height);
                    return false;
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(SocialPage), nameof(SocialPage.releaseLeftClick))]
        public class SocialPage_releaseLeftClick_Patch
        {
            public static bool Prefix(SocialPage __instance, int x, int y)
            {
                if (!Config.EnableMod)
                    return true;
                if (AccessTools.FieldRefAccess<OptionsDropDown, bool>(dropDown, "clicked"))
                {
                    if(dropDown.dropDownBounds.Contains(Game1.getMouseX() - (__instance.xPositionOnScreen + __instance.width / 2 - dropDown.bounds.Width / 2), Game1.getMouseY() - __instance.yPositionOnScreen - __instance.height))
                    {
                        dropDown.leftClickReleased(Game1.getMouseX() - (__instance.xPositionOnScreen + __instance.width / 2 - dropDown.bounds.Width / 2), Game1.getMouseY() - __instance.yPositionOnScreen - __instance.height);
                    }
                    return false;
                }
                return true;
            }
        }
        public static void ResortSocialList()
        {
            if (Game1.activeClickableMenu is GameMenu)
            {
                SocialPage page = (Game1.activeClickableMenu as GameMenu).pages[GameMenu.socialTab] as SocialPage;

                List<ClickableTextureComponent> sprites = new List<ClickableTextureComponent>(SHelper.Reflection.GetField<List<ClickableTextureComponent>>(page, "sprites").GetValue());
                List<NameSpriteSlot> nameSprites = new List<NameSpriteSlot>();
                for(int i = 0; i < page.names.Count; i++)
                {
                    nameSprites.Add(new NameSpriteSlot(page.names[i], sprites[i], page.characterSlots[i]));
                }
                switch (Config.CurrentSort)
                {
                    case 0: // friend asc
                        SMonitor.Log("sorting by friend asc");
                        nameSprites.Sort(delegate (NameSpriteSlot x, NameSpriteSlot y)
                        {
                            if (x.name is long && y.name is long) return 0;
                            else if (x.name is long)  return -1;
                            else if (y.name is long)  return 1;
                            int c = Game1.player.getFriendshipLevelForNPC(x.name as string).CompareTo(Game1.player.getFriendshipLevelForNPC(y.name as string));
                            if (c == 0)
                                c = GetNPCDisplayName(x.name as string).CompareTo(GetNPCDisplayName(y.name as string));
                            return c;

                        });
                        break;
                    case 1: // friend desc
                        SMonitor.Log("sorting by friend desc");
                        nameSprites.Sort(delegate (NameSpriteSlot x, NameSpriteSlot y)
                        {
                            if (x.name is long && y.name is long) return 0;
                            else if (x.name is long) return -1;
                            else if (y.name is long) return 1;
                            int c = -(Game1.player.getFriendshipLevelForNPC(x.name as string).CompareTo(Game1.player.getFriendshipLevelForNPC(y.name as string)));
                            if (c == 0)
                                c = GetNPCDisplayName(x.name as string).CompareTo(GetNPCDisplayName(y.name as string));
                            return c;

                        });
                        break;
                    case 2: // alpha asc
                        SMonitor.Log("sorting by alpha asc");
                        nameSprites.Sort(delegate (NameSpriteSlot x, NameSpriteSlot y)
                        {
                            return (x.name is long ? Game1.getFarmer((long)x.name).Name : GetNPCDisplayName(x.name as string)).CompareTo(y.name is long ? Game1.getFarmer((long)y.name).Name : GetNPCDisplayName(y.name as string));
                        });
                        break;
                    case 3: // alpha desc
                        SMonitor.Log("sorting by alpha desc");
                        nameSprites.Sort(delegate (NameSpriteSlot x, NameSpriteSlot y)
                        {
                            return -((x.name is long ? Game1.getFarmer((long)x.name).Name : GetNPCDisplayName(x.name as string)).CompareTo(y.name is long ? Game1.getFarmer((long)y.name).Name : GetNPCDisplayName(y.name as string)));
                        });
                        break;
                }
                for(int i = 0; i < nameSprites.Count; i++)
                {
                    ((Game1.activeClickableMenu as GameMenu).pages[GameMenu.socialTab] as SocialPage).names[i] = nameSprites[i].name;
                    sprites[i] = nameSprites[i].sprite;
                    ((Game1.activeClickableMenu as GameMenu).pages[GameMenu.socialTab] as SocialPage).characterSlots[i] = nameSprites[i].slot;
                }
                SHelper.Reflection.GetField<List<ClickableTextureComponent>>((Game1.activeClickableMenu as GameMenu).pages[GameMenu.socialTab], "sprites").SetValue(new List<ClickableTextureComponent>(sprites));

                int first_character_index = 0;
                for (int l = 0; l < page.names.Count; l++)
                {
                    if (!(((SocialPage)(Game1.activeClickableMenu as GameMenu).pages[GameMenu.socialTab]).names[l] is long))
                    {
                        first_character_index = l;
                        break;
                    }
                }
                SHelper.Reflection.GetField<int>((Game1.activeClickableMenu as GameMenu).pages[GameMenu.socialTab], "slotPosition").SetValue(first_character_index);
                SHelper.Reflection.GetMethod((Game1.activeClickableMenu as GameMenu).pages[GameMenu.socialTab], "setScrollBarToCurrentIndex").Invoke();
                ((SocialPage)(Game1.activeClickableMenu as GameMenu).pages[GameMenu.socialTab]).updateSlots();
            }
        }

        private static string GetNPCDisplayName(string name)
        {
            NPC n = Game1.getCharacterFromName(name);
            if (n != null)
                return n.displayName;
            return name;
        }
    }

    internal class NameSpriteSlot
    {
        public object name;
        public ClickableTextureComponent sprite;
        public ClickableTextureComponent slot;

        public NameSpriteSlot(object obj, ClickableTextureComponent clickableTextureComponent, ClickableTextureComponent slotComponent)
        {
            name = obj;
            sprite = clickableTextureComponent;
            slot = slotComponent;
        }
    }
}