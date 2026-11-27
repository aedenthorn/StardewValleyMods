using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using Object = StardewValley.Object;

namespace SmartBlocks
{
    public class SmartBlockData
    {
        public Rectangle[] InNodes { get; set; }
        public Rectangle[] OutNodes { get; set; }
        public int MinRadius { get; set; }
        public int MaxRadius { get; set; }
        public bool ItemSlot { get; set; }
        public bool CloneItem { get; set; }
        public bool FilterByItem { get; set; }
        public Type[] ItemTypes { get; set; }
        public string SpriteSheet { get; set; }
        public int SpriteIndex { get; set; }
        public string CraftingCost { get; set; } = "335 10 334 2";
    }
    public class SmartBlockInstanceData
    {
        public Object Obj { get; set; }
        public string BlockType { get; set; }
        public SmartBlockData Data { 
            get
            {
                return ModEntry.BlockTypes.TryGetValue(BlockType, out var data) ? data : null;
            } 
        }
        public bool Enabled { get; set; }
        public int Radius { get; set; }
        public ConnectionData[] InConnections { get; set; }
        public ConnectionData[] OutConnections { get; set; }
        public string ItemId { get; set; }
        public int ItemStack { get; set; }
        public int ItemQuality { get; set; }

        public bool TryReceiveItem(Item item)
        {
            if(item == null || Data == null || (ItemId is not null && ItemId != item.ItemId))  
                return false;
            switch (BlockType)
            {
                case "FilterBlock":
                    if (OutConnections is null || OutConnections[0] is null)
                        return false;
                    return TrySendItem(item, OutConnections[0]);
                case "SortBlock":
                    return TrySortItem(item);
            }
            return false;
        }

        public bool TrySortItem(Item item)
        {
            if (item is null)
                return false;
            foreach(var tile in Tiles)
            {
                if(Obj.Location.Objects.TryGetValue(tile, out var obj) && obj is Chest chest && chest.playerChest.Value && chest.Items.ContainsId(item.ItemId))
                {
                    foreach(var i in chest.Items)
                    {
                        if (item.canStackWith(i))
                        {
                            int toRemove = item.Stack - i.addToStack(item);
                            if (item.ConsumeStack(toRemove) == null)
                                return true;
                        }
                    }
                    for(int i = 0; i < chest.Items.Count; i++)
                    {
                        if (chest.Items[i] == null)
                        {
                            chest.Items[i] = item;
                                return true;
                        }
                    }
                }
            }
            return false;
        }

        public bool TrySendItem(Item item, ConnectionData connection)
        {
            SmartBlockInstanceData target = ModEntry.GetBlockAt(Obj.Location, connection);
            if (target is null)
                return false;
            return target.TryReceiveItem(item);
        }

        public bool TryMakeConnection(Point tile, Point pixel)
        {
            SmartBlockInstanceData target = ModEntry.GetBlocksAt(Obj.Location, tile);
            if (target is null || target.Data.InNodes?.Length < 1)
                return false;
            if(target.InConnections is null)
            {
                target.InConnections = new Point?[target.Data.InNodes.Length];
            }
            for(int i = 0; i < target.Data.InNodes.Length; i++)
            {
                if (target.Data.InNodes[i].Contains(pixel))
                {
                    if (target.InConnections[i] is not null)
                    {
                        return false;
                    }
                    target.InConnections[i] = Obj.TileLocation.ToPoint();
                    ModEntry.SetInstance(Obj.Location, tile, target);
                    return true;
                }
            }
            return false;
        }
        public List<Vector2> Tiles
        {
            get
            {
                List<Vector2> list = new();
                for(int x = 0; x < Radius * 2; x++)
                {
                    for (int y = 0; y < Radius * 2; y++)
                    {
                        var tile = Obj.TileLocation + new Vector2(x - Radius, y - Radius);
                        if(Vector2.Distance(tile, Obj.TileLocation) <= Radius)
                        {
                            list.Add(tile);
                        }
                    }
                }
                return list;
            }
        }
    }

    public class ConnectionData
    {
        public int Which { get; set; }
        public Point Tile { get; set; }
    }
}