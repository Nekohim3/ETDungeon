using System;
using SkiaSharp;

namespace Assets._Scripts.AStar
{
    public readonly struct Vector2Int : IEquatable<Vector2Int>
    {
        private static readonly double Sqr = Math.Sqrt(2);
        private readonly        int    hash;

        public Vector2Int(SKPointI p)
        {
            X    = p.X;
            Y    = p.Y;
            hash = HashCode.Combine(X, Y);
        }

        public Vector2Int(int x, int y)
        {
            X    = x;
            Y    = y;
            hash = HashCode.Combine(X, Y);
        }

        public int X { get; }
        public int Y { get; }

        /// <summary>
        /// Estimated path distance without obstacles.
        /// </summary>
        public double DistanceEstimate()
        {
            var linearSteps   = Math.Abs(Math.Abs(Y) - Math.Abs(X));
            var diagonalSteps = Math.Max(Math.Abs(Y), Math.Abs(X)) - linearSteps;
            return linearSteps + Sqr * diagonalSteps;
        }

        public static Vector2Int operator +(Vector2Int a, Vector2Int b) => new(a.X + b.X, a.Y + b.Y);
        public static Vector2Int operator -(Vector2Int a, Vector2Int b) => new(a.X - b.X, a.Y - b.Y);

        public bool Equals(Vector2Int other)
            => X.Equals(other.X) && Y.Equals(other.Y);

        public override int GetHashCode()
            => hash;

        public override string ToString()
            => $"({X}, {Y})";
    }
}
