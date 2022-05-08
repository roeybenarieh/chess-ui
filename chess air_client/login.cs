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
    public partial class Login : Form
    {
        private readonly Button[] capcha_b = new Button[6];

        public Login(Form f = null)
        {
            if(f !=null)
                f.Hide();
            InitializeComponent();
        }
        private void Login_FormClosing(object sender, FormClosingEventArgs e)
        {
            Program.Disconnect_server();
        }

        private void Loginbutton_Click(object sender, EventArgs e)
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
                            Program.Connect_server();
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
                    Boolean first = this.capcha_b[0].BackColor == Color.Green;
                    Boolean second = this.capcha_b[2].BackColor == Color.Green;
                    Boolean third = this.capcha_b[4].BackColor == Color.Green;
                    Program.SendMessage("###mail+capcha###" + password.Text + "%" + first + "%" + second + "%" + third);
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
                    if (textFromServer.StartsWith("captcha%"))
                    {
                        string[] message = textFromServer.Split('%');
                        this.BeginInvoke((MethodInvoker)delegate ()
                        {
                            //string[] capchas = textFromServer.Split('%');
                            int i = Int32.Parse(message[1]); //0-2 marking of the captcha

                            Label capcha = new Label();
                            capcha.Text = message[2];
                            capcha.Size = new System.Drawing.Size(550, 70);
                            capcha.Location = new Point(62, i * 80 + 20);
                            capcha.Font = new Font("Impact", 11);
                            Controls.Add(capcha);
                        });
                    }
                    else //incripted message
                    {
                        textFromServer = Program.rsa.Decrypt(textFromServer);
                        // what happen after the cliant register - what the server returns and what happen as a result
                        if (textFromServer.Equals("login done"))
                        {
                            this.BeginInvoke((MethodInvoker)delegate ()
                            {
                                label2.Visible = false;
                                pictureBox1.Visible = false;
                                pictureBox2.Visible = false;
                                registration.Visible = false;
                                label1.Visible = false;
                                username.Visible = false;
                                password.Text = "";
                                loginbutton.Text = "verify";
                                loginbutton.Location = new Point(570, 323);
                                label3.Text = "recived code:";
                                //
                                //string[] capchas = textFromServer.Split('%');
                                for (int i = 0; i < 3; i++)
                                {
                                    Button capchabp = new Button();
                                    this.capcha_b[i * 2] = capchabp;
                                    capchabp.Name = Convert.ToString(i * 2);
                                    capchabp.Location = new Point(620, 30 + i * 80);
                                    capchabp.Size = new Size(60, 60);
                                    capchabp.Text = "positive";
                                    capchabp.BackColor = Color.Transparent;
                                    capchabp.Click += new System.EventHandler(this.Capcha_Click);
                                    Controls.Add(capchabp);

                                    Button capchabn = new Button();
                                    this.capcha_b[(i * 2) + 1] = capchabn;
                                    capchabn.Name = Convert.ToString((i * 2) + 1);
                                    capchabn.Location = new Point(690, 30 + i * 80);
                                    capchabn.Size = new Size(60, 60);
                                    capchabn.Text = "negative";
                                    capchabn.BackColor = Color.Transparent;
                                    capchabn.Click += new System.EventHandler(this.Capcha_Click);
                                    Controls.Add(capchabn);
                                }
                            });
                        }
                        else if (textFromServer == "code done")
                        {
                            Change_outputtext_txt("login completed, starting game...");
                            Chess chessair = new Chess(this);
                            chessair.ShowDialog();
                        }
                        else if (textFromServer == "code done - change password")
                        {
                            Change_outputtext_txt("login completed, it's time to change password...");
                            System.Threading.Thread.Sleep(1500);
                            change_password change_password = new change_password(this, "one mounth past, its time to change your password..");
                            change_password.ShowDialog();
                        }
                        else if (textFromServer == "code or captcha incorect")
                        {
                            Change_outputtext_txt("the verefication code is incorrect ");
                        }
                        else if (textFromServer == "coudnt send mail")
                        {
                            Change_outputtext_txt("coudnt send mail to your mail adress");
                        }
                        else
                        {
                            Change_outputtext_txt("username or password are incorrect");
                        }
                    }
                    Program.client.GetStream().BeginRead(Program.data,
                                                 0,
                                                 System.Convert.ToInt32(Program.client.ReceiveBufferSize),
                                                 ReceiveMessage,
                                                 null);
                }
            }
            catch (Exception e)
            {
                // ignor the error... fired when the user loggs off
                return;
            }
        }

        private void Registration_Click(object sender, EventArgs e)
        {
            register registrationForm = new register(this);
            registrationForm.ShowDialog();
        }
        public void Change_outputtext_txt(string s)
        {
            outputtext.BeginInvoke((MethodInvoker)delegate () {
                outputtext.Text = s;
            });
        }
        private void Capcha_Click(object sender, EventArgs e)
        {
            try
            {
                Button button = sender as Button;
                if (button.BackColor == Color.Transparent)
                {
                    if (Int32.Parse(button.Name) % 2 == 0) //positive button
                    {
                        button.BackColor = Color.Green;
                        capcha_b[Int32.Parse(button.Name) + 1].BackColor = Color.Transparent;
                    }
                    else//negative button
                    {
                        button.BackColor = Color.Red;
                        capcha_b[Int32.Parse(button.Name) - 1].BackColor = Color.Transparent;
                    }
                }
                else
                    return;
            }
            catch (Exception)
            {
                // ignor the error... fired when the user loggs off
            }
        }

        private void Login_Load(object sender, EventArgs e)
        {

        }
    }
}
