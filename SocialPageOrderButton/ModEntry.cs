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

namespace SocialPageOrderButton
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {
        public static ModConfig Config;
        public static Dictionary<string, object> outdoorAreas = new Dictionary<string, object>();
        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        private static Texture2D buttonTexture;
        private static int xOffset = 16;
        private bool wasMenuOpen = false;

        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();
            if (!Config.EnableMod)
                return;
            SMonitor = Monitor;
            SHelper = Helper;

            buttonTexture = helper.Content.Load<Texture2D>("assets/button.png");

            Helper.Events.Input.ButtonPressed += Input_ButtonPressed;
            Helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }

        private void GameLoop_UpdateTicked(object sender, UpdateTickedEventArgs e)
        {

            if (Game1.activeClickableMenu is GameMenu)
            {
                if (!wasMenuOpen)
                {
                    ResortSocialList();
                    wasMenuOpen = true;
                }
            }
            else
                wasMenuOpen = false;
        }
        private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (Game1.activeClickableMenu is GameMenu && (Game1.activeClickableMenu as GameMenu).currentTab == GameMenu.socialTab)
            {
                Rectangle rect = new Rectangle(Game1.activeClickableMenu.xPositionOnScreen - xOffset, Game1.activeClickableMenu.yPositionOnScreen, buttonTexture.Width * 4, buttonTexture.Height * 4);
                if (rect.Contains(Game1.getMousePosition()))
                {
                    Config.CurrentSort++;
                    Config.CurrentSort %= 4;
                    Helper.WriteConfig(Config);
                    ResortSocialList();
                }
            }
        }

        [HarmonyPatch(typeof(SocialPage), nameof(SocialPage.draw), new Type[] { typeof(SpriteBatch) })]
        public class IClickableMenu_drawTextureBox_Patch
        {
            public static void Prefix(SpriteBatch b)
            {
                if (!Config.EnableMod)
                    return;
                b.Draw(buttonTexture, new Rectangle(Game1.activeClickableMenu.xPositionOnScreen - xOffset, Game1.activeClickableMenu.yPositionOnScreen, buttonTexture.Width * 4, buttonTexture.Height * 4), null, Color.White);
                Rectangle rect = new Rectangle(Game1.activeClickableMenu.xPositionOnScreen - xOffset, Game1.activeClickableMenu.yPositionOnScreen, buttonTexture.Width * 4, buttonTexture.Height * 4);
                if (rect.Contains(Game1.getMousePosition()))
                {
                    (Game1.activeClickableMenu as GameMenu).hoverText = SHelper.Translation.Get($"sort-{Config.CurrentSort}");
                }
            }
        }
        private void ResortSocialList()
        {
            if (Game1.activeClickableMenu is GameMenu)
            {
                SocialPage page = (Game1.activeClickableMenu as GameMenu).pages[GameMenu.socialTab] as SocialPage;

                List<ClickableTextureComponent> sprites = new List<ClickableTextureComponent>(Helper.Reflection.GetField<List<ClickableTextureComponent>>(page, "sprites").GetValue());
                List<NameSpriteSlot> nameSprites = new List<NameSpriteSlot>();
                for(int i = 0; i < page.names.Count; i++)
                {
                    nameSprites.Add(new NameSpriteSlot(page.names[i], sprites[i], page.characterSlots[i]));
                }
                switch (Config.CurrentSort)
                {
                    case 0: // friend asc
                        Monitor.Log("sorting by friend asc");
                        nameSprites.Sort(delegate (NameSpriteSlot x, NameSpriteSlot y)
                        {
                            if (x.name is long && y.name is long) return 0;
                            else if (x.name is long)  return -1;
                            else if (y.name is long)  return 1;
                            return Game1.player.getFriendshipLevelForNPC(x.name as string).CompareTo(Game1.player.getFriendshipLevelForNPC(y.name as string));
                        });
                        break;
                    case 1: // friend desc
                        Monitor.Log("sorting by friend desc");
                        nameSprites.Sort(delegate (NameSpriteSlot x, NameSpriteSlot y)
                        {
                            if (x.name is long && y.name is long) return 0;
                            else if (x.name is long) return -1;
                            else if (y.name is long) return 1;
                            return -(Game1.player.getFriendshipLevelForNPC(x.name as string).CompareTo(Game1.player.getFriendshipLevelForNPC(y.name as string)));
                        });
                        break;
                    case 2: // alpha asc
                        Monitor.Log("sorting by alpha asc");
                        nameSprites.Sort(delegate (NameSpriteSlot x, NameSpriteSlot y)
                        {
                            return (x.name is long ? Game1.getFarmer((long)x.name).Name : GetNPCDisplayName(x.name as string)).CompareTo(y.name is long ? Game1.getFarmer((long)y.name).Name : GetNPCDisplayName(y.name as string));
                        });
                        break;
                    case 3: // alpha desc
                        Monitor.Log("sorting by alpha desc");
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
                Helper.Reflection.GetField<List<ClickableTextureComponent>>((Game1.activeClickableMenu as GameMenu).pages[GameMenu.socialTab], "sprites").SetValue(new List<ClickableTextureComponent>(sprites));

                int first_character_index = 0;
                for (int l = 0; l < page.names.Count; l++)
                {
                    if (!(((SocialPage)(Game1.activeClickableMenu as GameMenu).pages[GameMenu.socialTab]).names[l] is long))
                    {
                        first_character_index = l;
                        break;
                    }
                }
                Helper.Reflection.GetField<int>((Game1.activeClickableMenu as GameMenu).pages[GameMenu.socialTab], "slotPosition").SetValue(first_character_index);
                Helper.Reflection.GetMethod((Game1.activeClickableMenu as GameMenu).pages[GameMenu.socialTab], "setScrollBarToCurrentIndex").Invoke();
                ((SocialPage)(Game1.activeClickableMenu as GameMenu).pages[GameMenu.socialTab]).updateSlots();
            }
        }

        private string GetNPCDisplayName(string name)
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