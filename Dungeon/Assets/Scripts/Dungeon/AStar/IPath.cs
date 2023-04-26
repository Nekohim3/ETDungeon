using System.Collections.Generic;

namespace Assets._Scripts.AStar
{
    public interface IPath
    {
        bool Calculate(Vector2Int start, Vector2Int target, IReadOnlyCollection<Vector2Int> obstacles, out IReadOnlyCollection<Vector2Int> path);
    }
}
