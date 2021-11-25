using Microsoft.Xna.Framework;

namespace UtilityGrid
{
    public class UtilityGridApi
    {
        public Vector2 ElectricityToFromTile(int x, int y)
        {
            return ModEntry.GetTileElectricPower(new Vector2(x, y));
        }
        public Vector2 WaterToFromTile(int x, int y)
        {
            return ModEntry.GetTileWaterPower(new Vector2(x, y));

        }
        public void RefreshElectricGrid()
        {
            ModEntry.RemakeGroups(ModEntry.GridType.electric);
        }
        public void RefreshWaterGrid()
        {
            ModEntry.RemakeGroups(ModEntry.GridType.water);
        }
    }
}