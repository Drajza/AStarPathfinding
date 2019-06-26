using System.Collections.Generic;

namespace AStarPathfinding
{
    /// <summary>
    /// Nádstavba nad třídou <code>List<></code>. Přidává automatizaci nad správou příznaků pro uzly.
    /// </summary>
    public class FlagNodeList : List<Node>
    {
        private NodeFlags managedFlags;
        private NodeFlags[,] nodesFlags;

        public FlagNodeList(NodeFlags managedFlags, NodeFlags[,] nodesFlags) : base()
        {
            this.managedFlags = managedFlags;
            this.nodesFlags = nodesFlags;
        }

        public new void Add(Node node)
        {
            Position pos = node.Position;
            nodesFlags[pos.X, pos.Y] = nodesFlags[pos.X, pos.Y] | managedFlags;
            base.Add(node);
        }

        public new void Remove(Node node)
        {
            Position pos = node.Position;
            nodesFlags[pos.X, pos.Y] = nodesFlags[pos.X, pos.Y] ^ managedFlags;
            base.Remove(node);
        }

        public new void Clear()
        {
            Enumerator e = GetEnumerator();
            while (e.MoveNext())
            {
                Position pos = e.Current.Position;
                nodesFlags[pos.X, pos.Y] = nodesFlags[pos.X, pos.Y] ^ managedFlags;
            }
            base.Clear();
        }

        public void ClearWithoutFlags()
        {
            base.Clear();
        }
    }
}
