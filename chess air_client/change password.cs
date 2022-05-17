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
        /// <summary>
        /// constructor of the change password form
        /// </summary>
        /// <param name="f"></param>
        /// <param name="reason_for_changing_password"></param>
        public change_password(Form f, string reason_for_changing_password)
        {
            Program.receive_message_handler = this.ReceiveMessage;
            Program.Close_form(f);
            InitializeComponent();
            sidenote_txt.Text = reason_for_changing_password;
            Program.Keep_reading();
        }

        /// <summary>
        /// when the user clicks the change password button, send the new password information to the server
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Changepasswordbutton_Click(object sender, EventArgs e)
        {
            if (username_txt.Text == "")
                Change_sidenote_txt("username must be inserted");
            else if (current_password_txt.Text == "")
                Change_sidenote_txt("current passworde must be inserted");
            else if (new_password_txt.Text == "")
                Change_sidenote_txt("new password must be inserted");
            else if (new_password2_txt.Text == "")
                Change_sidenote_txt("you must repead the new password");
            else
            {
                Program.SendMessage("###change_password###" + username_txt.Text + "$" + current_password_txt.Text + "$" + new_password_txt.Text + "$" + new_password2_txt.Text);
            }
        }
        
        /// <summary>
        /// handle string messages from the server
        /// </summary>
        /// <param name="ar"></param>
        private void ReceiveMessage(string textFromServer)
        {
            if (textFromServer == "bad password")
            {
                Change_sidenote_txt("bad password: too similar to the current password");
            }
            else if (textFromServer == "there is a problemo")
            {
                Change_sidenote_txt("there is a problem, the server wasnt able to change your password. please try again in 5 minutes");
            }
            else if (textFromServer == "repeated password isn't correct" || textFromServer == "username or current password are incorect!")
            {
                Change_sidenote_txt(textFromServer);
            }//
            else if (textFromServer == "password changed")
            {
                Change_sidenote_txt("password changed, opening the game");
                System.Threading.Thread.Sleep(1500);
                Chess chessair = new Chess(this);
                chessair.ShowDialog();
            }
        }
        
        /// <summary>
        /// invoke the alart lable and put the string s in it
        /// </summary>
        /// <param name="s"></param>
        public void Change_sidenote_txt(string s)
        {
            sidenote_txt.BeginInvoke((MethodInvoker)delegate () {
                sidenote_txt.Text = s;
            });
        }
    }
}
