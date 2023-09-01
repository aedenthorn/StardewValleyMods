using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using System.Collections.Generic;
using System.Reflection;

namespace SDVExplorer.UI
{
    public class FieldCheckbox : FieldElement
	{
		public FieldCheckbox(string label, object obj, List<object> hier, int x = -1, int y = -1) : base(label, obj, hier, x, y, 36, 36)
		{
			object obj2 = Game1.game1;
			foreach (var i in hier)
			{
				if (i is FieldInfo)
				{
					obj2 = AccessTools.Field(obj2.GetType(), (i as FieldInfo).Name).GetValue(obj2);
				}
				else if (i is PropertyInfo)
				{
					obj2 = AccessTools.Property(obj2.GetType(), (i as PropertyInfo).Name).GetValue(obj2);
				}
			}
			if (obj2 is bool)
			{
				//ModEntry.SMonitor.Log($"Set {label} to {(bool)obj2}");
				isChecked = (bool)obj2;
			}
		}

		public override void receiveLeftClick(int x, int y)
		{
			if (!greyedOut)
			{
				Game1.playSound("drumkit6");
				selected = this;
				base.receiveLeftClick(x, y);
				isChecked = !isChecked;
				object obj = Game1.game1;
				object lastObject = null;
				string objName = "game1";
				foreach(var i in hierarchy)
                {
					lastObject = obj;
					if(i is FieldInfo)
                    {
						var r = AccessTools.Field(obj.GetType(), (i as FieldInfo).Name);
						obj = r.GetValue(obj);
						objName = r.Name;
					}
					else if(i is PropertyInfo)
                    {
						var r = AccessTools.Property(obj.GetType(), (i as PropertyInfo).Name);
						obj = r.GetValue(obj);
						objName = r.Name;
					}
				}
				if(obj is bool)
                {
					ModEntry.SMonitor.Log($"Setting {label} for {AccessTools.Field(lastObject.GetType(), objName)} to {(bool)obj}");
					AccessTools.Field(lastObject.GetType(), objName).SetValue(lastObject, isChecked);
                }
				selected = null;
			}
		}

		public override void draw(SpriteBatch b, int slotX, int slotY, IClickableMenu context = null)
		{
			b.Draw(Game1.mouseCursors, new Vector2((float)(slotX + bounds.X), (float)(slotY + bounds.Y)), new Rectangle?(isChecked ? OptionsCheckbox.sourceRectChecked : OptionsCheckbox.sourceRectUnchecked), Color.White * (greyedOut ? 0.33f : 1f), 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.4f);
			base.draw(b, slotX, slotY, context);
		}

		public const int pixelsWide = 9;

		public static FieldCheckbox selected;

		public bool isChecked;

		public static Rectangle sourceRectUnchecked = new Rectangle(227, 425, 9, 9);

		public static Rectangle sourceRectChecked = new Rectangle(236, 425, 9, 9);
	}
}