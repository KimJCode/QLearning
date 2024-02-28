using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using AI.QLearning;

namespace TreasureHunt {

    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
            InitializeGameBoard();
            InitializeAI();
            InitializeGUI();
            InitalizeDisplay();
            InitializeTimer();
            UpdateStaticGUI();

            SetMode("Playing");
            UpdateDisplay();
            UpdateGUI();
        }

        #region --- Init Functions ---
        private void InitializeTimer() {
            if (null != Timer) {
                Timer.Stop();
            } else {
                Timer = new DispatcherTimer();
                Timer.Tick += LearnStep;
            }
        }

        private void InitalizeDisplay() {

            foreach (var Tile in Tiles) {
                GameGrid.Children.Remove(Tile);
            }

            int numRows = CurrentGame.Board.SizeY;
            int numCols = CurrentGame.Board.SizeX;

            GameGrid.RowDefinitions.Clear();
            GameGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1.0, GridUnitType.Star) });
            for (int row = 0; row < numRows; row++) {
                GameGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(128.0) });
            }
            GameGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1.0, GridUnitType.Star) });

            GameGrid.ColumnDefinitions.Clear();
            GameGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1.0, GridUnitType.Star) });
            for (int col = 0; col < numCols; col++) {
                GameGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(128) });
            }
            GameGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1.0, GridUnitType.Star) });

            Dictionary<GameBoard.TILE, ImageSource> TileImages = new Dictionary<GameBoard.TILE, ImageSource> {
                [GameBoard.TILE.EMPTY] = FindResource("Empty") as ImageSource,
                [GameBoard.TILE.TREASURE] = FindResource("Treasure") as ImageSource,
                [GameBoard.TILE.TRAP] = FindResource("Trap") as ImageSource,
                [GameBoard.TILE.BLOCKED] = FindResource("Blocked") as ImageSource
            };

            for (int row = 0; row < numRows; row++) {
                for (int col = 0; col < numCols; col++) {
                    Rectangle rect = new Rectangle() {
                        Fill = new ImageBrush(TileImages[CurrentGame.Board.Field(col, row)])
                    };
                    Tiles.Add(rect);
                    GameGrid.Children.Add(rect);
                    Grid.SetRow(rect, row + 1);
                    Grid.SetColumn(rect, col + 1);
                }
            }

        }

        private void InitializeGameBoard() {

            CurrentGame = new Game(Levels["Level 0"]);

        }

        private void InitializeGUI() {
            LevelSelect.Items.Clear();
            foreach (var item in Levels.Keys) {
                LevelSelect.Items.Add(item);
            }
            LevelSelect.SelectedIndex = 0;
            LevelSelect.SelectionChanged += LevelSelected;

            ModeSelect.Items.Clear();
            ModeSelect.Items.Add("Playing");
            ModeSelect.Items.Add("AI");
            ModeSelect.SelectedIndex = 0;
            ModeSelect.SelectionChanged += ModeSelected;
        }

        private void InitializeAI() {
            LearningAI = new QLearningAI() {
                GameState = CurrentGame,
            };
        }
        #endregion

        #region --- Update Functions ---

        private void UpdateDisplay() {
            // Update Player Position
            Grid.SetRow(Player, 1 + CurrentGame.PlayerPosY);
            Grid.SetColumn(Player, 1 + CurrentGame.PlayerPosX);
            // Update Health Bar
            double healthPercentage = CurrentGame.PlayerHealth / 3.0f;
            HealthBar.Width = HealthBarFrame.Width * healthPercentage;
            HealthBar.Fill = (healthPercentage > 0.5) ? Brushes.LawnGreen : Brushes.Red;
        }

        private void UpdateGUI() {
            Progress.Value = CurrentIteration * 100 / (MaxNumIterations);
        }

        private void UpdateStaticGUI() {

            DiscountRate.Text = "" + LearningAI.DiscountRate;
            NumIterations.Text = " " + MaxNumIterations;
            Penalty1.Text = "" + CurrentGame.Penalties[2];
            Penalty2.Text = "" + CurrentGame.Penalties[1];
            Penalty3.Text = "" + CurrentGame.Penalties[0];
            Reward.Text = "" + CurrentGame.Reward;
        }
        #endregion

        #region --- Callbacks ---
        private void OnKeyUp(object sender, KeyEventArgs e) {

            string actionName = "";
            switch (e.Key) {
                case Key.Down:
                    actionName = "Move Down";
                    break;
                case Key.Up:
                    actionName = "Move Up";
                    break;
                case Key.Left:
                    actionName = "Move Left";
                    break;
                case Key.Right:
                    actionName = "Move Right";
                    break;
            }
            CurrentGame.TryExecuteAction(actionName);
            UpdateDisplay();
        }

        private void ModeSelected(object sender, SelectionChangedEventArgs e) {

            if (ModeSelect.SelectedItem is string Mode) {
                SetMode(Mode);
            }
        }

        private void StartTraining(object sender, RoutedEventArgs e) {

            CurrentIteration = 0;
            uint n = UInt32.Parse(NumIterations.Text);
            double d1 = Double.Parse(Penalty1.Text);
            double d2 = Double.Parse(Penalty2.Text);
            double d3 = Double.Parse(Penalty3.Text);
            double r = Double.Parse(Reward.Text);
            MaxNumIterations = (int) n;
            CurrentGame.Reward = r;
            CurrentGame.Penalties[0] = d3;
            CurrentGame.Penalties[1] = d2;
            CurrentGame.Penalties[2] = d1;
            StartTimer(0.001);
        }

        private void ValidateZeroOne(object sender, RoutedEventArgs e) {
            if (sender is TextBox tb) {
                double d0 = Double.Parse(tb.Text);
                double d1 = d0;
                if (d1 < 0.0001) d1 = 0.0001;
                if (d1 > 0.9999) d1 = 0.9999;
                if (d0 != d1) tb.Text = "" + d1;
            }
        }

        private void ValidateDouble(object sender, RoutedEventArgs e) {
            if (sender is TextBox tb) {
                bool ok = Double.TryParse(tb.Text, out double d0);
                if (!ok) d0 = 0.0; 
                tb.Text = "" + d0;
            }
        }

        private void ValidatePositiveInt(object sender, RoutedEventArgs e) {
            if (sender is TextBox tb) {
                bool ok = UInt32.TryParse(tb.Text, out uint d0);
                if (!ok) d0 = 1000;
                tb.Text = "" + d0;
            }
        }
        private void LevelSelected(object sender, SelectionChangedEventArgs e) {
            if (LevelSelect.SelectedItem is string Level) {
                CurrentGame = new Game(Levels[Level]);
                InitializeAI();
                InitalizeDisplay();
                InitializeTimer();
                UpdateStaticGUI();

                UpdateDisplay();
                UpdateGUI();
            }
            Keyboard.Focus(GameGrid);
        }
        #endregion

        #region --- Helper Functions ---
        private void StartTimer(double seconds) {
            Timer.Stop();
            Timer.Interval = TimeSpan.FromSeconds(seconds);

            Timer.Start();
        }
        private void LearnStep(object sender, EventArgs e) {

            int LearnPhase = MaxNumIterations / 4;
            int LearnSteps = LearnPhase / 100;
            if (CurrentIteration < MaxNumIterations) {
                LearningAI.Learn(LearnSteps);
                CurrentIteration += LearnSteps;
            }

            if (CurrentIteration < LearnPhase) {
                LearningAI.LearningRate = 0.5;
                LearningAI.ExplorationRate = 1.0;
            } else if (CurrentIteration < 2 * LearnPhase) {
                LearningAI.LearningRate = 0.4;
                LearningAI.ExplorationRate = 0.7;
            } else if (CurrentIteration < 3 * LearnPhase) {
                LearningAI.LearningRate = 0.3;
                LearningAI.ExplorationRate = 0.5;
            } else if (CurrentIteration < 4 * LearnPhase) {
                LearningAI.LearningRate = 0.2;
                LearningAI.ExplorationRate = 0.3;
            } else {
                LearningAI.Learn(1);
                LearningAI.ExplorationRate = 0.0;
                LearningAI.LearningRate = 0.0;
                Timer.Interval = TimeSpan.FromSeconds(0.5);
            }

            UpdateDisplay();
            UpdateGUI();
        }
        private void SetMode(string Mode) {
            switch (Mode) {
                case "Playing":
                    Timer.Stop();
                    StatusBarParams.Visibility = Visibility.Hidden;
                    StatusBarRewards.Visibility = Visibility.Hidden;
                    TrainButton.IsEnabled = false;
                    DiscountRate.IsEnabled = false;
                    NumIterations.IsEnabled = false;
                    this.KeyUp += OnKeyUp;
                    break;
                case "AI":
                    StatusBarParams.Visibility = Visibility.Visible;
                    StatusBarRewards.Visibility = Visibility.Visible;
                    TrainButton.IsEnabled = true;
                    DiscountRate.IsEnabled = true;
                    NumIterations.IsEnabled = true;
                    this.KeyUp -= OnKeyUp;
                    break;
            }

            CurrentGame.Reset();
        }

        #endregion

        #region --- Private Members ---
        private int MaxNumIterations = 1000000;
        private int CurrentIteration = 0;

        private Game CurrentGame;

        private QLearningAI LearningAI;
        private DispatcherTimer Timer;
        private List<Rectangle> Tiles = new List<Rectangle>();
        private static Dictionary<string, string[]> Levels = new Dictionary<string, string[]> {

            ["Level 0"] =
            //Level1
            new string[] {
                "        ",
                " XXXXXX ",
                "S  T   G",
                " XXXXXX ",
                "        ",
                },
            ["Level 1"] =
            //Level2
            new string[] {
                "   T    ",
                "   T T  ",
                "S    T  ",
                " TT  T T",
                " T   T  ",
                " T   TTG",
                },
            ["Level 2"] =
            //Level2
            new string[] {
                "   T    ",
                " X X XX ",
                "ST T TT ",
                " X T XX ",
                " T X XX ",
                " T   TTG",
                },
        };
        #endregion


    }
}