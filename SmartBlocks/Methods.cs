using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using StardewValley;
using System.Collections.Generic;
using Object = StardewValley.Object;

namespace SmartBlocks
{
    public partial class ModEntry
    {
        public static List<SmartBlockInstanceData> GetCachedBlocksAt(GameLocation location, Vector2 tile)
        {
            if(!BlockCache.TryGetValue(location.NameOrUniqueName, out var dict) || !dict.TryGetValue(tile, out var list))
                return GetBlocksAt(location, tile.ToPoint());
            return list;
        }
        public static List<SmartBlockInstanceData> GetBlocksAt(GameLocation location, Point point)
        {
            var target = location.getObjectAtTile(point.X, point.Y);
            if (target is Object && target.modData.TryGetValue(blockKey, out var str))
            {
                var list = JsonConvert.DeserializeObject<List<SmartBlockInstanceData>>(str);
                foreach(var item in list)
                {
                    item.Obj = target;
                }
                if(!BlockCache.TryGetValue(location.NameOrUniqueName, out var dict))
                {
                    dict = new();
                    BlockCache[location.NameOrUniqueName] = dict;
                }
                dict[point.ToVector2()] = list;
                return list;
            }
            return null;
        }
        public static SmartBlockInstanceData GetBlockAt(GameLocation location, ConnectionData connection)
        {
            var blocks = GetBlocksAt(location, connection.Tile);
            if(blocks?.Count > connection.Which)
            {
                return blocks[connection.Which];
            }
            return null;
        }

        public static bool CheckForLoop(GameLocation l, Point original, Point point)
        {
             instance = GetBlocksAt(l, point);
            if (instance is null)
                return false;
            foreach (var c in instance.OutConnections)
            {
                if(c is null)
                    continue;
                if (c == original)
                {
                    return true;
                }
                if (CheckForLoop(l, original, c.Value))
                    return true;
            }
            return false;
        }
        public static void SetInstance(GameLocation l, Point tile, SmartBlockInstanceData instance)
        {
            var obj = l.getObjectAtTile(tile.X, tile.Y);
            obj.modData[blockKey] = JsonConvert.SerializeObject(instance);
            SendChangedMessage(l.NameOrUniqueName, tile.ToVector2());
        }
        public static void SendChangedMessage(string location, Vector2 tile)
        {
            MyMessage message = new MyMessage(location, tile);
            SHelper.Multiplayer.SendMessage(message, "UpdateBlockInstance", modIDs: new[] { context.ModManifest.UniqueID });
        }
    }
}