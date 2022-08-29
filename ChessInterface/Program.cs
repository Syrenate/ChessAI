using System;
using BoardLibrary;
using System.Threading;

namespace ChessInterface
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            ChessBoard Board = new ChessBoard();
            Board.FEN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
            //Board.FEN = "8/8/8/8/8/8/8/8 w KQkq - 0 1";
            Board.Setup();

            Board.Draw();

            Move("e4");
            Move("d5");
            Move("Nc3");



            void Move(string move)
            {
                Thread.Sleep(1000);
                Board.Move(move);
                Board.Draw();
            }
        }
    }
}
