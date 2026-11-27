using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using Object = StardewValley.Object;

namespace ChessBoards
{
    public partial class ModEntry
    {
        private static string pieceIndexes = "prnbqk";

        private static bool IsValidMove(Object pieceObj, string capture, Object lastMovedPiece, Vector2 cornerTile, Vector2 startTile, Vector2 endTile, out bool enPassant, out bool castle, out bool pawnAdvance)
        {
            enPassant = false;
            castle = false;
            pawnAdvance = false;
            if (Config.FreeMode)
                return true;
            string piece = pieceObj.modData[pieceKey];
            if((lastMovedPiece == null && piece[0] == 'b') || (lastMovedPiece?.modData[pieceKey][0] == piece[0]))
            {
                return false;
            }
            if (capture is not null && capture[0] == piece[0])
            {
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
                    if (startSquare.Y == 4 && Math.Abs(startSquare.X - endSquare.X) == 1 && startSquare.Y - endSquare.Y == 1 && Game1.currentLocation.objects.TryGetValue(endTile - new Vector2(0, 1), out Object passed) && passed == lastMovedPiece && passed.modData[pieceKey] == "wp" && passed.modData.ContainsKey(pawnKey))
                    {
                        enPassant = true;
                        return true;
                    }
                    if (startSquare.X != endSquare.X)
                        return false;
                    if(startSquare.Y - endSquare.Y == 2 && startSquare.Y == 7 && !Game1.currentLocation.objects.ContainsKey(startTile + new Vector2(0, 1)))
                    {
                        pawnAdvance = true;
                        return true;
                    }
                    return startSquare.Y - endSquare.Y == 1;
                case "wp":
                    if (capture is not null)
                    {
                        return (Math.Abs(startSquare.X - endSquare.X) == 1 && endSquare.Y - startSquare.Y == 1);
                    }
                    if (startSquare.Y == 5 && Math.Abs(startSquare.X - endSquare.X) == 1 && endSquare.Y - startSquare.Y == 1 && Game1.currentLocation.objects.TryGetValue(endTile + new Vector2(0, 1), out passed) && passed == lastMovedPiece && passed.modData[pieceKey] == "bp" && passed.modData.ContainsKey(pawnKey))
                    {
                        enPassant = true;
                        return true;
                    }
                    if (startSquare.X != endSquare.X)
                        return false;
                    if (endSquare.Y - startSquare.Y == 2 && startSquare.Y == 2 && !Game1.currentLocation.objects.ContainsKey(startTile - new Vector2(0, 1)))
                    {
                        pawnAdvance = true;
                        return true;
                    }
                    return endSquare.Y - startSquare.Y == 1;

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

        private static bool CanCastle(Object pieceObj, Vector2 cornerTile, Vector2 startTile, Vector2 endTile)
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

        private static bool IsSquareFreeToCastle(string piece, Vector2 cornerTile, Vector2 tile)
        {
            if (Game1.currentLocation.objects.ContainsKey(tile))
            {
                return false;
            }
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    var thisTile = new Vector2(cornerTile.X + x, cornerTile.Y - y);
                    if (Game1.currentLocation.objects.TryGetValue(thisTile, out Object obj) && obj.modData.TryGetValue(pieceKey, out string p))
                    {
                        if (p[0] != heldPiece.modData[pieceKey][0] && IsValidMove(obj, piece, null, cornerTile, obj.TileLocation, tile, out bool enPassant, out bool castle, out bool pawnAdvance))
                        {

                            return false;
                        }
                    }
                }
            }
            return true;
        }

