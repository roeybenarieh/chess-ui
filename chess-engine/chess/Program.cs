using chess.types_of_peaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace chess
{
    class Program
    {
        //not used in the server
        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Stopwatch stopwatch = new Stopwatch(); stopwatch.Start();
            Program p = new Program();
            //p.event_handler();

            
            string fen = "r3k2r/Pppp1ppp/1b3nbN/nP6/BBP1P3/q4N2/Pp1P2PP/R2Q1RK1 w kq - 0 1";
            Chessboard board = new Chessboard(fen);
            Console.WriteLine("start position");
            board.Printstatics();

            
            //int nodes = board.generator.Perft(5);
            //Console.WriteLine(nodes+" leaf nodes");
            Move best_move = board.generator.Choose_move(depth: 5,board.white_turn);
            Console.WriteLine(best_move.To_mininal_string());
            
            Console.WriteLine("end position");
            board.Printstatics();
            

            stopwatch.Stop(); Console.WriteLine("Elapsed Time is {0} seconds", (float)stopwatch.ElapsedMilliseconds/1000);
            Console.ReadLine();
        }
        private void event_handler()
        {
            Chessboard board = new Chessboard();
            string input = Console.ReadLine();
            while (!input.Equals("done"))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                switch (input)
                {
                    case "d":
                        Console.WriteLine(board.ToString());
                        break;
                    case string s when s.StartsWith("go perft"):
                        int num = Int32.Parse(input.Substring(9));
                        Console.WriteLine(board.generator.Perft(num));
                        break;
                    case string s when s.StartsWith("position fen"):
                        input.Remove(0, 13);
                        board = new Chessboard(input);
                        break;
                    case "":
                        Console.WriteLine();
                        break;
                    default:
                        Console.WriteLine("unknown command: " + input);
                        break;
                }
                Console.ForegroundColor = ConsoleColor.White;
                input = Console.ReadLine();
            }
        }
    }
}
