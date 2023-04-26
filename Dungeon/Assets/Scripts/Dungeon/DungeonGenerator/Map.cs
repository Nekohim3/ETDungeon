using System.Collections.Generic;
using System.Linq;
using Assets._Scripts.Extensions;
using SkiaSharp;

namespace Assets._Scripts.DungeonGenerator
{
    public class Map
    {
        public List<Room> Rooms  { get; set; }
        public List<Pass> Passes { get; set; }

        public Map()
        {
            Rooms  = new List<Room>();
            Passes = new List<Pass>();
        }

        public void NormalizeRooms()
        {
            var minX = Rooms.Min(_ => _.Bounds.Left);
            var minY = Rooms.Min(_ => _.Bounds.Top);
            foreach (var x in Rooms)
                x.Bounds = x.Bounds.OffsetRect(-minX, -minY);
        }

        //public IEnumerable<SKPointI> GetObstacles(Pass? excludePass = null)
        //{
        //    void FillRegionWith(SKRectI r, bool[,] arr)
        //    {
        //        for (var i = r.Top; i <= r.Bottom; i++)
        //        for (var j = r.Left; j <= r.Right; j++)
        //            arr[j, i] = true;
        //    }

        //    var area = GetArea();
        //    var arr  = new bool[area.Width + 1, area.Height + 1];
        //    foreach (var x in Rooms)
        //        for (var i = x.Bounds.Top; i < x.Bounds.Bottom; i++)
        //        for (var j = x.Bounds.Left; j < x.Bounds.Right; j++)
        //            arr[j, i] = true;

        //    foreach (var x in Passes.Where(x => excludePass != x))
        //    foreach (var c in x.LineList)
        //        FillRegionWith(c, arr);

        //    for (var i = 0; i <= arr.GetLength(0); i++)
        //    for (var j = 0; j <= arr.GetLength(1); j++)
        //        if (!arr[i, j])
        //            yield return new SKPointI(i, j);
        //}


        public int[,] GetMapArray(int extend = 0)
        {
            var area = GetArea();
            var arr  = new int[area.Width + 1 + extend * 2, area.Height + 1 + extend * 2];

            for (var i = 0; i < arr.GetLength(0); i++)
            for (var j = 0; j < arr.GetLength(1); j++)
                arr[i, j] = 1;

            foreach (var x in Rooms)
            {
                for (var i = x.Bounds.Top; i < x.Bounds.Bottom; i++)
                for (var j = x.Bounds.Left; j < x.Bounds.Right; j++)
                    arr[j + extend, i + extend] = 0;
                //foreach (var c in x.Elements)
                    //arr[x.Bounds.Left + c.Position.X, x.Bounds.Top + c.Position.Y].Element = c;
            }

            foreach (var x in Passes)
            {
                for (var i = 0; i < x.LineList.Count; i++)
                {
                    if (i == 0)
                        FillRegionWith(x.LineList[i], arr, extend);
                    if (i == 1)
                        FillRegionWith(x.LineList[i], arr, extend);
                    if (i == 2)
                        FillRegionWith(x.LineList[i], arr, extend);
                }
            }

            return arr;
        }
        public int[,] GetMapArrayExclude(Pass? excludePass = null)
        {
            var area = GetArea();
            var arr  = new int[area.Width + 1, area.Height + 1];
            for (var i = 0; i < arr.GetLength(0); i++)
            for (var j = 0; j < arr.GetLength(1); j++)
                arr[i, j] = 1;
            foreach (var x in Rooms)
                for (var i = x.Bounds.Top; i < x.Bounds.Bottom; i++)
                for (var j = x.Bounds.Left; j < x.Bounds.Right; j++)
                    arr[j, i] = 0;

            foreach (var x in Passes.Where(x => excludePass != x))
            {
                for (var i = 0; i < x.LineList.Count; i++)
                {
                    if (i == 0)
                        FillRegionWith(x.LineList[i], arr);
                    if (i == 1)
                        FillRegionWith(x.LineList[i], arr);
                    if (i == 2)
                        FillRegionWith(x.LineList[i], arr);
                }
            }

            return arr;
        }
        private void FillRegionWith(SKRectI r, int[,] arr, int extend = 0)
        {
            for (var i = r.Top; i <= r.Bottom; i++)
            for (var j = r.Left; j <= r.Right; j++)
                arr[j + extend, i + extend] = 0;
        }

        public SKRectI GetArea()
        {
            var rect = new SKRectI();
            foreach (var x in Rooms)
            {
                if (rect.Right < x.Bounds.Right)
                    rect.Right = x.Bounds.Right;
                if (rect.Bottom < x.Bounds.Bottom)
                    rect.Bottom = x.Bounds.Bottom;
            }

            return rect;
        }
    }
}
