using System;
using System.Linq;
using Assets._Scripts.Extensions;

namespace Assets._Scripts.AStar
{
    internal static class NodeExtensions
    {
        private static readonly (Vector2Int position, double cost)[] NeighboursTemplate =
        {
            (new Vector2Int(1, 0), 1), 
            (new Vector2Int(0, 1), 1), 
            (new Vector2Int(-1, 0), 1), 
            (new Vector2Int(0, -1), 1), 
            (new Vector2Int(1, 1), Math.Sqrt(2)), 
            (new Vector2Int(1, -1), Math.Sqrt(2)), 
            (new Vector2Int(-1, 1), Math.Sqrt(2)),
            (new Vector2Int(-1, -1), Math.Sqrt(2))
        };

        public static void Fill(this PathNode[] buffer, PathNode parent, Vector2Int target)
        {
            var i = 0;
            foreach ((var position, var cost) in buffer.Length == 8 ? NeighboursTemplate : NeighboursTemplate.Where(_ => _.cost.IsEqual(1)))
            {
                var nodePosition     = position                + parent.Position;
                var traverseDistance = parent.TraverseDistance + cost;
                buffer[i++] = new PathNode(nodePosition, target, traverseDistance);
            }
        }
    }
}
