using Microsoft.Xna.Framework;
using StardewValley.Extensions;
using System.Collections.Generic;

namespace ShowPlayerBehind
{
    public class FarmerPointSet
    {
        public HashSet<Point> innerPoints = new();
        public HashSet<Point> outerPoints = new();
        public HashSet<Point> alwaysTopOuterPoints = new();
        public HashSet<Point> alwaysTopCornerPoints = new();
        public HashSet<Point> cornerPoints = new();
        public HashSet<Point> allPoints {
            get { 
                var points = new HashSet<Point>();
                points.AddRange(innerPoints); 
                points.AddRange(outerPoints); 
                points.AddRange(cornerPoints);
                points.AddRange(alwaysTopCornerPoints);
                points.AddRange(alwaysTopOuterPoints);
                return points;
            }
        }
    }
}