using System.Collections.Generic;
using Assets._Scripts.Extensions;
using SkiaSharp;

namespace Assets._Scripts.DungeonGenerator
{
    public class Room
    {
        public string        Name     { get; set; }
        public SKRectI       Bounds   { get; set; }
        public SKPointI      MidPoint => Bounds.GetMidPoint();
        public List<Element> Elements { get; set; }

        public Room(SKRectI r, string name)
        {
            Bounds = r;
            Name   = name;
        }

    }
}