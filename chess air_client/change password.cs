using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace chessair_client
{
	public partial class change_password : Form
	{
        public change_password(Form l, string reason_for_changing_password)
        {
            
            l.BeginInvoke((MethodInvoker)delegate () { l.Hide(); });
            InitializeComponent();
            Program.connect_server();
            sidenote_txt.Text = reason_for_changing_password;
        }

        private void changepasswordbutton_Click(object sender, EventArgs e)
        {
            if (username_txt.Text == "")
                change_sidenote_txt("username must be inserted");
            else if (current_password_txt.Text == "")
                change_sidenote_txt("current passworde must be inserted");
            else if (new_password_txt.Text == "")
                change_sidenote_txt("new password must be inserted");
            else if (new_password2_txt.Text == "")
                change_sidenote_txt("you must repead the new password");
            else
            {
                Program.SendMessage("###change_password###" + username_txt.Text + "$" + current_password_txt.Text + "$" + new_password_txt.Text + "$" + new_password2_txt.Text);
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

                    if (textFromServer == "bad password")
                    {
                        change_sidenote_txt("bad password: too similar to the current password");
                    }
                    else if (textFromServer == "there is a problemo")
                    {
                        change_sidenote_txt("there is a problem, the server wasnt able to change your password. please try again in 5 minutes");
                    }
                    else if (textFromServer == "repeated password isn't correct" || textFromServer == "username or current password are incorect!")
                    {
                        change_sidenote_txt(textFromServer);
                    }//
                    else if (textFromServer == "password changed")
                    {
                        change_sidenote_txt("password changed, opening the game");
                        System.Threading.Thread.Sleep(1500);
                        chess chessair = new chess(this);
                        chessair.ShowDialog();
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
        public void change_sidenote_txt(string s)
        {
            sidenote_txt.BeginInvoke((MethodInvoker)delegate () {
                sidenote_txt.Text = s;
            });
        }
    }
}
