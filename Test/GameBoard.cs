using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConnectFour
{
    public class GameBoard
    {
        public const int ROWS = 3;
        public const int COLUMNS = 3;
        private string[,] board = new string[ROWS,COLUMNS];
        public const string PLAYER1 = "X";
        public const string PLAYER2 = "O";
        public const string EMPTY = "E";
        public const int MAXTILES = ROWS * COLUMNS;
        public string[,] Board
        {
            get { return board; }
        }
        public GameBoard()
        {
            emptyBoard();
        }
        public GameBoard(string[,] board)
        {
            setBoard(board);
        }
        public void setBoard(string[,] board)
        {   
            if(!checkRows(board))
            {
                throw new Exception("Invalid Board, too few rows");
            }
            if (checkCollums(board))
            {
                throw new Exception("Invalid Board, too few collums");
            }
            if(checkInvalidBoardElements(board))
            {
                throw new Exception("Invalid Board, invalid board Elements");
            }
            this.board = board;
        }
        public void setTile(int x, int y, string tile)
        {
            if (!checkInputTile(tile))
            {
                throw new Exception("Invalid Tile");
            }
            this.board[x, y] = tile;
        }
        public String getTile(int x, int y)
        {
            return this.board[x, y];
        }
        public Boolean checkEmpty(int x, int y)
        {
            return board[x, y].Equals("E");
        }
        private bool checkInputTile(string tile)
        {
            return (tile.Equals(EMPTY) || tile.Equals(PLAYER1) || tile.Equals(PLAYER2));
        }
        private Boolean checkRows(string[,] board)
        {
            return board.GetLength(0) == ROWS;
        }
        private Boolean checkCollums(string[,] board)
        {
            return board.GetLength(1) == COLUMNS;
        }
        private Boolean checkInvalidBoardElements(string[,] board)
        {
            for(int i = 0; i < ROWS; i++)
            {
                for(int j=0; j < COLUMNS; j++)
                {
                    if (!(board[i, j].Equals(EMPTY) || board[i, j].Equals(PLAYER1) || board[i, j].Equals(PLAYER2))){
                        return false;
                    }
                }
            }
            return true;
        }
        public void emptyBoard()
        {
            for (int i = 0; i < ROWS; i++)
            {
                for (int j = 0; j < COLUMNS; j++)
                {
                    board[i, j] = EMPTY;
                }
            }
        }
    }
}
