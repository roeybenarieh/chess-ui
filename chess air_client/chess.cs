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

        private Button[,] button = new Button[7,6];
        private Boolean match_turn =false;
        private int mycolor;

        public chess(Form f)
        {
            try
            {
                f.Hide();
            }
            catch (Exception)
            {
                f.BeginInvoke((MethodInvoker)delegate () { f.Hide(); });
            }

            InitializeComponent();
            this.Shown += Load_board;
        }

        private void Load_board(object sender, EventArgs e) 
        { //create a blank board and show it to the player
            System.Drawing.SolidBrush myBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Red);
            System.Drawing.Graphics formGraphics = this.CreateGraphics();

            const int rectangesize = 60;
            const int margin = 10;
            for (int i = 0; i < 7; i++)// טור
            {
                for (int j = 0; j < 6; j++)// שורה
                {
                    button[i,j] = new Button();
                    button[i, j].Name = i + "," + j;
                    //button[i, j].Text = i + "," + j;
                    int x_location = 70 * (i + 1) - 50;
                    int y_location = 70 * (j + 1) - 50;
                    button[i, j].Location = new Point(x_location, y_location);
                    button[i, j].Size = new Size(rectangesize, rectangesize);
                    button[i, j].FlatStyle = FlatStyle.Flat;
                    button[i, j].FlatAppearance.BorderSize = 0;
                    if(i==2)
                        button[i, j].BackColor = Color.Red;
                    else
                        button[i, j].BackColor = Color.Transparent;
                    button[i, j].BackgroundImage = imageList1.Images[2];
                    button[i, j].BackgroundImageLayout = ImageLayout.Center;
                    ///
                    button[i, j].MouseHover += new System.EventHandler(this.Mouse_Hover);
                    button[i, j].MouseLeave += new System.EventHandler(this.Mouse_Leave);
                    button[i, j].Click += new System.EventHandler(this.button_Click);
                    Controls.Add(button[i, j]);
                    formGraphics.FillRectangle(myBrush, new Rectangle(x_location- margin, y_location- margin, rectangesize + margin, rectangesize+ margin));
                }
            }
            myBrush.Dispose();
            formGraphics.Dispose();
        }

        private void Mouse_Hover(object sender, EventArgs e)
        { //handle what happen if mouse hover over a button
            Button sp_button = sender as Button;
            String[] position = sp_button.Name.Split(',');
            for (int j = 0; j < 6; j++)// שורה
            {
                if (same_images(button[Int32.Parse(position[0]), j].BackgroundImage, imageList1.Images[2]))
                {
                    button[Int32.Parse(position[0]), j].BackgroundImage = my_tiny_color_image();
                }
                else
                    j = 7;
            }
        }
        
        private void Mouse_Leave(object sender, EventArgs e)
        {//handle what happen if mouse stop hover over a button
            Button sp_button = sender as Button;
            String[] position = sp_button.Name.Split(',');
            for (int j = 0; j < 6; j++)// שורה
            {
                if (same_images(button[Int32.Parse(position[0]), j].BackgroundImage , my_tiny_color_image()))
                {
                    button[Int32.Parse(position[0]), j].BackgroundImage = imageList1.Images[2];
                }
            }
        }

        private void initialize_board()
        { // make the board blank
            for (int i = 0; i < 7; i++)// טור
            {
                for (int j = 0; j < 6; j++)// שורה
                {
                        button[i, j].BackgroundImage = imageList1.Images[2];
                }
            }
        }

        private void Start_game_vizualy(string mynick, string oppnick)// chnage the form componnent for the game
        {
            initialize_board();
            outcome_tx.BeginInvoke((MethodInvoker)delegate () { outcome_tx.Text = "";                                     });
            play.BeginInvoke((MethodInvoker)delegate ()       { play.Visible = false;                                     });
            me.BeginInvoke((MethodInvoker)delegate ()         { me.Text = mynick; this.me.Visible = true;                 });
            oponent.BeginInvoke((MethodInvoker)delegate ()    { oponent.Text = oppnick; this.oponent.Visible = true; });
        }
        
        private void End_game_vizualy(String outcome)// chnage the form componnent for the end of the game
        {

            match_turn = false;
            this.me.BeginInvoke((MethodInvoker)delegate ()      { this.me.Visible = false;      });
            this.oponent.BeginInvoke((MethodInvoker)delegate () { this.oponent.Visible = false; });
            this.play.BeginInvoke((MethodInvoker)delegate ()    { this.play.Font = new Font(play.Font.FontFamily, 30); this.play.Text = "play";    this.play.Visible = true; });
            if (outcome.StartsWith("###draw###"))
            {
                outcome = "Draw";
            }
            else // outcome Starts With "###win###"
                outcome = outcome.Remove(0,9);
            this.outcome_tx.BeginInvoke((MethodInvoker)delegate () { this.outcome_tx.Text = outcome; });
        }

        private void button_Click(object sender, EventArgs e)
        {
            if (match_turn)
            {
                Button button = sender as Button;
                Boolean can_play = false; // if it is posible to place your peace in that location
                String[] position = button.Name.Split(',');
                
                if (position[1] == "5" && same_images(button.BackgroundImage, my_tiny_color_image())) // at the buttom of the board
                { can_play = true; }
                
                else if ((position[1] != "5") && same_images(button.BackgroundImage, my_tiny_color_image()) && (! same_images(this.button[Convert.ToInt32(position[0]), Convert.ToInt32(position[1]) + 1].BackgroundImage, my_tiny_color_image())))
                { can_play = true; }// not at the buttom of the board

                if (can_play)
                {
                    match_turn = false;
                    button.BackgroundImage = color_image("me");
                    Program.SendMessage("###move###" + button.Name);
                    Program.client.GetStream().BeginRead(Program.data,
                                                     0,
                                                     System.Convert.ToInt32(Program.client.ReceiveBufferSize),
                                                     ReceiveMessage,
                                                     null);
                }
            }
        }
        
        private void play_Click(object sender, EventArgs e)
        {
            if (play.Text == "play")
            {
                play = sender as Button;
                play.Font = new Font(play.Font.FontFamily, 20);
                play.Text = "waiting for opponent...";
                //
                Program.connect_server();
                Program.SendMessage("###ready_to_play###");
                Program.client.GetStream().BeginRead(Program.data,
                                                 0,
                                                 System.Convert.ToInt32(Program.client.ReceiveBufferSize),
                                                 ReceiveMessage,
                                                 null);
            }
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
                    {//my nickname & opponent nickname & my color & if it's my turn
                        textFromServer = textFromServer.Remove(0, 16);
                        String[] gamedata = textFromServer.Split('&');
                        if (gamedata[3] == "yes") { match_turn = true; }
                        if (gamedata[2] == "green")
                        {
                            mycolor = 0;
                        }
                        if (gamedata[2] == "red")
                        {
                            mycolor = 1;
                        }
                        Start_game_vizualy(gamedata[0], gamedata[1]);
                    }
                    else if (textFromServer.StartsWith("###move###"))
                    {// after ###move## there is X,Y cordinations
                        textFromServer = textFromServer.Remove(0, 10);
                        String[] movedata = textFromServer.Split(',');
                        button[Convert.ToInt32(movedata[0]), Convert.ToInt32(movedata[1])].BeginInvoke((MethodInvoker)delegate ()
                        {
                            button[Convert.ToInt32(movedata[0]), Convert.ToInt32(movedata[1])].BackgroundImage = color_image("oponent");
                        });
                        match_turn = true;
                    }
                    else if (textFromServer.StartsWith("###win###") || textFromServer.StartsWith("###draw###"))// the game ended
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
            catch (Exception ex)
            {
                // ignor the error... fired when the user loggs off
            }
        }
        
        private Image color_image(String player)
        {
            if (player == "me")
            {
                return imageList1.Images[mycolor];
            }
            else
            {
                if  (mycolor == 0) { return imageList1.Images[1]; }
                else               { return imageList1.Images[0]; }
            }
        }
        private Image my_tiny_color_image()
        {
            if (mycolor == 0) { return imageList1.Images[3]; }
            else              { return imageList1.Images[4]; }
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
    }
}
