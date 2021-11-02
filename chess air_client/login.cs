using chessair_client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace connect4_client
{
    public partial class login : Form
    {
        public login(Form f = null)
        {
            if(f !=null)
                f.Hide();
            InitializeComponent();
        }
        private void login_FormClosing(object sender, FormClosingEventArgs e)
        {
            Program.disconnect_server();
        }

        private void loginbutton_Click(object sender, EventArgs e)
        {
            try
            {
                if (loginbutton.Text == "login")
                {
                    outputtext.Text = "";
                    if (password.Text.Length > 0 && username.Text.Length > 0) // אם הכל הוכנס
                    {
                        if (password.Text.StartsWith("'") || password.Text.StartsWith("/") || username.Text.StartsWith("'") || username.Text.StartsWith("/"))
                        {
                            outputtext.Text = "you cant put ' or / at the begging of a field";
                        }
                        else if (password.Text.EndsWith("'") || password.Text.EndsWith("/") || username.Text.EndsWith("'") || username.Text.EndsWith("/"))
                        {
                            outputtext.Text = "you cant put ' or / at the end of a field";
                        }
                        else if (password.Text.Contains("&") || password.Text.Contains("&"))
                        {
                            outputtext.Text = "you cant put '&' in a field";
                        }
                        else
                        {
                            outputtext.Text = "";
                            // connect to the server
                            Program.connect_server();
                            Program.SendMessage("###login###" + username.Text + "&" + password.Text);
                            Program.client.GetStream().BeginRead(Program.data,
                                                             0,
                                                             System.Convert.ToInt32(Program.client.ReceiveBufferSize),
                                                             ReceiveMessage,
                                                             null);
                        }
                    }
                    else
                    { // מציג מה לא הוכנס
                        String[] erroutput = new string[3];
                        if (username.Text.Length == 0) { erroutput[0] = "username"; } //if there is no username
                        if (password.Text.Length == 0) { erroutput[2] = "password"; } //if there is no password
                                                                                      //
                        erroutput[1] = "";
                        if (erroutput[0] != null && erroutput[2] != null) { erroutput[1] = " and "; }
                        outputtext.ResetText();
                        outputtext.Text = erroutput[0] + erroutput[1] + erroutput[2] + " must be inserted!";
                    }
                }
                else
                { //login == "verify"
                    Program.SendMessage("###mail###" + password.Text);
                    outputtext.Text = "";
                    Program.client.GetStream().BeginRead(Program.data,
                                                             0,
                                                             System.Convert.ToInt32(Program.client.ReceiveBufferSize),
                                                             ReceiveMessage,
                                                             null);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
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
                    if (textFromServer == "login done")
                    {
                        this.BeginInvoke((MethodInvoker)delegate () {
                            label2.Visible = false;
                            pictureBox1.Visible = false;
                            pictureBox2.Visible = false;
                            registration.Visible = false;
                            label1.Visible = false;
                            username.Visible = false;
                            password.Text = "";
                            loginbutton.Text = "verify";
                            label3.Text = "recived code:";
                        });
                    }
                    else if (textFromServer == "code done")
                    {
                        change_outputtext_txt("login completed, starting game...");
                        System.Threading.Thread.Sleep(1500);
                        chess chessair = new chess(this);
                        chessair.ShowDialog();
                    }
                    else if (textFromServer == "code done - change password")
                    {
                        change_outputtext_txt("login completed, it's time to change password...");
                        System.Threading.Thread.Sleep(1500);
                        change_password change_password = new change_password(this,"one mounth past, its time to change your password..");
                        change_password.ShowDialog();
                    }
                    else if (textFromServer == "code incorect")
                    {
                        change_outputtext_txt("the verefication code is incorrect ");
                    }
                    else if (textFromServer == "coudnt send mail")
                    {
                        change_outputtext_txt("coudnt send mail to your mail adress");
                    }
                    else
                    {
                        change_outputtext_txt("username or password are incorrect");
                    }
                }
            }
            catch (Exception ex)
            {
                // ignor the error... fired when the user loggs off
            }
        }

        private void registration_Click(object sender, EventArgs e)
        {
            register registrationForm = new register(this);
            registrationForm.ShowDialog();
        }
        public void change_outputtext_txt(string s)
        {
            outputtext.BeginInvoke((MethodInvoker)delegate () {
                outputtext.Text = s;
            });
        }
    }
}
