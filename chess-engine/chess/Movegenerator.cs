using chess.types_of_peaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace chess
{
    //structer for holding a move
    public struct Move
    {
        public readonly int startsquare;
        public readonly int endsquare;
        public Peace capturedpeace;
        public int edgecase;
        /*edge case to number:
         *1 - pawn promote to knight
         *2 - pawn promote to bishop
         *3 - pawn promote to rook
         *4 - pawn promote to queen
         *5 - castle
         *7 - king moved, cant castle any more
         *8 - rook moved, cant castle at that side anymove
        */

        // the user create new move, when making the move its cheked if it is a ligal move and if so 'capturedpeace' is added if necesary
        public Move(int startsquare, int endsquare, int edgecase_num = 0)
        {
            this.startsquare = startsquare;
            this.endsquare = endsquare;
            capturedpeace = null;
            this.edgecase = edgecase_num;
        }
        public const int None_edgcase = 0;
        public const int pawn_promote_to_knight = 1;
        public const int pawn_promote_to_bishop = 2;
        public const int pawn_promote_to_rook = 3;
        public const int pawn_promote_to_queen = 4;
        public const int castle = 5;
        public const int king_moving = 7; //the king hasnt moved once since the beggining of the game
        public const int rook_moving = 8; //the rook hasnt moved once since the beggining of the game
        public const int enpassant = 9;
        /// <summary>
        /// returns the string representation of the move
        /// </summary>
        /// <returns></returns>
        public string Print_in_notation()
        {
            string promotion_peace = "";
            if(this.edgecase !=0)
            {
                switch (this.edgecase)
                {
                    case pawn_promote_to_knight:
                        promotion_peace = "k";
                        break;
                    case pawn_promote_to_bishop:
                        promotion_peace = "b";
                        break;
                    case pawn_promote_to_rook:
                        promotion_peace = "r";
                        break;
                    case pawn_promote_to_queen:
                        promotion_peace = "q";
                        break;
                }
            }
            return "[" + Chessboard.Get_j_pos_as_letter(startsquare) + "," + Math.Abs(Chessboard.Get_i_pos(startsquare) - 8) + "]" + "[" + Chessboard.Get_j_pos_as_letter(endsquare) + "," + Math.Abs(Chessboard.Get_i_pos(endsquare) - 8) + "]"+ promotion_peace+": ";
        }
        /// <summary>
        /// return if the move is promotion
        /// </summary>
        /// <returns></returns>
        public bool Peacepromote()
        {
            if (this.edgecase != Move.None_edgcase && this.edgecase >= Move.pawn_promote_to_knight && this.edgecase <= Move.pawn_promote_to_queen)
                return true;
            return false;
        }
        public string To_mininal_string()
        {
            return (startsquare.ToString() +","+ endsquare.ToString()+"#"+edgecase);
        }
    }
    
    public class Movegenerator
    {
        public const Byte white = 0;
        public const Byte black = 1;
        private readonly Chessboard c;
        private readonly Move_list_ordering mo;
        public static int[][] squers_to_edge;
        //constractor.
        public Movegenerator(Chessboard chess)
        {
            this.c = chess;
            this.mo = new Move_list_ordering(chess);
            //generate the precomputed squers to edge //very not ifficient but it doesnt matter that much because this operation only happens once and because Im lazy
            squers_to_edge = new int[directions.Length][];
            for (int i =0; i < directions.Length; i++)
            {
                squers_to_edge[i] = new int[Chessboard.amount_of_sq];
                for(int v = 0; v < Chessboard.board_size; v++) //{ 1, -1, -8, 8, -7, -9, 9, 7 }
                {
                    for (int j = 0; j < Chessboard.board_size; j++)
                    {
                        if (directions[i] == 9)
                        {
                            squers_to_edge[i][v * 8 + j] = Math.Min(7-v, 7-j);
                        }
                        else if (directions[i] == -7)
                        {
                            squers_to_edge[i][v * 8 + j] = Math.Min(v, 7-j);
                        }
                        else if (directions[i] == -9)
                        {
                            squers_to_edge[i][v * 8 + j] = Math.Min(v, j);
                        }
                        else if (directions[i] == 7)
                        {
                            squers_to_edge[i][v * 8 + j] = Math.Min(7-v, j);
                        }
                        else if (directions[i] == 8)
                        {
                            squers_to_edge[i][v * 8 + j] = 7 - v;
                        }
                        else if(directions[i] == -8)
                        {
                            squers_to_edge[i][v * 8 + j] = v;
                        }
                        else if (directions[i] == 1)
                        {
                            squers_to_edge[i][v * 8 + j] = 7 - j;
                        }
                        else if (directions[i] == -1)
                        {
                            squers_to_edge[i][v * 8 + j] = j;
                        }
                    }
                }
            }
        }

        //generate all of the legal moves for one peace
        public List<Move> Generate_legal_moves(Peace peace)
        {
            if (peace.type == Peace.Pawn)
            {
                if (peace.color.Equals(white))
                    return Generatelegalmovesfrompseudolegal(Getmoves_w_pawn_pc(peace));
                return Generatelegalmovesfrompseudolegal(Getmoves_b_pawn_pc(peace));
            }
            else
            {
                List<Move> moves = new List<Move>();
                switch (peace.type)
                {
                    case Peace.Knight:
                        Getmoves_knight_pc(peace, moves);
                        return Generatelegalmovesfrompseudolegal(moves);
                    case Peace.Bishop:
                        Getmoves_diagonal_pc(peace, moves);
                        return Generatelegalmovesfrompseudolegal(moves);
                    case Peace.Rook:
                        Getmoves_verticle_pc(peace, moves);
                        return Generatelegalmovesfrompseudolegal(moves);
                    case Peace.Queen:
                        Getmoves_verticle_pc(peace, moves);
                        Getmoves_diagonal_pc(peace, moves);
                        return Generatelegalmovesfrompseudolegal(moves);
                    case Peace.King:
                        Getmoves_king_pc(peace, moves);
                        return Generatelegalmovesfrompseudolegal(moves);
                    default:
                        return null;
                }
            }
        }
        
        //generate all of the ligal moves.
        public List<Move> Generate_all_legal_moves()
        {
            return Generatelegalmovesfrompseudolegal(Generate_psudo_legal_moves());
        }

        //generates all the psudo-legal moves that exist in this chess board position
        // all the generated moves return ass a list that contain move objects
        private List<Move> Generate_psudo_legal_moves()
        {
            List<Move> moves = new List<Move>();
            List<Peace>[] parts;

            //go over all of the peaces of the curren player and for each peace add its moves to the "moves" list
            if (this.c.Is_white_turn())
            {
                parts = this.c.white_parts;
                foreach (Peace peace in parts[Peace.Pawn])//Peace.Knight
                {
                    moves.AddRange(Getmoves_w_pawn_pc(peace));
                }
            }
            else
            {
                parts = this.c.black_parts;
                foreach (Peace peace in parts[Peace.Pawn])
                {
                    moves.AddRange(Getmoves_b_pawn_pc(peace));
                }
            }

            foreach (Peace peace in parts[Peace.Knight])
            {
                Getmoves_knight_pc(peace, moves);
            }

            foreach (Peace peace in parts[Peace.Bishop])
            {
                Getmoves_diagonal_pc(peace, moves);
            }

            foreach (Peace peace in parts[Peace.Rook])
            {
                Getmoves_verticle_pc(peace, moves);
            }
            foreach (Peace peace in parts[Peace.Queen])
            {
                Getmoves_verticle_pc(peace, moves);
                Getmoves_diagonal_pc(peace, moves);
            }
            foreach (Peace peace in parts[Peace.King])
            {
                Getmoves_king_pc(peace, moves);
            }

            return moves;
        }
        //general directions (2d directions for 1d array board)
        public const int right =  0;
        public const int left  =  1;
        public const int up    =  2;
        public const int down  =  3;
        //
        public const int right_up   = 4;
        public const int left_up    = 5;
        public const int right_down = 6;
        public const int left_down  = 7;
        public static readonly int[] directions = { 1, -1, -8, 8, -7, -9, 9, 7 };
        //
        //these functions return or add to a input list the psudo-legal moves that a peace can do.
        //all of this fucntions are united in "generate_psudo_legal_moves()", in that function all of the posible psudo-legal moves in a board are assembled.
        private void Get_moves_at_direction(Peace peace, int direction, List<Move> moves, int edgecase = Move.None_edgcase)
        {
            int pos = peace.position;
            for (int i = 0; i < squers_to_edge[direction][peace.position]; i++)
            {
                pos += directions[direction];
                if (!this.c.board[pos].Isocupied())
                {
                    moves.Add(new Move(peace.position, pos, edgecase));
                }
                else
                {
                    if (!peace.color.Equals(c.board[pos].Peace.color))
                        moves.Add(new Move(peace.position, pos, edgecase));
                    break;
                }

            }
        }
        private void Getmoves_verticle_pc(Peace peace, List<Move> moves)
        {
            int i = peace.Get_i_pos();
            int j = peace.Get_j_pos();
            int edgecase = 0;
            int offset = Chessboard.Getoffset(peace.color);
            if(this.c.can_castle[offset] && peace.type == Peace.Rook)
            {
                int row = Chessboard.get_king_row_at_castling[peace.color];
                if (i == row)//if the rook in the start game row position of the rooks
                {
                    //if the left rook dindt move and this is the rook, the same for the right rook:
                    if((this.c.can_castle[offset + 1] && j==0)|| (this.c.can_castle[offset + 2] && j == 7))
                        edgecase = Move.rook_moving;
                }
            }
            Get_moves_at_direction(peace, right, moves, edgecase);
            Get_moves_at_direction(peace, left , moves, edgecase);
            Get_moves_at_direction(peace, down , moves, edgecase);
            Get_moves_at_direction(peace, up   , moves, edgecase);
            //generate all the moves right to the peace
            //generate all the moves left to the peace
            //generate all the moves down the peace
            //generate all the moves on top of the peace
        }
        private void Getmoves_diagonal_pc(Peace peace, List<Move> moves)
        {
            Get_moves_at_direction(peace, right_up  , moves);
            Get_moves_at_direction(peace, left_down , moves);
            Get_moves_at_direction(peace, left_up   , moves);
            Get_moves_at_direction(peace, right_down, moves);
            //generate all the moves right_up  to the peace
            //generate all the moves left_down to the peace
            //generate all the moves left_up   to the peace
            //generate all the moves right_down to the peace
        }
        private List<Move> Getmoves_w_pawn_pc(Peace wpawn)
        {
            List<Move> moves = new List<Move>();
            int i = wpawn.Get_i_pos();
            int j = wpawn.Get_j_pos();
            int jfinal = 15;
            int ifinal = 15;
            int istart = 15;
            Peace lastmovepeace;
            if (this.c.moves.Count != 0)
            {// En passant movement data
                Move lastmove = this.c.moves.Peek();
                jfinal = Chessboard.Get_j_pos(lastmove.endsquare);
                ifinal = Chessboard.Get_i_pos(lastmove.endsquare);
                istart = Chessboard.Get_i_pos(lastmove.startsquare);
            }
            if (!c.board[wpawn.position - 8].Isocupied()) //one step forwords
            {
                moves.Add(new Move(wpawn.position, wpawn.position -8));
                if (i == 6 && !c.board[wpawn.position - 16].Isocupied())         //two steps forwords
                    moves.Add(new Move(wpawn.position, wpawn.position -16));
            }
            if (j != Chessboard.board_size - 1)//not at the edge right of the board
            {
                if (c.board[wpawn.position - 7].Isocupied() && c.board[wpawn.position - 7].Peace.Is_black())//normal capture right
                {
                    moves.Add(new Move(wpawn.position, wpawn.position -7));
                }
                else if (jfinal != 15 && c.board[wpawn.position + 1].Isocupied())
                {
                    lastmovepeace = c.board[wpawn.position + 1].Peace;
                    if (Peace.Pawn == lastmovepeace.type && lastmovepeace.Is_black())// En passant right
                    {
                        if (jfinal == j + 1 && ifinal == i && istart == i - 2)
                            moves.Add(new Move(wpawn.position, wpawn.position -7,Move.enpassant));
                    }
                }
            }
            if (j != 0)//not at the edge left of the board
            {
                if (c.board[wpawn.position - 9].Isocupied() && c.board[wpawn.position - 9].Peace.Is_black())//normal capture left
                {
                    moves.Add(new Move(wpawn.position, wpawn.position -9));
                }
                else if (jfinal != 15 && c.board[wpawn.position - 1].Isocupied())
                {
                    lastmovepeace = c.board[wpawn.position - 1].Peace;
                    if (Peace.Pawn == lastmovepeace.type && lastmovepeace.Is_black())// En passant left
                    {
                        if (jfinal == j - 1 && ifinal == i && istart == i - 2)
                            moves.Add(new Move(wpawn.position, wpawn.position -9, Move.enpassant));
                    }
                }
            }
            if (i == 1)
            {
                List<Move> final_moves = new List<Move>(4);
                foreach (Move move in moves)
                {
                    final_moves.Add(new Move(move.startsquare, move.endsquare, Move.pawn_promote_to_knight)); //knight
                    final_moves.Add(new Move(move.startsquare, move.endsquare, Move.pawn_promote_to_bishop)); //bishop
                    final_moves.Add(new Move(move.startsquare, move.endsquare, Move.pawn_promote_to_rook)); //rook
                    final_moves.Add(new Move(move.startsquare, move.endsquare, Move.pawn_promote_to_queen)); //queen
                }
                return final_moves;
            }
            else
                return moves;
        }
        private List<Move> Getmoves_b_pawn_pc(Peace bpawn)
        {
            List<Move> moves = new List<Move>();
            int i = bpawn.Get_i_pos();
            int j = bpawn.Get_j_pos();
            int jfinal = 15;
            int ifinal = 15;
            int istart = 15;
            Peace lastmovepeace;
            if (this.c.moves.Count != 0)
            {// En passant movement data
                Move lastmove = this.c.moves.Peek();
                jfinal = Chessboard.Get_j_pos(lastmove.endsquare);
                ifinal = Chessboard.Get_i_pos(lastmove.endsquare);
                istart = Chessboard.Get_i_pos(lastmove.startsquare);
            }
            if (!c.board[bpawn.position + 8].Isocupied()) //one step forwords
            {
                moves.Add(new Move(bpawn.position, bpawn.position +8));
                if (i == 1 && !c.board[bpawn.position + 16].Isocupied())//two steps forwords
                    moves.Add(new Move(bpawn.position, bpawn.position +16));
            }
            if (j != 0)//not at the edge left of the board
            {
                if (c.board[bpawn.position + 7].Isocupied() && c.board[bpawn.position + 7].Peace.Is_white())//normal capture left
                {
                    moves.Add(new Move(bpawn.position, bpawn.position +7));
                }
                else if (jfinal != 15 && c.board[bpawn.position - 1].Isocupied())
                {
                    lastmovepeace = c.board[bpawn.position - 1].Peace;
                    if (Peace.Pawn == lastmovepeace.type && lastmovepeace.Is_white())// En passant left
                    {
                        if (jfinal == j - 1 && ifinal == i && istart == i + 2)
                            moves.Add(new Move(bpawn.position, bpawn.position + 7, Move.enpassant));
                    }
                }
            }
            if (j != Chessboard.board_size - 1) //not at the edge right of the board
            {
                if (c.board[bpawn.position + 9].Isocupied() && c.board[bpawn.position + 9].Peace.Is_white())//normal capture right
                {
                    moves.Add(new Move(bpawn.position, bpawn.position + 9));
                }
                else if (jfinal != 15 && c.board[bpawn.position + 1].Isocupied())
                {
                    lastmovepeace = c.board[bpawn.position + 1].Peace;
                    if (Peace.Pawn == lastmovepeace.type && lastmovepeace.Is_white())// En passant right
                    {
                        if (jfinal == j + 1 && ifinal == i && istart == i + 2)
                            moves.Add(new Move(bpawn.position, bpawn.position + 9, Move.enpassant));
                    }
                }
            }
            if (i == 6)
            {
                List<Move> final_moves = new List<Move>(4);
                foreach (Move move in moves)
                {
                    final_moves.Add(new Move(move.startsquare, move.endsquare, Move.pawn_promote_to_knight)); //knight
                    final_moves.Add(new Move(move.startsquare, move.endsquare, Move.pawn_promote_to_bishop)); //bishop
                    final_moves.Add(new Move(move.startsquare, move.endsquare, Move.pawn_promote_to_rook)); //rook
                    final_moves.Add(new Move(move.startsquare, move.endsquare, Move.pawn_promote_to_queen)); //queen
                }
                return final_moves;
            }
            else
                return moves;
        }
        private void Getmoves_knight_pc(Peace knight, List<Move> moves)
        {
            int i = knight.Get_i_pos();
            int j = knight.Get_j_pos();
            int first = 2;
            int second = 1;
            for (int numplace = 0; numplace < 2; numplace++)
            {
                for (int firstnumsighn = 0; firstnumsighn < 2; firstnumsighn++)
                {
                    for (int secondnumsighn = 0; secondnumsighn < 2; secondnumsighn++)
                    {
                        int tmpi = i + first;
                        int tmpj = j + second;
                        if (tmpi < 0 || tmpi > Chessboard.board_size - 1 || tmpj < 0 || tmpj > Chessboard.board_size - 1 || (c.board[tmpi * Chessboard.board_size + tmpj].Isocupied() && c.board[tmpi * Chessboard.board_size + tmpj].Peace.color.Equals(knight.color)))
                        {//the knight moves out of the board boundaries or land on a peace of its own color
                            second *= -1;
                            continue;
                        }
                        moves.Add(new Move(knight.position, (tmpi) * 8 + tmpj));
                        second *= -1;
                    }
                    first *= -1;
                }
                first = 1;
                second = 2;
            }
        }
        private void Getmoves_king_pc(Peace king, List<Move> moves)
        {
            int i = king.Get_i_pos();
            int j = king.Get_j_pos();
            int edgecase = 0;
            if (this.c.Kingdidntmoved())
                edgecase = Move.king_moving;
            int row = Chessboard.Getoffset(king.color);
            if (c.can_castle[row])//castle both sides
            {
                int oned_i = king.Get_i_pos() * Chessboard.board_size;
                if (c.can_castle[row + 1])//left
                {
                    if (!c.board[oned_i + 1].Isocupied() && !c.board[oned_i + 2].Isocupied() && !c.board[oned_i + 3].Isocupied())
                        moves.Add(new Move(king.position, king.position-2, Move.castle));
                }
                if (c.can_castle[row + 2])//right
                {
                    if (!c.board[oned_i + 5].Isocupied() && !c.board[oned_i + 6].Isocupied())
                    {
                        //if(king.position == 4)
                            //this.c.printstatics();
                        moves.Add(new Move(king.position, king.position + 2, Move.castle));
                    }
                }
            }
            //normal movement from here:

            Boolean[] borders = { j != 0, i != 7, j != 7, i != 0 };// 0left, 1down, 2right, 3up

            //same raw as starting posiotion
            if (borders[2] && c.board[king.position + 1].Canmove(king))
                moves.Add(new Move(king.position, king.position + 1, edgecase));
            if (borders[0] && c.board[king.position - 1].Canmove(king))
                moves.Add(new Move(king.position, king.position - 1, edgecase));
            //

            //raw down to the starting posiotion
            if (borders[1])
            {
                if (borders[0] && c.board[king.position + 7].Canmove(king))
                    moves.Add(new Move(king.position, king.position +7, edgecase));
                if (c.board[king.position+8].Canmove(king))
                    moves.Add(new Move(king.position, king.position +8, edgecase));
                if (borders[2] && c.board[king.position + 9].Canmove(king))
                    moves.Add(new Move(king.position, king.position +9, edgecase));
            }
            //

            //raw up to the starting posiotion
            if (borders[3])
            {
                if (borders[0] && c.board[king.position - 9].Canmove(king))
                    moves.Add(new Move(king.position, king.position -9, edgecase));
                if (c.board[king.position - 8].Canmove(king))
                    moves.Add(new Move(king.position, king.position -8, edgecase));
                if (borders[2] && c.board[king.position - 7].Canmove(king))
                    moves.Add(new Move(king.position, king.position -7, edgecase));
            }
            //
        }


        //Wrapping Operation - only for outside usege!
        /// <summary>
        /// return the number of leaf node in the minimx tree
        /// </summary>
        /// <param name="depth"></param>
        /// <returns></returns>
        public int Perft(int depth)
        {
            return Perfit(depth, depth);
        }
        //internal Operation - only for inside usege!
        /// <summary>
        /// return the number of leaf node in the minimx tree and print it
        /// </summary>
        /// <param name="depth"></param>
        /// <param name="headnode"></param>
        /// <returns></returns>
        private int Perfit(int depth, int headnode)
        {
            if (depth == 0) // if got to a leaf node
                return 1;
            List<Move> moves = Generate_all_legal_moves();
            int leafnodes = 0;
            foreach (Move move in moves)
            {
                c.Manualy_makemove(move);
                int num = Perfit(depth - 1, headnode);
                leafnodes += num;
                if (depth == headnode)
                {
                    Console.WriteLine(move.Print_in_notation() + num);
                    //Console.WriteLine(c.get_fen_notation());
                }
                c.Unmakelastmove();
            }
            return leafnodes;
        }

        /// <summary>
        /// gets a list of pseudo-legal moves and return a list with all of the legal moves in this list
        /// </summary>
        /// <param name="pseudolegalmoves"></param>
        /// <returns></returns>
        private List<Move> Generatelegalmovesfrompseudolegal(List<Move> pseudolegalmoves)
        {
            int king_pos = this.c.Get_king_pos(this.c.color_turn);
            if (this.c.There_arent_attacks_or_pins(king_pos)) //making sure king movement is ok
            {
                for(int i = pseudolegalmoves.Count-1; i >= 0; i--)//the king movements are at the end of the pseudolegalmoves list
                {
                    if(pseudolegalmoves[i].startsquare == king_pos)
                    {
                        c.Manualy_makemove_without_switching_turns(pseudolegalmoves[i]);
                        if (this.c.Current_player_king_in_check())
                        {
                            pseudolegalmoves.Remove(pseudolegalmoves[i]);//might be problematic
                            
                        }
                        else if (pseudolegalmoves[i].edgecase == Move.castle) //if the movement is a castle, making sure the move is legal
                        {
                            if (this.c.Pos_in_check(pseudolegalmoves[i].startsquare) ||
                                (pseudolegalmoves[i].endsquare % 8 == 6 && this.c.Pos_in_check(pseudolegalmoves[i].endsquare - 1)) ||
                                (pseudolegalmoves[i].endsquare % 8 == 2 && this.c.Pos_in_check(pseudolegalmoves[i].endsquare + 1))
                                )
                                pseudolegalmoves.Remove(pseudolegalmoves[i]);//might be problematic
                        }
                        this.c.Unmakelastmove_without_switching_turns();
                    }
                    else
                        return pseudolegalmoves;
                }
                return pseudolegalmoves;//
            }
            List<Move> legalmoves = new List<Move>();
            foreach (Move tmpmove in pseudolegalmoves)
            {
                c.Manualy_makemove_without_switching_turns(tmpmove);
                if (!this.c.Current_player_king_in_check())
                {
                    if (tmpmove.edgecase == Move.castle) //if the movement is a castle, making sure the move is legal
                    {
                        if (this.c.Pos_in_check(tmpmove.startsquare) ||
                            (tmpmove.endsquare % 8 == 6 && this.c.Pos_in_check(tmpmove.endsquare - 1)) ||
                            (tmpmove.endsquare % 8 == 2 && this.c.Pos_in_check(tmpmove.endsquare + 1))
                            )
                            goto End;
                    }
                    legalmoves.Add(tmpmove);
                }
                End:
                this.c.Unmakelastmove_without_switching_turns();
            }
            return legalmoves;
        }

        /// <summary>
        /// return the move the AI decided to play
        /// </summary>
        /// <param name="depth"></param>
        /// <param name="for_white_player"></param>
        /// <returns></returns>
        public Move Choose_move(int depth, bool for_white_player) //the assamption is that the game isnt over yet
        {
            //aplpha: the worst posible score for white - negative infinity
            //beta: the worst posible score for black - positive infinity
            if (for_white_player) { return AlphaBetaMax_getmove(alpha: int.MinValue, beta: int.MaxValue, depth); }
            //
            //
            return AlphaBetaMin_getmove(alpha: int.MinValue, beta: int.MaxValue, depth);
        }
        
        /// <summary>
        /// return the best move from of the current position for the maximizing player.
        /// </summary>
        /// <param name="alpha"></param>
        /// <param name="beta"></param>
        /// <param name="depth"></param>
        /// <returns></returns>
        public Move AlphaBetaMax_getmove(int alpha, int beta, int depth)
        {
            //assuming deph>0 and there are at least one move to be made at that position
            List<Move> child_nodes = this.mo.OrdereMoves(this.c.generator.Generate_all_legal_moves());
            Move best_move = child_nodes[0];//assuming the function will return a move(and there is one)

            for (int i = 0; i < child_nodes.Count; i++)
            {
                //Console.WriteLine(child_nodes[i].print_in_notation());
                this.c.Manualy_makemove(child_nodes[i]);
                int score = AlphaBetaMin(alpha, beta, depth - 1);
                //Console.WriteLine("Score: " + score);
                this.c.Unmakelastmove();
                if (score >= beta)
                    return child_nodes[i];   // fail hard beta-cutoff
                if (score > alpha)
                {
                    alpha = score; // alpha acts like max in MiniMax
                    best_move = child_nodes[i];
                }
            }
            return best_move;
        }
        /// <summary>
        /// return the best move from of the current position for the minimizing player.
        /// </summary>
        /// <param name="alpha"></param>
        /// <param name="beta"></param>
        /// <param name="depth"></param>
        /// <returns></returns>
        public Move AlphaBetaMin_getmove(int alpha, int beta, int depth)
        {
            //assuming deph>0 and there are at least one move to be made at that position
            List<Move> child_nodes = this.mo.OrdereMoves(this.c.generator.Generate_all_legal_moves());
            Move best_move = child_nodes[0];//assuming the function will return a move(and there is one)

            for (int i = 0; i < child_nodes.Count; i++)
            {
                //Console.WriteLine(child_nodes[i].print_in_notation());
                this.c.Manualy_makemove(child_nodes[i]);
                int score = AlphaBetaMax(alpha, beta, depth - 1);
                //Console.WriteLine("Score: " + score);
                this.c.Unmakelastmove();             
                if (score <= alpha)
                    return child_nodes[i];   // fail hard beta-cutoff
                if (score < beta)
                {
                    beta = score; // alpha acts like max in MiniMax
                    best_move = child_nodes[i];
                }
            }
            return best_move;
        }

        public int AlphaBetaMax(int alpha, int beta, int depthleft)
        {
            if (depthleft == 0) return AlphaBetaMincapture(alpha, beta);

            List<Move> child_nodes = this.mo.OrdereMoves(this.c.generator.Generate_all_legal_moves());
            // leaf node is reached
            if (child_nodes.Count == 0)// draw or someone won
            {
                if (this.c.Current_player_king_in_check()) //if checkmate
                    return Int32.MinValue;
                return 0;//draw
            }

            for (int i = 0; i < child_nodes.Count; i++)
            {
                this.c.Manualy_makemove(child_nodes[i]);
                int score = AlphaBetaMin(alpha, beta, depthleft - 1);
                this.c.Unmakelastmove();
                if (score >= beta)
                    return beta;   // fail hard beta-cutoff
                if (score > alpha)
                    alpha = score; // alpha acts like max in MiniMax
            }
            return alpha;
        }
        public int AlphaBetaMin(int alpha, int beta, int depthleft)
        {
            if (depthleft == 0) return AlphaBetaMincapture(alpha, beta);

            List<Move> child_nodes = this.mo.OrdereMoves(this.c.generator.Generate_all_legal_moves());
            // leaf node is reached
            if (child_nodes.Count == 0)// draw or someone won
            {
                if (this.c.Current_player_king_in_check()) //if checkmate
                    return Int32.MaxValue;
                return 0;//draw
            }

            for (int i = 0; i < child_nodes.Count; i++)
            {
                //Console.WriteLine(child_nodes[i].print_in_notation());
                this.c.Manualy_makemove(child_nodes[i]);
                int score = AlphaBetaMax(alpha, beta, depthleft - 1);
                //Console.WriteLine("Score: " + score);
                this.c.Unmakelastmove();
                if (score <= alpha)
                    return alpha; // fail hard alpha-cutoff
                if (score < beta)
                    beta = score; // beta acts like min in MiniMax
            }
            return beta;
        }

        /// <summary>
        /// generate captured legal moves
        /// </summary>
        /// <returns>capture_legal_moves</returns>
        private List<Move> Getcapturesmoves()
        {
            return Generatelegalmovesfrompseudolegal(Generate_psudo_legal_moves().FindAll(move => move.capturedpeace != null));
        }

        /// <summary>
        /// search tree for only captured moves
        /// </summary>
        /// <param name="alpha"></param>
        /// <param name="beta"></param>
        /// <returns></returns>
        public int AlphaBetaMaxcapture(int alpha, int beta)
        {
            int eval = this.c.Evaluate();
            if (eval >= beta)
                return beta;   // fail hard beta-cutoff
            if (eval > alpha)
                alpha = eval; // alpha acts like max in MiniMax
            /////////////////

            List<Move> child_nodes = this.mo.OrdereMoves(Getcapturesmoves());

            for (int i = 0; i < child_nodes.Count; i++)
            {
                this.c.Manualy_makemove(child_nodes[i]);
                int score = AlphaBetaMincapture(alpha, beta);
                this.c.Unmakelastmove();
                if (score >= beta)
                    return beta;   // fail hard beta-cutoff
                if (score > alpha)
                    alpha = score; // alpha acts like max in MiniMax
            }
            return alpha;
        }
        /// <summary>
        /// search tree for only captured moves
        /// </summary>
        /// <param name="alpha"></param>
        /// <param name="beta"></param>
        /// <returns></returns>
        public int AlphaBetaMincapture(int alpha, int beta)
        {
            int eval = this.c.Evaluate();
            if (eval <= alpha)
                return alpha; // fail hard alpha-cutoff
            if (eval < beta)
                beta = eval; // beta acts like min in MiniMax
            //////////////////

            List<Move> child_nodes = this.mo.OrdereMoves(Getcapturesmoves());

            for (int i = 0; i < child_nodes.Count; i++)
            {
                this.c.Manualy_makemove(child_nodes[i]);
                int score = AlphaBetaMaxcapture(alpha, beta);
                this.c.Unmakelastmove();
                if (score <= alpha)
                    return alpha; // fail hard alpha-cutoff
                if (score < beta)
                    beta = score; // beta acts like min in MiniMax
            }
            return beta;
        }

    }

}
