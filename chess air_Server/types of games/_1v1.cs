using chess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace chess_air_Server
{
    public class _1v1 : types_of_games.Game
    {
        private ManageClient Mclient2;

        public _1v1(ManageClient Mclient1, ManageClient Mclient2)
        {
            this.Mclient1 = Mclient1;
            this.Mclient2 = Mclient2;

            // send all of the initial information so the players coud start playing
            if (this.rn.Next(1)==0)
            {
                Mclient1.SendMessage("###start_game###" + Mclient1.get_nick() + "&" + Mclient2.get_nick() + "&" + "white"); this.Mclient1white = true;
                Mclient2.SendMessage("###start_game###" + Mclient2.get_nick() + "&" + Mclient1.get_nick() + "&" + "black"); Mclient2.my_turn = false;
            }
            else
            {
                Mclient1.SendMessage("###start_game###" + Mclient1.get_nick() + "&" + Mclient2.get_nick() + "&" + "black"); Mclient1.my_turn = false;
                Mclient2.SendMessage("###start_game###" + Mclient2.get_nick() + "&" + Mclient1.get_nick() + "&" + "white"); this.Mclient1white = false;
            }   
            Console.WriteLine("new game: " + Mclient1.get_nick() + " VS " + Mclient2.get_nick());
        }

        private ManageClient client_turn() // return the tcp of the current players turn to play
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

        private ManageClient oponent() // return the class of the oponent of the current players turn to play
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

        private void switch_players_turn()
        {
            base.switch_player_turn();
            Mclient2.changeturn();
        } //switch the indicator of the player current turn

        public override void GameMessageHandler(string messageReceived) // recieve message only from the client
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
                            Mclient1.SendMessage("###move###"+messageReceived);//send the white clients that the move has been made
                            Mclient2.SendMessage("###move###" +messageReceived);//send the black clients that the move has been made
                            this.chessboard.manualy_makemove(new Move(start_position, end_position,move.edgecase));
                            this.switch_players_turn();
                            Console.WriteLine(this.Mclient1.get_nick()+" VS "+this.Mclient2.get_nick()+"\n"+this.chessboard.ToString());
                            //check if the game ended:
                            //if the new players turn cant move anymore
                            if (this.chessboard.generator.Generatelegalmovesfrompseudolegal(this.chessboard.generator.generate_moves()).Count == 0) //the player cant move at all.
                            {
                                int kingpos = this.chessboard.getkingposition();
                                this.chessboard.switchplayerturn(); //so i could find out if the opponent attack the player
                                foreach(Move attackmove in this.chessboard.generator.generate_attacking_moves()) //check if the king in check - therfore checkmate.
                                {
                                    if (attackmove.endsquare == kingpos) 
                                    {
                                        if (this.Mclient1white == this.chessboard.whiteturn)
                                        {
                                            Mclient1.endgame("you won");
                                            Mclient2.endgame("you lost");
                                        }
                                        else
                                        {
                                            Mclient1.endgame("you lost");
                                            Mclient2.endgame("you won");
                                        }
                                        return;
                                    }
                                }
                                this.chessboard.switchplayerturn();
                                Mclient1.SendMessage("###endgame###draw");
                                Mclient2.SendMessage("###endgame###draw");
                            }
                        }
                    }
                }
            }
            else if (messageReceived.StartsWith("###pot_move###"))
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
                    if (ans.EndsWith(","))
                    {
                        ans = ans.Remove(ans.Length - 1);
                        client.SendMessage("###posmoves###" + ans);
                    }
                }
            }
        }

    }
}
