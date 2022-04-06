using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using Object = StardewValley.Object;

namespace Chess
{
    public partial class ModEntry
    {
        private static string pieceIndexes = "prnbqk";

        private bool IsValidMove(Object pieceObj, string capture, Object lastMovedPiece, Vector2 cornerTile, Vector2 startTile, Vector2 endTile, out bool enPassant, out bool castle)
        {
            string piece = pieceObj.modData[pieceKey];
            enPassant = false;
            castle = false;
            if (Config.FreeMode)
                return true;
            if(lastMovedPiece?.modData[pieceKey][0] == piece[0])
            {
                Game1.addHUDMessage(new HUDMessage("It's not your turn!", 1));
                return false;
            }
            if (capture is not null && capture[0] == piece[0])
            {
                Monitor.Log("Can't capture your own pieces");
                return false;
            }
            var startSquare = new Vector2(startTile.X + 1 - cornerTile.X, cornerTile.Y - startTile.Y + 1);
            var endSquare = new Vector2(endTile.X + 1 - cornerTile.X, cornerTile.Y - endTile.Y + 1);

            switch (piece)
            {
                case "bp":
                    if (capture is not null)
                    {
                        return (Math.Abs(startSquare.X - endSquare.X) == 1 && startSquare.Y - endSquare.Y == 1);
                    }
                    if (startSquare.Y == 3 && Math.Abs(startSquare.X - endSquare.X) == 1 && startSquare.Y - endSquare.Y == 1 && Game1.currentLocation.objects.TryGetValue(endTile - new Vector2(0, 1), out Object passed) && passed == lastMovedPiece && passed.modData[pieceKey] == "wp")
                    {
                        enPassant = true;
                        Monitor.Log("En passant");
                        return true;
                    }
                    return startSquare.X == endSquare.X && ((startSquare.Y - endSquare.Y == 1) || ((startSquare.Y - endSquare.Y == 2 && startSquare.Y == 7) && !Game1.currentLocation.objects.ContainsKey(startTile + new Vector2(0, 1))));
                case "wp":
                    if (capture is not null)
                    {
                        return (Math.Abs(startSquare.X - endSquare.X) == 1 && endSquare.Y - startSquare.Y == 1);
                    }
                    if (startSquare.Y == 5 && Math.Abs(startSquare.X - endSquare.X) == 1 && endSquare.Y - startSquare.Y == 1 && Game1.currentLocation.objects.TryGetValue(endTile + new Vector2(0, 1), out passed) && passed == lastMovedPiece && passed.modData[pieceKey] == "bp")
                    {
                        enPassant = true;
                        Monitor.Log("En passant");
                        return true;
                    }
                    return startSquare.X == endSquare.X && ((endSquare.Y - startSquare.Y == 1) || ((endSquare.Y - startSquare.Y == 2 && startSquare.Y == 2) && !Game1.currentLocation.objects.ContainsKey(startTile - new Vector2(0, 1))));
                case "wn":
                case "bn":
                    return (Math.Abs(startSquare.X - endSquare.X) == 1 && Math.Abs(startSquare.Y - endSquare.Y) == 2) || (Math.Abs(startSquare.Y - endSquare.Y) == 1 && Math.Abs(startSquare.X - endSquare.X) == 2);
                case "wb":
                case "bb":
                    return (Math.Abs(startSquare.X - endSquare.X) == Math.Abs(startSquare.Y - endSquare.Y)) && NoDiagonalBlock(startTile, endTile);
                case "wq":
                case "bq":
                    return ((Math.Abs(startSquare.X - endSquare.X) == Math.Abs(startSquare.Y - endSquare.Y)) && NoDiagonalBlock(startTile, endTile)) 
                        || (startSquare.Y == endSquare.Y && NoHorizontalBlock(startTile, endTile)) 
                        || ((startSquare.X == endSquare.X) && NoVerticalBlock(startTile, endTile));
                case "wr":
                case "br":
                    return (startSquare.Y == endSquare.Y && NoHorizontalBlock(startTile, endTile)) 
                        || ((startSquare.X == endSquare.X) && NoVerticalBlock(startTile, endTile));
                case "wk":
                case "bk":
                    if (capture == null && CanCastle(pieceObj, cornerTile, startTile, endTile))
                    {
                        castle = true;
                        return true;
                    }
                    return (Math.Abs(startSquare.X - endSquare.X) <= 1 && Math.Abs(startSquare.Y - endSquare.Y) <= 1);
            }
            return false;
        }

