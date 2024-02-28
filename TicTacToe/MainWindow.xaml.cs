using AI.QLearning;
using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using TicTacToe;
using Color = System.Windows.Media.Color;

namespace TicTacToe

{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private enum GameMode
        {
            Training,
            Play,
            AIvsBot,
        }
        private GameMode currentGameMode = GameMode.Training;
        private Game game;
        private List<Button> tiles;
        private QLearningAI player1;
        private QLearningAI player2;
        private QLearningAI bot;
        private int MaxNumIterations = 100000;
        private int CurrentIteration = 0;
        private DispatcherTimer LearnTimer;
        private DispatcherTimer BotTimer;
        private bool learningFinished = false;
        private Counter counter;
        private readonly double PlayTime = 2.0;
        private readonly double FastPlayTime = 0.0001;
        private bool FastPlayOn = false;
        private bool SetPlayTimeOnce;

        public SeriesCollection SeriesCollection { get; set; }


        public MainWindow()
        {
            InitializeComponent();
            InitializeStatistic();
            InitializeCounter();
            InitalizeGame();
            InitializeAI();
            InitializeTimer();
            InitializeGUI();
        }
        /// <summary>
        /// Initializes Statistics by adding Series Collection and custom X/Y Axis
        /// </summary>
        private void InitializeStatistic()
        {
            SeriesCollection = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Player 1 Wins",
                    Values = new ChartValues<int> { 0 }
                },
                new LineSeries
                {
                    Title = "Player 2 Wins",
                    Values = new ChartValues<int> { 0 }
                },
                new LineSeries
                {
                    Title = "Draws",
                    Values = new ChartValues<int> { 0 }
                }
            };
            MyCartesianChart.AxisY.Add(new Axis
            {
                MinValue = 0,
                Title = "Number of Wins",
                LabelFormatter = value => value.ToString()
            });
            MyCartesianChart.AxisX.Add(new Axis
            {
                MinValue = 0,
                Title = "100 Total Games played per Sample Point",
                LabelFormatter = value => value.ToString()
            });
            MyCartesianChart.Series = SeriesCollection;
        }

        private void ResetChart()
        {
            foreach (var series in SeriesCollection)
            {
                series.Values.Clear();
            }
            counter.Reset();
        }
        private void InitializeGUI()
        {
            tiles = GetAllTiles();
            DeactivateTiles();
            ModeBox.Items.Clear();
            ModeBox.Items.Add("AI vs AI");
            ModeBox.Items.Add("Player vs AI");
            ModeBox.Items.Add("AI vs Bot");
            RewardBox.Text = game.Reward.ToString();
            ImmeadiateRewardBox.Text = game.SmallReward.ToString();
            PenaltyBox.Text = game.Penalty.ToString();
            DiscountRate.Text = player1.DiscountRate.ToString();
            ModeBox.SelectedIndex = 0;
            ModeBox.SelectionChanged += ModeBoxChange;
            if (!learningFinished) { FastPlayCheckBox.Visibility = Visibility.Hidden; }
        }
        private void InitializeCounter()
        {
            counter = new Counter();
        }
        private void DeactivateTiles()
        {
            tiles.ForEach(tile => { tile.IsEnabled = false; });
        }
        private void ActivateTiles()
        {
            tiles.ForEach(tile => { tile.IsEnabled = true; });
        }
        /// <summary>
        /// Handles human player clicks, if Player vs. AI is selected and after
        /// each click, the AI shall make its move.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClickTile(object sender, RoutedEventArgs e)
        {
            if (currentGameMode == GameMode.Play)
            {
                Button btn = (Button)sender;
                int x = Grid.GetRow(btn);
                int y = Grid.GetColumn(btn);
                btn.IsEnabled = false;
                game.ExecuteAction(new Action("Human Action", (X: x, Y: y)));
                Counter();
                UpdateGUI();
                ResetGUIOnWin();
            }
            if (!game.gameWon && !game.gameDraw && game.playerTurn)
            {
                player2.Learn(1);
                Counter();
                UpdateGUI();
                ResetGUIOnWin();
            }
        }
        /// <summary>
        /// Switches to the given GameMode
        /// </summary>
        /// <param name="mode"></param>
        private void SwitchGameMode(GameMode mode)
        {
            currentGameMode = mode;
            game.Reset();
            ResetGUI();
            UpdateGUI();
            if (mode == GameMode.Training )
            {
                DeactivateTiles();
                StartLearnTimer(PlayTime);
            }
            if(mode == GameMode.Play)
            {
                StopAllTimers();
                ActivateTiles();
            }
            if(mode == GameMode.AIvsBot)
            {
                DeactivateTiles();
                StartBotTimer(PlayTime);
            }
        }
        private void ResetGUI()
        {
            game.gameWon = false;
            game.gameDraw = false;
            tiles.ForEach(button => { button.Content = " "; });
            if(currentGameMode == GameMode.Play)
            {
                ActivateTiles();
            }
        }
        private async void ResetGUIOnWin()
        {
            if (game.gameWon || game.gameDraw)
            {
                DeactivateTiles();
                if ((currentGameMode == GameMode.Play || currentGameMode == GameMode.AIvsBot || learningFinished) && !FastPlayOn)
                {
                    await Task.Delay(900);
                }
                game.playerTurn = false;
                game.Reset();
                ResetGUI();
            }
        }
        /// <summary>
        /// Updates the board each time, makes tiles unclickable after they were set
        /// </summary>
        private void UpdateGUI()
        {
            int x = 0;
            Progress.Value = CurrentIteration * 100 / (MaxNumIterations);
            for (int i = 0; i < game.Board.Rows; i++)
            {
                for(int j = 0; j < game.Board.Rows; j++)
                {
                    if(!game.Board.GetTile(i, j).Equals(GameBoard.EMPTY))
                    {
                       tiles[x].Content = game.Board.GetTile(i, j);
                       tiles[x].IsEnabled = false;
                    }
                    x++;
                }
            }
        }
        private void InitalizeGame()
        {
            game = new Game();
        }
        private void InitializeAI()
        {
            player1 = new QLearningAI() { GameState = game };
            player2 = new QLearningAI() { GameState = game };
            bot = new QLearningAI() { GameState = game };
        }
        /// <summary>
        /// Gets all the buttons/tiles and stores them in a List
        /// </summary>
        /// <returns>Returns a list of buttons</returns>
        private List<Button> GetAllTiles()
        {
            List<Button> list = new List<Button>();
            foreach (var child in GameGrid.Children)
            {
                if(child is Button button)
                {
                    list.Add((Button)child);
                }
            }
            return list;
        }
        private void InitializeTimer()
        {
            if (null != LearnTimer)
            {
                LearnTimer.Stop();
                BotTimer.Stop();
            }
            else
            {
                LearnTimer = new DispatcherTimer();
                BotTimer = new DispatcherTimer();
                LearnTimer.Tick += LearnStep;
                BotTimer.Tick += AIvsBotStep;
            }
        }

        private void AIvsBotStep(object? sender, EventArgs e)
        {
            QLearningAI currentAIPlayer = game.playerTurn ? bot : player1;
            player1.LearningRate = 0;
            player1.ExplorationRate = 0;
            bot.LearningRate = 0;
            bot.ExplorationRate = 0;
            currentAIPlayer.Learn(1);
            Counter();
            UpdateGUI();
            ResetGUIOnWin();
        }

        /// <summary>
        /// Gets called each tick of the timer. Each Iteration, both AI players will learn depending
        /// on learning and exploration rates.s
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LearnStep(object? sender, EventArgs e)
        {
            if (currentGameMode == GameMode.Training)
            {
                QLearningAI currentAIPlayer = game.playerTurn ? player2 : player1;
                if (CurrentIteration < MaxNumIterations)
                {
                    CurrentIteration++;
                }
                AdjustLearningAndExplorationRates();
                currentAIPlayer.Learn(1);
                Counter();
                UpdateGUI();
                ResetGUIOnWin();
            }
        }
        private void AdjustLearningAndExplorationRates()
        {
            double totalPhases = 4;
            double phaseDuration = MaxNumIterations / totalPhases;
            double currentPhase = Math.Floor(CurrentIteration / phaseDuration);

            // learning and exploration rates for each phase
            double[] learningRates = { 0.5, 0.4, 0.3, 0.2 };
            double[] explorationRates = { 1.0, 0.7, 0.5, 0.3 };

            // set rates according the current phase
            if (currentPhase < totalPhases)
            {
                player1.LearningRate = learningRates[(int)currentPhase];
                player1.ExplorationRate = explorationRates[(int)currentPhase];
                player2.LearningRate = learningRates[(int)currentPhase];
                player2.ExplorationRate = explorationRates[(int)currentPhase];
            }
            else
            {
                //if all phasses are complete, set them to zero and let the AIs play
                player1.LearningRate = 0;
                player1.ExplorationRate = 0;
                player2.LearningRate = 0;
                player2.ExplorationRate = 0;
                if (!SetPlayTimeOnce)
                {
                    SetPlayTimeOnce = true;
                    LearnTimer.Interval = TimeSpan.FromSeconds(PlayTime);
                }
                learningFinished = true;
                if (learningFinished)
                {
                    FastPlayCheckBox.Visibility = Visibility.Visible;
                }
            }
        }
        private void StartLearnTimer(double seconds)
        {
            StopAllTimers();
            LearnTimer.Interval = TimeSpan.FromSeconds(seconds);
            LearnTimer.Start();
        }
        private void StartBotTimer(double seconds)
        {
            StopAllTimers();
            BotTimer.Interval = TimeSpan.FromSeconds(seconds);
            BotTimer.Start();
        }
        private void StopAllTimers()
        {
            LearnTimer.Stop();
            BotTimer.Stop();
        }
        /// <summary>
        /// Handles Behaviour for the ComboBox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ModeBoxChange(object sender, SelectionChangedEventArgs e)
        {
            if (ModeBox.SelectedItem is string Mode)
            {
                FastPlayCheckBox.IsChecked = false;
                FastPlayOn = false;
                if(Mode.Equals("AI vs AI"))
                {
                    StartTrainingButton.Visibility = Visibility.Visible;
                    if (learningFinished) { FastPlayCheckBox.Visibility = Visibility.Visible; }
                    SwitchGameMode(GameMode.Training);
                }
                if(Mode.Equals("Player vs AI"))
                {
                    StartTrainingButton.Visibility = Visibility.Hidden;
                    FastPlayCheckBox.Visibility = Visibility.Hidden;
                    SwitchGameMode(GameMode.Play);
                }
                if(Mode.Equals("AI vs Bot"))
                {
                    StartTrainingButton.Visibility = Visibility.Hidden;
                    if (learningFinished) { FastPlayCheckBox.Visibility = Visibility.Visible; }
                    SwitchGameMode(GameMode.AIvsBot);
                }
                ResetChart();
            }
        }
        private void Counter()
        {
            if ((learningFinished || currentGameMode == GameMode.AIvsBot || currentGameMode == GameMode.Play) && (game.gameWon || game.gameDraw))
            {
                if (game.gameWon)
                {
                    if (game.playerTurn)
                    {
                        counter.WinCounterPlayer1++;
                        counter.LossCounterPlayer2++;
                    }
                    else
                    {
                        counter.WinCounterPlayer2++;
                        counter.LossCounterPlayer1++;
                    }
                }
                if (game.gameDraw)
                {
                    counter.DrawCounter++;
                }
                counter.GamesPlayedCounter++;
                if(counter.GamesPlayedCounter == 100 || counter.GamesPlayedCounter % 100 == 0) {
                    SeriesCollection[0].Values.Add(counter.WinCounterPlayer1);
                    SeriesCollection[1].Values.Add(counter.WinCounterPlayer2);
                    SeriesCollection[2].Values.Add(counter.DrawCounter);
                }
                if (currentGameMode == GameMode.Play)
                {
                    SeriesCollection[0].Values.Add(counter.WinCounterPlayer1);
                    SeriesCollection[1].Values.Add(counter.WinCounterPlayer2);
                    SeriesCollection[2].Values.Add(counter.DrawCounter);
                }
            }
        }
        /// <summary>
        /// Handles behaviour for the StartTraining Button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartTraining(object sender, RoutedEventArgs e)
        {
            ResetChart();
            InitializeAI();
            SetPlayTimeOnce = false;
            learningFinished = false;
            FastPlayOn = false;
            FastPlayCheckBox.IsChecked = false;
            FastPlayCheckBox.Visibility = Visibility.Hidden;
            CurrentIteration = 0;
            uint n = UInt32.Parse(NumberOfIterationsBox.Text);
            double r = Double.Parse(RewardBox.Text.Replace(",", "."), CultureInfo.InvariantCulture);
            double p = Double.Parse(PenaltyBox.Text.Replace(",", "."), CultureInfo.InvariantCulture);
            double sr = Double.Parse(ImmeadiateRewardBox.Text.Replace(",","."), CultureInfo.InvariantCulture);
            double dr = Double.Parse(DiscountRate.Text.Replace(",", "."), CultureInfo.InvariantCulture);
            player1.DiscountRate = dr;
            player2.DiscountRate = dr;
            MaxNumIterations = (int)n;
            game.Reward = r;
            game.SmallReward = sr;
            game.Penalty = p;
            StartLearnTimer(0.0001);
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if(learningFinished)
            {
                FastPlayOn = true;
                StopAllTimers();
                if(currentGameMode == GameMode.Training)
                {
                    StartLearnTimer(FastPlayTime);
                }
                if(currentGameMode == GameMode.AIvsBot)
                {
                    StartBotTimer(FastPlayTime);
                }
            }
        }
        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if(learningFinished)
            {
                FastPlayOn = false;
                StopAllTimers();
                if (currentGameMode == GameMode.Training)
                {
                    StartLearnTimer(PlayTime);
                }
                if (currentGameMode == GameMode.AIvsBot)
                {
                    StartBotTimer(PlayTime);
                }
            }
        }

        private void DiscountRate_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
