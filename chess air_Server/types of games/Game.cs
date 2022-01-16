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
            return;
        }
    }
}
