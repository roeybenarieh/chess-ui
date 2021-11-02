using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace Server
{
    class _1v1// can add:
        //what happen if a player gets out at the middle of a game
        //giving score
        //show whos turn is it
    {

        public ManageClient Mclient1; //TcpClient client1; string username1;
        public ManageClient Mclient2; //TcpClient client2; string username2;

        private byte[] data;
        public String winner = "false";
        public String player_turn ="player1";
        public int[,] board = new int[7, 6];

        public _1v1(ManageClient Mclient1, ManageClient Mclient2)
        {
            this.Mclient1 = Mclient1; 
            this.Mclient2 = Mclient2;
            data = new byte[Mclient1.get_tcp().ReceiveBufferSize];

            for (int i = 0; i < 7; i++)// טור
            {
                for (int j = 0; j < 6; j++)// שורה
                {
                    board[i, j] = 0;
                }
            }
            // send all of the initial information so the players coud start playing
            Mclient1.SendMessage("###start_game###" + Mclient1.get_nick() + "&" + Mclient2.get_nick() + "&" + "green" + "&" + "yes");
            Mclient2.SendMessage("###start_game###" + Mclient2.get_nick() + "&" + Mclient1.get_nick() + "&" + "red" + "&" + "no");
            Console.WriteLine("new game: "+ Mclient1.get_nick()+" VS "+ Mclient2.get_nick());
        }
        
        public void movement_handler(string messageReceived)
        {
            messageReceived = messageReceived.Remove(0, 10);
            String[] movedata = messageReceived.Split(',');
            board[Convert.ToInt32(movedata[0]), Convert.ToInt32(movedata[1])] = player_to_number();
            if_no_one_won();// in case of a drew
            if_player_won(); // in case someone won
            oponent().SendMessage("###move###" + messageReceived);
            if (winner == "false")
            {
                switch_player_turn();
            }
            else
            {
                if (winner == "no_one")
                {
                    Mclient1.SendMessage("###draw###");
                    Mclient2.SendMessage("###draw###");
                }
                else
                {// send who is the winner
                    if(winner == "player1")
                    {
                        Mclient1.SendMessage("###win###You won!");
                        Mclient2.SendMessage("###win###" + Mclient1.get_nick() + " won");
                    }
                    else //winner == "player2"
                    {
                        Mclient1.SendMessage("###win###" + Mclient2.get_nick() + " won");
                        Mclient2.SendMessage("###win###You won!");
                    }
                    Mclient1.end_game();
                    Mclient2.end_game();
                    return;//end the game, exiting this class/obj
                }
            }
        }

        public TcpClient client_turn() // return the tcp of the current players turn to play
        {
            if (player_turn == "player1")
            {
                return Mclient1.get_tcp();
            }
            else// (player_turn == "player2")
            {
                return Mclient2.get_tcp();
            }
        }

        public ManageClient oponent() // return the class of the oponent of the current players turn to play
        {
            if (player_turn == "player1")
            {
                return Mclient2;
            }
            else// (player_turn == "player2")
            {
                return Mclient1;
            }
        }
        //

        public void switch_player_turn(){
            if(player_turn == "player1")
            {
                player_turn = "player2";
            }
            else if (player_turn == "player2")
            {
                player_turn = "player1";
            }
        } //switch the indicator of the player current turn

        public int player_to_number()
        {
            if (player_turn == "player1")
            {
                return 1;
            }
            else //player_turn == "player2"
            {
                return 2;
            }
        } // return the number of the current players number indicator, for the board in the server

        public void if_player_won()
        {
            int count_columns = 0;
            int count_rows = 0;
            for (int i = 0; i < 7; i++)// טור
            {
                for (int j = 0; j < 6; j++)// שורה
                {
                    if(board[i, j] == player_to_number()) //columns
                    {
                        count_columns++;
                        if (count_columns >= 4)
                        {
                            winner = player_turn;
                            return;
                        }
                    }
                    else
                    {
                        count_columns = 0;
                    }
                }
            }// check for winning rows and columns
            for (int i = 0; i < 6; i++)// טור
            {
                for (int j = 0; j < 7; j++)// שורה
                {
                    if (board[j,i] == player_to_number()) //rows
                    {
                        count_rows++;
                        if (count_rows >= 4)
                        {
                            winner = player_turn;
                            return;
                        }
                    }
                    else
                    {
                        count_rows = 0;
                    }
                }
            }
            diagonalOrder(board);// check for winning diagonal
        } //check if player won, if so, the parameter "winner" contane the name of the winner will appere

        public void diagonalOrder(int[,] matrix)
        {
            int ROW = matrix.GetLength(0);
            int COL = matrix.GetLength(1);
            // There will be ROW+COL-1 lines in the output
            for (int line = 1; line <= (ROW + COL - 1); line++)
            {

                // Get column index of the first element
                // in this line of output.The index is 0
                // for first ROW lines and line - ROW for
                // remaining lines
                int start_col = max(0, line - ROW);

                // Get count of elements in this line. The
                // count of elements is equal to minimum of
                // line number, COL-start_col and ROW
                int count = min(line, Math.Min((COL - start_col), ROW));
                if (count >= 4) { // if there are at least 4 elemnts to come through
                    int count_sequence_down_to_up = 0;
                    int count_sequence_up_to_down = 0;
                    for (int j = 0; j < count; j++)
                    {
                        if (matrix[min(ROW, line) - j - 1, start_col + j] == player_to_number())//down to up
                        {
                            count_sequence_down_to_up++;
                            if (count_sequence_down_to_up >= 4)
                            {
                                winner = player_turn;
                                return;
                            }
                        }
                        else
                        {
                            count_sequence_down_to_up = 0;
                        }
                        //////////////////////////////////////////////////////////////////////////////
                        //int tmp2 = - (min(ROW, line) - j - 1);
                        if (matrix[matrix.GetLength(0)-1 - (min(ROW, line) - j -1), start_col + j] == player_to_number())//up to down
                        {
                            count_sequence_up_to_down++;
                            if (count_sequence_up_to_down >= 4)
                            {
                                winner = player_turn;
                                return;
                            }
                        }
                        else
                        {
                            count_sequence_up_to_down = 0;
                        }
                    }
                }
                /*
                // Print elements of this line
                for (int j = 0; j < count; j++)
                    Console.Write(matrix[min(ROW, line) - j - 1, start_col + j] + " ");
                */
            }
        }//https://www.geeksforgeeks.org/zigzag-or-diagonal-traversal-of-matrix/

        static int min(int a, int b)
        {
            return (a < b) ? a : b;
        }
        static int min(int a, int b, int c)
        {// A utility function to find min
         // of three integers
            return min(min(a, b), c);
        }
        static int max(int a, int b)
        {// A utility function to find max
         // of two integers
            return (a > b) ? a : b;
        }

        public void if_no_one_won()
        {
            Boolean canplay = false;
            for (int j = 0; j < 7; j++)
            {
                if(board[j, 0] == 0)
                    canplay = true;
            }
            if (! canplay)
                winner = "no_one";
        }


    }
}