        private static void MovePiece(Vector2 cornerTile, Vector2 moveTile, bool enPassant, bool castle, bool pawnAdvance)
        {
            Vector2 startTile = heldPiece.TileLocation;
            Game1.currentLocation.objects.Remove(startTile);
            Game1.currentLocation.objects.TryGetValue(moveTile, out Object toCapture);
            heldPiece.TileLocation = moveTile;
            Game1.currentLocation.objects[moveTile] = heldPiece;

            Object king = null;
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
            if (!Config.FreeMode && king != null && IsInCheck(king, cornerTile))
            {
                if (toCapture != null)
                {
                    Game1.currentLocation.objects[heldPiece.TileLocation] = toCapture;
                }
                else
                    Game1.currentLocation.objects.Remove(heldPiece.TileLocation);
                heldPiece.TileLocation = startTile;
                Game1.currentLocation.objects[startTile] = heldPiece;
                PlaySound(Config.CancelSound);
                Game1.addHUDMessage(new HUDMessage(SHelper.Translation.Get("check"), 1));
                SMonitor.Log("Your king is in check");

            }
            else
            {
                if(!Config.FreeMode && enPassant)
                    Game1.currentLocation.objects.Remove(GetLastMovedPiece(cornerTile).TileLocation);
                if (!Config.FreeMode && castle)
                {
                    if(moveTile.X == startTile.X - 2)
                    {
                        Object rook = Game1.currentLocation.objects[heldPiece.TileLocation - new Vector2(2, 0)];
                        rook.modData[movedKey] = "true";
                        Game1.currentLocation.objects.Remove(rook.TileLocation);
                        rook.TileLocation = heldPiece.TileLocation + new Vector2(1, 0);
                        rook.modData[squareKey] = $"{rook.TileLocation.X - cornerTile.X + 1},{cornerTile.Y - rook.TileLocation.Y + 1}";
                        Game1.currentLocation.objects[rook.TileLocation] = rook;
                    }
                    else
                    {
                        Object rook = Game1.currentLocation.objects[heldPiece.TileLocation + new Vector2(1, 0)];
                        rook.modData[movedKey] = "true";
                        Game1.currentLocation.objects.Remove(rook.TileLocation);
                        rook.TileLocation = heldPiece.TileLocation - new Vector2(1, 0);
                        rook.modData[squareKey] = $"{rook.TileLocation.X - cornerTile.X + 1},{cornerTile.Y - rook.TileLocation.Y + 1}";
                        Game1.currentLocation.objects[rook.TileLocation] = rook;
                    }
                }
                heldPiece.modData[movedKey] = "true";
                heldPiece.modData[squareKey] = $"{heldPiece.TileLocation.X - cornerTile.X + 1},{cornerTile.Y - heldPiece.TileLocation.Y + 1}";
                if(pawnAdvance)
                {
                    heldPiece.modData[pawnKey] = "true";
                    SMonitor.Log("Two-square pawn advancement");
                }
                else
                    heldPiece.modData.Remove(pawnKey);
                SetLastMovedPiece(heldPiece, cornerTile);
                PlaySound(Config.PlaceSound);
                if(!Config.FreeMode && (heldPiece.modData[pieceKey] == "bp" && heldPiece.TileLocation.Y == cornerTile.Y) || (heldPiece.modData[pieceKey] == "wp" && heldPiece.TileLocation.Y == cornerTile.Y - 7))
                {
                    // promote
                    SMonitor.Log("Showing promote menu");
                    Game1.currentLocation.createQuestionDialogue(SHelper.Translation.Get("which-promote"), new Response[]
                    {
                        new Response("rook",SHelper.Translation.Get("rook") ),
                        new Response("knight",SHelper.Translation.Get("knight") ),
                        new Response("bishop",SHelper.Translation.Get("bishop") ),
                        new Response("queen",SHelper.Translation.Get("queen") )
                    }, "ChessBoards-mod-promote-question");
                }
                else
                {
                    heldPiece = null;
                }
            }
        }

        private static void PromotePiece(string whichAnswer)
        {
            var piece = GetPieceCode(heldPiece.modData[pieceKey].Substring(0,1), whichAnswer);
            heldPiece.modData[pieceKey] = piece;
            heldPiece = null;
        }

        private static string GetPieceCode(string color, string name)
        {
            switch (name)
            {
                case "rook":
                    return (color + "r");
                case "knight":
                    return (color + "n");
                case "bishop":
                    return (color + "b");
                case "queen":
                    return (color + "q");
            }
            return (color + "p");
        }

        private static void PlaySound(string sound)
        {
            if(sound.Length > 0)
                Game1.currentLocation.playSound(sound);
        }

        private static bool IsInCheck(Object king, Vector2 cornerTile)
        {
            List<Object> enemies = new List<Object>();
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    var thisTile = new Vector2(cornerTile.X + x, cornerTile.Y - y);
                    if (Game1.currentLocation.objects.TryGetValue(thisTile, out Object obj) && obj.modData.TryGetValue(pieceKey, out string p))
                    {
                        if (p[0] != heldPiece.modData[pieceKey][0] && IsValidMove(obj, king.modData[pieceKey], null, cornerTile, obj.TileLocation, king.TileLocation, out bool enPassant, out bool castle, out bool pawnAdvance))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private static bool NoDiagonalBlock(Vector2 startPos, Vector2 endPos)
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
        private static bool NoHorizontalBlock(Vector2 startPos, Vector2 endPos)
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
        private static bool NoVerticalBlock(Vector2 startPos, Vector2 endPos)
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

        private static bool GetChessBoardsBoardTileAt(Vector2 tile, out Point ChessBoardsTile)
        {
            ChessBoardsTile = new Point();
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
                //SMonitor.Log($"Not on board! left {left}, right {right}", StardewModdingAPI.LogLevel.Warn);
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
                //Monitor.Log($"up {up}, down {down}");
                return false;
            }
            ChessBoardsTile = new Point(left + 1, down + 1);
            //Monitor.Log($"ChessBoards tile {ChessBoardsTile}");
            return true;
        }

        private static Rectangle GetSourceRectForPiece(string piece)
        {
            int width = piecesSheet.Width / 6;
            int height = piecesSheet.Height / 2;
            int x = pieceIndexes.IndexOf(piece[1]) * width;
            int y = piece[0] == 'b' ? height : 0;
            return new Rectangle(x, y, width, height);
        }
        private static void SetLastMovedPiece(Object heldPiece, Vector2 cornerTile)
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
        private static Object GetLastMovedPiece(Vector2 cornerTile)
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

        private static Vector2 GetFlippedTile(Vector2 cornerTile, Point square)
        {
            Vector2 result = new Vector2(cornerTile.X + (8 - square.X), cornerTile.Y - (8 - square.Y));
            return result;
        }
    }
}