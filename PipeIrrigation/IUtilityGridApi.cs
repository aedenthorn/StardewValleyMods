using Microsoft.Xna.Framework;
using StardewValley;
using System;
using System.Collections.Generic;

namespace PipeIrrigation
{
    public interface IUtilityGridApi
    {
        public void AddPowerFunction(Func<string, int, List<Vector2>, Vector2> function);
        public void AddHideGridAction(EventHandler<KeyValuePair<GameLocation, int>> handler);
        public void AddRefreshAction(EventHandler<KeyValuePair<GameLocation, int>> handler);
        public void AddShowGridAction(EventHandler<KeyValuePair<GameLocation, int>> handler);
        public bool IsObjectPowered(GameLocation location, int x, int y);
        public bool IsObjectWorking(GameLocation location, int x, int y);
        public List<List<Vector2>> LocationElectricObjects(GameLocation location);
        public List<List<Vector2>> LocationElectricPipes(GameLocation location);
        public List<List<Vector2>> LocationWaterObjects(GameLocation location);
        public List<List<Vector2>> LocationWaterPipes(GameLocation location);
        public Vector2 ObjectWaterElectricityVector(GameLocation location, int x, int y);
        public void RefreshElectricGrid(GameLocation location);
        public void RefreshWaterGrid(GameLocation location);
        public bool ShowingElectrictyGrid();
        public bool ShowingWaterGrid();
        public List<Vector2> TileGroupElectricityObjects(GameLocation location, int x, int y);
        public Vector2 TileGroupElectricityVector(GameLocation location, int x, int y);
        public List<Vector2> TileGroupWaterObjects(GameLocation location, int x, int y);
        public Vector2 TileGroupWaterVector(GameLocation location, int x, int y);
        public List<Vector2> TileElectricGrid(GameLocation location, int x, int y);
        public List<Vector2> TileWaterGrid(GameLocation location, int x, int y);
    }
}