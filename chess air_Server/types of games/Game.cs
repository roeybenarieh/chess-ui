using chess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace chess_air_Server.types_of_games
{
    public class Game
    {
        internal Random rn = new Random();
        internal chessboard chessboard = new chessboard();

        internal ManageClient Mclient1;
        internal bool Mclient1white;

        internal void switch_player_turn()
        {
            this.Mclient1.changeturn();
        } //switch the indicator of the player current turn

        public virtual void GameMessageHandler(string messageReceived) // recieve message only from the client
        {
            return;// only implimentation in the inheritate objs.
        }
        public void savegame(int white_player_id=-1, int black_player_id=-1)
        {
            string w_player_id = white_player_id.ToString(); string b_player_id = black_player_id.ToString();
            if (black_player_id == -1)
                b_player_id = null;
            if(white_player_id == -1)
                w_player_id = null;
            DBH.insert_game(w_player_id, b_player_id, chessboard.get_all_played_moves(false));
        }
    }
}
