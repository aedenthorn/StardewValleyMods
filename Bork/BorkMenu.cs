using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;
using System;
using static System.Net.Mime.MediaTypeNames;

namespace Bork
{
    internal class BorkMenu : DialogueBox
    {
        private TextBox textInput;
        public ClickableComponent cc;
        public ClickableTextureComponent ctc;

        public BorkMenu(Dialogue dialogue) : base(dialogue)
        {
            textInput = new TextBox(Game1.content.Load<Texture2D>("LooseSprites\\textBox"), null, Game1.smallFont, Game1.textColor)
            {
                X = x - 8,
                Y = y + height + 8,
                Width = width + 8,
            };
            //textInput.Y -= textInput.Height;
            textInput.SelectMe();
            cc = new(new(textInput.X, textInput.Y, textInput.Width, textInput.Height), "cc");
            ctc = new(new(x + borderWidth + width, y + height, 48, 64), Game1.mouseCursors, new(448, 96, 24, 32), 2);

        }
        public override void draw(SpriteBatch b)
        {
            base.draw(b);
            textInput.Draw(b);
            ctc.draw(b);
            drawMouse(b);
        }
        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (cc.containsPoint(x, y))
            {
                textInput.Update();
            }
            else if (ctc.containsPoint(x, y))
            {
                Game1.activeClickableMenu = new BorkMenu(ModEntry.GetDialogue(textInput.Text));
            }
            else if (new Rectangle((int)dialogueIcon?.Position.X, (int)dialogueIcon?.Position.Y, 48, 48).Contains(x, y) || characterDialoguesBrokenUp.Count > 1 || (characterDialogue.dialogues[characterDialogue.currentDialogueIndex].Text.Length > 2 && characterDialogue.currentDialogueIndex < characterDialogue.dialogues.Count - 1))
            {
                base.receiveLeftClick(x, y, playSound);
            }
        }
        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
            if (cc.containsPoint(x, y))
            {
                textInput.Update();
            }
            else if (characterDialoguesBrokenUp.Count > 1 || (characterDialogue.dialogues[characterDialogue.currentDialogueIndex].Text.Length > 2 && characterDialogue.currentDialogueIndex < characterDialogue.dialogues.Count - 1))
            {
                base.receiveRightClick(x, y, playSound);
            }
        }
        public override void receiveKeyPress(Keys key)
        {
            if (key >= Keys.A && key <= Keys.Z)
            {

            }
            else if (key == Keys.Space)
            {

            }
            else if (textInput.Text.Length > 0 && key == Keys.Enter)
            {
                Game1.activeClickableMenu = new BorkMenu(ModEntry.GetDialogue(textInput.Text));
            }
            else if (Game1.options.doesInputListContain(Game1.options.menuButton, key))
            {
                exitThisMenu();
            }
            else if (characterDialoguesBrokenUp.Count > 1 || (characterDialogue.dialogues[characterDialogue.currentDialogueIndex].Text.Length > 2 && characterDialogue.currentDialogueIndex < characterDialogue.dialogues.Count - 1))
            {
                base.receiveKeyPress(key);
            }
            Game1.playSound("shiny4");
        }
    }
}