using System;
using System.Collections.Generic;
using System.Linq;
using Assets._Scripts.Extensions;
using SkiaSharp;
using UnityEditor;

namespace Assets._Scripts.DungeonGenerator
{
    public static class Generator
    {
        private static Range  _roomWidth;
        private static Range  _roomHeight;
        private static Range  _roomCount;
        private static Range  _distanceBetweenRooms;
        private static Range  _passWidth;
        private static int    _passPercent;
        private static int    _seed;
        private static Random _rand;
        private static Map    Map;

        public static Map GenerateMap(List<ElementRate> elementGenerateList,
                                      Range             roomWidth,
                                      Range             roomHeight,
                                      Range             roomCount,
                                      Range             distanceBetweenRooms,
                                      Range             passWidth,
                                      int               passPercent,
                                      int               seed)
            //int roomMinWidth = 15, 
            //int roomMinHeight = 15, 
            //int roomMaxWidth = 30, 
            //int roomMaxHeight = 30,
            //int roomMinCount = 5, 
            //int roomMaxCount = 10, 
            //int minDistanceBetweenRooms = 0, 
            //int maxDistanceBetweenRooms = 30,
            //int minPassWidth = 1,
            //int maxPassWidth = 3, 
            //int passPercent = 50)
        {
            _roomWidth            = roomWidth;
            _roomHeight           = roomHeight;
            _roomCount            = roomCount;
            _distanceBetweenRooms = distanceBetweenRooms;
            _passWidth            = passWidth;
            _passPercent          = passPercent;
            _seed                 = seed == -1 ? new Random().Next(int.MinValue, int.MaxValue) : seed;
            _rand                 = new Random(_seed);
            Map                   = new Map();

            if (GenerateRooms())
            {
                GeneratePasses();
                
                return Map;
            }

            return null;
        }

        #region Room

        private static bool GenerateRooms()
        {
            var rc = _rand.GetRand(_roomCount);
            while (Map.Rooms.Count < rc)
            {
                if (Map.Rooms.Count == 0)
                    Map.Rooms.Add(new Room(new SKRectI(0, 0, _rand.GetRand(_roomWidth), _rand.GetRand(_roomHeight)), (Map.Rooms.Count + 1).ToString()));
                else
                {
                    var room = RepeatableCode.RepeatResult(() =>
                                                           {
                                                               var room = GenerateRoom((Map.Rooms.Count + 1).ToString());
                                                               return CheckRoom(room) ? room : null;
                                                           }, 100000);

                    if (room == null)
                        return false;

                    Map.Rooms.Add(room);
                }
                Map.NormalizeRooms();
            }
            return true;
        }

        private static Room GenerateRoom(string name)
        {
            var area = GetGenerateArea();
            var r    = new SKRectI { Left = _rand.GetRand(area.Left, area.Right), Top = _rand.GetRand(area.Top, area.Bottom) };
            r.Right  = r.Left + _rand.GetRand(_roomWidth);
            r.Bottom = r.Top  + _rand.GetRand(_roomHeight);
            return new Room(r, name);
        }


        public static SKRectI GetGenerateArea()
        {
            var r = Map.Rooms[0].Bounds;
            foreach (var x in Map.Rooms)
                r.Union(x.Bounds);
            return r.ExpandAll(_distanceBetweenRooms.End.Value).ExpandLeft(_roomWidth.End.Value).ExpandTop(_roomHeight.End.Value);
        }
        private static bool CheckRoom(Room r) => Map.Rooms.All(_ => _.Bounds.GetDistanceToRect(r.Bounds) >= _distanceBetweenRooms.Start.Value) &&
                                                 Map.Rooms.Any(_ => _.Bounds.GetDistanceToRect(r.Bounds) <= _distanceBetweenRooms.End.Value)   &&
                                                 Map.Rooms.All(_ => !_.Bounds.IntersectsWith(r.Bounds))                                        &&
                                                 (Map.Rooms.Count < 3 || GetNearestRooms(r).Count > 1);

        public static List<Room> GetNearestRooms(Room r) => Map.Rooms.Where(_ => r != _).Where(_ => r.Bounds.GetDistanceToRect(_.Bounds) <= _distanceBetweenRooms.End.Value).ToList();

        #endregion

        #region Pass