        private bool CanCastle(Object pieceObj, Vector2 cornerTile, Vector2 startTile, Vector2 endTile)
        {
            string piece = pieceObj.modData[pieceKey];

            if (Game1.currentLocation.objects[startTile].modData.ContainsKey(movedKey))
                return false;
            if (endTile.Y != startTile.Y)
                return false;
            if (IsInCheck(pieceObj, cornerTile))
                return false;
            if (endTile.X == startTile.X + 2 && Game1.currentLocation.objects.TryGetValue(startTile + new Vector2(3, 0), out Object pairedRight) && pairedRight.modData.TryGetValue(pieceKey, out string pairedPiece) && pairedPiece[0] == piece[0] && pairedPiece[1] == 'r' && !pairedRight.modData.ContainsKey(movedKey) && IsSquareFreeToCastle(piece, cornerTile, startTile + new Vector2(1, 0)))
                return true;
            if (endTile.X == startTile.X - 2 && Game1.currentLocation.objects.TryGetValue(startTile - new Vector2(4, 0), out Object pairedLeft) && pairedLeft.modData.TryGetValue(pieceKey, out string pairedLeftPiece) && pairedLeftPiece[0] == piece[0] && pairedLeftPiece[1] == 'r' && !pairedLeft.modData.ContainsKey(movedKey) && IsSquareFreeToCastle(piece, cornerTile, startTile - new Vector2(1, 0)))
                return true;
            return false;
        }

