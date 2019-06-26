using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AStarPathfinding
{
    public class Renderer
    {
        private int rows, cols;
        private Position prevStart, prevEnd;
        private TileState[,] tiles;

        private readonly Dictionary<TileState, Color> brushes = new Dictionary<TileState, Color>();

        public int RectWidth;
        public int RectHeight;
        public bool DrawText;

        public WriteableBitmap BitmapSoure { get; private set; }

        public Renderer()
        {
            brushes.Add(TileState.Waklable,     Colors.White);
            brushes.Add(TileState.Impassable,   Colors.Black);
            brushes.Add(TileState.OnOpenList,   Colors.LawnGreen);
            brushes.Add(TileState.OnClosedList, Colors.Salmon);
            brushes.Add(TileState.Start,        Colors.CornflowerBlue);
            brushes.Add(TileState.End,          Colors.Crimson);
            brushes.Add(TileState.FinalPath,    Colors.DarkViolet);
        }

        public void Initialize(Tileset tileset, double width, double height)
        {
            BitmapSoure = BitmapFactory.New((int)width, (int)height);

            tiles = (TileState[,])(object)tileset.AsArray();
            rows = tileset.Rows; cols = tileset.Cols;
            RectWidth = ((int)width / tileset.Cols);
            RectHeight = ((int)height / tileset.Rows);

            prevStart = Position.Empty;
            prevEnd = Position.Empty;

            DrawEmptyGrid();
        }

        public void DrawEmptyGrid()
        {
            using (BitmapSoure.GetBitmapContext())
            {
                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < cols; j++)
                    {
                        int x = j * RectWidth;
                        int y = i * RectHeight;
                        BitmapSoure.DrawRectangle(x, y, x + RectWidth, y + RectHeight, Colors.Black);
                    }
                }
            }
        }

        public void DrawTileset()
        {
            using (BitmapSoure.GetBitmapContext())
            {
                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < cols; j++)
                    {
                        Position pos = new Position(i, j);
                        DrawFilledRectangle(pos, brushes[tiles[i, j]]);
                    }
                }
                RenewStartEnd();
            }
        }

        public void Reset()
        {
            for (int i = 0; i < rows; i++)
                for (int j = 0; j < cols; j++)
                    tiles[i, j] = tiles[i, j] == TileState.Impassable ? TileState.Impassable : TileState.Waklable;
            DrawTileset();
        }

        public void MarkStart(Position position)
        {
            using(BitmapSoure.GetBitmapContext())
            {
                if(!prevStart.Equals(Position.Empty))
                {
                    tiles[prevStart.X, prevStart.Y] = TileState.Waklable;
                    DrawFilledRectangle(prevStart, brushes[TileState.Waklable]);
                }
                tiles[position.X, position.Y] = TileState.Start;
                DrawFilledRectangle(position, brushes[TileState.Start]);
                prevStart = position;
            }
        }

        public void MarkEnd(Position position)
        {
            using (BitmapSoure.GetBitmapContext())
            {
                if (!prevEnd.Equals(Position.Empty))
                {
                    tiles[prevEnd.X, prevEnd.Y] = TileState.Waklable;
                    DrawFilledRectangle(prevEnd, brushes[TileState.Waklable]);
                }
                tiles[position.X, position.Y] = TileState.End;
                DrawFilledRectangle(position, brushes[TileState.End]);
                prevEnd = position;
            }
        }

        public void MarkTile(Position position, TileState state)
        {
            using (BitmapSoure.GetBitmapContext())
            {
                tiles[position.X, position.Y] = state;
                DrawFilledRectangle(position, brushes[state]);
            }
        }

        public void MarkLists(IEnumerable<Node> openList, IEnumerable<Node> closedList)
        {
            using (BitmapSoure.GetBitmapContext())
            {
                foreach (Node n in openList)
                {
                    tiles[n.Position.X, n.Position.Y] = TileState.OnOpenList;
                    DrawFilledRectangle(n.Position, brushes[TileState.OnOpenList]);
                }
                foreach (Node n in closedList)
                {
                    tiles[n.Position.X, n.Position.Y] = TileState.OnClosedList;
                    DrawFilledRectangle(n.Position, brushes[TileState.OnClosedList]);
                }
                RenewStartEnd();
            }
        }

        public void MarkFinalPath(IEnumerable<Node> finalPath)
        {
            using(BitmapSoure.GetBitmapContext())
            {
                foreach (Node n in finalPath)
                {
                    tiles[n.Position.X, n.Position.Y] = TileState.FinalPath;
                    DrawFilledRectangle(n.Position, brushes[TileState.FinalPath]);
                }
                RenewStartEnd();
            }
        }
        
        private void DrawFilledRectangle(Position position, Color fillColor)
        {
            int x = position.Y * RectWidth;
            int y = position.X * RectHeight;
            BitmapSoure.FillRectangle(x, y, x + RectWidth, y + RectHeight, fillColor);
            BitmapSoure.DrawRectangle(x, y, x + RectWidth, y + RectHeight, Colors.Black);
        }

        private void RenewStartEnd()
        {
            DrawFilledRectangle(prevStart, brushes[TileState.Start]);
            DrawFilledRectangle(prevEnd, brushes[TileState.End]);
        }
    }
}
