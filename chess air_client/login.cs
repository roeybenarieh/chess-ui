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
        Button[] capchab = new Button[6];

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
                    Boolean first = this.capchab[0].BackColor == Color.Green;
                    Boolean second = this.capchab[2].BackColor == Color.Green;
                    Boolean third = this.capchab[4].BackColor == Color.Green;
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
                    // what happen after the cliant register - what the server returns and what happen as a result
                    if (textFromServer.StartsWith("login done"))
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
                            loginbutton.Location = new Point(570, 323);
                            label3.Text = "recived code:";
                            //
                            string[] capchas = textFromServer.Split('%');
                            for (int i = 0; i < 3; i++)
                            {
                                Label capcha = new Label();
                                capcha.Text = capchas[i + 1];
                                capcha.Size = new System.Drawing.Size(550, 70);
                                capcha.Location = new Point(62, i * 80 + 20);
                                capcha.Font = new Font("Impact", 11);
                                Controls.Add(capcha);

                                Button capchabp = new Button();
                                this.capchab[i * 2] = capchabp;
                                capchabp.Name = Convert.ToString(i * 2);
                                capchabp.Location = new Point(620, 30 + i * 80);
                                capchabp.Size = new Size(60, 60);
                                capchabp.Text = "positive";
                                capchabp.BackColor = Color.Transparent;
                                capchabp.Click += new System.EventHandler(this.capcha_Click);
                                Controls.Add(capchabp);

                                Button capchabn = new Button();
                                this.capchab[(i * 2) + 1] = capchabn;
                                capchabn.Name = Convert.ToString((i * 2) + 1);
                                capchabn.Location = new Point(690, 30 + i * 80);
                                capchabn.Size = new Size(60, 60);
                                capchabn.Text = "negative";
                                capchabn.BackColor = Color.Transparent;
                                capchabn.Click += new System.EventHandler(this.capcha_Click);
                                Controls.Add(capchabn);
                            }
                        });
                    }
                    else if (textFromServer == "code done")
                    {
                        change_outputtext_txt("login completed, starting game...");
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
                    else if (textFromServer == "code or captcha incorect")
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
            catch (Exception)
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
        private void capcha_Click(object sender, EventArgs e)
        {
            try
            {
                Button button = sender as Button;
                if (button.BackColor == Color.Transparent)
                {
                    if (Int32.Parse(button.Name) % 2 == 0) //positive button
                    {
                        button.BackColor = Color.Green;
                        capchab[Int32.Parse(button.Name) + 1].BackColor = Color.Transparent;
                    }
                    else//negative button
                    {
                        button.BackColor = Color.Red;
                        capchab[Int32.Parse(button.Name) - 1].BackColor = Color.Transparent;
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

        private void login_Load(object sender, EventArgs e)
        {

        }
    }
}