        private bool IsSquareFreeToCastle(string piece, Vector2 cornerTile, Vector2 tile)
        {
            if (Game1.currentLocation.objects.ContainsKey(tile))
            {
                Monitor.Log("Intermediate castling square occupied!");
                return false;
            }
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    var thisTile = new Vector2(cornerTile.X + x, cornerTile.Y - y);
                    if (Game1.currentLocation.objects.TryGetValue(thisTile, out Object obj) && obj.modData.TryGetValue(pieceKey, out string p))
                    {
                        if (p[0] != heldPiece.modData[pieceKey][0] && IsValidMove(obj, piece, null, cornerTile, obj.TileLocation, tile, out bool enPassant, out bool castle))
                        {

                            Monitor.Log("Intermediate castling square under check!");
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        private void MovePiece(Vector2 cornerTile, bool enPassant, bool castle)
        {
            Vector2 startTile = heldPiece.TileLocation;
            Game1.currentLocation.objects.Remove(startTile);
            Game1.currentLocation.objects.TryGetValue(Game1.lastCursorTile, out Object toCapture);
            heldPiece.TileLocation = Game1.lastCursorTile;
            Game1.currentLocation.objects[Game1.lastCursorTile] = heldPiece;

            Object king = null;
            List<Object> enemies = new List<Object>();
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    var thisTile = new Vector2(cornerTile.X + x, cornerTile.Y - y);
                    if (Game1.currentLocation.objects.TryGetValue(thisTile, out Object obj) && obj.modData.TryGetValue(pieceKey, out string p))
                    {
                        if (p[1] == 'k')
                            king = obj;
                    }
                }
            }
            if (king != null && IsInCheck(king, cornerTile))
            {
                if (toCapture != null)
                {
                    Game1.currentLocation.objects[heldPiece.TileLocation] = toCapture;
                }
                else
                    Game1.currentLocation.objects.Remove(heldPiece.TileLocation);
                heldPiece.TileLocation = startTile;
                Game1.currentLocation.objects[startTile] = heldPiece;
                Game1.currentLocation.playSound("leafrustle");
                Game1.addHUDMessage(new HUDMessage("Your king is in check!", 1));
                Monitor.Log("Your king is in check");

            }
            else
            {
                if(enPassant)
                    Game1.currentLocation.objects.Remove(GetLastMovedPiece(cornerTile).TileLocation);
                if (castle)
                {
                    if(Game1.lastCursorTile.X == startTile.X - 2)
                    {
                        Object rook = Game1.currentLocation.objects[heldPiece.TileLocation - new Vector2(2, 0)];
                        rook.modData[movedKey] = "true";
                        Game1.currentLocation.objects.Remove(rook.TileLocation);
                        rook.TileLocation = heldPiece.TileLocation + new Vector2(1, 0);
                        Game1.currentLocation.objects[rook.TileLocation] = rook;
                    }
                    else
                    {
                        Object rook = Game1.currentLocation.objects[heldPiece.TileLocation + new Vector2(1, 0)];
                        rook.modData[movedKey] = "true";
                        Game1.currentLocation.objects.Remove(rook.TileLocation);
                        rook.TileLocation = heldPiece.TileLocation - new Vector2(1, 0);
                        Game1.currentLocation.objects[rook.TileLocation] = rook;
                    }
                }
                heldPiece.modData[movedKey] = "true";
                SetLastMovedPiece(heldPiece, cornerTile);
                Game1.currentLocation.playSound("bigDeSelect");
            }
        }

        private bool IsInCheck(Object king, Vector2 cornerTile)
        {
            List<Object> enemies = new List<Object>();
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    var thisTile = new Vector2(cornerTile.X + x, cornerTile.Y - y);
                    if (Game1.currentLocation.objects.TryGetValue(thisTile, out Object obj) && obj.modData.TryGetValue(pieceKey, out string p))
                    {
                        if (p[0] != heldPiece.modData[pieceKey][0] && IsValidMove(obj, king.modData[pieceKey], null, cornerTile, obj.TileLocation, king.TileLocation, out bool enPassant, out bool castle))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private bool NoDiagonalBlock(Vector2 startPos, Vector2 endPos)
        {
            Vector2 tile = startPos;
            int i = 0;
            while (i++ < 8)
            {
                float x = startPos.X > endPos.X ? tile.X - 1 : tile.X + 1;
                float y = startPos.Y > endPos.Y ? tile.Y - 1 : tile.Y + 1;
                tile = new Vector2(x, y);
                if (tile.X == endPos.X)
                    return true;
                if (Game1.currentLocation.objects.ContainsKey(tile))
                    return false;
            }
            return false;
        }
        private bool NoHorizontalBlock(Vector2 startPos, Vector2 endPos)
        {
            Vector2 tile = startPos;
            int i = 0;
            while(i++ < 8)
            {
                float x = startPos.X > endPos.X ? tile.X - 1 : tile.X + 1;
                float y = startPos.Y;
                tile = new Vector2(x, y);
                if (tile.X == endPos.X)
                    return true;
                if (Game1.currentLocation.objects.ContainsKey(tile))
                    return false;
            }
            return false;
        }
        private bool NoVerticalBlock(Vector2 startPos, Vector2 endPos)
        {
            Vector2 tile = startPos;
            int i = 0;
            while (i++ < 8)
            {
                float x = startPos.X;
                float y = startPos.Y > endPos.Y ? tile.Y - 1 : tile.Y + 1;

                tile = new Vector2(x, y);
                if (tile.Y == endPos.Y)
                    return true;
                if (Game1.currentLocation.objects.ContainsKey(tile))
                    return false;
            }
            return false;
        }

        private bool GetChessBoardTileAt(Vector2 tile, out Point chessTile)
        {
            chessTile = new Point();
            if (!Game1.currentLocation.terrainFeatures.TryGetValue(tile, out var t) || t is not Flooring)
                return false;
            int left = 0;
            int right = 0;
            int up = 0;
            int down = 0;
            Flooring f1 = t as Flooring;
            Flooring f2 = null;
            for (int i = 1; i < 8; i++)
            {
                if (!Game1.currentLocation.terrainFeatures.TryGetValue(tile - new Vector2(i, 0), out var t2) || t2 is not Flooring)
                    break;
                if (f2 is null)
                    f2 = t as Flooring;
                else if (i % 2 == 0 && f1.whichFloor != (t as Flooring).whichFloor)
                    break;
                else if (i % 2 == 1 && f2.whichFloor != (t as Flooring).whichFloor)
                    break;
                left = i;
            }
            if (left < 7)
            {
                for (int i = 1; i < 8 - left; i++)
                {
                    if (!Game1.currentLocation.terrainFeatures.TryGetValue(tile + new Vector2(i, 0), out var t2) || t2 is not Flooring)
                        break;
                    if (f2 is null)
                        f2 = t as Flooring;
                    else if (right % 2 == 0 && f2.whichFloor != (t as Flooring).whichFloor)
                        break;
                    else if (right % 2 == 1 && f1.whichFloor != (t as Flooring).whichFloor)
                        break;
                    right = i;
                }
            }
            if (left + right != 7)
            {
                Monitor.Log($"left {left}, right {right}");
                return false;
            }
            for (int i = 1; i < 8; i++)
            {
                if (!Game1.currentLocation.terrainFeatures.TryGetValue(tile - new Vector2(0, i), out var t2) || t2 is not Flooring)
                    break;
                if (f2 is null)
                    f2 = t as Flooring;
                else if (i % 2 == 0 && f1.whichFloor != (t as Flooring).whichFloor)
                    break;
                else if (i % 2 == 1 && f2.whichFloor != (t as Flooring).whichFloor)
                    break;
                up = i;
            }
            if (up < 7)
            {
                for (int i = 1; i < 8 - up; i++)
                {
                    if (!Game1.currentLocation.terrainFeatures.TryGetValue(tile + new Vector2(0, i), out var t2) || t2 is not Flooring)
                        break;
                    if (f2 is null)
                        f2 = t as Flooring;
                    else if (down % 2 == 0 && f2.whichFloor != (t as Flooring).whichFloor)
                        break;
                    else if (down % 2 == 1 && f1.whichFloor != (t as Flooring).whichFloor)
                        break;
                    down = i;
                }
            }
            if (down + up != 7)
            {
                Monitor.Log($"up {up}, down {down}");
                return false;
            }
            chessTile = new Point(left + 1, down + 1);
            Monitor.Log($"Chess tile {chessTile}");
            return true;
        }

        private static Point GetSourceRectForPiece(string piece)
        {
            int x = pieceIndexes.IndexOf(piece[1]) * 64;
            int y = piece[0] == 'b' ? 128 : 0;
            return new Point(x, y);
        }
        private void SetLastMovedPiece(Object heldPiece, Vector2 cornerTile)
        {
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    var thisTile = new Vector2(cornerTile.X + x, cornerTile.Y - y);
                    if (Game1.currentLocation.objects.TryGetValue(thisTile, out Object obj))
                    {
                        obj.modData.Remove(lastKey);
                    }
                }
            }
            heldPiece.modData[lastKey] = "true";
        }
        private Object GetLastMovedPiece(Vector2 cornerTile)
        {
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    var thisTile = new Vector2(cornerTile.X + x, cornerTile.Y - y);
                    if (Game1.currentLocation.objects.TryGetValue(thisTile, out Object obj) && obj.modData.ContainsKey(lastKey))
                    {
                        return obj;
                    }
                }
            }
            return null;
        }
    }
}