using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using StardewValley;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using xTile.Tiles;
using Object = StardewValley.Object;

namespace ToolSmartSwitch
{
    public class ToolSmartSwitchAPI : IToolSmartSwitchAPI
    {
        public void SmartSwitch(Farmer f)
        {
            ModEntry.SmartSwitch(f);
        }

    }

    public interface IToolSmartSwitchAPI
    {
        public void SmartSwitch(Farmer f);

    }
}