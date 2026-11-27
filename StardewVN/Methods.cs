using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using System.Collections.Generic;
using System.Linq;

namespace StardewVN
{
	public partial class ModEntry : Mod
    {
        public IEnumerable<string> GetOptions(VisualNovelData data, List<VNOption> options)
        {
            return options.Where(o => RequirementPasses(data, o.Requirement) && Game1.random.NextDouble() < o.Probability).Select(o => o.Which);
        }
        public string GetOptionUnique(VisualNovelData data, List<VNOptionUnique> options)
        {
            int totalWeights = 0;
            List<VNOptionUnique> passedOptions = new();
            foreach(var o in options)
            {
                if(RequirementPasses(data, o.Requirement))
                {
                    passedOptions.Add(o);
                    totalWeights += o.Weight;
                }
            }
            if (!passedOptions.Any())
                return null;
            double roll = Game1.random.Next(totalWeights);
            totalWeights = 0;
            foreach (var o in passedOptions)
            {
                totalWeights += o.Weight;
                if(roll < totalWeights)
                    return o.Which;
            }
            return null;
        }
        public bool RequirementPasses(VisualNovelData data, VNRequirement req)
        {
            if (data.Variables.TryGetValue(req.Variable, out var v))
            {
                switch (v.Type)
                {
                    case VNVariableType.Boolean:
                        switch (req.Test)
                        {
                            case VNTest.Equals:
                                return (bool)v.Value;
                            case VNTest.EqualsVariable:
                                if (!data.Variables.TryGetValue((string)req.Value, out var v2))
                                    return false;
                                return v.Value == v2.Value;
                            default:
                                return false;
                        }
                    case VNVariableType.String:
                        switch (req.Test)
                        {
                            case VNTest.Equals:
                                return v.Value == req.Value;
                            case VNTest.EqualsVariable:
                                if (!data.Variables.TryGetValue((string)req.Value, out var v2))
                                    return false;
                                return v.Value == v2.Value;
                            default:
                                return false;
                        }
                    case VNVariableType.Integer:
                        switch (req.Test)
                        {
                            case VNTest.Equals:
                                return v.Value == req.Value;
                            case VNTest.EqualsVariable:
                                if (!data.Variables.TryGetValue((string)req.Value, out var v2))
                                    return false;
                                return v.Value == v2.Value;
                            case VNTest.MoreThan:
                                return (int)v.Value > (int)req.Value;
                            case VNTest.LessThan:
                                return (int)v.Value < (int)req.Value;
                            case VNTest.MoreThanVariable:
                                if (!data.Variables.TryGetValue((string)req.Value, out var v3))
                                    return false;
                                return (int)v.Value > (int)v3.Value;
                            case VNTest.LessThanVariable:
                                if (!data.Variables.TryGetValue((string)req.Value, out var v4))
                                    return false;
                                return (int)v.Value < (int)v4.Value;
                            default:
                                return false;
                        }
                    case VNVariableType.Decimal:
                        switch (req.Test)
                        {
                            case VNTest.Equals:
                                return v.Value == req.Value;
                            case VNTest.EqualsVariable:
                                if (!data.Variables.TryGetValue((string)req.Value, out var v2))
                                    return false;
                                return v.Value == v2.Value;
                            case VNTest.MoreThan:
                                return (float)v.Value > (float)req.Value;
                            case VNTest.LessThan:
                                return (float)v.Value < (float)req.Value;
                            case VNTest.MoreThanVariable:
                                if (!data.Variables.TryGetValue((string)req.Value, out var v3))
                                    return false;
                                return (float)v.Value > (float)v3.Value;
                            case VNTest.LessThanVariable:
                                if (!data.Variables.TryGetValue((string)req.Value, out var v4))
                                    return false;
                                return (float)v.Value < (float)v4.Value;
                            default:
                                return false;
                        }
                    default:
                        return false;
                }
            }
            if(req.AND != null)
            {
                foreach (var r in req.AND) 
                {
                    if (!RequirementPasses(data, r))
                        return false;
                }
            }
            if(req.OR != null)
            {
                bool any = false;
                foreach (var r in req.AND) 
                {
                    if (RequirementPasses(data, r))
                        any = true;
                }
                if (!any)
                    return false;
            }
            if(req.XOR != null)
            {
                bool one = false;
                foreach (var r in req.AND) 
                {
                    if (RequirementPasses(data, r))
                    {
                        if (one)
                            return false;
                        one = true;
                    }
                }
                if (!one)
                    return false;
            }
            if(req.NOT != null)
            {
                foreach (var r in req.AND) 
                {
                    if (RequirementPasses(data, r))
                    {
                        return false;
                    }
                }
            }
            return req.AND is not null || req.OR != null || req.XOR != null || req.NOT != null;
        }
        private void DrawMenuSlot(SpriteBatch b, Rectangle rectangle)
        {
            var text = SHelper.Translation.Get("menu-text");
            IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(384, 396, 15, 15), rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, Color.White, 4f, false, -1f);
            b.Draw(SHelper.ModContent.Load<Texture2D>("assets/menu.png"), new Rectangle(rectangle.Location + new Point(8, 8), rectangle.Size - new Point(16, 16)), Color.White);
            SpriteText.drawStringHorizontallyCenteredAt(b, text, Game1.viewport.Width / 2 + 64, rectangle.Y + 64, color: Color.LightGoldenrodYellow);
        }

        private void LoadGame(Rectangle area, int x, int y)
        {
            if (Config.ModEnabled)
                TitleMenu.subMenu = new VNSelectMenu();
        }
    }
}
