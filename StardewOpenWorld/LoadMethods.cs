using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using xTile.Tiles;

namespace StardewOpenWorld
{
    public partial class ModEntry
    {

        private void DoCachePoll()
        {
            HashSet<Point> points = new HashSet<Point>();
            foreach (var cp in loadedChunks)
            {
                points.AddRange(GetSurroundingPointArrayAbs(cp, true));
            }
            foreach (var cp in points)
            {
                if (!cachedChunks.TryGetValue(cp, out var chunk) || !chunk.cached)
                {
                    chunksWaitingToCache.Add(cp);
                    return;
                }
            }
        }
        private void CheckForChunkChange()
        {
            bool force = false;
            List<Point> points = new List<Point>();
            foreach (var f in Game1.getAllFarmers())
            {
                if (f.currentLocation == openWorldLocation)
                {
                    if (!playerTilePoints.TryGetValue(f.UniqueMultiplayerID, out var tile) || f.TilePoint != tile)
                    {
                        var newChunk = GetPlayerChunk(f);
                        if (!playerChunks.TryGetValue(f.UniqueMultiplayerID, out var cp) || newChunk != cp)
                        {
                            if (IsChunkInMap(newChunk))
                                points.Add(newChunk);
                            playerChunks[f.UniqueMultiplayerID] = newChunk;
                        }
                        playerTilePoints[f.UniqueMultiplayerID] = f.TilePoint;
                    }

                }
                else
                {
                    if(playerChunks.TryGetValue(f.UniqueMultiplayerID, out var pc))
                    {
                        playerChunks.Remove(f.UniqueMultiplayerID);
                        playerTilePoints.Remove(f.UniqueMultiplayerID);
                        force = true;
                    }
                }
            }
            if (force || points.Any())
            {
                PlayerChunkChanged(points);
            }
        }
        public static void PlayerChunkChanged(List<Point> centers)
        {
            int size = Config.OpenWorldSize / openWorldChunkSize;
            List<Point> keep = new List<Point>();
            keep.AddRange(centers);
            foreach (var c in centers)
            {
                foreach (var v in Utility.getSurroundingTileLocationsArray(c.ToVector2()))
                {
                    var p = v.ToPoint();
                    if (IsChunkInMap(p) && !keep.Contains(p))
                        keep.Add(p);
                }
            }

            for (int i = keep.Count - 1; i >= 0; i--)
            {
                if (!IsChunkInMap(keep[i]))
                    keep.RemoveAt(i);
            }

            for (int i = loadedChunks.Count - 1; i >= 0; i--)
            {
                var cp = loadedChunks[i];
                if (!keep.Contains(cp))
                {
                    chunksUnloading.Add(cp);
                }
            }
            for (int i = keep.Count - 1; i >= 0; i--)
            {
                if (loadedChunks.Contains(keep[i]))
                {
                    keep.RemoveAt(i);
                }
            }

            chunksWaitingToCache.AddRange(keep);
            chunksWaitingToBuild.AddRange(keep);
            chunksWaitingToLoad.AddRange(keep);

        }

