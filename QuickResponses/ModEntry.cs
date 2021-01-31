using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace QuickResponses
{
    public class ModEntry : Mod 
    {
        public static ModEntry context;

        public static ModConfig Config;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            context = this;
            Config = Helper.ReadConfig<ModConfig>();
            if (!Config.EnableMod)
                return;

            Helper.Events.Input.ButtonPressed += Input_ButtonPressed;
            Helper.Events.Display.RenderedActiveMenu += Display_RenderedActiveMenu;

        }

        private void Display_RenderedActiveMenu(object sender, StardewModdingAPI.Events.RenderedActiveMenuEventArgs e)
        {
            if (!Config.ShowNumbers || Game1.activeClickableMenu == null || !(Game1.activeClickableMenu is DialogueBox) || !Helper.Reflection.GetField<bool>(Game1.activeClickableMenu, "isQuestion").GetValue())
                return;

            DialogueBox db = Game1.activeClickableMenu as DialogueBox;
            if (Helper.Reflection.GetField<int>(db, "characterIndexInDialogue").GetValue() < db.getCurrentString().Length - 1 || Helper.Reflection.GetField<bool>(db, "transitioning").GetValue())
                return;

            int x = Helper.Reflection.GetField<int>(Game1.activeClickableMenu, "x").GetValue();
            int y = Helper.Reflection.GetField<int>(Game1.activeClickableMenu, "y").GetValue();
            int heightForQuestions = Helper.Reflection.GetField<int>(Game1.activeClickableMenu, "heightForQuestions").GetValue();
            List<Response> responses = Helper.Reflection.GetField<List<Response>>(Game1.activeClickableMenu, "responses").GetValue();
            int count = responses.Count;
            int responseY = y - (heightForQuestions - Game1.activeClickableMenu.height) + SpriteText.getHeightOfString((Game1.activeClickableMenu as DialogueBox).getCurrentString(), Game1.activeClickableMenu.width - 16) + 44;
            for (int i = 0; i < count; i++)
            {
                e.SpriteBatch.DrawString(Game1.dialogueFont, $"{i + 1}", new Vector2(x, responseY), Config.NumberColor, 0, Vector2.Zero, 0.4f, SpriteEffects.None, 0.86f);
                responseY += SpriteText.getHeightOfString(responses[i].responseText, Game1.activeClickableMenu.width) + 16;
            }

        }

        private void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            if (Game1.activeClickableMenu == null || !(Game1.activeClickableMenu is DialogueBox) || !Helper.Reflection.GetField<bool>(Game1.activeClickableMenu, "isQuestion").GetValue())
                return;
            if (e.Button == Config.SelectFirstResponseKey && Helper.Reflection.GetField<List<Response>>(Game1.activeClickableMenu, "responses").GetValue().Count == 2)
            {
                Monitor.Log("Pressed selectFirstResponse button key on question dialogue");
                Helper.Reflection.GetField<int>(Game1.activeClickableMenu, "selectedResponse").SetValue(0);
                Game1.activeClickableMenu.receiveLeftClick(0, 0, true);
                Helper.Input.Suppress(e.Button);
                return;
            }

            List<SButton> sbs = new List<SButton> { SButton.D1, SButton.D2, SButton.D3, SButton.D4, SButton.D5, SButton.D6, SButton.D7, SButton.D8, SButton.D9, SButton.D0 };
            if (sbs.Contains(e.Button) && sbs.IndexOf(e.Button) < Helper.Reflection.GetField<List<Response>>(Game1.activeClickableMenu, "responses").GetValue().Count)
            {
                Monitor.Log($"Pressed {e.Button} key on question dialogue");
                Helper.Reflection.GetField<int>(Game1.activeClickableMenu, "selectedResponse").SetValue(sbs.IndexOf(e.Button));
                Game1.activeClickableMenu.receiveLeftClick(0, 0, true);
                Helper.Input.Suppress(e.Button);
                return;
            }
        }
    }
}
