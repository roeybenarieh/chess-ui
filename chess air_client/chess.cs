using connect4_client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace chessair_client
{
    public partial class Chess : Form
    {
        public const int boardsize = 8;
        public const int squeresize = 80;
        public const int boarders_from_window_diagonal = 30;
        public const int boarders_from_window_verticale = 350;

        private Button[,] board;
        internal FlowLayoutPanel moves_history;
        internal Promotion_hundler ph;
        internal Boolean in_the_middle_of_game = false;
        internal Boolean my_turn =false;
        internal Boolean iswhite = true;
        internal string xymarkedpeace = null; // X,Y format;
        //all the moves in the list must be inserted in the right format before entering.
        // represent a name of a button that his potential moves are displayed on the board
        List<string> markedmoves = new List<string>();

        /// <summary>
        /// constructor of the chess board form
        /// </summary>
        /// <param name="f"></param>
        public Chess(Form f)
        {
            Program.receive_message_handler = this.ReceiveMessage;
            Program.Close_form(f);

            InitializeComponent();
            this.Shown += Load_board;
            Program.keep_reading();
        }

        /// <summary>
        /// create all of the chess board's necesery buttons on the form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Load_board(object sender, EventArgs e) 
        { //create a blank board and show it to the player
            this.my_nickname.Invoke((MethodInvoker)delegate () { this.my_nickname.Location = new Point(boarders_from_window_verticale, boarders_from_window_diagonal + (squeresize * boardsize));});

            this.oponent_nickname.Invoke((MethodInvoker)delegate () { this.oponent_nickname.Location =  new Point(boarders_from_window_verticale, boarders_from_window_diagonal-30);});
            board = new Button[boardsize, boardsize];
            Button resign = new Button();
            resign.Text = "resign";
            resign.Size = new Size(squeresize, squeresize/2);
            resign.Location = new Point(boarders_from_window_verticale-resign.Size.Width-10, boarders_from_window_diagonal /* resign.Size.Height*/);
            resign.FlatStyle = FlatStyle.Flat;
            resign.TextAlign = ContentAlignment.MiddleCenter;
            resign.FlatStyle = FlatStyle.Flat;
            resign.BackColor = Color.Green;
            resign.FlatAppearance.BorderColor = Color.Black;
            resign.FlatAppearance.BorderSize = 2;
            resign.Click += new System.EventHandler(Resign_Click);
            Controls.Add(resign);
            for (int i = 0; i < boardsize; i++)// טור
            {
                for (int j = 0; j < boardsize; j++)// שורה
                {
                    board[i,j] = new Button();
                    board[i, j].Name = i + "," + j;
                    board[i, j].Text = i + "," + j;
                    board[i, j].Location = new Point(squeresize * j + boarders_from_window_verticale , squeresize * i + boarders_from_window_diagonal);
                    board[i, j].Size = new Size(squeresize, squeresize);
                    board[i, j].FlatStyle = FlatStyle.Flat;
                    board[i, j].FlatAppearance.BorderSize = 5;
                    if ((i + j) % 2 == 0)
                        board[i, j].BackColor = Color.Green;
                    else
                        board[i, j].BackColor = Color.White;

                    board[i, j].FlatAppearance.BorderColor = board[i, j].BackColor;
                    board[i, j].BackgroundImageLayout = ImageLayout.Center;
                    ///
                    board[i, j].Click += new System.EventHandler(this.Sq_Button_Click);
                    Controls.Add(board[i, j]);
                }
            }
            Put_peaces_in_start_position();
            this.moves_history = new FlowLayoutPanel();
            this.moves_history.Location = new Point(boarders_from_window_verticale+(squeresize * boardsize) + 15, boarders_from_window_diagonal);
            this.moves_history.Size = new Size(this.Size.Width - this.moves_history.Location.X - 30, squeresize * boardsize);
            this.moves_history.AutoScroll = true;
            this.moves_history.BackColor = Color.White;
            this.moves_history.FlowDirection = FlowDirection.LeftToRight;
            this.moves_history.AutoScroll = true;
            this.moves_history.WrapContents = true;
            //this.moves_history.AutoSize = false;
            this.moves_history.Margin = new Padding(0, 0, 0, 0);
            this.moves_history.AutoScrollMargin = new Size(5, 0);
            Controls.Add(this.moves_history);
            
            this.ph = new Promotion_hundler(0, this, false);
        }

        /// <summary>
        /// put all of the peaces in a start chess game position accourding to the client's color
        /// </summary>
        private void Put_peaces_in_start_position()
        {
            // make the board blank
            for (int i = 0; i < boardsize; i++)// טור
            {
                for (int j = 0; j < boardsize; j++)// שורה
                {
                    this.board[i, j].BackgroundImage = null;
                    this.board[i, j].Tag = null;
                }
            }
            int uptowhite = 0;
            int downtowhite = 0;
            int kingjpos;
            int queenjpos;
            if (this.iswhite)
            {
                kingjpos=4;
                queenjpos=3;
                downtowhite = 6;
            }
            else
            {
                uptowhite = 6;
                kingjpos = 3;
                queenjpos = 4;
            }
            //up peaces
            //first row:
            Add_peace(0, 0, Rook + uptowhite);
            Add_peace(0, 1, Knight + uptowhite);
            Add_peace(0, 2, Bishop + uptowhite);
            Add_peace(0, queenjpos, Queen + uptowhite);
            Add_peace(0, kingjpos , King + uptowhite);
            Add_peace(0, 5, Bishop + uptowhite);
            Add_peace(0, 6, Knight + uptowhite);
            Add_peace(0, 7, Rook + uptowhite);
            //second row:
            for (int i = 0; i < boardsize; i++)
            {
                Add_peace(1, i, Pawn + uptowhite);
            }

            //down peaces
            //first row:
            Add_peace(7, 0, Rook +   downtowhite);
            Add_peace(7, 1, Knight + downtowhite);
            Add_peace(7, 2, Bishop + downtowhite);
            Add_peace(7, queenjpos, Queen +  downtowhite);
            Add_peace(7, kingjpos , King +   downtowhite);
            Add_peace(7, 5, Bishop + downtowhite);
            Add_peace(7, 6, Knight + downtowhite);
            Add_peace(7, 7, Rook +   downtowhite);
            //second row:
            for (int i = 0; i < boardsize; i++)
            {
                Add_peace(6, i, Pawn + downtowhite);
            }

        }

        /// <summary>
        /// shows the player that a new game has began and all of the nicknames involved in the game
        /// </summary>
        /// <param name="mynick"></param>
        /// <param name="oppnick"></param>
        private void Start_game_vizualy(string mynick, string oppnick)// chnage the form componnent for the game
        {
            Put_peaces_in_start_position();
            outcome_tx.Invoke((MethodInvoker)delegate () { outcome_tx.Text = "";                                     });
            play_friend.Invoke((MethodInvoker)delegate ()       { play_friend.Visible = false;                                     });
            playai.Invoke((MethodInvoker)delegate () { playai.Visible = false; });
            my_nickname.Invoke((MethodInvoker)delegate ()         { my_nickname.Text = mynick; this.my_nickname.Visible = true;                 });
            oponent_nickname.Invoke((MethodInvoker)delegate ()    { oponent_nickname.Text = oppnick; this.oponent_nickname.Visible = true; });
            in_the_middle_of_game = true;
        }
        
        /// <summary>
        /// shows the player the game has ended and shows the outcome of it
        /// </summary>
        /// <param name="outcome"></param>
        private void End_game_vizualy(String outcome)// chnage the form componnent for the end of the game
        {
            my_turn = false;
            //this.my_nickname.Invoke((MethodInvoker)delegate ()      { this.my_nickname.Visible = false;      });
            //this.oponent_nickname.Invoke((MethodInvoker)delegate () { this.oponent_nickname.Visible = false; });
            this.play_friend.Invoke((MethodInvoker)delegate ()    { this.play_friend.Text = "play vs friend";    this.play_friend.Visible = true; });
            playai.Invoke((MethodInvoker)delegate () { playai.Visible = true; });
            outcome = outcome.Remove(0,13);
            this.outcome_tx.Invoke((MethodInvoker)delegate () { this.outcome_tx.Text = outcome+"!"; });
            in_the_middle_of_game = false;
        }

        /// <summary>
        /// handle what happen when the player click one of the board's squere buttons
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Sq_Button_Click(object sender, EventArgs e)
        {
            this.ph.Stop_show();
            if (my_turn /*&& g*/) //and check if its one of my peaces
            {
                Button button = sender as Button;
                if (this.xymarkedpeace == null && Button_image_is_my_color(button.Name[0], button.Name[2]))//if want to mark a peace so he could move it
                {
                    this.xymarkedpeace = button.Name;// the [2] is befor the [0], just exept it!!
                    Program.SendMessage("###pot_move###" + Chartointposition(button.Name[0]) + Chartointposition(button.Name[2])); //asks the server - which legal moves exist in that position
                }
                else if (this.xymarkedpeace != null && button.FlatAppearance.BorderColor == Color.Red)//if allready marked a peace and want to move it
                {
                    string[] pos = xymarkedpeace.Split(',');
                    int correction = iswhite? 6 : 0;
                    if (Int32.Parse(this.board[Int32.Parse(pos[0]), Int32.Parse(pos[1])].Tag.ToString()) == Pawn +correction &&
                        button.Name[0].Equals('0'))
                        this.ph = new Promotion_hundler(Int32.Parse(button.Name.Split(',')[1]), this,true);
                    else
                        Send_move_to_server(Chartointposition(this.xymarkedpeace[0]),
                            Chartointposition(this.xymarkedpeace[2]),
                            Chartointposition(button.Name[0]),
                            Chartointposition(button.Name[2])); //tell the server to make a move
                }
                else// clicked on a unrelated button - reset the first "if" statements
                {
                    Remove_all_potmoves();
                    this.xymarkedpeace = null;
                }
            }
        }

        /// <summary>
        /// send the server this player whants to resign
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Resign_Click(object sender, EventArgs e)
        {
            if(in_the_middle_of_game)
                Program.SendMessage("###resignation###");
        }

        /// <summary>
        /// gets the start and end X,Y cordinates. than format the move and send it to the server
        /// </summary>
        /// <param name="start_j"></param>
        /// <param name="start_i"></param>
        /// <param name="end_j"></param>
        /// <param name="end_i"></param>
        /// <param name="promotion"></param>
        public void Send_move_to_server(int start_j, int start_i, int end_j, int end_i, string promotion =null)
        {
            if(promotion == null)
                Program.SendMessage("###move###" + 
                    start_j + start_i + 
                    end_j + end_i); //tell the server to make a move
            else
                Program.SendMessage("###move###" +
                    start_j + start_i +
                    end_j + end_i +
                    promotion); //tell the server to make a move
        }
        
        /// <summary>
        /// remove any potential moves shown on the board
        /// </summary>
        private void Remove_all_potmoves()
        {
            foreach (string move in this.markedmoves)
            {
                // the moves in the list are allready in the right format so there is no need to format them again...
                int i = Chartoint(move[0]);
                int j = Chartoint(move[1]);
                //board[i, j].FlatAppearance.BorderColor = board[i, j].BackColor;
                if (board[i, j].InvokeRequired)
                {
                    board[i, j].Invoke((MethodInvoker)delegate ()
                    {
                        board[i, j].FlatAppearance.BorderColor = board[i, j].BackColor;
                    });

                }
                else
                    board[i, j].FlatAppearance.BorderColor = board[i, j].BackColor;
            }
            this.markedmoves = new List<string>();
        }

        /// <summary>
        /// checks if a button in X,Y cordinated is the same color of this player
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private Boolean Button_image_is_my_color(char x, char y)
        {
            //supose to be - '0;
            int xvalue = Chartoint(x);
            int yvalue = Chartoint(y);
            if (this.board[xvalue, yvalue].Tag == null)
                return false;
            if (this.iswhite)
            {
                return Convert.ToInt32(this.board[xvalue, yvalue].Tag)>5;
            }
            return Convert.ToInt32(this.board[xvalue, yvalue].Tag) <6;
        } 
        
        /// <summary>
        /// gets a character and return a integer representation of it
        /// </summary>
        /// <param name="character"></param>
        /// <returns></returns>
        internal static int Chartoint(char character)
        {
            return character - '0';
        }
        /// <summary>
        /// gets a char that represent a collomb and format it to integer from the prospective of white - for the server
        /// </summary>
        /// <param name="position_character"></param>
        /// <returns></returns>
        internal int Chartointposition(char position_character) {

            if (this.iswhite) //no need to change the format since the server calculate the positions as white.
                return Chartoint(position_character);
            return Math.Abs(position_character - '0' - 7);
        }
        /// <summary>
        /// handle messages from the server
        /// </summary>
        /// <param name="ar"></param>
        private void ReceiveMessage(string textFromServer)
        {
            // what happen after the cliant register - what the server returns and what happen as a result
            if (textFromServer.StartsWith("###start_game###"))
            {//my nickname & opponent nickname & my color 
                textFromServer = textFromServer.Remove(0, 16);
                String[] gamedata = textFromServer.Split('&');
                if (gamedata[2] == "white")
                {
                    this.iswhite = true; my_turn = true;
                }
                else
                {
                    this.iswhite = false; my_turn = false;
                }

                Start_game_vizualy(gamedata[0], gamedata[1]);
            }
            else if (textFromServer.StartsWith("###move###"))
            {// after ###move## there is X,Y start cordinations and X,Y end cordinations
                textFromServer = textFromServer.Remove(0, 10);
                Remove_all_potmoves();
                //come buck to the current board position:
                moves_history.Invoke((MethodInvoker)delegate () {
                    if (moves_history.Controls.Count != 0)
                    {
                        Control lastmove = moves_history.Controls[moves_history.Controls.Count - 1];
                        Set_notation_at_board(lastmove.Tag.ToString());
                    }
                });
                // the [1] is before the [0], just exept it!!
                int[] movedata ={ Chartointposition(textFromServer[0]),
                            Chartointposition(textFromServer[1]),
                            Chartointposition(textFromServer[2]),
                            Chartointposition(textFromServer[3])};
                Move_peace(movedata[0], movedata[1], movedata[2], movedata[3]);//moving peace to the new position
                string[] edgecase = textFromServer.Split('#');
                if (!edgecase[1].Equals(no_edgecase.ToString()))//incase of a edgecase
                {
                    string edgecasemassage = "";
                    if (edgecase[1].Equals(castle.ToString())) //castling
                    {
                        if (movedata[3] - movedata[1] == 2)
                        {  //king side
                            Move_peace(movedata[0], 7, movedata[0], iswhite ? 5 : 4);
                            edgecasemassage = "0 - 0";
                        }
                        else
                        {
                            Move_peace(movedata[0], 0, movedata[0], iswhite ? 3 : 2);
                            edgecasemassage = "0-0-0";
                        }
                    }
                    else if (edgecase[1].Equals(enpassant.ToString())) //unpasant
                    {
                        Delete_peace(movedata[2] + (iswhite ? 1 : -1), movedata[3]);//delete the captured pawn
                    }
                    else //promotion
                    {
                        int color = 0;
                        if (!(this.iswhite ^ this.my_turn))//if only one of the bool is true, otherwise false
                            color = 6;
                        edgecasemassage = "-";
                        if (edgecase[1].StartsWith(pawn_promote_to_queen.ToString()))
                        {
                            Add_peace(movedata[2], movedata[3], Queen + color);
                            edgecasemassage += "q";
                        }
                        else if (edgecase[1].StartsWith(pawn_promote_to_rook.ToString()))
                        {
                            Add_peace(movedata[2], movedata[3], Rook + color);
                            edgecasemassage += "r";
                        }
                        else if (edgecase[1].StartsWith(pawn_promote_to_bishop.ToString()))
                        {
                            Add_peace(movedata[2], movedata[3], Bishop + color);
                            edgecasemassage += "b";
                        }
                        else if (edgecase[1].StartsWith(pawn_promote_to_knight.ToString()))
                        {
                            Add_peace(movedata[2], movedata[3], Knight + color);
                            edgecasemassage += "k";
                        }
                    }
                    Add_historical_move(textFromServer, edgecasemassage);//add historical move
                }
                else
                    Add_historical_move(textFromServer);//add historical move
                this.xymarkedpeace = null;//unmark pot moves of a peace
                this.my_turn = !this.my_turn;//change the turn
            }
            else if (textFromServer.StartsWith("###posmoves###"))
            { // a set of all of the posible XY, end cordinations of a move. -in a XY,XY,XY format
                textFromServer = textFromServer.Remove(0, 14);
                string[] movedata = textFromServer.Split(',');
                foreach (string move in movedata)
                {
                    int i = Chartointposition(move[0]);// the [1] is before the [0], just exept it!!
                    int j = Chartointposition(move[1]);
                    board[i, j].Invoke((MethodInvoker)delegate ()
                    {
                        board[i, j].FlatAppearance.BorderColor = Color.Red;
                    });
                    markedmoves.Add(i.ToString() + j.ToString());
                }
            }
            else if (textFromServer.StartsWith("###endgame###"))// the game ended
            {
                End_game_vizualy(textFromServer);
            }
        }
        
        /// <summary>
        /// gets a char representation of column and return the integer representation of it
        /// </summary>
        /// <param name="j_pos"></param>
        /// <returns></returns>
        private static char Get_j_pos_as_letter(Char j_pos)
        {
            const string letters = "ABCDEFGH";
            return letters[Chartoint(j_pos)];
        }
        /// <summary>
        /// gets a string representation of a move and add new historical button
        /// </summary>
        /// <param name="move"></param>
        /// <param name="edgecase_message"></param>
        private void Add_historical_move(string move, string edgecase_message="")
        {
            string print_in_notation(string move_edgecase, char j_start_pos, char i_start_pos, char j_end_pos, char i_end_pos)
            {
                if (move_edgecase.StartsWith("0"))
                    return move_edgecase;
                return Get_j_pos_as_letter(j_start_pos) + (8 - Chartoint(i_start_pos)).ToString() + "," 
                    + Get_j_pos_as_letter(j_end_pos) + (8 - Chartoint(i_end_pos)).ToString() + move_edgecase;
            }
            Button b = new Button()
            {
                Text = print_in_notation(edgecase_message, move[1], move[0], move[3], move[2]),
                Font = new Font("Microsoft Sans Serif", (squeresize / 5)),
                Size = new Size(this.moves_history.Width / 2, squeresize), //-10
                Margin = new Padding(0, 0, 0, 0),
                BackColor = (my_turn == iswhite) ? Color.White : Color.Black,
                ForeColor = (my_turn == iswhite) ? Color.Black : Color.White,
                FlatStyle = FlatStyle.Flat
            };
            b.FlatAppearance.BorderSize = 5;
            b.FlatAppearance.BorderColor = b.BackColor;
            b.Tag = Get_notation();
            b.Click += new System.EventHandler(this.Histocial_button_Click);
            moves_history.Invoke((MethodInvoker)delegate () { moves_history.Controls.Add(b);
                Control control = moves_history.Controls[moves_history.Controls.Count - 1];
                moves_history.ScrollControlIntoView(control);//scroll to the new button
            });//add new button
            
        }
        /// <summary>
        /// brings the board to the board position after the move that is writen on the button has been done
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Histocial_button_Click(object sender, EventArgs e)
        {
            Button button = sender as Button;
            Remove_all_potmoves();
            this.ph.Stop_show();
            Set_notation_at_board(button.Tag.ToString());
        }
        private const string no_peace = "15";
        /// <summary>
        /// get a string representation of the chess board
        /// </summary>
        /// <returns></returns>
        private string Get_notation()
        {
            //notation format: 1,2,3,4,5,6,7,8/9,0,11,12,15,10
            //15 means there is no peace in there, /means a new row, numberms represent the pictured corolated to the wanted peace
            string notation = "";
            for (int i = 0; i < boardsize; i++)
            {
                for (int j = 0; j < boardsize; j++)
                {
                    if (board[i, j].BackgroundImage != null)
                    {
                        notation += board[i, j].Tag;//gets the number of the 
                    }
                    else
                    {
                        notation += no_peace;//gets the number of the 
                    }
                    if (j != boardsize - 1) //if not the last line
                        notation += ",";
                }
                if (i != boardsize - 1) //if not the last line
                    notation += "/";
            }
            return notation;
        }

        /// <summary>
        /// change the chess board according the string representation "notation"
        /// </summary>
        /// <param name="notation"></param>
        private void Set_notation_at_board(string notation)
        {
            string[] rows = notation.Split('/');
            Parallel.For(0, boardsize, (row) =>
            {
                string[] peaces = rows[row].Split(',');
                for(int j=0;j<peaces.Length;j++)
                {
                    if (peaces[j].Equals(no_peace))
                    {
                        Delete_peace(row, j);
                    }
                    else
                    {
                        //int peace = Int32.Parse(peaces[j]);
                        //if (board[row, j].Tag != null && board[row, j].Tag.Equals(peaces[j]))
                        //    break;
                        Add_peace(board[row, j], Int32.Parse(peaces[j]));
                    }
                }
            });
        }
        /// <summary>
        /// gets initial and final X,Y cordinates and move the piece acordingly on the board
        /// </summary>
        /// <param name="in_i"></param>
        /// <param name="in_j"></param>
        /// <param name="fn_i"></param>
        /// <param name="fn_j"></param>
        private void Move_peace(int in_i, int in_j, int fn_i, int fn_j)
        {
            //moving peace to the new position
            Add_peace(board[fn_i, fn_j], Int32.Parse(board[in_i, in_j].Tag.ToString()));

            //deleting records of the peace in the old position
            Delete_peace(in_i, in_j);
        }

        /// <summary>
        /// remove a peace from a button in the X,Y cordinates
        /// </summary>
        /// <param name="i"></param>
        /// <param name="j"></param>
        private void Delete_peace(int i, int j)
        {
            board[i,j].BackgroundImage = null; board[i,j].Tag = null;
        }
        
        /// <summary>
        /// add a peace to a button object
        /// </summary>
        /// <param name="button"></param>
        /// <param name="peacenum"></param>
        internal void Add_peace(Button button, int peacenum)
        {
            button.BackgroundImage = imageList1.Images[peacenum];
            button.Tag = peacenum.ToString();
        }
        /// <summary>
        /// add a peace to a button in the X,Y cordinates
        /// </summary>
        /// <param name="i"></param>
        /// <param name="j"></param>
        /// <param name="peacenum"></param>
        private void Add_peace(int i, int j, int peacenum)
        {
            this.board[i,j].BackgroundImage = imageList1.Images[peacenum];
            this.board[i, j].Tag = peacenum.ToString();
        }
        /// <summary>
        /// when the form is closing the client close the connection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Chessair_FormClosing(object sender, FormClosingEventArgs e)
        {
            Program.Disconnect_server();
        }

        /// <summary>
        /// handle what happen when the player wants to player versus another player
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Play_Click(object sender, EventArgs e)
        {
            if (play_friend.Text == "play vs friend")
            {
                play_friend = sender as Button;
                //play_friend.Font = new Font(play_friend.Font.FontFamily, 20);
                play_friend.Text = "waiting for a friend..(hopefully you have one)";

                //
                Program.SendMessage("###ready_to_play_vs_friend###");
            }
        }
        /// <summary>
        /// handle what happen when the player wants to player versus the engine
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Playai_Click(object sender, EventArgs e)
        {
            playai = sender as Button;
            Program.SendMessage("###ready_to_play_vs_ai###");
        }


        public const int Pawn = 0;
        public const int Knight = 1;
        public const int Bishop = 2;
        public const int Rook = 3;
        public const int Queen = 4;
        public const int King = 5;
        public const int dot = 12;
        //
        public const int no_edgecase = 0;
        public const int pawn_promote_to_knight = 1;
        public const int pawn_promote_to_bishop = 2;
        public const int pawn_promote_to_rook = 3;
        public const int pawn_promote_to_queen = 4;
        public const int castle = 5;
        public const int enpassant = 9;
        //
    }
}
