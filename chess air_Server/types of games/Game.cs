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
        internal Chessboard chessboard = new Chessboard();
        public List<Move> potmoves = new List<Move>();
        public string moves ="";

        internal ManageClient Mclient1;
        internal bool Mclient1white;

        /// <summary>
        /// switch the first player's turn
        /// </summary>
        internal void switch_player_turn()
        {
            this.Mclient1.changeturn();
        } //switch the indicator of the player current turn

        /// <summary>
        /// virtual function to check messages related to the game
        /// </summary>
        /// <param name="messageReceived"></param>
        public virtual void GameMessageHandler(string messageReceived) // recieve message only from the client
        {
            return;// only implimentation in the inheritate objs.
        }
        /// <summary>
        /// virtual function that handle resignation
        /// </summary>
        /// <param name="resigned_client"></param>
        public virtual void ResignationHandler(ManageClient resigned_client) // recieve message only from the client
        {
            return;// only implimentation in the inheritate objs.
        }

        /// <summary>
        /// save this game to the database
        /// </summary>
        /// <param name="white_player_id"></param>
        /// <param name="black_player_id"></param>
        public void savegame(int white_player_id=-1, int black_player_id=-1)
        {
            if (!moves.Equals(""))
            {
                string w_player_id = white_player_id.ToString(); string b_player_id = black_player_id.ToString();
                DBH.insert_game(w_player_id, b_player_id, moves.Remove(moves.Length - 1));
            }
        }
        
        /// <summary>
        /// send all the legal moves in X,Y position's piece to the client
        /// </summary>
        /// <param name="messageReceived"></param>
        /// <param name="client"></param>
        internal void send_pot_moves(string messageReceived, ManageClient client)
        {
            messageReceived = messageReceived.Remove(0, 14); 
            int starti = char_to_int(messageReceived[0]);
            int startj = char_to_int(messageReceived[1]);
            string ans = "";
            if (this.chessboard.board[starti * Chessboard.board_size + startj].Isocupied()) //if the peace exists - if not, probably hackers ;-)
            {
                this.potmoves = this.chessboard.generator.Generate_legal_moves(this.chessboard.board[starti * Chessboard.board_size + startj].Peace);
                foreach (Move move in this.potmoves)
                {
                    ans += Chessboard.Get_i_pos(move.endsquare).ToString() + Chessboard.Get_j_pos(move.endsquare).ToString() + ",";
                }
                if (ans.EndsWith(",")) //if there are moves at all there has to be "," at the end of the string
                {
                    ans = ans.Remove(ans.Length - 1);
                    client.SendMessage("###posmoves###" + ans);
                }
            }
        }
        /// <summary>
        /// send a formated move message to a client
        /// </summary>
        /// <param name="client"></param>
        /// <param name="move_messageReceived"></param>
        /// <param name="edgecase"></param>
        internal void send_move(ManageClient client, string move_messageReceived, int edgecase)
        {
            client.SendMessage(move_messageReceived+"#"+edgecase);//send the white clients that the move has been made
        }

        /// <summary>
        /// function for checking if the move that the client wants to do is legal
        /// </summary>
        /// <param name="messageReceived"></param>
        /// <returns></returns>
        internal Move find_legal_move(string messageReceived)
        {
            messageReceived = messageReceived.Remove(0, 10);
            int starti = char_to_int(messageReceived[0]);
            int startj = char_to_int(messageReceived[1]);
            if (this.chessboard.board[starti * Chessboard.board_size + startj].Isocupied()) //if the peace exists - if not, probably hackers ;-)
            {
                int start_position = starti * 8 + startj;
                int end_position = char_to_int(messageReceived[2]) * 8 + char_to_int(messageReceived[3]);
                bool promotion=false;
                if (messageReceived.Length == 5)
                {//there is a promotion edgecase in the move
                    promotion = true;
                }
                foreach (Move move in this.potmoves)
                {
                    Console.WriteLine(move.To_mininal_string());
                    if (move.startsquare == start_position && move.endsquare == end_position) //check the move legality
                    {
                        if (promotion && move.edgecase != char_to_int(messageReceived[4]))
                            continue;
                        return move;
                    }
                }
            }
            return new Move(-1,-1);
        }

        internal int char_to_int(char c) { return c - '0'; }
    }
}
