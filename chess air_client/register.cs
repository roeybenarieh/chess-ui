using connect4_client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace chessair_client
{
    public partial class register : Form
    {
        readonly Form f = null;
        public register(Form f)
        {
            f.Hide();
            this.f = f;
            InitializeComponent();
        }
        private void Regist_Click(object sender, EventArgs e)
        {
            try
            {
                if (rpassword.Text.Length > 0 && rusername.Text.Length > 0 && remail.Text.Length > 0 && remail.Text.Contains("@")) // אם הכל הוכנס
                {
                    if (rpassword.Text.StartsWith("'")|| rpassword.Text.StartsWith("/")|| rusername.Text.StartsWith("'")|| rusername.Text.StartsWith("/"))
                    {
                        outputtext.Text = "you cant put ' or / at the begging of the information";
                    }
                    else if (rpassword.Text.EndsWith("'") || rpassword.Text.EndsWith("/") || rusername.Text.EndsWith("'") || rusername.Text.EndsWith("/"))
                    {
                        outputtext.Text = "you cant put ' or / at the end of the information";
                    }
                    else {
                        outputtext.Text = "";
                        Program.Connect_server();
                        Program.SendMessage("###regist###" + rusername.Text + "&" + rpassword.Text + "&" + rnickname.Text + "&" + remail.Text + "&" + rage.Text + "&" + rcountry.Text + "&" + rcity.Text);
                        Program.client.GetStream().BeginRead(Program.data,
                                                         0,
                                                         System.Convert.ToInt32(Program.client.ReceiveBufferSize),
                                                         ReceiveMessage,
                                                         null);
                    }
                }
                else{ // מציג מה לא הוכנס
                    outputtext.ResetText();
                    if(!(remail.Text.Length > 0) || !(remail.Text.Contains("@")))
                    {
                        outputtext.Text = "you must enter a valid mail adress";
                    }
                    else
                    {
                        String[] erroutput = new string[3];
                        if (rusername.Text.Length == 0) { erroutput[0] = "username"; } //if there is no username
                        if (rpassword.Text.Length == 0) { erroutput[1] = "password"; } //if there is no password
                                                                                       //
                        String correction1 = "";
                        if (erroutput[0] != null && erroutput[1] != null) { correction1 = " and "; }
                        outputtext.Text = erroutput[0] + correction1 + erroutput[1] + " must be inserted!";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void Label2_Click(object sender, EventArgs e){ }

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
                    String tmp = textFromServer;
                    if (textFromServer == "regist complited")
                    {
                        outputtext.BeginInvoke((MethodInvoker)delegate () {
                            outputtext.Text = "registration complete!";
                        });
                        System.Threading.Thread.Sleep(1000);
                        f.BeginInvoke((MethodInvoker)delegate () {
                            this.Hide();
                            f.ShowDialog();
                        });
                        //login login = new login(this);
                        //login.ShowDialog();
                    }
                    else if(textFromServer == "regist mail incorrect")
                    {
                        outputtext.BeginInvoke((MethodInvoker)delegate () { outputtext.Text = "server couldnt confirm youre mail"; });
                    }
                    else if (textFromServer == "username already exist!")
                    {
                        outputtext.BeginInvoke((MethodInvoker)delegate () { outputtext.Text = textFromServer; });
                    }
                    else
                        outputtext.BeginInvoke((MethodInvoker)delegate () { outputtext.Text = textFromServer; });
                }
            }
            catch (Exception)
            {
                // ignor the error... fired when the user loggs off
            }
        }

        private void Register_FormClosing(object sender, FormClosingEventArgs e)
        {
            Program.Disconnect_server();
        }

        private void Back_Click(object sender, EventArgs e)
        {
            Login login = new Login(this);
            login.ShowDialog();
        }

        private void Remail_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
