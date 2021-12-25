using chess;
using System;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace chess_air_Server
{
    public class _1v1
    {
        public ManageClient Mclient1; //TcpClient client1; string username1;
        public ManageClient Mclient2; //TcpClient client2; string username2;

        public String winner = "false";
        private Random rn = new Random();
        private chessboard chessboard = new chessboard();

        public _1v1(ManageClient Mclient1, ManageClient Mclient2)
        {
            this.Mclient1 = Mclient1;
            this.Mclient2 = Mclient2;

            // send all of the initial information so the players coud start playing
            if (this.rn.Next(1)==0)
            {
                Mclient1.SendMessage("###start_game###" + Mclient1.get_nick() + "&" + Mclient2.get_nick() + "&" + "white");
                Mclient2.SendMessage("###start_game###" + Mclient2.get_nick() + "&" + Mclient1.get_nick() + "&" + "black"); Mclient2.my_turn = false;
            }
            else
            {
                Mclient1.SendMessage("###start_game###" + Mclient1.get_nick() + "&" + Mclient2.get_nick() + "&" + "black"); Mclient1.my_turn = false;
                Mclient2.SendMessage("###start_game###" + Mclient2.get_nick() + "&" + Mclient1.get_nick() + "&" + "white");
            }   
            Console.WriteLine("new game: " + Mclient1.get_nick() + " VS " + Mclient2.get_nick());
        }

        /*public void movement_handler(string messageReceived)
        {
            messageReceived = messageReceived.Remove(0, 10);
            String[] movedata = messageReceived.Split(',');
            //board[Convert.ToInt32(movedata[0]), Convert.ToInt32(movedata[1])] = player_to_number();
            //if_no_one_won();// in case of a drew
            //if_player_won(); // in case someone won
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
                    if (winner == "player1")
                    {
                        Mclient1.SendMessage("###win###You won!");
                        Mclient2.SendMessage("###win###" + Mclient1.get_nick() + " won");
                    }
                    else //winner == "player2"
                    {
                        Mclient1.SendMessage("###win###" + Mclient2.get_nick() + " won");
                        Mclient2.SendMessage("###win###You won!");
                    }
                    Mclient1.friendgame = null;
                    Mclient2.friendgame = null;
                    return;//end the game, exiting this obj;
                }
            }
        }*/

        public ManageClient client_turn() // return the tcp of the current players turn to play
        {
            if (Mclient1.my_turn)
            {
                return Mclient1;
            }
            else// (Mclient2.my_turn ==true)
            {
                return Mclient2;
            }
        }

        public ManageClient oponent() // return the class of the oponent of the current players turn to play
        {
            if (Mclient1.my_turn)
            {
                return Mclient2;
            }
            else // (Mclient2.my_turn ==true)
            {
                return Mclient1;
            }
        }
        //

        public void switch_players_turn()
        {
            Mclient1.changeturn();
            Mclient2.changeturn();
        } //switch the indicator of the player current turn

        public void ReceiveMessage(string messageReceived) // recieve message only from the client
        {
            int char_to_int(char c) { return c - '0'; }

            ManageClient client = client_turn();
            if (messageReceived.StartsWith("###move###")) //the client propose to make a move
            { // XYXY format. the first XY is the start position and the second XY is the end position.
                messageReceived = messageReceived.Remove(0, 10);
                int starti = char_to_int(messageReceived[0]);
                int startj = char_to_int(messageReceived[1]);
                if (this.chessboard.board[starti, startj].isocupied()) //if the peace exists - if not, probably hackers ;-)
                {
                    int start_position = starti * 8 + startj;
                    int end_position = char_to_int(messageReceived[2]) * 8 + char_to_int(messageReceived[3]);
                    foreach(Move move in client.potmoves)
                    {
                        if(move.startsquare == start_position && move.endsquare == end_position) //check the move legality
                        {
                            Mclient1.SendMessage("###move###"+messageReceived);//send the clients that the move has been made
                            Mclient2.SendMessage("###move###" +messageReceived);
                            this.chessboard.manualy_makemove(new Move(start_position, end_position,move.edgecase));
                            switch_players_turn();
                            Console.WriteLine(this.chessboard.ToString());
                        }
                    }
                }
            }
            if (messageReceived.StartsWith("###pot_move###"))
            { //message: ###pot_move###XY
                messageReceived = messageReceived.Remove(0, 14);
                int starti = char_to_int(messageReceived[0]);
                int startj = char_to_int(messageReceived[1]);
                string ans = "";
                if (this.chessboard.board[starti,startj].isocupied()) //if the peace exists - if not, probably hackers ;-)
                {
                    client.potmoves = this.chessboard.generator.Generatelegalmovesfrompseudolegal(this.chessboard.generator.generate_moves(this.chessboard.board[starti, startj].Peace));
                    foreach (Move move in client.potmoves)
                    {
                        ans += chessboard.get_i_pos(move.endsquare).ToString() + chessboard.get_j_pos(move.endsquare).ToString() + ",";
                    }
                    client.SendMessage("###posmoves###" + ans.Remove(ans.Length - 1));
                }
            }
        }

    }
}
