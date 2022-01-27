using chess;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace chess_air_Server.types_of_games
{
    internal class _1vsAI : types_of_games.Game
    {
        public _1vsAI(ManageClient Mclient1)
        {
            this.Mclient1 = Mclient1;

            // send all of the initial information so the players coud start playing
            if (this.rn.Next(1) == 0)
            {
                Mclient1.SendMessage("###start_game###" + Mclient1.get_nick() + "&" + "AI" + "&" + "white");
                Mclient1.my_turn = true;
                this.Mclient1white = true;
            }
            else
            {
                Mclient1.SendMessage("###start_game###" + Mclient1.get_nick() + "&" + "AI" + "&" + "black"); 
                Mclient1.my_turn = false;
                this.Mclient1white = false;
            }
            Console.WriteLine("new game: " + Mclient1.get_nick() + " VS AI");
        }

        public override void GameMessageHandler(string messageReceived) // recieve message only from the client
        {
            if (messageReceived.StartsWith("###move###")) //the client propose to make a move
            { // XYXY format. the first XY is the start position and the second XY is the end position.
                Move move = base.find_legal_move(messageReceived);
                if (move.startsquare != -1) //found a ligal move
                {
                    Mclient1.SendMessage(messageReceived);//send the white clients that the move has been made
                    this.chessboard.manualy_makemove(move);
                    Console.WriteLine(this.Mclient1.get_nick() + " VS AI\n" + this.chessboard.ToString());
                    //check if the game ended:
                    //if the new players turn cant move anymore
                    if (this.chessboard.generator.generate_all_legal_moves().Count == 0) //the player cant move at all.
                    {
                        if (this.chessboard.current_player_king_in_check()) //checkmate
                        {
                            if (this.Mclient1white == this.chessboard.whiteturn) //its Mclient1white current turn to move
                            {
                                Mclient1.endgame("you lost");
                            }
                            else
                            {
                                Mclient1.endgame("you won");
                            }
                        }
                        else //draw
                        {
                            Mclient1.SendMessage("###endgame###draw");
                        }
                        if (Mclient1white)
                            this.savegame(white_player_id: this.Mclient1.client_id);
                        else
                            this.savegame(black_player_id: this.Mclient1.client_id);
                    }
                    else //the AI needs to make a move...
                    {
                        Stopwatch stopwatch = new Stopwatch(); stopwatch.Start();
                        Move aimove = this.chessboard.generator.choose_move(5);
                        this.chessboard.manualy_makemove(aimove);
                        stopwatch.Stop(); Console.WriteLine(this.chessboard.ToString() + "\n" + this.Mclient1.get_nick() + " VS AI, found a move in {0} miliseconds", (float)stopwatch.ElapsedMilliseconds / 1000);
                        //this might be wrong and it shoud be for a set1,2,3,4 to send 2,1,4,3
                        Mclient1.SendMessage("###move###"+ chessboard.get_i_pos(aimove.startsquare) + chessboard.get_j_pos(aimove.startsquare)
                            + chessboard.get_i_pos(aimove.endsquare) + chessboard.get_j_pos(aimove.endsquare));
                    }
                }
            }
            else if (messageReceived.StartsWith("###pot_move###"))
            { //message: ###pot_move###XY
                send_pot_moves(messageReceived, this.Mclient1);
            }
        }
    }
}
