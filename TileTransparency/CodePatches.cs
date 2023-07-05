using Microsoft.Xna.Framework;
using xTile.Tiles;

namespace TileTransparency
{
    public partial class ModEntry
    {
        public static void DrawTile_Prefix(Tile tile, ref Color ___m_modulationColour, ref Color? __state)
        {
            if (!Config.ModEnabled || !tile.Properties.TryGetValue("@Opacity", out var p))
                return;
            __state = ___m_modulationColour;
            ___m_modulationColour *= (float)p;
        }
        public static void DrawTile_Postfix(ref Color ___m_modulationColour, ref Color? __state)
        {
            if (__state is null)
                return;
            ___m_modulationColour = __state.Value;
        }
    }
}