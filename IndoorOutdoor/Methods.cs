using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace IndoorOutdoor
{
    public partial class ModEntry
    {
        public static Rectangle ToXna(xTile.Dimensions.Rectangle rect)
        {
            return new Rectangle(rect.X, rect.Y, rect.Width, rect.Height);
        }
        public static xTile.Dimensions.Rectangle ToXTile(Rectangle rect)
        {
            return new xTile.Dimensions.Rectangle(rect.X, rect.Y, rect.Width, rect.Height);
        }



        public static bool CheckScreenRect(Rectangle drawRect)
        {
            foreach (var kvp in currentLocationIndoorRectDict.Value)
            {
                if (kvp.Key == currentIndoors.Value)
                {
                    bool tl = false;
                    bool bl = false;
                    bool tr = false;
                    bool br = false;
                    foreach (var rect in kvp.Value)
                    {
                        var screenRect = new Rectangle(rect.Location - new Point(Game1.viewport.X, Game1.viewport.Y), rect.Size);
                        if (drawRect.Contains(screenRect))
                        {
                            return true;
                        }
                        if (screenRect.Contains(drawRect.Center))
                        {
                            return true;
                        }
                        if (screenRect.Contains(drawRect.Location))
                        {
                            tl = true;
                        }
                        if (screenRect.Contains(drawRect.Location + new Point(drawRect.Width, 0)))
                        {
                            tr = true;
                        }
                        if (screenRect.Contains(drawRect.Location + new Point(drawRect.Width, drawRect.Height)))
                        {
                            br = true;
                        }
                        if (screenRect.Contains(drawRect.Location + new Point(0, drawRect.Height)))
                        {
                            bl = true;
                        }
                    }
                    return tl && bl && tr && br;
                }
                else
                {
                    bool tl = false;
                    bool bl = false;
                    bool tr = false;
                    bool br = false;
                    foreach (var rect in kvp.Value)
                    {
                        var screenRect = new Rectangle(rect.Location - new Point(Game1.viewport.X, Game1.viewport.Y), rect.Size);
                        if (screenRect.Contains(drawRect.Center))
                        {
                            return false;
                        }
                        if (screenRect.Contains(drawRect.Location))
                        {
                            tl = true;
                        }
                        if (screenRect.Contains(drawRect.Location + new Point(drawRect.Width, 0)))
                        {
                            tr = true;
                        }
                        if (screenRect.Contains(drawRect.Location + new Point(drawRect.Width, drawRect.Height)))
                        {
                            br = true;
                        }
                        if (screenRect.Contains(drawRect.Location + new Point(0, drawRect.Height)))
                        {
                            bl = true;
                        }
                    }
                    return !tl || !bl || !tr || !br;
                }
            }
            return currentIndoors.Value == null;
        }
    }
}