using System;

namespace AStarPathfinding
{
    /// <summary>
    /// Struktura reprezentující souřadnice v prostoru
    /// </summary>
    public struct Position : IEquatable<Position>
    {
        public int X { get; }
        public int Y { get; }

        static public Position Empty => new Position(-1, -1);

        public Position(int x, int y)
        {
            X = x;
            Y = y;
        }

        public bool Equals(Position other)
        {
            return X.Equals(other.X) && Y.Equals(other.Y);
        }

        public override string ToString()
        {
            return $"X: {X} Y: {Y}";
        }
    }
}
