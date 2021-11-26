using Microsoft.Xna.Framework;

namespace UtilityGrid
{
    public interface IUtilityGridApi
    {
        public Vector2 ElectricityToFromTile(string location, int x, int y);
        public Vector2 WaterToFromTile(string location, int x, int y);
        public void RefreshElectricGrid(string location);
        public void RefreshWaterGrid(string location);
    }

    public class UtilityGridApi
    {
        public Vector2 ElectricityToFromTile(string location, int x, int y)
        {
            return ModEntry.GetTileElectricPower(location, new Vector2(x, y));
        }
        public Vector2 WaterToFromTile(string location, int x, int y)
        {
            return ModEntry.GetTileWaterPower(location, new Vector2(x, y));

        }
        public void RefreshElectricGrid(string location)
        {
            ModEntry.RemakeGroups(location, ModEntry.GridType.electric);
        }
        public void RefreshWaterGrid(string location)
        {
            ModEntry.RemakeGroups(location, ModEntry.GridType.water);
        }
    }
}