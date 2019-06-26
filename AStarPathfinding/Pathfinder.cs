using System.Linq;
using System.ComponentModel;
using System.Collections.Generic;
using System.Threading;

namespace AStarPathfinding
{
    /// <summary>
    /// Argumenty reprezentující stav iterace algoritmu
    /// </summary>
    public class PathfinderProgressArgs
    {
        public IReadOnlyCollection<Node> OpenList { get; }
        public IReadOnlyCollection<Node> ClosedList { get; }

        public PathfinderProgressArgs(List<Node> openList, List<Node> closedList)
        {
            OpenList = new List<Node>(openList).AsReadOnly();
            ClosedList = new List<Node>(closedList).AsReadOnly();
        }
    }

    /// <summary>
    /// Argumenty reprezentující konečné výsledky algoritmu
    /// </summary>
    public class PathfinderResultArgs
    {
        public bool PathFound { get; }
        public IReadOnlyCollection<Node> FinalPath { get; }

        public PathfinderResultArgs(bool pathFound, List<Node> finalPath)
        {
            PathFound = pathFound;
            FinalPath = new List<Node>(finalPath).AsReadOnly();
        }
    }

    /// <summary>
    /// Třída implementující AStar algoritmus
    /// </summary>
    public class Pathfinder
    {
        private FlagNodeList openList, closedList;
        private Position end;
        private NodeFlags[,] nodesFlags;
        private int tilesWalkalbe, tilesProcessed, rows, cols;

        private int waitMs = 100;
        /// <summary>
        /// Doba čekání mezi jednotlivými iteracemi algoritmu
        /// </summary>
        public int WaitMs
        {
            get { return waitMs; }
            set { waitMs = value; }
        }

        /// <summary>
        /// Inicializace prohledávání pro aktuální pole. Před každým novým prohledáváním je potřeba zavolat tuhle metodu.
        /// </summary>
        public void Initialize(Tileset tileset, Position start, Position end)
        {
            rows = tileset.Rows;
            cols = tileset.Cols;

            nodesFlags = (NodeFlags[,])(object)tileset.AsArray();

            this.end = end;            

            openList = new FlagNodeList(NodeFlags.OnOpenList, nodesFlags);
            closedList = new FlagNodeList(NodeFlags.OnClosedList, nodesFlags);

            CountWalkableTiles();

            openList.Add(new Node(start));
        }

