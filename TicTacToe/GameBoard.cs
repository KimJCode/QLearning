using System;
using System.Windows.Controls.Primitives;

namespace TicTacToe
{
    public class GameBoard
    {
        public int Rows { get; set; } = 3;
        public int Collumns { get; set; } = 3;
        private string[,] board;
        public string Player1 { get; set; } = "X";
        public string Player2 { get; set; } = "O";
        public const string EMPTY = "E";
        public int MaxTiles {
            get {
                return Rows * Collumns;
            }
        }
        public string[,] Board
        {
            get { return board; }
        }
        /// <summary>
        /// Creates an empty 3x3 TicTacToe Gameboard.
        /// </summary>
        public GameBoard()
        {
            EmptyBoard();
        }
        /// <summary>
        /// Creates an empty custom GameBoard.
        /// </summary>
        public GameBoard(int rows, int collums, string player1, string player2)
        {
            Player1 = player1;
            Player2 = player2;
            Rows = rows;
            Collumns = collums;
            EmptyBoard();
        }
        /// <summary>
        /// Creates an custom given Gameboard
        /// </summary>
        /// <param name="board">String Array representing game board</param>
        public GameBoard(string[,] board, string player1, string player2)
        {
            Player1 = player1;
            Player2 = player2;
            Rows = board.GetLength(0);
            Collumns = board.GetLength(1);
            SetBoard(board);
        }
        /// <summary>
        /// Sets String array and checks the board for correctness.
        /// Should only contain given player symbols and E for empty spaces.
        /// </summary>
        /// <param name="board"></param>
        /// <exception cref="Exception">throws Exception if board is invalid</exception>
        private void SetBoard(string[,] board)
        {
            if (!CheckRows(board))
            {
                throw new Exception("Invalid Board, too few rows");
            }
            if (CheckCollums(board))
            {
                throw new Exception("Invalid Board, too few collums");
            }
            if (CheckInvalidBoardElements(board))
            {
                throw new Exception("Invalid Board, invalid board Elements");
            }
            this.board = board;
        }
        /// <summary>
        /// Sets tile to the given String Character. Should be player symbol or empty symbol.
        /// </summary>
        /// <param name="x">X-Coordinate of the board</param>
        /// <param name="y">Y-Coordinate of the board</param>
        /// <param name="tile">tile symbol that should be set to the board</param>
        /// <exception cref="Exception">throws Exception if tile is invalid</exception>
        public void SetTile(int x, int y, string tile)
        {
            if (!CheckInputTile(tile))
            {
                throw new Exception("Invalid Tile");
            }
            board[x, y] = tile;
        }
        /// <summary>
        /// Gets string character at the current position
        /// </summary>
        /// <param name="x">X-Coordinate of the board</param>
        /// <param name="y">Y-Coordinate of the board</param>
        /// <returns>returns tile string character at the current position</returns>
        public string GetTile(int x, int y)
        {
            return board[x, y];
        }
        /// <summary>
        /// Checks if the tile at the given position is empty
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>returns true, if it is empty and false if not</returns>
        public bool CheckEmpty(int x, int y)
        {
            return board[x, y].Equals(EMPTY);
        }
        /// <summary>
        /// Checks if the inputed tile is valid
        /// </summary>
        /// <param name="tile"></param>
        /// <returns>returns true if it is valid and false if not</returns>
        private bool CheckInputTile(string tile)
        {
            return (tile.Equals(EMPTY) || tile.Equals(Player1) || tile.Equals(Player2));
        }
        /// <summary>
        /// Checks if the board has correct rows
        /// </summary>
        /// <param name="board"></param>
        /// <returns>returns true if it is valid and false if not</returns>
        private bool CheckRows(string[,] board)
        {
            return board.GetLength(0) == Rows;
        }
        /// <summary>
        /// Checks if the board has correct collumns
        /// </summary>
        /// <param name="board"></param>
        /// <returns>returns true if it is valid and false if not</returns>
        private bool CheckCollums(string[,] board)
        {
            return board.GetLength(1) == Collumns;
        }
        /// <summary>
        /// Checks if the board has valid  lements
        /// </summary>
        /// <param name="board"></param>
        /// <returns>returns true if it is valid and false if not</returns>
        private bool CheckInvalidBoardElements(string[,] board)
        {
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Collumns; j++)
                {
                    if (!(board[i, j].Equals(EMPTY) || board[i, j].Equals(Player1) || board[i, j].Equals(Player2)))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        /// <summary>
        /// Empties board or rather replaces all characters with the Empty character.
        /// </summary>
        public void EmptyBoard()
        {
            board = new string[Rows,Collumns]; 
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Collumns; j++)
                {
                    board[i, j] = EMPTY;
                }
            }
        }
    }
}
