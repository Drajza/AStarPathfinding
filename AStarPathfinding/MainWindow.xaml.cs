using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Input;
using System.Runtime.CompilerServices;

namespace AStarPathfinding
{
    /// <summary>
    /// Interakční logika pro MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region Binded Variables
        private string buttonText = "Začít";      // Text tlačítka
        public string ButtonText
        {
            get => buttonText;
            set
            {
                buttonText = value;
                OnPropertyChanged();
            }
        }

        private string messageText = "";
        public string MessageText
        {
            get => messageText;
            set
            {
                messageText = value;
                OnPropertyChanged();
            }
        }

        private int pathfindingProgress = 0;      // Progress bar
        public int PathfindingProgress
        {
            get => pathfindingProgress;
            set
            {
                pathfindingProgress = value;
                OnPropertyChanged();
            }
        }

        private ImageSource drawingSource;        // Obrázek k vykreslení
        public ImageSource DrawingSource
        {
            get => drawingSource;
            set
            {
                drawingSource = value;
                OnPropertyChanged();
            }
        }
             
        public int WaitMs                         // Doba čekání mezi iteracemi
        {
            get => pathfinder.WaitMs;
            set
            {
                pathfinder.WaitMs = value;
                OnPropertyChanged();
            }
        }

        public bool DrawText                      // Vykreslit ohodnocení
        {
            get => renderer.DrawText;
            set
            {
                renderer.DrawText = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region State Variables
        private bool isLeftMousePressed = false;
        private bool isRightMousePressed = false;
        private bool canModify = true;
        #endregion

        #region Variablesss
        private const byte BASE_ROWS = 10;
        private const byte BASE_COLS = 20;

        private byte multiplier   = 1;
        private double imgWidth   = 0;
        private double imgHeight  = 0;

        private Position prevTile = Position.Empty;
        private Position start    = new Position(0, 0);
        private Position end      = new Position(BASE_ROWS - 1, BASE_COLS - 1);
        #endregion

        private Tileset tileset                = new Tileset(BASE_ROWS, BASE_COLS);
        private readonly Pathfinder pathfinder = new Pathfinder();
        private readonly BackgroundWorker bw   = new BackgroundWorker();
        private readonly Renderer renderer     = new Renderer();

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindow()
        {
            InitializeComponent();
            RootWindow.DataContext = this;

            bw.WorkerReportsProgress = true;
            bw.WorkerSupportsCancellation = true;
            bw.DoWork += pathfinder.DoPathfinding;
            bw.ProgressChanged += PathfindingProgressChanged;
            bw.RunWorkerCompleted += PathfindingCompleted;
        }

        private void RootWindow_Loaded(object sender, RoutedEventArgs e)
        {
            MeasureImageCanvas();
            renderer.Initialize(tileset, imgWidth, imgHeight);
            DrawingSource = renderer.BitmapSoure;
            renderer.MarkStart(start);
            renderer.MarkEnd(end);
        }
        
        private void RootWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            MeasureImageCanvas();
            // TODO: změna velikosti plátna, pravděpodobně nic nedělat
        }

        private void MeasureImageCanvas()
        {
            imgWidth = MainGrid.ActualWidth;
            imgHeight = MainGrid.RowDefinitions[1].ActualHeight;
        }

        private void PrepareUIForState(UIState state)
        {
            if (state == UIState.Creation)
            {
                ButtonText = "Začít";
                MessageText = string.Empty;
                PathfindingProgress = 0;
                canModify = true;
                EnableButtons(true);
                renderer.Reset();
            }
            else if (state == UIState.Pathfinding)
            {
                ButtonText = "Zastavit";
                EnableButtons(false);
                canModify = false;
            }
            else if (state == UIState.Result)
            {
                ButtonText = "Vyčistit";
                EnableButtons(false);
                canModify = false;
            }
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private Position SelectedTile(Point mousePosition)
        {
            int tileX = (int)Math.Floor(mousePosition.Y / renderer.RectHeight);
            int tileY = (int)Math.Floor(mousePosition.X / renderer.RectWidth);

            if (tileX >= tileset.Rows)
                tileX = tileset.Rows - 1;
            if (tileX < 0)
                tileX = 0;
            if (tileY >= tileset.Cols)
                tileY = tileset.Cols - 1;
            if (tileY < 0)
                tileY = 0;

            return new Position(tileX, tileY);
        }

        private void PlaceWalkable(Position actTile)
        {
            if (actTile.Equals(start) || actTile.Equals(end))
                return;
            tileset[actTile] = (int)TileState.Waklable;
            renderer.MarkTile(actTile, TileState.Waklable);
        }

        private void PlaceImpassable(Position actTile)
        {
            if (actTile.Equals(start) || actTile.Equals(end))
                return;
            tileset[actTile] = (int)TileState.Impassable;
            renderer.MarkTile(actTile, TileState.Impassable);
        }

        private void ResetStartEnd()
        {
            start = Position.Empty;
            end = Position.Empty;
        }

        private void EnableButtons(bool isEnabled)
        {
            ButtonResizeUp.IsEnabled = isEnabled;
            ButtonResizeDown.IsEnabled = isEnabled;
        }

        private void EnableTextDrawing(bool isEnabled)
        {
            if (!isEnabled)
                DrawText = false;
            CheckBoxDrawText.IsEnabled = isEnabled;
        }

        #region BackgroudWorker events
        public void PathfindingProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            PathfinderProgressArgs pe = e.UserState as PathfinderProgressArgs;
            PathfindingProgress = e.ProgressPercentage;
            renderer.MarkLists(pe.OpenList, pe.ClosedList);
        }

        public void PathfindingCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                MessageText = "Prohledávání zrušeno";
            }
            else
            {
                PathfinderResultArgs pe = e.Result as PathfinderResultArgs;
                MessageText = "Cesta nenalezena";
                if (pe.PathFound)
                {
                    MessageText = "Cesta nalezena";
                    renderer.MarkFinalPath(pe.FinalPath);
                }
            }

