using System;
using System.Collections.Generic;
using System.Linq;
using SkiaSharp;

namespace Assets._Scripts.AStar
{
    public class AStar
    {
        private readonly bool       _canDiagonalMove;
        private readonly int        _maxNeighbours;
        private readonly PathNode[] _neighbours;

        private readonly int                                 _maxSteps;
        private readonly IBinaryHeap<Vector2Int, PathNode>   _frontier;
        private readonly HashSet<Vector2Int>                 _ignoredPositions;
        private readonly List<Vector2Int>                    _output;
        private readonly IDictionary<Vector2Int, Vector2Int> _links;
        private          List<PathNode>                      _allNodes;

        /// <summary>
        /// Creation of new path finder.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public AStar(bool canDiagonalMove = true, int maxSteps = int.MaxValue, int initialCapacity = 0)
        {
            if (maxSteps <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxSteps));
            if (initialCapacity < 0)
                throw new ArgumentOutOfRangeException(nameof(initialCapacity));
            _canDiagonalMove  = canDiagonalMove;
            _maxNeighbours    = !_canDiagonalMove ? 4 : 8;
            _neighbours       = new PathNode[_maxNeighbours];
            this._maxSteps    = maxSteps;
            _frontier         = new BinaryHeap<Vector2Int, PathNode>(a => a.Position, initialCapacity);
            _ignoredPositions = new HashSet<Vector2Int>(initialCapacity);
            _output           = new List<Vector2Int>(initialCapacity);
            _links            = new Dictionary<Vector2Int, Vector2Int>(initialCapacity);
            _allNodes         = new List<PathNode>();
        }

        public List<Vector2Int>? Calculate(SKPointI start, SKPointI end, int[,] map)
        {
            var lst = new List<Vector2Int>();
            for (var i = 0; i < map.GetLength(1); i++)
            for (var j = 0; j < map.GetLength(0); j++)
                if (map[j, i] == 1)
                    lst.Add(new Vector2Int(j, i));

            return Calculate(new Vector2Int(start), new Vector2Int(end), lst);
        }

        public List<Vector2Int>? Calculate(SKPointI start, SKPointI end, List<Vector2Int> obstacles)
        {
            return Calculate(new Vector2Int(start), new Vector2Int(end), obstacles);
        }

        public SKPointI GetNextPoint(SKPointI start, SKPointI end, int[,] map)
        {
            var lst = Calculate(start, end, map);
            if (lst == null)
                return start;
            if (lst.Count < 2)
                return new SKPointI(lst[0].X, lst[0].Y);
            lst.Reverse();
            return new SKPointI(lst[1].X, lst[1].Y);
        }

        /// <summary>
        /// Calculate a new path between two points.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        public List<Vector2Int>? Calculate(Vector2Int start, Vector2Int target, List<Vector2Int> obstacles)
        {
            var path = new List<Vector2Int>();
            if (obstacles == null)
                throw new ArgumentNullException(nameof(obstacles));

            if (!GenerateNodes(start, target, obstacles))
                return null;

            _output.Clear();
            _output.Add(target);

            while (_links.TryGetValue(target, out target))
                _output.Add(target);
            path = _output;
            return path;
        }

        public float GetPathDistance(SKPointI start, SKPointI end, int[,] map)
        {
            var lst = new List<Vector2Int>();
            for (var i = 0; i < map.GetLength(1); i++)
            for (var j = 0; j < map.GetLength(0); j++)
                if (map[j, i] == 1)
                    lst.Add(new Vector2Int(j, i));

            var l = Calculate(new Vector2Int(start), new Vector2Int(end), lst);
            if (l == null || l.Count == 0)
                return -1;
            return (float)_allNodes.FirstOrDefault(_ => _.Position.X == end.X && _.Position.Y == end.Y).TraverseDistance;
        }

        public float GetPathDistance(SKPointI start, SKPointI end, IEnumerable<SKPointI> obstacles)
        {
            var l = Calculate(new Vector2Int(start), new Vector2Int(end), obstacles.Select(_ => new Vector2Int(_)).ToList());
            if (l == null || l.Count == 0)
                return -1;
            return (float)_allNodes.FirstOrDefault(_ => _.Position.X == end.X && _.Position.Y == end.Y).TraverseDistance;
        }

        private bool GenerateNodes(Vector2Int start, Vector2Int target, IReadOnlyCollection<Vector2Int> obstacles)
        {
            _allNodes.Clear();
            _frontier.Clear();
            _ignoredPositions.Clear();
            _links.Clear();
            _frontier.Enqueue(new PathNode(start, target, 0));
            _ignoredPositions.UnionWith(obstacles);
            var step = 0;
            while (_frontier.Count > 0 && step++ <= _maxSteps)
            {
                var current = _frontier.Dequeue();
                _ignoredPositions.Add(current.Position);

                if (current.Position.Equals(target))
                    return true;

                GenerateFrontierNodes(current, target);
            }

            // All nodes analyzed - no path detected.
            return false;
        }

        private void GenerateFrontierNodes(PathNode parent, Vector2Int target)
        {
            _neighbours.Fill(parent, target);
            foreach (var newNode in _neighbours)
            {
                _allNodes.Add(newNode);
                // Position is already checked or occupied by an obstacle.
                if (_ignoredPositions.Contains(newNode.Position))
                    continue;

                // Node is not present in queue.
                if (!_frontier.TryGet(newNode.Position, out var existingNode))
                {
                    _frontier.Enqueue(newNode);
                    _links[newNode.Position] = parent.Position;
                }

                // Node is present in queue and new optimal path is detected.
                else if (newNode.TraverseDistance < existingNode.TraverseDistance)
                {
                    _frontier.Modify(newNode);
                    _links[newNode.Position] = parent.Position;
                }
            }
        }
    }
}
