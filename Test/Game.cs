using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using AI;
using ConnectFour;

namespace Test
{
    public class Game
    {
        //player 1 (X) = false, player2 (O) = true;
        public Boolean playerTurn = false;
        public GameBoard board { get; private set; }

        public Game(string[,] board)
        {
            this.board = new GameBoard(board);
        }
        public Game()
        {
            this.board = new GameBoard();
        }
        public void move(int x, int y)
        {
            if (!board.checkEmpty(x,y))
            {
                throw new InvalidOperationException("Tile is already set");
            }
            if(!playerTurn)
            {
                board.setTile(x, y, GameBoard.PLAYER1);
            }
            else
            {
                board.setTile(x,y, GameBoard.PLAYER2);
            }
            if(!won())
            { 
                turn();
            }
        }
        public Boolean won() {
            if (checkWin(GameBoard.PLAYER1) || checkWin(GameBoard.PLAYER2))
            {
                String player = this.playerTurn ? "Player2" : "Player1";
                Console.WriteLine(player +" "+ "wins");
                Environment.Exit(0);
            }
            return false;
        }
        public Boolean checkWin(String symbol)
        {
            String combination = "";
            String victoryCondition = symbol+symbol+symbol;
            for (int i = 0; i < GameBoard.ROWS;  i++)
            {
                combination = "";
                for (int j = 0; j < GameBoard.COLUMNS; j++)
                {
                    combination += board.getTile(i,j);
                }
                if (combination.Equals(victoryCondition))
                {
                    return true;
                }
            }
            for (int i = 0; i < GameBoard.COLUMNS; i++)
            {
                combination = "";
                for (int j = 0; j < GameBoard.ROWS; j++)
                {
                    combination += board.getTile(j, i);
                }
                if (combination.Equals(victoryCondition))
                {
                    return true;
                }
            }
            combination = "";
            for (int i = 0; i < GameBoard.COLUMNS; i++)
            { 
                combination += board.getTile(i,i);
                if(i == GameBoard.COLUMNS-1 && combination.Equals(victoryCondition))
                {
                    return true;
                }
            }
            combination = "";
            int x = GameBoard.COLUMNS - 1;
            int y = 0;
            while (x >= 0 && y <= GameBoard.COLUMNS)
            {
                combination += board.getTile(y, x);
                if (x == 0 && combination.Equals(victoryCondition))
                {
                    return true;
                }
                x--;
                y++;
            }
            return false;
        }
        private void turn()
        {
            this.playerTurn = !this.playerTurn;
        }
    }
}
