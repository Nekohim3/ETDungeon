using System;
using System.Collections.Generic;
using System.Linq;
using SkiaSharp;

namespace Assets._Scripts.Extensions
{
    public static class GeometricExtension
    {
        #region Rectangle

        public static SKRectI  ExpandLeft(this   SKRectI r, int left)                                 => new(r.Left - left, r.Top, r.Right, r.Bottom); // r with { Left = r. Left     - left };
        public static SKRectI  ExpandTop(this    SKRectI r, int top)                                  => new(r.Left, r.Top - top, r.Right, r.Bottom);//r with { Top = r.Top       - top };
        public static SKRectI  ExpandRight(this  SKRectI r, int right)                                => new(r.Left, r.Top, r.Right + right, r.Bottom);//r with { Right = r.Right   + right };
        public static SKRectI  ExpandBottom(this SKRectI r, int bottom)                               => new(r.Left, r.Top, r.Right, r.Bottom + bottom);//r with { Bottom = r.Bottom + bottom };
        public static SKRectI  ExpandWidth(this  SKRectI r, int width)                                => r.ExpandLeft(width).ExpandRight(width);
        public static SKRectI  ExpandHeight(this SKRectI r, int height)                               => r.ExpandTop(height).ExpandBottom(height);
        public static SKRectI  ExpandAll(this    SKRectI r, int n)                                    => r.ExpandWidth(n).ExpandHeight(n);
        public static SKRectI  Expand(this       SKRectI r, int left, int top, int right, int bottom) => r.ExpandLeft(left).ExpandTop(top).ExpandRight(right).ExpandBottom(bottom);
        public static SKPointI GetMidPoint(this  SKRectI r) => new(r.MidX, r.MidY);

        public static SKRectI OffsetRect(this SKRectI r, int x, int y) => new(r.Left + x, r.Top + y, r.Right + x, r.Bottom + y);
        //public static List<(SKPointI s, SKPointI e)> GetRectLines(this  SKRectI r) => new() { new SKLineI(r.Left, r.Top, r.Left, r.Bottom), new SKLineI(r.Left, r.Top, r.Right, r.Top), new SKLineI(r.Right, r.Top, r.Right, r.Bottom), new SKLineI(r.Left, r.Bottom, r.Right, r.Bottom) };
        public static List<(SKPointI s, SKPointI e)> GetRectLines(this SKRectI r) => new() { (new SKPointI(r.Left,    r.Top), new SKPointI(r.Left,     r.Bottom)),
                                                                                               (new SKPointI(r.Left,  r.Top), new SKPointI(r.Right,    r.Top)),
                                                                                               (new SKPointI(r.Right, r.Top), new SKPointI(r.Right,    r.Bottom)),
                                                                                               (new SKPointI(r.Left,  r.Bottom), new SKPointI(r.Right, r.Bottom)) };
        public static List<SKPointI>                 GetRectPoints(this SKRectI r) => new() { new SKPointI(r.Left, r.Top), new SKPointI(r.Right, r.Top), new SKPointI(r.Right, r.Bottom), new SKPointI(r.Left, r.Bottom) };

        public static float GetDistanceToRect(this SKRectI r, SKRectI r1)
        {
            if (r1.Right > r.Left + 2 && r1.Left < r.Right - 2)
                return Math.Min(Math.Abs(r.Top - r1.Bottom), Math.Abs(r1.Top - r.Bottom));
            if (r1.Bottom > r.Top + 2 && r1.Top < r.Bottom - 2)
                return Math.Min(Math.Abs(r.Left - r1.Right), Math.Abs(r1.Left - r.Right));
            return r.GetRectPoints().SelectMany(x => r1.GetRectPoints().Select(c => (x - c).Length)).Min();
        }

        public static int GetRectSidePos(this SKRectI r, RectangleSideDirection d) => r.GetRectSidePos((int)d);
        public static int GetRectSidePos(this SKRectI r, int d) => d switch
        {
            0 => r.Left,
            1 => r.Top,
            2 => r.Right,
            3 => r.Bottom,
            _ => throw new ArgumentOutOfRangeException(nameof(d), d, null)
        };

        #endregion


        #region Point

        public static SKPoint OffsetPoint(this SKPoint p, float x, float y) => new(p.X + x, p.Y + y); // p with { X = p.X + x, Y = p.Y + y };
        //public static float   DistanceToPoint(this  SKPointI p, SKPointI pp) => new SKLineI(p, pp).Length;

        #endregion

        #region Other