        private void CheckForChunkLoading()
        {

            if (chunksUnloading.Any())
            {
                for (int i = chunksUnloading.Count - 1; i >= 0; i--)
                {
                    UnloadChunk(chunksUnloading[i]);
                }
                chunksUnloading.Clear();
            }
            else if (chunksCaching.Any())
            {
                CacheChunk(chunksCaching[0], true);
                chunksCaching.RemoveAt(0);
            }
            else if (chunksWaitingToCache.Any())
            {
                chunksCaching.AddRange(chunksWaitingToCache);
                chunksWaitingToCache.Clear();
            }
            else if (chunksBuilding.Any())
            {
                BuildWorldChunks(chunksBuilding);
            }
            else if (chunksWaitingToBuild.Any())
            {
                currentBuildStage = 0;
                chunksBuilding.AddRange(chunksWaitingToBuild);
                chunksWaitingToBuild.Clear();
            }
            else if (chunksLoading.Any())
            {
                foreach (var chunk in chunksLoading)
                {
                    TryLoadChunk(chunk);
                }
                if (currentLoadStage == LoadStage.Done)
                {
                    chunksLoading.Clear();
                    currentLoadStage = 0;
                }
                else
                {
                    currentLoadStage++;
                }
            }
            else if (chunksWaitingToLoad.Any())
            {
                currentLoadStage = 0;
                chunksLoading.AddRange(chunksWaitingToLoad);
                chunksWaitingToLoad.Clear();
            }
        }
        private static void UnloadChunk(Point cp)
        {
            for (int i = openWorldLocation.characters.Count - 1; i >= 0; i--)
            {
                Character c = openWorldLocation.characters[i];
                if (c.modData.TryGetValue(modKey, out var ps))
                {
                    if (ps == $"{cp.X},{cp.Y}")
                    {
                        openWorldLocation.characters.RemoveAt(i);
                    }
                }
            }
            if (!cachedChunks.TryGetValue(cp, out var chunk))
            {
                return;
            }
            foreach (var obj in chunk.objects)
            {
                if(openWorldLocation.objects.TryGetValue(obj.Key, out var o) && !o.modData.ContainsKey(modPlacedKey))
                {
                    openWorldLocation.objects.Remove(obj.Key);
                }
            }
            foreach (var obj in chunk.overlayObjects)
            {
                if (openWorldLocation.overlayObjects.TryGetValue(obj.Key, out var o) && !o.modData.ContainsKey(modPlacedKey))
                {
                    openWorldLocation.overlayObjects.Remove(obj.Key);
                }
            }
            foreach (var tf in chunk.terrainFeatures)
            {
                openWorldLocation.terrainFeatures.Remove(tf.Key);
            }
            foreach (var kvp in chunk.largeTerrainFeatures)
            {
                openWorldLocation.largeTerrainFeatures.Remove(kvp.Value);
            }
            foreach (var kvp in chunk.resourceClumps)
            {
                openWorldLocation.resourceClumps.Remove(kvp.Value);
            }
            openWorldLocation.largeTerrainFeatures.RemoveWhere(tf => tf.modData.TryGetValue(modChunkKey, out var ps) && ps == $"{cp.X},{cp.Y}");
            loadedChunks.Remove(cp);
        }
        private static WorldChunk CacheChunk(Point cp, bool full)
        {
            if (!cachedChunks.TryGetValue(cp, out var chunk))
            {
                chunk = new WorldChunk();
                chunk.tiles["Back"] = new Tile[openWorldChunkSize, openWorldChunkSize];
                chunk.tiles["Buildings"] = new Tile[openWorldChunkSize, openWorldChunkSize];
                chunk.tiles["Front"] = new Tile[openWorldChunkSize, openWorldChunkSize];
                chunk.tiles["Paths"] = new Tile[openWorldChunkSize, openWorldChunkSize];
                cachedChunks[cp] = chunk;
            }
            if (!full || chunk.cached)
            {
                return chunk;
            }
            AddLandmarksToChunk(cp);
            AddLakesToChunk(cp);
            AddGrassTilesToChunk(cp);
            AddBorderToChunk(cp);
            chunk.cached = true;
            return chunk;
        }
        public static void BuildWorldChunks(List<Point> chunks)
        {
            Stopwatch s = Stopwatch.StartNew();
            switch (currentBuildStage)
            {
                case BuildStage.Begin:
                    for (int i = chunks.Count - 1; i >= 0; i--)
                    {
                        if (cachedChunks[chunks[i]].built)
                        {
                            chunks.RemoveAt(i);
                            continue;
                        }
                    }
                    break;
                case BuildStage.Landmarks:
                    foreach (var cp in chunks)
                    {
                        AddPathsObjectsToChunk(cp);
                    }
                    break;
                case BuildStage.Chests:
                    foreach (var cp in chunks)
                    {
                        AddChestsToChunk(cp);
                    }
                    break;
                case BuildStage.Trees:
                    foreach (var cp in chunks)
                    {
                        AddTreesToChunk(cp);
                    }
                    break;
                case BuildStage.Bushes:
                    foreach (var cp in chunks)
                    {
                        AddBushesToChunk(cp);
                    }
                    break;
                case BuildStage.Artifacts:
                    foreach (var cp in chunks)
                    {
                        AddArtifactsToChunk(cp);
                    }
                    break;
                case BuildStage.Chunks:
                    foreach (var cp in chunks)
                    {
                        AddClumpsToChunk(cp);
                    }
                    break;
                case BuildStage.Forage:
                    foreach (var cp in chunks)
                    {
                        AddForageToChunk(cp);
                    }
                    break;
                case BuildStage.Rocks:
                    foreach (var cp in chunks)
                    {
                        AddRocksToChunk(cp);
                    }
                    break;
                case BuildStage.Grass:
                    foreach (var cp in chunks)
                    {
                        AddGrassToChunk(cp);
                    }
                    break;
                case BuildStage.Monsters:
                    foreach (var cp in chunks)
                    {
                        AddMonstersToChunk(cp);
                    }
                    break;
                case BuildStage.Done:
                    foreach (var cp in chunks)
                    {
                        cachedChunks[cp].built = true;
                    }
                    chunks.Clear();
                    currentBuildStage = 0;
                    return;
            }
            SMonitor.Log($"Build stage {currentBuildStage.ToString()} took {s.ElapsedMilliseconds}ms");
            currentBuildStage++;
        }

        private static void TryLoadChunk(Point pc)
        {
            if (loadedChunks.Contains(pc))
                return;
            Stopwatch s = Stopwatch.StartNew();
            switch (currentLoadStage)
            {
                case LoadStage.TerrainFeatures:
                    AddTerrainFeaturesFromChunk(pc);
                    break;
                case LoadStage.Objects:
                    AddObjectsFromChunk(pc);
                    break;
                case LoadStage.Monsters:
                    AddMonstersFromChunk(pc);
                    break;
                case LoadStage.Done:
                    loadedChunks.Add(pc);
                    break;
            }
            SMonitor.Log($"Load stage {currentLoadStage.ToString()} for chunk {pc} took {s.ElapsedMilliseconds}ms");

        }
    }
}