        /// <summary>
        /// Vynuluje počitadla a spočítá počet průchozích čtverců
        /// </summary>
        private void CountWalkableTiles()
        {
            tilesWalkalbe = 0;
            tilesProcessed = 0;
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    if (nodesFlags[i, j] == NodeFlags.Walkable)
                        tilesWalkalbe++;
                }
            }
        }

        /// <summary>
        /// Kontrola zda-li souřadnice nepřesahují hranice.
        /// </summary>
        /// <param name="x">X souřadnice</param>
        /// <param name="y">Y souřadnice</param>
        /// <returns>TRUE pokud obě souřadnice nepřesahují hranice, jinak FALSE</returns>
        private bool IsInBounds(int x, int y)
        {
            return x >= 0 && x < rows &&
                   y >= 0 && y < cols;
        }

        /// <summary>
        /// Zkontroluje, zda-li se jedná o platné souřadnice
        /// </summary>
        /// <param name="position">Souřadnice ke kontrole</param>
        /// <returns>Platnost souřadnic</returns>
        private bool IsInBounds(Position position)
        {
            return IsInBounds(position.X, position.Y);
        }

        /// <summary>
        /// Vrátí všechny příznaky pro danou pozici
        /// </summary>
        /// <param name="position">Pozice, pro kterou se vracení příznaky</param>
        /// <returns>Příznaky pozice</returns>
        private NodeFlags GetFlags(Position position)
        {
            return nodesFlags[position.X, position.Y];
        }

        /// <summary>
        /// Pomocná metoda. Vrátí uzel ze seznamu na základně jeho pozice.
        /// </summary>
        /// <param name="list">Seznam, ve kterém se bude vyhledávat</param>
        /// <param name="position">Souřadnice hledaného uzlu</param>
        /// <returns>Uzel v případě že se na seznamu nachází, jinak <code>NULL</code></returns>
        private Node GetNodeFromListByPosition(IList<Node> list, Position position)
        {
            Node node = new Node(position);
            if (list.Contains(node))
                return list[list.IndexOf(node)];

            return null;
        }

        /// <summary>
        /// Zkontroluje zda-li při průchodu není protnuto neprůchozí pole. Použití při diagonálním pohybu o jeden čtverec.
        /// </summary>
        /// <param name="first">Výchozí čtverec</param>
        /// <param name="second">Konečný čtverec</param>
        /// <returns></returns>
        private bool IsCrossingImpassable(Position first, Position second)
        {
            int dx = second.X - first.X; Position possibleX = new Position(first.X + dx, first.Y);
            int dy = second.Y - first.Y; Position possibleY = new Position(first.X, first.Y + dy);
            return IsInBounds(possibleX) && (GetFlags(possibleX) & NodeFlags.Impassable) > 0 ||
                   IsInBounds(possibleY) && (GetFlags(possibleY) & NodeFlags.Impassable) > 0;
        }

        public void DoPathfinding(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            // Podmínky pro ukončení prohledávání:
            // Zrušení prohledávání uživatlem; Cíl byl přidán na closedList; Již neexistuje další uzel k prohledání
            while (!worker.CancellationPending && (GetFlags(end) & NodeFlags.OnClosedList) == 0 && openList.Count > 0)
            {
                // Výběr uzlu s nejmenší hodnotou F
                Node selectedNode = openList.OrderBy(s => s.F).First();

                // Přídání uzlu na closed list
                openList.Remove(selectedNode);
                closedList.ClearWithoutFlags();    // Není potřeba si uchovávat kompletní seznam všech projítých uzlů. Ovšem nutné zachovat příznaky.
                closedList.Add(selectedNode);
                tilesProcessed++;

                // Prohledání a ohodnocení okolí
                bool isDiagonal = true;
                for (int i = -1; i < 2; i++)
                {
                    for (int j = -1; j < 2; j++)
                    {
                        // Aktuální uzel přeskočíme
                        if (i == 0 && j == 0)
                        {
                            isDiagonal = !isDiagonal;
                            continue;
                        }

                        int x = selectedNode.Position.X + i;
                        int y = selectedNode.Position.Y + j;

                        if (IsInBounds(x, y))
                        {
                            // Pokud prohledáváme uzel na diagonále, zkontrolujeme jestli neprotínámí neprůchozí čtverec
                            if (isDiagonal && IsCrossingImpassable(selectedNode.Position, new Position(x, y)))
                            {
                                isDiagonal = !isDiagonal;
                                continue;
                            }

                            NodeFlags currNodeFlags = nodesFlags[x, y];

                            // Pokud se políčko již nachází na open listu, zkontrolujeme zda-li jsme nenašli výhodnější cestu na něj
                            if (currNodeFlags == (NodeFlags.Walkable | NodeFlags.OnOpenList))
                            {
                                Node currNode = GetNodeFromListByPosition(openList, new Position(x, y));
                                int stepCost = isDiagonal ? (int)(1.4 * currNode.Cost) : currNode.Cost;
                                if (selectedNode.G + stepCost < currNode.G)
                                {
                                    currNode.ParentNode = selectedNode;
                                    currNode.G = selectedNode.G + stepCost;
                                }
                            }
                            else if (currNodeFlags == NodeFlags.Walkable)
                            {
                                // Průchozí políčko, které se nenachází na open listu na něj přidáme a ohodnotíme
                                Position newNodePosition = new Position(x, y);
                                Node newNode = new Node(newNodePosition, selectedNode);
                                
                                int stepCost = isDiagonal ? (int)(1.4 * newNode.Cost) : newNode.Cost;
                                newNode.G = selectedNode.G + stepCost;
                                newNode.CalculateH(end);

                                openList.Add(newNode);
                            }
                        }

                        isDiagonal = !isDiagonal;
                    }
                }

                // Hlášení průběžného stavu prohledávání
                PathfinderProgressArgs userState = new PathfinderProgressArgs(openList, closedList);
                int percentage = (int)((tilesProcessed / (double)tilesWalkalbe) * 100);
                worker.ReportProgress(percentage, userState);

                // Čekání mezi iteracemi
                Thread.Sleep(waitMs);
            }

            // Prohledávání bylo dokončeno, nebo přerušeno
            bool pathFound = (GetFlags(end) & NodeFlags.OnClosedList) > 0;
            if (pathFound)
            {
                List<Node> finalPath = GetNodeFromListByPosition(closedList, end).GetPathToRoot();
                e.Result = new PathfinderResultArgs(pathFound, finalPath);
            }
            else
            {
                e.Result = new PathfinderResultArgs(pathFound, new List<Node>());
                if (worker.CancellationPending)
                    e.Cancel = true;
            }
        }
    }
}