        public static bool IsOpposite(this (RectangleSideDirection line, RectangleSideDirection side)                       a, (RectangleSideDirection line, RectangleSideDirection side)                       b) => (int)a.line % 2 == (int)b.line % 2 && a.line   != b.line;
        public static bool IsOpposite(this ((RectangleSideDirection e, int i) line, (RectangleSideDirection e, int i) side) a, ((RectangleSideDirection e, int i) line, (RectangleSideDirection e, int i) side) b) => a.line.i    % 2 == b.line.i    % 2 && a.line.i != b.line.i;
        public static ((RectangleSideDirection e, int i) line, (RectangleSideDirection e, int i) side) GetRectIntersectType(this SKRectI r, SKPointI s, SKPointI e)
        {
            var intersections = r.GetRectLines().Select(_ => Intersect(_.s, _.e, s, e)).ToList();
            var point = intersections.FirstOrDefault(_ => _ != default);
            if (point == default)
                throw new Exception();
            var index = intersections.IndexOf(point);
            return (((RectangleSideDirection)index, index), index % 2 == 0 ? r.MidY > point.Y ? (RectangleSideDirection.Top, 1) : (RectangleSideDirection.Bottom, 3) : r.MidX > point.X ? (RectangleSideDirection.Left, 0) : (RectangleSideDirection.Right, 2));
        }

        public static ((RectangleSideDirection e, int i) line, (RectangleSideDirection e, int i) side) GetRectIntersectType(this SKRectI r, int x1, int y1, int x2, int y2) => r.GetRectIntersectType(new SKPointI(x1, y1), new SKPointI(x2, y2));

        #endregion

        #region Line

        public static SKPoint Intersect(SKPointI s1, SKPointI e1, SKPointI s2, SKPointI e2, float tolerance = 0.00005f)
        {
            bool IsInsideLine(SKPointI s, SKPointI e, SKPoint p, float tol)
            {
                float x = p.X, y = p.Y;

                var leftX = s.X;
                var leftY = s.Y;

                var rightX = e.X;
                var rightY = e.Y;

                return ((x.IsGreaterThanOrEqual(leftX, tol) && x.IsLessThanOrEqual(rightX, tol))
                        || (x.IsGreaterThanOrEqual(rightX, tol) && x.IsLessThanOrEqual(leftX, tol)))
                       && ((y.IsGreaterThanOrEqual(leftY, tol) && y.IsLessThanOrEqual(rightY, tol))
                           || (y.IsGreaterThanOrEqual(rightY, tol) && y.IsLessThanOrEqual(leftY, tol)));
            }

            if (s1 == s2 && e1 == e2)
                throw new Exception("Both lines are the same.");

            if (s1.X.CompareTo(s2.X) > 0)
                (s1, e1, s2, e2) = (s2, e2, s1, e1);
            else if (s1.X.CompareTo(s2.X) == 0)
                if (s1.Y.CompareTo(s2.Y) > 0)
                    (s1, e1, s2, e2) = (s2, e2, s1, e1);

            float x1 = s1.X, y1 = s1.Y;
            float x2 = e1.X, y2 = e1.Y;
            float x3 = s2.X, y3 = s2.Y;
            float x4 = e2.X, y4 = e2.Y;

            if (x1.IsEqual(x2) && x3.IsEqual(x4) && x1.IsEqual(x3))
            {
                var firstIntersection = new SKPoint(x3, y3);
                if (IsInsideLine(s1, e1, firstIntersection, tolerance) &&
                    IsInsideLine(s2, e2, firstIntersection, tolerance))
                    return new SKPoint(x3, y3);
            }

            if (y1.IsEqual(y2) && y3.IsEqual(y4) && y1.IsEqual(y3))
            {
                var firstIntersection = new SKPoint(x3, y3);
                if (IsInsideLine(s1, e1, firstIntersection, tolerance) &&
                    IsInsideLine(s2, e2, firstIntersection, tolerance))
                    return new SKPoint(x3, y3);
            }

            if (x1.IsEqual(x2) && x3.IsEqual(x4))
                return default;

            if (y1.IsEqual(y2) && y3.IsEqual(y4))
                return default;

            float x, y;
            if (x1.IsEqual(x2))
            {
                var m2 = (y4 - y3) / (x4 - x3);
                var c2 = -m2 * x3 + y3;
                x = x1;
                y = c2 + m2 * x1;
            }
            else if (x3.IsEqual(x4))
            {
                var m1 = (y2 - y1) / (x2 - x1);
                var c1 = -m1 * x1 + y1;
                x = x3;
                y = c1 + m1 * x3;
            }
            else
            {
                var m1 = (y2 - y1) / (x2 - x1);
                var c1 = -m1 * x1 + y1;
                var m2 = (y4 - y3) / (x4 - x3);
                var c2 = -m2 * x3 + y3;
                x = (c1 - c2) / (m2 - m1);
                y = c2 + m2 * x;

                if (!((-m1 * x + y).IsEqual(c1)
                      && (-m2 * x + y).IsEqual(c2)))
                    return default;
            }

            var result = new SKPoint(x, y);

            if (IsInsideLine(s1, e1, result, tolerance) &&
                IsInsideLine(s2, e2, result, tolerance))
                return result;

            return default;
        }

        #endregion
    }
}
