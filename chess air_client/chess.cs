using connect4_client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace chessair_client
{
    public partial class chess : Form
    {
        public const int boardsize = 8;
        public const int squeresize = 80;
        public const int boarders_from_window_diagonal = 30;
        public const int boarders_from_window_verticale = 350;

        private Button[,] board;
        internal FlowLayoutPanel moves_history;
        internal promotion_hundler ph;
        internal Boolean my_turn =false;
        internal Boolean iswhite = true;
        internal string xymarkedpeace = null; // X,Y format;
        //all the moves in the list must be inserted in the right format before entering.
        // represent a name of a button that his potential moves are displayed on the board
        List<string> markedmoves = new List<string>();

        public chess(Form f)
        {
            try
            {
                f.Hide();
            }
            catch (Exception)
            {
                f.Invoke((MethodInvoker)delegate () { f.Hide(); });
            }

            InitializeComponent();
            this.Shown += Load_board;
        }

        private void Load_board(object sender, EventArgs e) 
        { //create a blank board and show it to the player
            this.my_nickname.Invoke((MethodInvoker)delegate () { this.my_nickname.Location = new Point(boarders_from_window_verticale, boarders_from_window_diagonal + (squeresize * boardsize));});

            this.oponent_nickname.Invoke((MethodInvoker)delegate () { this.oponent_nickname.Location =  new Point(boarders_from_window_verticale, boarders_from_window_diagonal-30);});
            board = new Button[boardsize, boardsize];
            for (int i = 0; i < boardsize; i++)// טור
            {
                for (int j = 0; j < boardsize; j++)// שורה
                {
                    board[i,j] = new Button();
                    board[i, j].Name = i + "," + j;
                    board[i, j].Text = i + "," + j;
                    board[i, j].Location = new Point(squeresize * i + boarders_from_window_verticale , squeresize * j + boarders_from_window_diagonal);
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
                    board[i, j].Click += new System.EventHandler(this.button_Click);
                    Controls.Add(board[i, j]);
                }
            }
            put_peaces_in_start_position();
            this.moves_history = new FlowLayoutPanel();
            this.moves_history.Location = new Point(boarders_from_window_verticale+(squeresize * boardsize) + 15, boarders_from_window_diagonal);
            this.moves_history.Size = new Size(this.Size.Width - this.moves_history.Location.X - 30, squeresize * boardsize);
            this.moves_history.AutoScroll = true;
            this.moves_history.BackColor = Color.White;
            this.moves_history.BorderStyle = BorderStyle.FixedSingle;
            this.moves_history.FlowDirection = FlowDirection.LeftToRight;
            this.moves_history.WrapContents = true;
            this.moves_history.AutoSize = false;
            this.moves_history.AutoScroll = true;
            Controls.Add(this.moves_history);
            for (int i = 0; i < 20; i++)
            {
                this.moves_history.Controls.Add(new Label() { Text = "Moves History"+i.ToString() });
            }
            Controls.Add(this.moves_history);
            
            this.ph = new promotion_hundler(0, this, false);
        }

        //sets all of the peaces in a start chess game position accourding to the player's color
        private void put_peaces_in_start_position()
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
            add_peace(this.board[0, 0], Rook + uptowhite);
            add_peace(this.board[1, 0], Knight + uptowhite);
            add_peace(this.board[2, 0], Bishop + uptowhite);
            add_peace(this.board[queenjpos, 0], Queen + uptowhite);
            add_peace(this.board[kingjpos, 0], King + uptowhite);
            add_peace(this.board[5, 0], Bishop + uptowhite);
            add_peace(this.board[6, 0], Knight + uptowhite);
            add_peace(this.board[7, 0], Rook + uptowhite);
            //second row:
            for (int i = 0; i < boardsize; i++)
            {
                add_peace(this.board[i,1], Pawn + uptowhite);
            }

            //down peaces
            //first row:
            add_peace(this.board[0, 7], Rook +   downtowhite);
            add_peace(this.board[1, 7], Knight + downtowhite);
            add_peace(this.board[2, 7], Bishop + downtowhite);
            add_peace(this.board[queenjpos, 7], Queen +  downtowhite);
            add_peace(this.board[kingjpos, 7], King +   downtowhite);
            add_peace(this.board[5, 7], Bishop + downtowhite);
            add_peace(this.board[6, 7], Knight + downtowhite);
            add_peace(this.board[7, 7], Rook +   downtowhite);
            //second row:
            for (int i = 0; i < boardsize; i++)
            {
                add_peace(this.board[i,6], Pawn + downtowhite);
            }

        }

        private void Start_game_vizualy(string mynick, string oppnick)// chnage the form componnent for the game
        {
            put_peaces_in_start_position();
            outcome_tx.Invoke((MethodInvoker)delegate () { outcome_tx.Text = "";                                     });
            play_friend.Invoke((MethodInvoker)delegate ()       { play_friend.Visible = false;                                     });
            playai.Invoke((MethodInvoker)delegate () { playai.Visible = false; });
            aivsai.Invoke((MethodInvoker)delegate () { aivsai.Visible = false; });
            my_nickname.Invoke((MethodInvoker)delegate ()         { my_nickname.Text = mynick; this.my_nickname.Visible = true;                 });
            oponent_nickname.Invoke((MethodInvoker)delegate ()    { oponent_nickname.Text = oppnick; this.oponent_nickname.Visible = true; });
        }
        
        private void End_game_vizualy(String outcome)// chnage the form componnent for the end of the game
        {
            my_turn = false;
            //this.my_nickname.Invoke((MethodInvoker)delegate ()      { this.my_nickname.Visible = false;      });
            //this.oponent_nickname.Invoke((MethodInvoker)delegate () { this.oponent_nickname.Visible = false; });
            this.play_friend.Invoke((MethodInvoker)delegate ()    { this.play_friend.Text = "play vs friend";    this.play_friend.Visible = true; });
            playai.Invoke((MethodInvoker)delegate () { playai.Visible = true; });
            aivsai.Invoke((MethodInvoker)delegate () { aivsai.Visible = true; });
            outcome = outcome.Remove(0,13);
            this.outcome_tx.Invoke((MethodInvoker)delegate () { this.outcome_tx.Text = outcome+"!"; });
        }

        private void button_Click(object sender, EventArgs e)
        {
            this.ph.stop_show();
            if (my_turn /*&& g*/) //and check if its one of my peaces
            {
                Button button = sender as Button;
                if (this.xymarkedpeace == null && button_image_is_my_color(button.Name[0], button.Name[2]))//if want to mark a peace so he could move it
                {
                    this.xymarkedpeace = button.Name;// the [2] is befor the [0], just exept it!!
                    Program.SendMessage("###pot_move###" + chartointposition(button.Name[2]) + chartointposition(button.Name[0])); //asks the server - which legal moves exist in that position
                }
                else if (this.xymarkedpeace != null && button.FlatAppearance.BorderColor == Color.Red)//if allready marked a peace and want to move it
                {
                    string[] pos = xymarkedpeace.Split(',');
                    int correction = 0;
                    if (iswhite)
                        correction = 6;
                    if (Int32.Parse(this.board[Int32.Parse(pos[0]), Int32.Parse(pos[1])].Tag.ToString()) == Pawn +correction &&
                        button.Name[2].Equals('0'))
                        this.ph = new promotion_hundler(Int32.Parse(button.Name.Split(',')[0]), this,true);
                    else
                        send_move_to_server(chartointposition(this.xymarkedpeace[2]),
                            chartointposition(this.xymarkedpeace[0]),
                            chartointposition(button.Name[2]),
                            chartointposition(button.Name[0])); //tell the server to make a move
                }
                else// clicked on a unrelated button - reset the first "if" statements
                {
                    remove_all_potmoves();
                    this.xymarkedpeace = null;
                }
            }
        }
        
        public void send_move_to_server(int start_j, int start_i, int end_j, int end_i, string promotion =null)
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
        
        private void remove_all_potmoves()
        {
            foreach (string move in this.markedmoves)
            {
                // the moves in the list are allready in the right format so there is no need to format them again...
                int i = chartoint(move[0]);
                int j = chartoint(move[1]);
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

        private Boolean button_image_is_my_color(char x, char y)
        {
            //supose to be - '0;
            int xvalue = chartoint(x);
            int yvalue = chartoint(y);
            if (this.board[xvalue, yvalue].Tag == null)
                return false;
            if (this.iswhite)
            {
                return Convert.ToInt32(this.board[xvalue, yvalue].Tag)>5;
            }
            return Convert.ToInt32(this.board[xvalue, yvalue].Tag) <6;
        } 
        
        internal int chartoint(char character)
        {
            return character - '0';
        }
        internal int chartointposition(char position_character) {

            if (this.iswhite) //no need to change the format since the server calculate the positions as white.
                return chartoint(position_character);
            return Math.Abs(position_character - '0' - 7);
        }

        private void ReceiveMessage(IAsyncResult ar)
        {
            try
            {
                int bytesRead;

                // read the data from the server
                bytesRead = Program.client.GetStream().EndRead(ar);

                if (bytesRead < 1)
                {
                    return;
                }
                else
                {
                    // invoke the delegate to display the recived data
                    string textFromServer = System.Text.Encoding.ASCII.GetString(Program.data, 0, bytesRead);
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
                            this.iswhite =false; my_turn = false;
                        }
                            
                        Start_game_vizualy(gamedata[0], gamedata[1]);
                    }
                    else if (textFromServer.StartsWith("###move###"))
                    {// after ###move## there is X,Y start cordinations and X,Y end cordinations
                        textFromServer = textFromServer.Remove(0, 10);
                        remove_all_potmoves();
                        // the [1] is before the [0], just exept it!!
                        int[] movedata ={ chartointposition(textFromServer[1]), 
                            chartointposition(textFromServer[0]), 
                            chartointposition(textFromServer[3]), 
                            chartointposition(textFromServer[2])};
                        move_peace(movedata[0], movedata[1], movedata[2], movedata[3]);//moving peace to the new position

                        string[] edgecase = textFromServer.Split('#');
                        if (!edgecase[1].Equals(no_edgecase.ToString()))//incase of a edgecase
                        {
                            for(int i=0;i<movedata.Length;i++)
                                Console.WriteLine(movedata[i]);
                            if (edgecase[1].Equals(castle.ToString())) //castling
                            {
                                if (movedata[2] == 6) //right castle
                                    move_peace(7, movedata[1], 5, movedata[1]);
                                else if (movedata[2] == 2)
                                    move_peace(0, movedata[1], 3, movedata[1]);

                            }
                            else if (edgecase[1].Equals(enpassant.ToString())) //unpasant
                            {
                                delete_peace(movedata[2], movedata[3] - 1);//delete the captured pawn
                            }
                            else //promotion
                            {
                                int color = 0;
                                if (!(this.iswhite ^ this.my_turn))//if only one of the bool is true, otherwise false
                                    color = 6;
                                if (edgecase[1].StartsWith(pawn_promote_to_queen.ToString()))
                                {
                                    add_peace(movedata[2], movedata[3], Queen + color);
                                }
                                else if (edgecase[1].StartsWith(pawn_promote_to_rook.ToString()))
                                {
                                    add_peace(movedata[2], movedata[3], Rook + color);
                                }
                                else if (edgecase[1].StartsWith(pawn_promote_to_bishop.ToString()))
                                {
                                    add_peace(movedata[2], movedata[3], Bishop + color);
                                }
                                else if (edgecase[1].StartsWith(pawn_promote_to_knight.ToString()))
                                {
                                    add_peace(movedata[2], movedata[3], Knight + color);
                                }
                            }
                        }
                        
                        this.xymarkedpeace = null;//unmark pot moves of a peace
                        this.my_turn =! this.my_turn;//change the turn
                    }
                    else if (textFromServer.StartsWith("###posmoves###"))
                    { // a set of all of the posible XY, end cordinations of a move. -in a XY,XY,XY format
                        textFromServer = textFromServer.Remove(0, 14);
                        string[] movedata = textFromServer.Split(',');
                        foreach (string move in movedata)
                        {
                            int i = chartointposition(move[1]);// the [1] is before the [0], just exept it!!
                            int j = chartointposition(move[0]);
                            board[i,j].Invoke((MethodInvoker)delegate ()
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
                Program.client.GetStream().BeginRead(Program.data,
                                             0,
                                             System.Convert.ToInt32(Program.client.ReceiveBufferSize),
                                             ReceiveMessage,
                                             null);
            }
            catch (Exception)
            {
                // ignor the error... fired when the user loggs off
                Console.WriteLine("uston we have a problemo");
            }
        }

        private void move_peace(int in_i, int in_j, int fn_i, int fn_j)
        {
            //moving peace to the new position
            add_peace(board[fn_i, fn_j], Int32.Parse(board[in_i, in_j].Tag.ToString()));

            //deleting records of the peace in the old position
            delete_peace(in_i, in_j);
        }
        //remove a peace from a button
        private void delete_peace(int i, int j)
        {
            board[i,j].BackgroundImage = null; board[i,j].Tag = null;
        }
        //both of these fun add a peace to a button
        internal void add_peace(Button button, int peacenum)
        {
            button.BackgroundImage = imageList1.Images[peacenum];
            button.Tag = peacenum.ToString();
        }
        private void add_peace(int i, int j, int peacenum)
        {
            this.board[i,j].BackgroundImage = imageList1.Images[peacenum];
            this.board[i, j].Tag = peacenum.ToString();
        }

        private bool same_images(Image ibmp1, Image ibmp2)
        {
            Bitmap bmp1 = new Bitmap(ibmp1);
            Bitmap bmp2 = new Bitmap(ibmp2);
            bool equals = true;
            bool flag = true;  //Inner loop isn't broken
            //Test to see if we have the same size of image
            if (bmp1.Size == bmp2.Size)
            {
                for (int x = 0; x < bmp1.Width; ++x)
                {
                    for (int y = 0; y < bmp1.Height; ++y)
                    {
                        if (bmp1.GetPixel(x, y) != bmp2.GetPixel(x, y))
                        {
                            equals = false;
                            flag = false;
                            break;
                        }
                    }
                    if (!flag)
                    {
                        break;
                    }
                }
            }
            else
            {
                equals = false;
            }
            return equals;
        }

        private void chessair_FormClosing(object sender, FormClosingEventArgs e)
        {
            Program.disconnect_server();
        }

        private void play_Click(object sender, EventArgs e)
        {
            if (play_friend.Text == "play vs friend")
            {
                play_friend = sender as Button;
                //play_friend.Font = new Font(play_friend.Font.FontFamily, 20);
                play_friend.Text = "waiting for a friend..(hopefully you have one)";

                //
                Program.SendMessage("###ready_to_play_vs_friend###");
                Program.client.GetStream().BeginRead(Program.data,
                                                 0,
                                                 System.Convert.ToInt32(Program.client.ReceiveBufferSize),
                                                 ReceiveMessage,
                                                 null);
            }
        }

        private void playai_Click(object sender, EventArgs e)
        {
            playai = sender as Button;
            Program.SendMessage("###ready_to_play_vs_ai###");
            Program.client.GetStream().BeginRead(Program.data,
                                             0,
                                             System.Convert.ToInt32(Program.client.ReceiveBufferSize),
                                             ReceiveMessage,
                                             null);
        }

        private void aivsai_Click(object sender, EventArgs e)
        {
            aivsai = sender as Button;
            Program.SendMessage("###ai_vs_ai###");
            Program.client.GetStream().BeginRead(Program.data,
                                             0,
                                             System.Convert.ToInt32(Program.client.ReceiveBufferSize),
                                             ReceiveMessage,
                                             null);
        }

        private void chess_Load(object sender, EventArgs e)
        {

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
