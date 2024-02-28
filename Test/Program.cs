using ConnectFour;
using System;
using Test;

class Program
{
    static void Main(string[] args)
    {
        Game game = new Game();
        GameBoard gameBoard = game.board;
        for (int i = 0; i < gameBoard.Board.GetLength(0); i++)
        {
            for (int j = 0; j < gameBoard.Board.GetLength(1); j++)
            {
                Console.Write(gameBoard.Board[i, j] + "\t");
            }
            Console.WriteLine();
        }
        Console.WriteLine();
        game.move(0, 0);
        game.move(0, 1);
        game.move(0, 2);
        game.move(1, 2);
        game.move(1, 1);
        game.move(2, 1);
        game.move(2, 0);
        for (int i = 0; i < gameBoard.Board.GetLength(0); i++)
        {
            for (int j = 0; j < gameBoard.Board.GetLength(1); j++)
            {
                Console.Write(gameBoard.Board[i, j] + "\t");
            }
            Console.WriteLine();
        }
    }
}