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
        public const int Pawn = 0;
        public const int Knight = 1;
        public const int Bishop = 2;
        public const int Rook = 3;
        public const int Queen = 4;
        public const int King = 5;
        public const int dot = 12;
        private const int boardsize = 8;
        private const int rectangesize = 80;
        private const int boarders_from_window_diagonal = 30;
        private const int boarders_from_window_verticale = 350;

        private Button[,] board = new Button[boardsize, boardsize];
        private Boolean my_turn =false;
        private Boolean iswhite = true;
        private string xymarkedpeace = null; // X,Y format
        List<string> markedmoves = new List<string>();  //all the moves in the list must be inserted in the right format before entering.

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
            this.my_nickname.Invoke((MethodInvoker)delegate () { this.my_nickname.Location = new Point(boarders_from_window_verticale, boarders_from_window_diagonal + (rectangesize * boardsize));});

            this.oponent_nickname.Invoke((MethodInvoker)delegate () { this.oponent_nickname.Location =  new Point(boarders_from_window_verticale, boarders_from_window_diagonal-30);});
            for (int i = 0; i < boardsize; i++)// טור
            {
                for (int j = 0; j < boardsize; j++)// שורה
                {
                    board[i,j] = new Button();
                    board[i, j].Name = i + "," + j;
                    board[i, j].Text = i + "," + j;
                    board[i, j].Location = new Point(rectangesize * i + boarders_from_window_verticale , rectangesize * j + boarders_from_window_diagonal);
                    board[i, j].Size = new Size(rectangesize, rectangesize);
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
        }

        //sets all of the peaces in a start chess game position
        private void put_peaces_in_start_position()
        {
            void changebackgroundimage(Button button, int peacenum)
            {
                button.BackgroundImage = imageList1.Images[peacenum];
                button.Tag = peacenum.ToString();
            }
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
            changebackgroundimage(this.board[0, 0], Rook + uptowhite);
            changebackgroundimage(this.board[1, 0], Knight + uptowhite);
            changebackgroundimage(this.board[2, 0], Bishop + uptowhite);
            changebackgroundimage(this.board[queenjpos, 0], Queen + uptowhite);
            changebackgroundimage(this.board[kingjpos, 0], King + uptowhite);
            changebackgroundimage(this.board[5, 0], Bishop + uptowhite);
            changebackgroundimage(this.board[6, 0], Knight + uptowhite);
            changebackgroundimage(this.board[7, 0], Rook + uptowhite);
            //second row:
            for (int i = 0; i < boardsize; i++)
            {
                changebackgroundimage(this.board[i,1], Pawn + uptowhite);
            }

            //down peaces
            //first row:
            changebackgroundimage(this.board[0, 7], Rook +   downtowhite);
            changebackgroundimage(this.board[1, 7], Knight + downtowhite);
            changebackgroundimage(this.board[2, 7], Bishop + downtowhite);
            changebackgroundimage(this.board[queenjpos, 7], Queen +  downtowhite);
            changebackgroundimage(this.board[kingjpos, 7], King +   downtowhite);
            changebackgroundimage(this.board[5, 7], Bishop + downtowhite);
            changebackgroundimage(this.board[6, 7], Knight + downtowhite);
            changebackgroundimage(this.board[7, 7], Rook +   downtowhite);
            //second row:
            for (int i = 0; i < boardsize; i++)
            {
                changebackgroundimage(this.board[i,6], Pawn + downtowhite);
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
            //bool g = true;
            //startagain:
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
                    Program.SendMessage("###move###" + chartointposition(this.xymarkedpeace[2]) + chartointposition(this.xymarkedpeace[0]) + chartointposition(button.Name[2]) + chartointposition(button.Name[0])); //tell the server to make a move
                }
                else// clicked on a unrelated button - reset the first "if" statements
                {
                    //g = false;
                    remove_all_potmoves();
                    this.xymarkedpeace = null;
                    //goto startagain;
                }
            }
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
        
        private int chartoint(char character)
        {
            return character - '0';
        }
        private int chartointposition(char position_character) {

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
                        int[] movedata ={ chartointposition(textFromServer[1]), chartointposition(textFromServer[0]), chartointposition(textFromServer[3]), chartointposition(textFromServer[2])};
                        board[movedata[2], movedata[3]].BackgroundImage = (Image)board[movedata[0], movedata[1]].BackgroundImage.Clone();
                        board[movedata[2], movedata[3]].Tag = board[movedata[0], movedata[1]].Tag;
                        board[movedata[0], movedata[1]].BackgroundImage = null; board[movedata[0], movedata[1]].Tag = null;
                        this.xymarkedpeace = null;
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
            }
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
    }
}