            PrepareUIForState(UIState.Result);
        }
        #endregion

        #region UI Buttons Events
        private void ButtonPathfinding_Click(object sender, RoutedEventArgs e)
        {
            if (bw.IsBusy)
            {
                bw.CancelAsync();
            }
            else if(!canModify)
            {
                PrepareUIForState(UIState.Creation);
            }
            else
            {
                if (start.Equals(Position.Empty) || end.Equals(Position.Empty))
                {
                    MessageBox.Show("Musíte nejprve zvolit začátek a cíl");
                    return;
                }

                PrepareUIForState(UIState.Pathfinding);

                pathfinder.Initialize(tileset, start, end);

                bw.RunWorkerAsync();
            }
        }

        private void ButtonResizeUp_Click(object sender, RoutedEventArgs e)
        {
            if (multiplier + 1 > 5)
                return;

            multiplier++;
            Tileset newTileset = new Tileset(BASE_ROWS * multiplier, BASE_COLS * multiplier);
            tileset = newTileset;

            ResetStartEnd();

            if (multiplier > 2)
                EnableTextDrawing(false);
            renderer.Initialize(tileset, imgWidth, imgHeight);
            DrawingSource = renderer.BitmapSoure;
        }

        private void ButtonResizeDown_Click(object sender, RoutedEventArgs e)
        {
            if (multiplier - 1 < 1)
                return;

            multiplier--;
            Tileset newTileset = new Tileset(BASE_ROWS * multiplier, BASE_COLS * multiplier);
            tileset = newTileset;

            ResetStartEnd();

            if (multiplier <= 2)
                EnableTextDrawing(true);
            renderer.Initialize(tileset, imgWidth, imgHeight);
            DrawingSource = renderer.BitmapSoure;
        }
        #endregion

        #region Canvas Events
        private void ImageCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            isLeftMousePressed = e.LeftButton == MouseButtonState.Pressed;
            isRightMousePressed = e.RightButton == MouseButtonState.Pressed;

            if (!canModify)
                return;

            Position actTile = SelectedTile(e.GetPosition(ImageCanvas));

            if (isLeftMousePressed)
            {
                if (Keyboard.IsKeyDown(Key.S))
                {
                    start = actTile;
                    renderer.MarkStart(start);
                }
                else if (Keyboard.IsKeyDown(Key.E))
                {
                    end = actTile;
                    renderer.MarkEnd(end);
                }
                else
                {
                    PlaceImpassable(actTile);
                }
            }
            else if (isRightMousePressed)
            {
                PlaceWalkable(actTile);
            }
        }
                
        private void ImageCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            isLeftMousePressed = e.LeftButton == MouseButtonState.Pressed;
            isRightMousePressed = e.RightButton == MouseButtonState.Pressed;
        }

        private void ImageCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (!canModify)
                return;

            Position actTile = SelectedTile(e.GetPosition(ImageCanvas));

            if(!actTile.Equals(prevTile))
            {
                if(isLeftMousePressed)
                {
                    PlaceImpassable(actTile);
                }
                else if (isRightMousePressed)
                {
                    PlaceWalkable(actTile);
                }
            }

            prevTile = actTile;
        }
        #endregion
    }
}
