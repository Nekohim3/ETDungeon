﻿using System;
using System.Linq;

namespace Assets._Scripts.Extensions
{
    public static class RandomExtensions
    {
        public static void Shuffle<T>(this Random rng, T[] array)
        {
            var n = array.Length;
            while (n > 1)
            {
                var k = rng.Next(n--);
                (array[n], array[k]) = (array[k], array[n]);
            }
        }

        public static int[] CreateShuffleInt(this Random rng, int n)
        {
            var array = Enumerable.Range(0, n).ToArray();
            while (n > 1)
            {
                var k = rng.Next(n--);
                (array[n], array[k]) = (array[k], array[n]);
            }

            return array;
        }

        public static int GetRand(this Random r, int   min, int   max) => min <= max ? r.Next(min,      max) : r.Next(max,           min);
        public static int GetRand(this Random r, float min, float max) => min <= max ? r.Next((int)min, (int)max) : r.Next((int)max, (int)min);
        public static int GetRand(this Random r, Range range) => r.Next(range.Start.Value, range.End.Value);
    }
}
