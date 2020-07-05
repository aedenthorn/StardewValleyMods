using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace MultipleSpouses
{
    public class OutdoorAreaData
    {
        public Dictionary<string, OutdoorArea> areas = new Dictionary<string, OutdoorArea>();
    }

    public class OutdoorArea
    {
        public int startX;
        public int startY;
        public List<SpecialTile> specialTiles = new List<SpecialTile>();
        public NPCOffset npcOffset = null;

        public Point NpcPos(string name)
        {
            if(npcOffset != null)
            {
                return new Point(startX + npcOffset.x, startY + +npcOffset.y);
            }
            else if (NPCPatches.spousePatioLocations.ContainsKey(name))
            {
                return new Point(startX + NPCPatches.spousePatioLocations[name][0], startY + NPCPatches.spousePatioLocations[name][1]);
            }
            return new Point(startX + 2, startY + 4);
        }
    }

    public class NPCOffset
    {
        public int x;
        public int y;
    }

    public class SpecialTile
    {
        public int x;
        public int y;
        public string layer;
        public int tileIndex;
        public int tilesheet;
    }
}