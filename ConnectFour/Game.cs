using AI;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Data.Common;
using System.Numerics;
using System.Windows.Media;

namespace TicTacToe
{
    public class Game : IGameState
    {
        //player 1 (X) = false, player2 (O) = true;
        public bool playerTurn { get; set; } = false;
        public bool gameWon { get; set; } = false;
        public bool gameDraw { get; set; } = false;
        public GameBoard Board { get; private set; }
        private Action[] allActions;
        public string CurrentPlayerSymbol { get; private set; }
        public double Reward { get; set; } = 1.0;
        public double Penalty { get; set; } = -1.0;
        public double SmallReward {get;set;} = 0.6;
        /// <summary>
        /// returns a unique State Id needed for QLearning
        /// </summary>
        public uint Id {
            get
            {
                Dictionary<string, uint> tileToValueMap = new Dictionary<string, uint>
                {
                    { "E", 0 },
                    { Board.Player1, 1 },
                    { Board.Player2, 2 }
                };
                uint result = 0;
                uint factor = 1;
                for(int i = 0; i < Board.Rows; i++)
                {
                    for(int j = 0; j < Board.Collumns; j++)
                    {
                        result += tileToValueMap[Board.GetTile(i, j)] * factor;
                        factor *= 3;
                    }
                }
                if (!CheckValidTileCount())
                {
                    throw new InvalidOperationException("Invalid state");
                }
                return result;
            }
        }
        /// <summary>
        /// Returns all possible actions at the current turn.
        /// </summary>
        public List<IAction> PossibleActions {
            get
            {
                List<IAction> possibleActions = new List<IAction>();
                foreach(Action action in allActions)
                {
                    if (Board.GetTile(action.position.X,action.position.Y).Equals("E"))
                    {
                        possibleActions.Add(action);
                    }
                }
                return possibleActions;
            }
        }
        /// <summary>
        /// creates a game with an empty board
        /// </summary>
        public Game()
        {
            Board = new GameBoard();
            InitializeAllActions();
        }
        /// <summary>
        /// initializes all action that you can do in the game
        /// </summary>
        public void InitializeAllActions()
        {
            allActions = new Action[Board.Rows*Board.Collumns];
            int x = 0;
            for (int i = 0; i < Board.Rows; i++)
            {
                for (int j = 0; j < Board.Collumns; j++)
                {
                    allActions[x] = new Action() { Name = $"x:{i}y:{j}", position = (X: i, Y: j) };
                    x++;
                }
            }
        }
        /// <summary>
        /// A game move where the current player sets a tile at the given postion.
        /// The function determines if the game has ended and at which conclusion after the tile has been set.
        /// </summary>
        /// <param name="x">X-Coordinate</param>
        /// <param name="y">Y-Coordinate</param>
        /// <returns>Depending on the conclusion of the game, a double
        /// value get returned, serving as a feedback for the QLearning AI</returns>
        /// <exception cref="InvalidOperationException">Can't set a tile if it is already occupied by another player</exception>
        public double Move (int x, int y)
        {
            double reward = 0.0;
            string currentPlayer = playerTurn ? Board.Player2 : Board.Player1;
            string opponentPlayer = playerTurn ? Board.Player1 : Board.Player2;
            if (!Board.CheckEmpty(x, y))
            {
                throw new InvalidOperationException("Tile is already set");
            }
            Board.SetTile(x, y, currentPlayer);
            // Encourages the AI to place 2 tiles in a row, coloum or diagonal
            if (ImmediateGoalAchieved(x, y, currentPlayer))
            {
                reward += SmallReward;
            }
            if (CouldWinNext(opponentPlayer))
            {
                // Penalizes AI if current move doesnt get blocked
                if (!BlocksWinningMove(x, y, opponentPlayer))
                {
                    reward = Penalty;
                }
            }
            // Rewards AI for winning
            if (HasWon())
            {
                gameWon = true;
                reward += Reward;
            }
            if(HasDrawn())
            {
                gameDraw = true;
            }
            if (!CheckValidTileCount())
            {
                throw new InvalidOperationException("Invalid state");
            }
            else
            {
                Turn();
                CurrentPlayerSymbol = currentPlayer;
            }
            return reward;
        }
        /// <summary>
        /// Checks whether the current move has archived the immeadiate goal of placing 2 tiles in row, column or diagonal
        /// </summary>
        /// <param name="x">X-Position of the Board</param>
        /// <param name="y">Y-Position of the Board</param>
        /// <param name="player">Checks for the given player</param>
        /// <returns>Returns false for not archiving the goal and true if the current move did</returns>
        private bool ImmediateGoalAchieved(int x, int y, string player)
        {
            // Check if the current move forms two in a row
            if (IsWinningLineAtRisk(x, player, checkRow: true))
            {
                return true;
            }

            // Check if the current move forms two in a column
            if (IsWinningLineAtRisk(y, player, checkRow: false))
            {
                return true;
            }

            // Check if the current move forms two in a diagonal, if applicable
            if ((x == y && IsDiagonalAtRisk(player, mainDiagonal: true)) ||
                (x + y == Board.Rows - 1 && IsDiagonalAtRisk(player, mainDiagonal: false)))
            {
                return true;
            }

            return false;
        }
        /// <summary>
        /// Checks whether the current move has blocked the winning move of the opponent in the row, column and diagonals. 
        /// </summary>
        /// <param name="x">X-Position of set tile</param>
        /// <param name="y">X-Position of set tile</param>
        /// <param name="opponentPlayer"></param>
        /// <returns>returns false if it didnt block a potential winning move of the opponent player and returns true if it did</returns>
        private bool BlocksWinningMove(int x, int y, string opponentPlayer)
        {
            // checks row
            if (IsWinningLineAtRisk(x, opponentPlayer, checkRow: true))
                return true;

            // checks collumn
            if (IsWinningLineAtRisk(y, opponentPlayer, checkRow: false))
                return true;

            // checks diagonals if set tile is in the middle
            bool onMainDiagonal = x == y;
            bool onOffDiagonal = x + y == Board.Rows - 1;
            if (onMainDiagonal && IsDiagonalAtRisk(opponentPlayer, mainDiagonal: true) ||
                onOffDiagonal && IsDiagonalAtRisk(opponentPlayer, mainDiagonal: false))
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// Checks whether the current row or collumn at the given index is at risk of being the winning line of opponent
        /// </summary>
        /// <param name="index">Row or collumn index which should get checked</param>
        /// <param name="opponentPlayer"></param>
        /// <param name="checkRow">Whether one wants to check the row or the collumn</param>
        /// <returns></returns>
        private bool IsWinningLineAtRisk(int index, string opponentPlayer, bool checkRow)
        {
            int opponentCount = 0;
            for (int i = 0; i < Board.Collumns; i++)
            {
                string tile = checkRow ? Board.GetTile(index, i) : Board.GetTile(i, index);
                if (tile == opponentPlayer) opponentCount++;
            }
            return opponentCount == 2;
        }
        /// <summary>
        /// checks if the diagonals are at risk of being the winning line of the opponent
        /// </summary>
        /// <param name="opponentPlayer"></param>
        /// <param name="mainDiagonal">whether the main diagonal or the off diagonal should get checked</param>
        /// <returns></returns>
        private bool IsDiagonalAtRisk(string opponentPlayer, bool mainDiagonal)
        {
            int opponentCount = 0;
            for (int i = 0; i < Board.Rows; i++)
            {
                string tile = mainDiagonal ? Board.GetTile(i, i) : Board.GetTile(i, Board.Rows - 1 - i);
                if (tile == opponentPlayer) opponentCount++;
            }
            return opponentCount == 2;
        }
        /// <summary>
        /// Checks whether the opponent could win in the next round
        /// </summary>
        /// <param name="player"></param>
        /// <returns>returns true if there is a winning chance and false if there </returns>
        private bool CouldWinNext(string player)
        {
            for (int i = 0; i < Board.Rows; i++)
            {
                if (CheckRowForWinningChance(i, player))
                {
                    return true;
                }
            }
            for (int j = 0; j < Board.Collumns; j++)
            {
                if (CheckColumnForWinningChance(j, player))
                {
                    return true;
                }
            }
            if (CheckDiagonalsForWinningChance(player))
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// Checks both diagonals for a winning chance of the given player
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        private bool CheckDiagonalsForWinningChance(string player)
        {
            //checks main diagonal
            int playerCount = 0;
            int emptyCount = 0;
            for (int i = 0; i < Board.Rows; i++)
            {
                if (Board.GetTile(i, i).Equals(player)) {
                    playerCount++;
                }
                else if (Board.GetTile(i, i).Equals(GameBoard.EMPTY)) {
                    emptyCount++; 
                }
            }
            if (playerCount == 2 && emptyCount == 1)
            {
                return true;
            }
            // checks off diagonal
            playerCount = 0;
            emptyCount = 0;
            for (int i = 0; i < Board.Rows; i++)
            {
                if (Board.GetTile(i, Board.Rows - 1 - i).Equals(player))
                {
                    playerCount++;
                }
                else if (Board.GetTile(i, Board.Rows - 1 - i).Equals(GameBoard.EMPTY))
                {
                    emptyCount++; 
                }
            }
            return playerCount == 2 && emptyCount == 1;
        }
        /// <summary>
        /// Checks whether the collumn at hand has a winning chance for the given player.
        /// </summary>
        /// <param name="collumnIndex"></param>
        /// <param name="player"></param>
        /// <returns></returns>
        private bool CheckColumnForWinningChance(int collumnIndex, string player)
        {
            int playerCount = 0;
            int emptyCount = 0;
            for (int i = 0; i < Board.Rows; i++)
            {
                if (Board.GetTile(i, collumnIndex).Equals(player))
                {
                    playerCount++;
                }
                else if (Board.GetTile(i, collumnIndex).Equals(GameBoard.EMPTY))
                {
                    emptyCount++;
                }
            }
            return playerCount == 2 && emptyCount == 1;
        }
        /// <summary>
        /// Checks whether the row at hand has a winning chance for the given player.
        /// </summary>
        /// <param name="i"></param>
        /// <param name="player"></param>
        /// <returns></returns>
        private bool CheckRowForWinningChance(int i, object player)
        {
            int playerCount = 0;
            int emptyCount = 0;
            for (int j = 0; j < Board.Collumns; j++)
            {
                if (Board.GetTile(i, j).Equals(player))
                {
                    playerCount++;
                }
                else if (Board.GetTile(i, j).Equals("E"))
                {
                    emptyCount++;
                }
                    
            }
            return playerCount == 2 && emptyCount == 1;
        }
        /// <summary>
        /// Checks if the game has been drawn (board full).
        /// </summary>
        /// <returns></returns>
        public bool HasDrawn() {
            int counter = 0;
            for (int i = 0;i < Board.Rows;i++)
            {
                for(int j = 0;j < Board.Collumns; j++)
                {
                    if(!Board.GetTile(i, j).Equals("E"))
                    {
                        counter++;
                    }
                }
            }
            return counter == 9 && gameWon == false ? true : false;
        }
        /// <summary>
        /// Checks the win for both players
        /// </summary>
        /// <returns></returns>
        public bool HasWon()
        {
            if (CheckWin(Board.Player1) || CheckWin(Board.Player2))
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// Checks all win conditions. 
        /// </summary>
        /// <param name="symbol">Player Symbol</param>
        /// <returns></returns>
        private bool CheckWin(string symbol)
        {
            string victoryCondition = symbol + symbol + symbol;
            string combination;
            // checks all rows
            for (int i = 0; i < Board.Rows; i++)
            {
                combination = "";
                for (int j = 0; j < Board.Collumns; j++)
                {
                    combination += Board.GetTile(i, j);
                }
                if (combination.Equals(victoryCondition))
                {
                    return true;
                }
            }
            // checks all collumns
            for (int i = 0; i < Board.Collumns; i++)
            {
                combination = "";
                for (int j = 0; j < Board.Rows; j++)
                {
                    combination += Board.GetTile(j, i);
                }
                if (combination.Equals(victoryCondition))
                {
                    return true;
                }
            }
            // checks the main diagonal
            combination = "";
            for (int i = 0; i < Board.Collumns; i++)
            {
                combination += Board.GetTile(i, i);
                if (i == Board.Collumns - 1 && combination.Equals(victoryCondition))
                {
                    return true;
                }
            }
            // checks the off diagonal
            combination = "";
            int x = Board.Collumns - 1;
            int y = 0;
            while (x >= 0 && y <= Board.Collumns)
            {
                combination += Board.GetTile(y, x);
                if (x == 0 && combination.Equals(victoryCondition))
                {
                    return true;
                }
                x--;
                y++;
            }
            return false;
        }
        /// <summary>
        /// In TicTacToe, X's have to be equal or be greater by one. If this isn't the case
        /// it is not TicTacToe. This function checks for a valid Tile Count.
        /// </summary>
        /// <returns>returns true if valid and false if invalid</returns>
        public bool CheckValidTileCount()
        {
            int countX = 0;
            int countO = 0;

            for (int i = 0; i < Board.Rows; i++)
            {
                for (int j = 0; j < Board.Collumns; j++)
                {
                    if (Board.GetTile(i,j) == Board.Player1)
                    {
                        countX++;
                    }
                    else if (Board.GetTile(i, j) == Board.Player2)
                    {
                        countO++;
                    }
                }
            }
            // the count of Xs has to be equal or greater by one to be a valid TicTacToe Game
            return countX == countO || countX == countO + 1;
        }
        /// <summary>
        /// Resets the board and resets the turn back to player 1.
        /// </summary>
        public void Reset()
        {
            playerTurn = false;
            Board.EmptyBoard();
        }
        /// <summary>
        /// Executes a Action by using the move function depending on position information of the Actio.
        /// </summary>
        /// <param name="a"></param>
        /// <returns>a double value that represents the reward needed for the reward function</returns>
        /// <exception cref="ArgumentException">thrown if the given Action is not a valid action</exception>
        public double ExecuteAction(IAction a)
        {
            if(a is Action action)
            {
                return Move(action.position.X, action.position.Y);
            }
            throw new ArgumentException("Not a valid action!");
        }
        /// <summary>
        /// Changes the turn
        /// </summary>
        private void Turn()
        {
            this.playerTurn = !this.playerTurn;
        }
    }
    /// <summary>
    /// Action that stores name and position
    /// </summary>
    class Action : IAction
    {
        public string Name { get; set; }
        public (int X, int Y) position { get; set; }
        public Action(string name, (int X, int Y) position)
        {
            Name = name;
            this.position = position;
        }
        public Action()
        {

        }
        public override string ToString()
        {
            return Name;
        }
    }
}
