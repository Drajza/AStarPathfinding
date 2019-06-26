using System;

namespace AStarPathfinding
{
    /// <summary>
    /// Třída reprezentující 2D prostor ve kterém bude probíhat prohledávání.
    /// </summary>
    public class Tileset
    {
        private int rows;
        public int Rows
        {
            get => rows;
            set => rows = value;
        }

        private int cols;
        public int Cols
        {
            get => cols;
            set => cols = value;
        }

        private int[,] tiles;   

        public Tileset(int rows, int cols)
        {
            this.rows = rows;
            this.cols = cols;
            tiles = new int[rows, cols];
            for (int i = 0; i < rows; i++)
                for (int j = 0; j < cols; j++)
                    tiles[i, j] = 1;
        }

        public int this[Position key]
        {
            get => tiles[key.X, key.Y];
            set => tiles[key.X, key.Y] = value;
        }

        /// <summary>
        /// Změní cenu kroku čtverce na určené pozici.
        /// </summary>
        /// <param name="position">Pozice čtverce</param>
        /// <param name="value">Hodnota, která se přičte ke stávající hodnotě</param>
        public void ChangeStepCostForTile(Position position, int value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Vrátí pozměněnou cenu kroku na určené pozici. Pokud pozice nemá změněnou cenu kroku, vrátí základní cenu kroku.
        /// </summary>
        /// <param name="position">Pozice čtverce</param>
        /// <returns>Cena kroku na daný čtverec</returns>
        public int GetStepCostForTile(Position position)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Vrátí vnitřní reprezentaci 2D prostoru.
        /// </summary>
        /// <returns>Kopie vnitřního pole</returns>
        public int[,] AsArray()
        {
            return (int[,])tiles.Clone();
        }        
    }
}
