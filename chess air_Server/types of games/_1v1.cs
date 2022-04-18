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

        private void switch_players_turn()
        {
            base.switch_player_turn();
            Mclient2.changeturn();
        } //switch the indicator of the player current turn

        public override void GameMessageHandler(string messageReceived) // recieve message only from the client
        {
            ManageClient client = client_turn();
            if (messageReceived.StartsWith("###move###")) //the client propose to make a move
            { // XYXY format. the first XY is the start position and the second XY is the end position.
                Move move = base.find_legal_move(messageReceived);
                if (move.startsquare != -1) //found a ligal move
                {
                    base.send_move(Mclient1, messageReceived, move.edgecase);//send the white clients that the move has been made
                    base.send_move(Mclient2, messageReceived, move.edgecase);//send the white clients that the move has been made
                    this.chessboard.manualy_makemove(move);
                    this.switch_players_turn();
                    Console.WriteLine(this.Mclient1.get_nick() + " VS " + this.Mclient2.get_nick() + "\n" + this.chessboard.ToString());
                    //check if the game ended:
                    //if the new players turn cant move anymore
                    if (this.chessboard.generator.generate_all_legal_moves().Count == 0) //the player cant move at all.
                    {
                        if (this.chessboard.current_player_king_in_check()) //checkmate
                        {
                            if (this.Mclient1white == this.chessboard.white_turn) //its Mclient1white current turn to move
                            {
                                Mclient1.endgame("you lost");
                                Mclient2.endgame("you won");
                            }
                            else
                            {
                                Mclient1.endgame("you won");
                                Mclient2.endgame("you lost");
                            }
                        }
                        else //draw
                        {
                            Mclient1.endgame("draw");
                            Mclient2.endgame("draw");
                        }
                        if (Mclient1white)
                            base.savegame(this.Mclient1.client_id, this.Mclient2.client_id);
                        else
                            base.savegame(this.Mclient2.client_id, this.Mclient1.client_id);
                    }
                }
            }
            else if (messageReceived.StartsWith("###pot_move###"))
            { //message: ###pot_move###XY
                send_pot_moves(messageReceived, client);
            }
            else if (messageReceived.Equals("###resignation###")) 
            {

            }
        }

        public override void resignationHandler(ManageClient resigned_client)
        {
            resigned_client.endgame("you resigned");
            (this.Mclient1.Equals(resigned_client) ? Mclient2 : Mclient1).endgame("you won,\r\nother player resigned");

        }

    }
}
