using System;
using System.Collections.Generic;

namespace AStarPathfinding
{
    /// <summary>
    /// Třída reprezentující uzel nacházející se v prostoru
    /// </summary>
    public class Node : IEquatable<Node>, IComparable<Node>
    {
        public Position Position { get; }

        private Node parentNode;         // Předchozí uzel
        public Node ParentNode
        {
            get => parentNode;
            set => parentNode = value;
        }

        private int cost = 10;           // Cena kroku na daný uezl
        public int Cost
        {
            get => cost;
            set => cost = value;
        }

        private int g = 0;              // Součet cen kroků od počátečního uzlu
        public int G
        {
            get => g;
            set => g = value;
        }

        private int h = 0;              // Odhadnutá cena z tohotu uzlu do konce
        public int H
        {
            get => h;
            set => h = value;
        }

        public int F                   // Hodnotící funkce f = g + h;
        {
            get => g + h;
        }

        public Node(Position position, Node parentNode = null)
        {
            Position = position;
            this.parentNode = parentNode;
        }

        /// <summary>
        /// Vrátí seznam uzlů vedoucí od kořenového uzlu po tento.
        /// </summary>
        /// <returns>seznam uzlů vedoucí od kořenového uzlu po tento.</returns>
        public List<Node> GetPathToRoot()
        {
            List<Node> path = new List<Node>();
            Node n = this;
            while (n != null)
            {
                path.Add(n);
                n = n.parentNode;
            }
            path.Reverse();
            return path;
        }

        /// <summary>
        /// Vypočítá Manhattanskou metriku.
        /// </summary>
        /// <param name="end">Souřadnice, ke kterým počítá metriku</param>
        public void CalculateH(Position end)
        {
            h = (Math.Abs(end.X - Position.X) + Math.Abs(end.Y - Position.Y)) * 10;
        }

        public bool Equals(Node other)
        {
            return Position.Equals(other.Position);
        }

        public int CompareTo(Node other)
        {
            return F.CompareTo(other.F);
        }
    }
}