        private static void GeneratePasses()
        {
            var area = Map.GetArea();
            foreach (var x in Map.Rooms)
            {
                foreach (var c in GetNearestRooms(x))
                {
                    if (PassExist(x, c))
                        continue;
                    var lines = new List<SKRectI>();
                    if (c.Bounds.Right > x.Bounds.Left + 2 && c.Bounds.Left < x.Bounds.Right - 2) // straight pass vertical
                    {
                        var passX = _rand.GetRand(Math.Max(x.Bounds.Left, c.Bounds.Left) + 1, Math.Min(x.Bounds.Right, c.Bounds.Right) - 1);
                        lines.Add(x.Bounds.Bottom < c.Bounds.Top
                                      ? new SKRectI(passX, x.Bounds.Bottom, passX, c.Bounds.Top - 1).Standardized
                                      : new SKRectI(passX, x.Bounds.Top                         - 1, passX, c.Bounds.Bottom).Standardized);
                    }
                    else if (c.Bounds.Bottom > x.Bounds.Top + 2 && c.Bounds.Top < x.Bounds.Bottom - 2) // straight pass horizontal
                    {
                        var passY = _rand.GetRand(Math.Max(x.Bounds.Top, c.Bounds.Top) + 1, Math.Min(x.Bounds.Bottom, c.Bounds.Bottom) - 1);
                        lines.Add(x.Bounds.Right < c.Bounds.Left
                                      ? new SKRectI(x.Bounds.Right, passY, c.Bounds.Left - 1, passY).Standardized
                                      : new SKRectI(x.Bounds.Left                        - 1, passY, c.Bounds.Right, passY).Standardized);
                    }
                    else
                    {
                        var xp = x.Bounds.GetRectIntersectType(x.Bounds.MidX, x.Bounds.MidY, c.Bounds.MidX, c.Bounds.MidY);
                        var cp = c.Bounds.GetRectIntersectType(x.Bounds.MidX, x.Bounds.MidY, c.Bounds.MidX, c.Bounds.MidY);
                        if (xp.IsOpposite(cp))
                        {
                            var output = _rand.GetRand(x.Bounds.GetRectSidePos(xp.side.i) - ((float)xp.side.i).CompareTo(1.1f), xp.line.i % 2 == 0 ? x.Bounds.MidY : x.Bounds.MidX);
                            var input  = _rand.GetRand(c.Bounds.GetRectSidePos(cp.side.i) - ((float)cp.side.i).CompareTo(1.1f), cp.line.i % 2 == 0 ? c.Bounds.MidY : c.Bounds.MidX);
                            var br = _rand.GetRand((xp.line.i < 2 ? c : x).Bounds.GetRectSidePos(xp.line.i % 2 + 2) + ((xp.line.i < 2 ? x : c).Bounds.GetRectSidePos(xp.line.i % 2) - (xp.line.i < 2 ? c : x).Bounds.GetRectSidePos(xp.line.i % 2 + 2)) * (1 / (float)3),
                                                   (xp.line.i < 2 ? c : x).Bounds.GetRectSidePos(xp.line.i % 2 + 2) + ((xp.line.i < 2 ? x : c).Bounds.GetRectSidePos(xp.line.i % 2) - (xp.line.i < 2 ? c : x).Bounds.GetRectSidePos(xp.line.i % 2 + 2)) * (2 / (float)3));

                            lines.Add(new SKRectI(xp.line.i % 2 == 0 ? x.Bounds.GetRectSidePos(xp.line.i) - (xp.line.i == 0 ? 1 : 0) : output,
                                                  xp.line.i % 2 == 1 ? x.Bounds.GetRectSidePos(xp.line.i) - (xp.line.i == 1 ? 1 : 0) : output,
                                                  xp.line.i % 2 == 0 ? br : output,
                                                  xp.line.i % 2 == 1 ? br : output).Standardized);

                            lines.Add(new SKRectI(xp.line.i % 2 == 0 ? br : output,
                                                  xp.line.i % 2 == 1 ? br : output,
                                                  xp.line.i % 2 == 0 ? br : input,
                                                  xp.line.i % 2 == 1 ? br : input).Standardized);

                            lines.Add(new SKRectI(xp.line.i % 2 == 0 ? br : input,
                                                  xp.line.i % 2 == 1 ? br : input,
                                                  xp.line.i % 2 == 0 ? c.Bounds.GetRectSidePos(cp.line.i) - (cp.line.i == 0 ? 1 : 0) : input,
                                                  xp.line.i % 2 == 1 ? c.Bounds.GetRectSidePos(cp.line.i) - (cp.line.i == 1 ? 1 : 0) : input).Standardized);
                        }
                        else
                        {
                            var output = _rand.GetRand(x.Bounds.GetRectSidePos(xp.side.i) - ((float)xp.side.i).CompareTo(1.1f), xp.line.i % 2 == 0 ? x.Bounds.MidY : x.Bounds.MidX);
                            var input  = _rand.GetRand(c.Bounds.GetRectSidePos(cp.side.i) - ((float)cp.side.i).CompareTo(1.1f), cp.line.i % 2 == 0 ? c.Bounds.MidY : c.Bounds.MidX);

                            lines.Add(new SKRectI(xp.line.i % 2 == 0 ? x.Bounds.GetRectSidePos(xp.line.i) - (xp.line.i == 0 ? 1 : 0) : output,
                                                  xp.line.i % 2 == 1 ? x.Bounds.GetRectSidePos(xp.line.i) - (xp.line.i == 1 ? 1 : 0) : output,
                                                  xp.line.i % 2 == 0 ? input : output,
                                                  xp.line.i % 2 == 1 ? input : output).Standardized);

                            lines.Add(new SKRectI(cp.line.i % 2 == 0 ? c.Bounds.GetRectSidePos(cp.line.i) - (cp.line.i == 0 ? 1 : 0) : input,
                                                  cp.line.i % 2 == 1 ? c.Bounds.GetRectSidePos(cp.line.i) - (cp.line.i == 1 ? 1 : 0) : input,
                                                  cp.line.i % 2 == 0 ? output : input,
                                                  cp.line.i % 2 == 1 ? output : input).Standardized);
                        }
                    }

                    for (var i = 0; i < lines.Count; i++)
                    {
                        if (lines[i].Left < 0)
                            lines[i] = new(0, lines[i].Top, lines[i].Right, lines[i].Bottom); // lines[i] with { Left = 0 };
                        if (lines[i].Right < 0)
                            lines[i] = new(lines[i].Left, lines[i].Top, 0, lines[i].Bottom);
                        if (lines[i].Top < 0)
                            lines[i] = new(lines[i].Left, 0, lines[i].Right, lines[i].Bottom);
                        if (lines[i].Bottom < 0)
                            lines[i] = new(lines[i].Left, lines[i].Top, lines[i].Right, 0);
                        if (lines[i].Right > area.Right)
                            lines[i] = new(lines[i].Left, lines[i].Top, lines[i].Right, area.Right);
                        if (lines[i].Left > area.Right)
                            lines[i] = new(area.Right, lines[i].Top, lines[i].Right, lines[i].Bottom);
                        if (lines[i].Bottom > area.Bottom)
                            lines[i] = new(lines[i].Left, lines[i].Top, lines[i].Right, area.Bottom);
                        if (lines[i].Top > area.Bottom)
                            lines[i] = new(lines[i].Left, area.Bottom, lines[i].Right, lines[i].Bottom);
                    }

                    var pw = _rand.GetRand(_passWidth) - 1;
                    if (pw > 0)
                    {
                        var side = Convert.ToBoolean(_rand.GetRand(0, 2));
                        switch (lines.Count)
                        {
                            case 1:
                                if (lines[0].Left == lines[0].Right)
                                    lines[0] = new(lines[0].Left - pw / 2 - (!side ? pw % 2 : 0), lines[0].Top, lines[0].Right + pw / 2 + (side ? pw % 2 : 0), lines[0].Bottom);
                                else
                                    lines[0] = new(lines[0].Left, lines[0].Top - pw / 2 - (!side ? pw % 2 : 0), lines[0].Right, lines[0].Bottom + pw / 2 + (side ? pw % 2 : 0));
                                break;
                            case 2:
                                if (lines[0].Left == lines[0].Right)
                                {
                                    lines[0] = new(lines[0].Left               - pw / 2 - (x.MidPoint.X > c.MidPoint.X ? pw % 2 : 0), lines[0].Top, lines[0].Right    + pw / 2 + (x.MidPoint.X < c.MidPoint.X ? pw % 2 : 0), lines[0].Bottom);
                                    lines[1] = new(lines[1].Left, lines[1].Top - pw / 2 - (x.MidPoint.Y > c.MidPoint.Y ? pw % 2 : 0), lines[1].Right, lines[1].Bottom + pw / 2 + (x.MidPoint.Y < c.MidPoint.Y ? pw % 2 : 0));
                                }
                                else
                                {
                                    lines[0] = new(lines[0].Left, lines[0].Top - pw / 2 - (x.MidPoint.Y > c.MidPoint.Y ? pw % 2 : 0), lines[0].Right, lines[0].Bottom + pw / 2 + (x.MidPoint.Y < c.MidPoint.Y ? pw % 2 : 0));
                                    lines[1] = new(lines[1].Left               - pw / 2 - (x.MidPoint.X > c.MidPoint.X ? pw % 2 : 0), lines[1].Top, lines[1].Right    + pw / 2 + (x.MidPoint.X < c.MidPoint.X ? pw % 2 : 0), lines[1].Bottom);
                                }

                                break;
                            case 3:
                                if (lines[0].Left == lines[0].Right)
                                {
                                    lines[0] = new(
                                                   lines[0].Left   - pw / 2 - (lines[0].Left  > lines[2].Left ? pw % 2 : 0),
                                                   lines[0].Top    - (lines[0].Top            > lines[2].Top ? pw / 2 + (side ? 0 : pw % 2) : 0),
                                                   lines[0].Right  + pw / 2 + (lines[0].Right < lines[2].Right ? pw % 2 : 0),
                                                   lines[0].Bottom + (lines[0].Top            < lines[2].Top ? pw / 2 + (side ? pw % 2 : 0) : 0)
                                                  );
                                    lines[1] = new(lines[1].Left, lines[1].Top - pw / 2 - (!side ? pw % 2 : 0), lines[1].Right, lines[1].Bottom + pw / 2 + (side ? pw % 2 : 0));
                                    lines[2] = new(
                                                   lines[2].Left   - pw / 2 - (lines[0].Left  < lines[2].Left ? pw % 2 : 0),
                                                   lines[2].Top    - (lines[0].Top            < lines[2].Top ? pw / 2 + (side ? 0 : pw % 2) : 0),
                                                   lines[2].Right  + pw / 2 + (lines[0].Right > lines[2].Right ? pw % 2 : 0),
                                                   lines[2].Bottom + (lines[0].Top            > lines[2].Top ? pw / 2 + (side ? pw % 2 : 0) : 0)
                                                  );
                                }
                                else
                                {
                                    lines[0] = new(

                                                   lines[0].Left  - (lines[0].Left         > lines[2].Left ? pw / 2 + (side ? 0 : pw % 2) : 0),
                                                   lines[0].Top   - pw / 2 - (lines[0].Top > lines[2].Top ? pw % 2 : 0),
                                                   lines[0].Right + (lines[0].Left         < lines[2].Left ? pw / 2 + (side ? pw % 2 : 0) : 0),
                                                   lines[0].Bottom + pw / 2 + (lines[0].Bottom < lines[2].Bottom ? pw % 2 : 0)
                                                  );
                                    lines[1] = new(lines[1].Left - pw / 2 - (!side ? pw % 2 : 0), lines[1].Top, lines[1].Right + pw / 2 + (side ? pw % 2 : 0), lines[1].Bottom);
                                    lines[2] = new(
                                                   lines[2].Left  - (lines[0].Left         < lines[2].Left ? pw / 2 + (side ? 0 : pw % 2) : 0),
                                                   lines[2].Top   - pw / 2 - (lines[0].Top < lines[2].Top ? pw % 2 : 0),
                                                   lines[2].Right + (lines[0].Left         > lines[2].Left ? pw / 2 + (side ? pw % 2 : 0) : 0),
                                                   lines[2].Bottom + pw / 2 + (lines[0].Bottom > lines[2].Bottom ? pw % 2 : 0)
                                                  );
                                }

                                break;
                            default:
                                break;
                        }
                    }

                    if (!Map.Rooms.Any(_ => lines.Any(__ => __.IntersectsWith(_.Bounds))) && !Map.Passes.SelectMany(_ => _.LineList).Any(_ => lines.Any(__ => __.IntersectsWith(_))))
                        Map.Passes.Add(new Pass(x, c, lines.ToArray()));
                }
            }

            var p = new AStar.AStar(1000);
            for (var i = 0; i < Map.Passes.Count; i++)
            {
                var x = Map.Passes[i];
                if (x.LineList.Count == 1)
                    continue;
                var basePath = p.GetPathDistance(x.StartRoom.MidPoint, x.EndRoom.MidPoint, Map.GetMapArrayExclude());
                if (basePath.IsEqual(-1))
                    continue;
                var longPath = p.GetPathDistance(x.StartRoom.MidPoint, x.EndRoom.MidPoint, Map.GetMapArrayExclude(x));
                if (longPath.IsEqual(-1))
                    continue;
                if (basePath + basePath * _passPercent / 100 > longPath)
                {
                    Map.Passes.RemoveAt(i);
                    i--;
                }
            }
        }

        public static bool PassExist(Room r1, Room r2) => Map.Passes.Count(_ => (_.StartRoom == r1 && _.EndRoom == r2) || (_.StartRoom == r2 && _.EndRoom == r1)) != 0;

        #endregion

        #region Element



        #endregion



    }
}