using chess.types_of_peaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace chess
{
    public struct Tile
    {
        public Peace Peace;

        public Tile(Peace peace = null)
        {
            this.Peace = peace;
        }
        /// <summary>
        /// return wheter a tile is empty or not
        /// </summary>
        /// <returns></returns>
        public Boolean Isocupied()
        {
            if (this.Peace == null)
                return false;
            return true;
        }
        /// <summary>
        /// return wheter a peace can move to that tile
        /// </summary>
        /// <param name="peace"></param>
        /// <returns></returns>
        public Boolean Canmove(Peace peace) // if psudolegaly allowed to move a peace to that tile.
        {
            if (Isocupied())
            {
                if (!peace.color.Equals(this.Peace.color))
                    return true;
                return false;
            }
            return true;
        }
    }

    public class Chessboard
    {
        public const int board_size = 8;
        public const int amount_of_sq = board_size * board_size;
        public const int peaces_types_amount = 6;
        public List<Peace>[] white_parts = new List<Peace>[peaces_types_amount]; // | 0pawns | 1knights | 2bishops | 3rooks | 4queens | 5king
        public List<Peace>[] black_parts = new List<Peace>[peaces_types_amount]; // | 0pawns | 1knights | 2bishops | 3rooks | 4queens | 5king
        public List<Peace>[][] peaces;//this variable responsible for accesing all of the peaces
        public Tile[] board = new Tile[board_size * board_size]; // 8x8 board that represent the chess board
        public Stack<Move> moves = new Stack<Move>(40); // in an average chess game there are 40 moves
        public Boolean[] can_castle = { true, true, true , true, true, true }; // white all, white left , white right, black all , black left , black right, ||before got eaten: white left, white right, black left , black right                               
        public const Byte white = 0;
        public const Byte black = 1;
        public Byte color_turn = white;// true is white and false is black
        public bool white_turn = true;
        public Movegenerator generator;
        public PositionEval Position_evaluatior;

        /*   (i)matrix representations
         *      +---+---+---+---+---+---+---+---+
         *    0 |0,0|0,1|0,2|0,3|0,4|0,5|0,6|0,7|
         *      +---+---+---+---+---+---+---+---+
         *    1 |1,0|1,1|1,2|1,3|1,4|1,5|1,6|1,7|
         *      +---+---+---+---+---+---+---+---+
         *    2 |2,0|2,1|2,2|2,3|2,4|2,5|2,6|2,7|
         *      +---+---+---+---+---+---+---+---+
         *    3 |3,0|3,1|3,2|3,3|3,4|3,5|3,6|3,7|
         *      +---+---+---+---+---+---+---+---+
         *    4 |4,0|4,1|4,2|4,3|4,4|4,5|4,6|4,7|
         *      +---+---+---+---+---+---+---+---+
         *    5 |5,0|5,1|5,2|5,3|5,4|5,5|5,6|5,7|
         *      +---+---+---+---+---+---+---+---+
         *    6 |6,0|6,1|6,2|6,3|6,4|6,5|6,6|6,7|
         *      +---+---+---+---+---+---+---+---+
         *    7 |7,0|7,1|7,2|7,3|7,4|7,5|7,6|7,7|
         *      +---+---+---+---+---+---+---+---+
         *        0   1   2   3   4   5   6   7  (j)
         *        
         *        
         *        
         *   (i)//position representation at my project
         *      +---+---+---+---+---+---+---+---+
         *    0 | 0 | 1 | 2 | 3 | 4 | 5 | 6 | 7 |
         *      +---+---+---+---+---+---+---+---+
         *    1 | 8 | 9 |10 |11 |12 |13 |14 |15 |
         *      +---+---+---+---+---+---+---+---+
         *    2 |16 |17 |18 |19 |20 |21 |22 |23 |
         *      +---+---+---+---+---+---+---+---+
         *    3 |24 |25 |26 |27 |28 |29 |30 |31 |
         *      +---+---+---+---+---+---+---+---+
         *    4 |32 |33 |34 |35 |36 |37 |38 |39 |
         *      +---+---+---+---+---+---+---+---+
         *    5 |40 |41 |42 |43 |44 |45 |46 |47 |
         *      +---+---+---+---+---+---+---+---+
         *    6 |48 |49 |50 |51 |52 |53 |54 |55 |
         *      +---+---+---+---+---+---+---+---+
         *    7 |56 |57 |58 |59 |60 |61 |62 |63 |
         *      +---+---+---+---+---+---+---+---+
         *        0   1   2   3   4   5   6   7  (j)
         *        
         *    
         *    (i) //real world representation
         *      +---+---+---+---+---+---+---+---+
         *    8 | 0 | 1 | 2 | 3 | 4 | 5 | 6 | 7 |
         *      +---+---+---+---+---+---+---+---+
         *    7 | 8 | 9 |10 |11 |12 |13 |14 |15 |
         *      +---+---+---+---+---+---+---+---+
         *    6 |16 |17 |18 |19 |20 |21 |22 |23 |
         *      +---+---+---+---+---+---+---+---+
         *    5 |24 |25 |26 |27 |28 |29 |30 |31 |
         *      +---+---+---+---+---+---+---+---+
         *    4 |32 |33 |34 |35 |36 |37 |38 |39 |
         *      +---+---+---+---+---+---+---+---+
         *    3 |40 |41 |42 |43 |44 |45 |46 |47 |
         *      +---+---+---+---+---+---+---+---+
         *    2 |48 |49 |50 |51 |52 |53 |54 |55 |
         *      +---+---+---+---+---+---+---+---+
         *    1 |56 |57 |58 |59 |60 |61 |62 |63 |
         *      +---+---+---+---+---+---+---+---+
         *        a   b   c   d   e   f   g   h  (j)
        */

        /// <summary>
        /// initialize all of the objects needed
        /// </summary>
        private void Initialize_objects() 
        {
            for (int i = 0; i < amount_of_sq; i++)
            {
                this.board[i] = new Tile(); //makes a blank board and initialize all tiles
            }
            for (int i = 0; i < peaces_types_amount; i++)
            {
                this.white_parts[i] = new List<Peace>();
                this.black_parts[i] = new List<Peace>();
            }

            // in a chess game every side can only have 1 king
            this.white_parts[Peace.King].Capacity = 1;
            this.black_parts[Peace.King].Capacity = 1;

            // in a chess game every side can only have 8 pawns
            this.white_parts[Peace.Pawn].Capacity = 8;
            this.black_parts[Peace.Pawn].Capacity = 8;

            // some general assumptions
            this.white_parts[Peace.Bishop].Capacity = 4;
            this.black_parts[Peace.Bishop].Capacity = 4;
            this.white_parts[Peace.Knight].Capacity = 4;
            this.black_parts[Peace.Knight].Capacity = 4;
            this.white_parts[Peace.Rook].Capacity = 4;
            this.black_parts[Peace.Rook].Capacity = 4;
            this.white_parts[Peace.Queen].Capacity = 3;
            this.black_parts[Peace.Queen].Capacity = 3;

            //this variable responsible for accesing all of the peaces
            this.peaces = new List<Peace>[][] { white_parts, black_parts };

            //create the position evaluator
            this.Position_evaluatior = new PositionEval(this);

            //create the move generator
            generator = new Movegenerator(this);
        }

        /// <summary>
        /// create an empty chess board and initialize it to beggining chess position
        /// </summary>
        public Chessboard()
        {

            Console.WriteLine("done");
            Initialize_objects();

            //create all of the pawns
            for (int j = 0; j < 8; j++) 
            {
                Addpeacetoboardandlist(new Pawn(black, j + 8));
                Addpeacetoboardandlist(new Pawn(white, j + 48));
            }
            //create all of the knights
            Addpeacetoboardandlist(new Knight(black, 1)); Addpeacetoboardandlist(new Knight(black, 6));
            Addpeacetoboardandlist(new Knight(white, 57)); Addpeacetoboardandlist(new Knight(white, 62));

            //create all of the bishops
            Addpeacetoboardandlist(new Bishop(black, 2)); Addpeacetoboardandlist(new Bishop(black, 5));
            Addpeacetoboardandlist(new Bishop(white, 58)); Addpeacetoboardandlist(new Bishop(white, 61));

            //create all of the rooks
            Addpeacetoboardandlist(new Rook(black, 0)); Addpeacetoboardandlist(new Rook(black, 7));
            Addpeacetoboardandlist(new Rook(white, 56)); Addpeacetoboardandlist(new Rook(white, 63));

            //create all of the queens
            Addpeacetoboardandlist(new Queen(black, 3)); Addpeacetoboardandlist(new Queen(white, 59));

            //create all of the kings
            Addpeacetoboardandlist(new King(black, 4)); Addpeacetoboardandlist(new King(white, 60));
            //seting whites turn
            Setplayerturn(true);
        }

        /// <summary>
        /// create an empty chess board and initialize it accordint to the FEN notation
        /// </summary>
        /// <param name="FEN_notation"></param>
        public Chessboard(string FEN_notation)
        {
            Initialize_objects();
            int i = 0;
            int j = 0;
            int counter = 0;
            var dic = new Dictionary<char, int>()
                            {
                                {'p' , Peace.Pawn} ,
                                {'n' , Peace.Knight} ,
                                {'b' , Peace.Bishop} ,
                                {'r' , Peace.Rook} ,
                                {'q' , Peace.Queen} ,
                                {'k' , Peace.King}
                            };
            foreach (char character in FEN_notation)
            {
                int num= (int)Char.GetNumericValue(character);
                if(num != -1)//if represent a true number
                {
                    j += num;
                }
                else
                {
                    switch (character)
                    {
                        case '/':
                            j = 0;
                            i++;
                            break;
                        case ' ':
                            counter++;
                            goto LoopEnd;
                        default:
                            int peacetype = dic[Char.ToLower(character)];
                            if (Char.IsUpper(character))//if white peace
                                Addpeacetoboardandlist(new Peace(white, i*8+j , peacetype));
                            else
                                Addpeacetoboardandlist(new Peace(black, i * 8 + j, peacetype));
                            j++;
                            break;
                    }
                }
                counter++;
            }
            
            LoopEnd:
            if (Char.ToLower(FEN_notation[counter++]) == 'w')
                Setplayerturn(true);
            else
                Setplayerturn(false);
            counter++;
            this.can_castle[1] = false;
            this.can_castle[2] = false;
            this.can_castle[4] = false;
            this.can_castle[5] = false;
            while (FEN_notation[counter] != ' ')
            {// white all, white left , white right, black all , black left , black right
                if (FEN_notation[counter] == 'K')
                    this.can_castle[2] = true;
                else if (FEN_notation[counter] == 'Q')
                    this.can_castle[1] = true;
                else if (FEN_notation[counter] == 'k')
                    this.can_castle[5] = true;
                else if (FEN_notation[counter] == 'q')
                    this.can_castle[4] = true;
                else if(FEN_notation[counter] == '-')//no one can castle
                {
                    this.can_castle[1] = false;
                    this.can_castle[2] = false;
                    this.can_castle[4] = false;
                    this.can_castle[5] = false;
                    break;
                }
                counter++;
                if (FEN_notation.Length == counter)
                    break;
            }
            //En passant target square ,Halfmove Clock and Fullmove counter are ignored;
        }

        /// <summary>
        /// return fen notation of the curent chess board
        /// </summary>
        /// <returns></returns>
        public string Get_fen_notation()
        {
            string fen = "";
            int counter = 0;
            for (int i = 0; i < board_size; i++)
            {
                for (int j = 0; j < board_size; j++)
                {
                    if (board[i * board_size + j].Isocupied())
                    {
                        if(counter != 0)
                            fen += counter.ToString();
                        counter = 0;
                        fen += board[i * board_size + j].Peace.Get_type_char_rep();
                    }
                    else
                    {
                        counter++;
                        if(j == board_size-1)
                            fen += counter.ToString();
                    }
                }
                if(i!= board_size-1)
                    fen += "/";
                counter = 0;
            }
            if(Is_white_turn())
                fen += " w ";
            else
                fen += " b ";
            if(!this.can_castle[1] && !this.can_castle[2] && !this.can_castle[4] && !this.can_castle[5])
                fen += "- ";
            else
            {
                if (this.can_castle[0])
                {
                    if (this.can_castle[2]) fen += "K";
                    if (this.can_castle[1]) fen += "Q";
                }
                if (this.can_castle[3])
                {
                    if (this.can_castle[5]) fen += "k";
                    if (this.can_castle[4]) fen += "q";
                }
            }
            return fen;
        }
        
        //add peace in a unocupied Tile + in the list
        private void Addpeacetoboardandlist(Peace peace) 
        {
            board[peace.position].Peace = peace;
            Addpeacetolist(peace);
        }
        //remove a peace from the coresponding list
        private void Removepeacefromlist(Peace peace)
        {
            this.peaces[peace.color][peace.type].Remove(peace);
        }
        //add a piece to the coresponding list
        private void Addpeacetolist(Peace peace)
        {
            this.peaces[peace.color][peace.type].Add(peace);
        }
        
        /// <summary>
        /// make a move, change all the data asociated with the board accordingly, afterwards changing color
        /// </summary>
        /// <param name="move"></param>
        public void Manualy_makemove(Move move) //make sure if its last move of the pawn that it is correct
        {
            Manualy_makemove_without_switching_turns(move);
            Switchplayerturn();
        }
        internal static readonly int[] get_king_row_at_castling = { 7, 0 };
        //take a move - not matter if the move eats a peace of the same color of the eater
        //takes a move - no matter whose turn to play it is!!!!
        //no matter if there is a peace from the same color in it!!!!
        //not chacking if the peace than need to move exist!!!
        //not cheking if the move is legal
        /// <summary>
        /// makemake a move, change all the data asociated with the board accordingly
        /// </summary>
        /// <param name="move"></param>
        public void Manualy_makemove_without_switching_turns(Move move) //make sure if its last move of the pawn that it is correct
        {
            if (this.board[move.endsquare].Isocupied()) //if there is a cptured peace in this move
            {
                move.capturedpeace = this.board[move.endsquare].Peace; //save the captured peace
                Removepeacefromlist(move.capturedpeace);

                if (move.capturedpeace.type == Peace.Rook)
                {// what happen if rook gets eaten - you have to change the castling rights
                    int offset = Getoffset(move.capturedpeace.color);
                    if (this.can_castle[offset])//if king hasnt moved
                    {
                        int row = get_king_row_at_castling[move.capturedpeace.color];
                        if (Get_i_pos(move.endsquare) == row)//if the eaten rook is the one that can castle
                        {
                            int jendpo = Get_j_pos(move.endsquare);
                            //debug issue in fen :r3k2r/Pppp1ppp/1b3nbN/nPP5/BB2P3/q4N2/P2P2PP/r2Q1RK1 w kq - 0 1
                            //this is why the row value got in, otherwise the board think white eat his left side rook and now he cant castle that side.
                            if (jendpo == 0 && this.can_castle[offset + 1])//eating the left rook that can castle
                            {
                                this.can_castle[offset + 1] = false; // the right rook moved (cant caslte anymore)
                                move.capturedpeace.position -= 64; //mark for the unmakemove func
                            }
                            else if (jendpo == 7 && this.can_castle[offset + 2]) //eating the right rook that can castle
                            { //right rook
                                this.can_castle[offset + 2] = false;// the left rook moved (cant caslte anymore)
                                move.capturedpeace.position -= 64;//mark for the unmakemove func
                            }
                        }
                    }
                }
            }
            else if (move.edgecase == Move.enpassant) // if a En passant movement
            {
                int istartpo = Get_i_pos(move.startsquare);
                int jendpo = Get_j_pos(move.endsquare);
                move.capturedpeace = this.board[istartpo * board_size + jendpo].Peace; //save the captured pawn
                Removepeacefromlist(move.capturedpeace);
                this.board[istartpo * board_size + jendpo].Peace = null;//the captured peace squer isnt ocupied anymore
            }
            else if (move.edgecase == Move.castle) // if a castle movement
            { //the king moves more than one square in one move
                int row = get_king_row_at_castling[this.board[move.startsquare].Peace.color] * board_size;
                if(move.endsquare == move.startsquare +2)//right castle
                {//changing the position of the right rook
                    this.board[row + 5].Peace = this.board[row + 7].Peace;
                    this.board[row + 5].Peace.position = row +5;
                    this.board[row + 7].Peace = null;
                }
                else if(move.endsquare == move.startsquare - 2)//left castle
                {//changing the position of the left rook
                    this.board[row + 3].Peace = this.board[row + 0].Peace;
                    this.board[row + 3].Peace.position = row +3;
                    this.board[row + 0].Peace = null;
                }
            }
            //change board representation
            this.board[move.endsquare].Peace = this.board[move.startsquare].Peace; // change the position of the mover in the board
            this.board[move.endsquare].Peace.position = move.endsquare;//change the position of the mover in the list
            this.board[move.startsquare].Peace = null;//the current squer isnt ocupied
            
            Change_castling_rights(this.board[move.endsquare].Peace.color,move.edgecase, Get_j_pos(move.startsquare));
            
            //if a pawn does promotion
            if (move.edgecase > 0 && move.edgecase <= 4)
            {
                Removepeacefromlist(this.board[move.endsquare].Peace);
                this.board[move.endsquare].Peace.type = move.edgecase;
                Addpeacetolist(this.board[move.endsquare].Peace);
            }
            moves.Push(move);
        }
        
        //changing the castling rights according to the peace than moved and its position and if you make move or unmaking it
        //start position refers to the place the peace was before you make the move!!!
        //return edgecase number for a move
        /// <summary>
        /// change the casling rights after making a move
        /// </summary>
        /// <param name="iswhite"></param>
        /// <param name="edgecase"></param>
        /// <param name="jstartpo"></param>
        public void Change_castling_rights(Byte iswhite, int edgecase, int jstartpo)
        {
            if (Kingdidntmoved() && edgecase !=0)
            {
                int offset = Getoffset(iswhite);
                if (edgecase == Move.rook_moving)
                {
                    if (jstartpo == 0)//left rook moved, cant castle at left side anymove
                    { //left rook
                        this.can_castle[offset + 1] = false;
                    }
                    else //(jstartpo == 7) //right rook moved, cant castle at right side anymove
                    { //right rook
                        this.can_castle[offset + 2] = false;
                    }
                }
                else if (edgecase == Move.king_moving || edgecase == Move.castle)//king moved, cant castle any more
                { //if king moved or castled:
                    this.can_castle[offset] = false;
                }
            }// white all, white left , white right, black all , black left , black right
        }
        
        /// <summary>
        /// change the casling rights after unmaking a move
        /// </summary>
        /// <param name="edgecase"></param>
        /// <param name="iswhite"></param>
        /// <param name="startpo"></param>
        public void Unmake_castling_rights(int edgecase, Byte iswhite, int startpo)
        {
            if (edgecase != 0)
            {
                int offset = Getoffset(iswhite);
                if (edgecase == Move.king_moving || edgecase == Move.castle)//king can now castle
                {
                    this.can_castle[offset] = true;
                }
                else if (edgecase == Move.rook_moving)//left rook can now castle
                {
                    if (Get_j_pos(startpo) == 0)//left rook moved, can castle at left side
                    { //left rook
                        this.can_castle[offset + 1] = true;
                    }
                    else //(jstartpo == 7) //right rook moved, ca castle at right side
                    { //right rook
                        this.can_castle[offset + 2] = true;
                    }
                }
            }
        }

        /// <summary>
        /// unmake a move, change all the data asociated with the board accordingly, afterwards changing color
        /// </summary>
        public void Unmakelastmove() //make sure if its last move of the pawn that it is correct
        {
            Unmakelastmove_without_switching_turns();
            Switchplayerturn();
        }

        /// <summary>
        /// unmake a move, change all the data asociated with the board accordingly
        /// </summary>
        public void Unmakelastmove_without_switching_turns()
        {
            Move move = this.moves.Pop();

            bool peace_got_captured = move.capturedpeace != null;
            if (peace_got_captured && move.capturedpeace.position <0)//rook got eaten and could castle before that
            {// if rook got eaten - you have to change the castling rights
                move.capturedpeace.position += 64;
                int offset = Getoffset(move.capturedpeace.color);
                int jendpo = Get_j_pos(move.endsquare);
                if (jendpo == 0)//eating the left rook
                {
                    this.can_castle[offset + 1] = true; 
                }
                else if (jendpo == 7) //eating the right rook
                { //right rook
                    this.can_castle[offset + 2] = true;
                }
                
            }
            //
            Unmake_castling_rights(move.edgecase, this.board[move.endsquare].Peace.color, move.startsquare);
            //move the peace position value back
            this.board[move.endsquare].Peace.position = move.startsquare;
            //move the peace position back in the board
            this.board[move.startsquare].Peace = this.board[move.endsquare].Peace;//the start squer is now ocupied
            //if pawn promoted
            if (move.Peacepromote())
            {
                Removepeacefromlist(this.board[move.startsquare].Peace);
                this.board[move.startsquare].Peace.type = Peace.Pawn;
                Addpeacetolist(this.board[move.startsquare].Peace);
            }
            //
            //
            if (peace_got_captured) //if a peace needs to come back
            {

                //add the captured peace in the board
                this.board[move.capturedpeace.position].Peace = move.capturedpeace;

                if(move.endsquare != move.capturedpeace.position) // if its a En passant move
                    this.board[move.endsquare].Peace = null;//the end squer isnt ocupied anymore

                //add the captured peace in the list
                this.peaces[move.capturedpeace.color][move.capturedpeace.type].Add(move.capturedpeace);
            }
            else
            {
                this.board[move.endsquare].Peace = null;//the end squer isnt ocupied
                if (move.edgecase == Move.castle) // if a castle movement
                { //the king moves more than one square in one move
                    int row = get_king_row_at_castling[this.board[move.startsquare].Peace.color] * board_size;
                    if (move.endsquare == move.startsquare +2)//right castle
                    {//changing the position of the right rook
                        this.board[row + 7].Peace = this.board[row + 5].Peace;
                        this.board[row + 7].Peace.position = row +7;
                        this.board[row + 5].Peace = null;
                    }
                    if (move.endsquare == move.startsquare - 2)//left castle
                    {//changing the position of the left rook
                        this.board[row + 0].Peace = this.board[row + 3].Peace;
                        this.board[row + 0].Peace.position = row;
                        this.board[row + 3].Peace = null;
                    }
                }
            }
        }
        
        /// <summary>
        /// change the notation of a string move to the notation used in the engine, than make the move
        /// </summary>
        /// <param name="four_letters_position"></param>
        public void Manualy_makemove(string four_letters_position)
        {// example: "a1a2"
            //this function works acording to the real notation in a real chess game:
            /*
               +---+---+---+---+---+---+---+---+
               | r |   |   |   | k |   |   | r | 8
               +---+---+---+---+---+---+---+---+
               | p |   | p | p | q | p | b |   | 7
               +---+---+---+---+---+---+---+---+
               | b | n |   |   | p | n | p |   | 6
               +---+---+---+---+---+---+---+---+
               |   |   |   | P | N |   |   |   | 5
               +---+---+---+---+---+---+---+---+
               |   | p |   |   | P |   |   |   | 4
               +---+---+---+---+---+---+---+---+
               |   |   | N |   |   | Q |   | p | 3
               +---+---+---+---+---+---+---+---+
               | P | P | P | B | B | P | P | P | 2
               +---+---+---+---+---+---+---+---+
               | R |   |   |   | K |   |   | R | 1
               +---+---+---+---+---+---+---+---+
                 a   b   c   d   e   f   g   h 
            */
            int init_i = Math.Abs(four_letters_position[1] - '0' - 8);
            int init_j = char.ToUpper(four_letters_position[0]) - 65;
            int final_i = Math.Abs(four_letters_position[3] - '0' - 8);
            int final_j = char.ToUpper(four_letters_position[2]) - 65;

            List<Move> moves = this.generator.Generate_legal_moves(this.board[init_i * board_size + init_j].Peace);
            moves.Any(move => move.startsquare == init_i * 8 + init_j & move.endsquare == final_i * 8 + final_j);
            foreach (Move move in moves)
            {
                if(move.startsquare == init_i * 8 + init_j & move.endsquare == final_i * 8 + final_j)
                {
                    Manualy_makemove(new Move(init_i * 8 + init_j, final_i * 8 + final_j, move.edgecase));
                    return;
                }
            }
            Console.WriteLine("move is ilegal...");
        }

        /// <summary>
        /// return a string representation of the board from the board's matrix
        /// </summary>
        /// <returns>string_notation</returns>
        override public string ToString()
        {
            string tmp = "";
            for (int i = 0; i < board_size * 2 + 1; i++)
            {
                if (i % 2 == 0)
                    tmp += "   +---+---+---+---+---+---+---+---+\n";
                else
                {
                    tmp += " " + i / 2 + " ";
                    for (int j = 0; j < board_size; j++)
                    {
                        if(board[(i / 2) * board_size + j].Isocupied())
                            tmp += "| " + board[(i / 2) * board_size + j].Peace.Get_type_char_rep() + " ";
                        else
                            tmp += "| . ";
                    }
                    tmp += "|\n";
                }
            }
            tmp += "     a   b   c   d   e   f   g   h  \n";
            if (!tmp.Equals(ToStringfromlist()))
            {
                Console.WriteLine("the list and board arent syncronised!");
                Console.WriteLine(tmp);
                Console.WriteLine(ToStringfromlist());
            }
            return tmp;
        }

        /// <summary>
        /// return a string representation of the board from the list
        /// </summary>
        /// <returns>string_notation</returns>
        private string ToStringfromlist()
        { //return a string representation of the board
            char[,] board2 = new char[board_size, board_size];
            for (int i = 0; i < board_size; i++)
            {
                for (int j = 0; j < board_size; j++)
                {
                    board2[i, j] = '.';
                }
            }
            int blackpeacecont = 0;
            int whitepeacecont = 0;
            for (int i = 0; i < Chessboard.peaces_types_amount; i++)
            {
                foreach (Peace peace in this.white_parts[i])
                {
                    if (board2[Get_i_pos(peace.position), Get_j_pos(peace.position)] == '.')
                    {
                        //Console.WriteLine(peace.ToString());
                        board2[Get_i_pos(peace.position), Get_j_pos(peace.position)] = peace.Get_type_char_rep();
                        whitepeacecont++;
                    }
                    else
                        Console.WriteLine("second peace on the same position: " + peace.ToString());
                }
                foreach (Peace peace in this.black_parts[i])
                {
                    if (board2[Get_i_pos(peace.position), Get_j_pos(peace.position)] == '.')
                    {
                        //Console.WriteLine(peace.ToString());
                        board2[Get_i_pos(peace.position), Get_j_pos(peace.position)] = peace.Get_type_char_rep();
                        blackpeacecont++;
                    }
                    else
                        Console.WriteLine("second peace on the same position: " + peace.ToString());
                }
            }
            //Console.WriteLine("black peace count: "+ blackpeacecont);
            //Console.WriteLine("white peace count: " + whitepeacecont);
            string tmp = "";
            for (int i = 0; i < board_size * 2+1; i++)
            {
                if (i % 2 == 0)
                    tmp += "   +---+---+---+---+---+---+---+---+\n";
                else 
                {
                    tmp += " "+i / 2 +" ";
                    for (int j = 0; j < board_size; j++)
                    {
                        tmp += "| "+ board2[i/2, j].ToString() +" ";
                        //tmp += "| " + "X" + " ";
                    }
                    tmp += "|\n";
                }
            }
            tmp += "     a   b   c   d   e   f   g   h  \n";
            return tmp;
        }

        /// <summary>
        /// get all of the played moves on the board so far
        /// </summary>
        /// <param name="with_new_lines"></param>
        /// <returns></returns>
        public string Get_all_played_moves(bool with_new_lines =true)
        {
            if (this.moves.Count == 0)
                return "no move has been played";
            string movesst = "";
            Stack<Move> tmpmoves = new Stack<Move>();
            while (this.moves.Count != 0)
            {
                tmpmoves.Push(this.moves.Pop());
            }
            while (tmpmoves.Count != 0)
            {
                Move move = tmpmoves.Pop();
                movesst += move.To_mininal_string();
                if (with_new_lines)
                    movesst += "\n";
                else
                    movesst += "-";
                this.moves.Push(move);
            }
            return movesst.Remove(movesst.Length - 1);
        }

        public void Printstatics()
        {
            Console.WriteLine(this.ToString());
            Console.WriteLine(this.Get_fen_notation());
            Console.WriteLine(this.Get_all_played_moves());
        }

        //this fun check if the king is in check from the king's eye sight
        /// <summary>
        /// retun wheter the current player king is in check
        /// </summary>
        /// <returns></returns>
        public Boolean Current_player_king_in_check()
        {
            return Pos_in_check(Get_king_pos(color_turn));
        }
        private Boolean Pos_in_check_at_diagonaly_up(int position, int direction)
        {
            int pos = position;
            for (int i = 0; i < Movegenerator.squers_to_edge[direction][position]; i++)
            {
                pos += Movegenerator.directions[direction];
                if (board[pos].Isocupied())
                {
                    if (!board[pos].Peace.color.Equals(color_turn))
                    {
                        int pc_type = board[pos].Peace.type;
                        if (pc_type == Peace.Bishop || pc_type == Peace.Queen)
                            return true;
                        if (i == 0)
                        {
                            if (pc_type == Peace.King)
                                return true;
                            if (Is_white_turn() && pc_type == Peace.Pawn)
                                return true;
                        }
                    }
                    break;
                }
            }
            return false;
        }
        private Boolean Pos_in_check_at_diagonaly_down(int position, int direction)
        {
            int pos = position;
            for (int i = 0; i < Movegenerator.squers_to_edge[direction][position]; i++)
            {
                pos += Movegenerator.directions[direction];
                if (board[pos].Isocupied())
                {
                    if (!board[pos].Peace.color.Equals(color_turn))
                    {
                        int pc_type = board[pos].Peace.type;
                        if (pc_type == Peace.Bishop || pc_type == Peace.Queen)
                            return true;
                        if (i == 0)
                        {
                            if (pc_type == Peace.King)
                                return true;
                            if (Is_black_turn() && pc_type == Peace.Pawn)
                                return true;
                        }
                    }
                    break;
                }
            }
            return false;
        }
        private Boolean Pos_in_check_at_verticle(int position, int direction)
        {
            int pos = position;
            for (int i = 0; i < Movegenerator.squers_to_edge[direction][position]; i++)
            {
                pos += Movegenerator.directions[direction];
                if (board[pos].Isocupied())
                {
                    if (!board[pos].Peace.color.Equals(color_turn))
                    {
                        int pc_type = board[pos].Peace.type;
                        if (pc_type == Peace.Rook || pc_type == Peace.Queen)
                            return true;
                        if (pc_type == Peace.King && i == 0)
                            return true;
                    }
                    break;
                }
            }
            return false;
        }
        /// <summary>
        /// check if the position is ander attack
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public Boolean Pos_in_check(int position)
        {
            if (
                Pos_in_check_at_diagonaly_up(position, Movegenerator.right_up) ||
                Pos_in_check_at_diagonaly_down(position, Movegenerator.left_down) ||
                Pos_in_check_at_diagonaly_up(position, Movegenerator.left_up) ||
                Pos_in_check_at_diagonaly_down(position, Movegenerator.right_down) ||
                Pos_in_check_at_verticle(position, Movegenerator.right) ||
                Pos_in_check_at_verticle(position, Movegenerator.left) ||
                Pos_in_check_at_verticle(position, Movegenerator.down) ||
                Pos_in_check_at_verticle(position, Movegenerator.up)
                )
                return true;
            //generate all the moves right_up to the peace
            //generate all the moves left_down to the peace
            //generate all the moves left_up to the peace
            //generate all the moves right_down to the peace
            ////////////////////////////////////////////////////////////vertical movement
            //generate all the moves right to the peace
            //generate all the moves left to the peace
            //generate all the moves down the peace
            //generate all the moves on top of the peace
            int[] op_knight_positions = new int[this.peaces[Opponent_color()][Peace.Knight].Count];
            int t;
            for (t = 0; t < op_knight_positions.Length; t++)
                op_knight_positions[t] = this.peaces[Opponent_color()][Peace.Knight][t].position;
            int i = Get_i_pos(position);
            int j = Get_j_pos(position);
            for (t = 0; t < op_knight_positions.Length; t++)
            {
                if (i != Get_i_pos(op_knight_positions[t]) && j != Get_j_pos(op_knight_positions[t]) && (Math.Abs(i - Get_i_pos(op_knight_positions[t])) + Math.Abs(j - Get_j_pos(op_knight_positions[t])) == 3))
                    return true;
            }
            return false;
        }

        private Boolean There_are_attacks_or_pins_at_diagonaly_up(int position, int direction)
        {
            int defenders = 0;
            int pos = position;
            for (int i = 0; i < Movegenerator.squers_to_edge[direction][position]; i++)
            {
                pos += Movegenerator.directions[direction];
                if (board[pos].Isocupied())
                {
                    if (board[pos].Peace.color != color_turn) //oponent peace
                    {
                        int pc_type = board[pos].Peace.type;
                        if ((pc_type == Peace.Bishop || pc_type == Peace.Queen) && defenders <= 1)
                            return true;
                        if (i == 0)
                        {
                            if (pc_type == Peace.King)
                                return true;
                            if (Is_white_turn() && pc_type == Peace.Pawn)
                                return true;
                        }
                    }
                    if (defenders == 1)
                        break;
                    else
                        defenders++;
                }
            }
            return false;
        }
        private Boolean There_are_attacks_or_pins_at_diagonaly_down(int position, int direction)
        {
            int defenders = 0;
            int pos = position;
            for (int i = 0; i < Movegenerator.squers_to_edge[direction][position]; i++)
            {
                pos += Movegenerator.directions[direction];
                if (board[pos].Isocupied())
                {
                    if (board[pos].Peace.color != color_turn) //oponent peace
                    {
                        int pc_type = board[pos].Peace.type;
                        if ((pc_type == Peace.Bishop || pc_type == Peace.Queen) && defenders <= 1)
                            return true;
                        if (i == 0)
                        {
                            if (pc_type == Peace.King)
                                return true;
                            if (Is_black_turn() && pc_type == Peace.Pawn)
                                return true;
                        }
                    }
                    if (defenders == 1)
                        break;
                    else
                        defenders++;
                }
            }
            return false;
        }
        private Boolean There_are_attacks_or_pins_at_verticle(int position, int direction)
        {
            int defenders = 0;
            int pos = position;
            for (int i = 0; i < Movegenerator.squers_to_edge[direction][position]; i++)
            {
                pos += Movegenerator.directions[direction];
                if (board[pos].Isocupied())
                {
                    if (board[pos].Peace.color != color_turn)
                    {
                        int pc_type = board[pos].Peace.type;
                        if ((pc_type == Peace.Rook || pc_type == Peace.Queen) && defenders <= 1)
                            return true;
                        if (pc_type == Peace.King && i == 0)
                            return true;
                    }
                    if (defenders == 1)
                        break;
                    else
                        defenders++;
                }
            }
            return false;
        }

        /// <summary>
        /// check if there are no pins or attacks, if so that means all of the psudo legal moves are also legal in this position
        /// </summary>
        /// <param name="king_pos"></param>
        /// <returns></returns>
        public bool There_arent_attacks_or_pins(int king_pos)
        {
            if (
                There_are_attacks_or_pins_at_diagonaly_up(king_pos, Movegenerator.right_up) ||
                There_are_attacks_or_pins_at_diagonaly_down(king_pos, Movegenerator.left_down) ||
                There_are_attacks_or_pins_at_diagonaly_up(king_pos, Movegenerator.left_up) ||
                There_are_attacks_or_pins_at_diagonaly_down(king_pos, Movegenerator.right_down) ||
                There_are_attacks_or_pins_at_verticle(king_pos, Movegenerator.right) ||
                There_are_attacks_or_pins_at_verticle(king_pos, Movegenerator.left) ||
                There_are_attacks_or_pins_at_verticle(king_pos, Movegenerator.down) ||
                There_are_attacks_or_pins_at_verticle(king_pos, Movegenerator.up)
                )
                return false;
            //generate all the moves right_up to the peace
            //generate all the moves left_down to the peace
            //generate all the moves left_up to the peace
            //generate all the moves right_down to the peace
            ////////////////////////////////////////////////////////////vertical movement
            //generate all the moves right to the peace
            //generate all the moves left to the peace
            //generate all the moves down the peace
            //generate all the moves on top of the peace
            int[] op_knight_positions = new int[this.peaces[Opponent_color()][Peace.Knight].Count];
            int t;
            for (t = 0; t < op_knight_positions.Length; t++)
                op_knight_positions[t] = this.peaces[Opponent_color()][Peace.Knight][t].position;
            int i_king_pos = Get_i_pos(king_pos);
            int j_king_pos = Get_j_pos(king_pos);
            for (t = 0; t < op_knight_positions.Length; t++)
            {
                if (i_king_pos != Get_i_pos(op_knight_positions[t]) && j_king_pos != Get_j_pos(op_knight_positions[t]) && (Math.Abs(i_king_pos - Get_i_pos(op_knight_positions[t])) + Math.Abs(j_king_pos - Get_j_pos(op_knight_positions[t])) == 3))
                    return false;
            }
            return true;
        }

        //return if the king moved at least once sience the beggining of the game.
        public bool Kingdidntmoved()
        {// check if the king of that peace didnt move from there position once.
            return Is_white_turn() ? this.can_castle[0] : this.can_castle[3];
        }
        public int Get_king_pos(Byte color)
        {// get the position of the king with the input color
            if(peaces[color][Peace.King].Count ==0)
                Printstatics();
            return peaces[color][Peace.King][0].position;
        }
        private static readonly int[] offset = { 0, 3 };
        public static int Getoffset(Byte color)
        {
            return offset[color];
        }
        //gets i/j index for the board matrix representation from a one number position 
        public static int Get_i_pos(int position)
        {//return the i value of the peace/move the board matrix
            return position / 8;
        }
        public static int Get_j_pos(int position)
        {//return the j value of the peace/move the board matrix
            return position % 8;
        }
        public static char Get_j_pos_as_letter(int position)
        {
            const string letters = "ABCDEFGH";
            return letters[Get_j_pos(position)];
        }
        
        //switch the boolean value indicates which player turn it is to play
        public void Switchplayerturn()
        {
            color_turn ^= black; //XOR's the first bit with 1, which toggles it.
            white_turn = !white_turn;
        }
        public void Setplayerturn(bool iswhiteturn)
        {
            color_turn = iswhiteturn ? white : black; //XOR's the first bit with 1, which toggles it.
            white_turn = iswhiteturn;
        }

        public Boolean Is_white_turn()
        {
            return white_turn;
        }
        public Boolean Is_black_turn() => !Is_white_turn();
        public Byte Opponent_color()
        {
            return (Byte)(color_turn ^ 1);//XOR's the first bit with 1, which toggles it.
        }

        public int Evaluate()
        {
            return this.Position_evaluatior.Evaluate();
        }
    }
}