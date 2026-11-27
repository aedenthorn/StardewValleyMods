using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FurnitureRecolor
{
    public class ColorGroup
    {
        public List<ColorData> colors = new();
        public float RG;
        public float RB;
        public float GB;

        public bool FitsGroup(Color color)
        {
            float rg = color.R / (float)color.G;
            float rb = color.R / (float)color.B;
            float gb = color.G / (float)color.B;
            return 
                Math.Abs(rg - RG) < ModEntry.Config.MaxDiff &&
                Math.Abs(rb - RB) < ModEntry.Config.MaxDiff &&
                Math.Abs(gb - GB) < ModEntry.Config.MaxDiff;
        }

        public float GetFit(ColorData data)
        {
            float rg = data.color.R / (float)data.color.G;
            float rb = data.color.R / (float)data.color.B;
            float gb = data.color.G / (float)data.color.B;
            var diff = (Math.Abs(rg - RG) + Math.Abs(rb - RB) + Math.Abs(gb - GB)) / 3f;
            return diff;
        }

        public bool FitsBetterThanGroup(ColorData data, ColorGroup group)
        {
            if (group is null)
                return true;
            float rg = data.color.R / (float)data.color.G;
            float rb = data.color.R / (float)data.color.B;
            float gb = data.color.G / (float)data.color.B;
            var diff = (Math.Abs(rg - RG) + Math.Abs(rb - RB) + Math.Abs(gb - GB)) / 3f;
            var odiff = (Math.Abs(rg - group.RG) + Math.Abs(rb - group.RB) + Math.Abs(gb - group.GB)) / 3f;
            return diff < odiff;
        }

        public void AddColor(Color color)
        {
            AddColor(new ColorData()
            {
                color = color,
                rg = color.R / (float)color.G,
                rb = color.R / (float)color.B,
                gb = color.G / (float)color.B,
            });
        }

        public void AddColor(ColorData data)
        {
            int idx = colors.FindIndex(c => c.color == data.color);
            if (idx == -1)
            {
                colors.Add(data);
                RG = colors.Average(c => c.rg);
                RB = colors.Average(c => c.rb);
                GB = colors.Average(c => c.gb);
            }
            else
            {
                colors[idx].count++;
            }
        }
    }

    public class ColorData
    {
        public Color color;
        public int count = 1;
        public float rg;
        public float rb;
        public float gb;
    }
}