using System;
using chess;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Data.SqlClient;
using System.Net;
using System.Net.Sockets;
using System.Windows;
using System.Collections.Generic;
using chess_air_Server.types_of_games;
using System.Threading;

namespace chess_air_Server
{
    public class ManageClient
    {
        // Store list of all clients connecting to the server
        // the list is static so all memebers of the chat will be able to obtain list
        // of current connected client
        public static Hashtable AllClients = new Hashtable();

        // information about the client
        public TcpClient _client;
        public string _clientIP;
        private RSA rsa;
        public int client_id; //used in DB
        private String mailcode;
        public Boolean ready_to_play = false;
        private string[] captcha = new string[3];
        public Boolean my_turn = true;
        public types_of_games.Game game;

        // used for sending and reciving data
        public byte[] data;

        /// When the client gets connected to the server the server will create an instance of the ChatClient and pass the TcpClient
        public ManageClient(TcpClient client)
        {
            _client = client;
            // get the ip address of the client to register him with our client list
            _clientIP = client.Client.RemoteEndPoint.ToString();

            // Add the new client to our clients collection
            AllClients.Add(_clientIP, this);

            // Read data from the client async
            data = new byte[_client.ReceiveBufferSize];

            if (MainProgram.Not_spufing(_clientIP))
            {
                Console.WriteLine("new connection");
                MainProgram.Log_new_event("new connection", _clientIP);
                //initialize RSA
                this.rsa = new RSA(this);
                // BeginRead will begin async read from the NetworkStream
                // This allows the server to remain responsive and continue accepting new connections from other clients
                // When reading complete control will be transfered to the ReviveMessage() function.
                _client.GetStream().BeginRead(data,
                                                 0,
                                                 System.Convert.ToInt32(_client.ReceiveBufferSize),
                                                 ReceiveMessage,
                                                 null);
            }

        }

        /// allow the server to send message to the client.
        public bool SendMessage(string message, bool incripted = true) // send message only to the client.
        {
            try
            {
                System.Net.Sockets.NetworkStream ns;

                // use lock to present multiple threads from using the networkstream object
                // this is likely to occur when the server is connected to multiple clients all of 
                // them trying to access to the networkstram at the same time.
                lock (_client.GetStream())
                {
                    ns = _client.GetStream();
                }

                if (incripted)
                {
                    //message = "heheh";
                    message = rsa.Encrypt(message);
                }
                // Send data to the client
                byte[] bytesToSend = System.Text.Encoding.ASCII.GetBytes(message);
                ns.Write(bytesToSend, 0, bytesToSend.Length);
                ns.Flush();
                return true;
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.ToString());
                return false;
            }
        }

        /// recieve and handel incomming streem 
        /// Asynchrom
        /// </summary>
        /// <param name="ar">IAsyncResult Interface</param>
        public void ReceiveMessage(IAsyncResult ar) // recieve message only from the client
        {
            int bytesRead;
            try
            {
                lock (_client.GetStream())
                {
                    // call EndRead to handle the end of an async read.
                    bytesRead = _client.GetStream().EndRead(ar);
                }
                // if bytesread<1 -> the client disconnected
                string messageReceived = System.Text.Encoding.ASCII.GetString(data, 0, bytesRead); //the recieved message in string
                messageReceived = rsa.Decrypt(messageReceived);
                if (bytesRead < 1 || messageReceived == "###quit###")
                {
                    // remove the client from the list of clients
                    AllClients.Remove(_clientIP);
                    return;
                }
                else // client still connected
                {
                    Console.WriteLine(messageReceived);
                    if (messageReceived.StartsWith("###login###"))// window1- login
                    {
                        String userdatatmp = messageReceived.Remove(0, 11);
                        String[] userdata = userdatatmp.Split('&');
                        if (DBH.login(userdata))
                        {
                            this.client_id = DBH.get_id(userdata);
                            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
                            var stringChars = new char[10];
                            var random = new Random();
                            for (int i = 0; i < stringChars.Length; i++)
                            {
                                stringChars[i] = chars[random.Next(chars.Length)];
                            }
                            this.mailcode = new String(stringChars);
                            if (send_email("chessair - confirmation code", "your confirmation code: \n" + mailcode, DBH.get_mail(this.client_id)) || true)/////////////email doesnt count!!!!!!
                            {
                                SendMessage("login done");
                                for (int i = 0; i < 3; i++)
                                {
                                    string[] tmp = MainProgram.Get_random_captcha();
                                    captcha[i] = tmp[1];
                                    //message += "%" + tmp[0];
                                    SendMessage("captcha%" + i + "%" + tmp[0], incripted: false);
                                    Thread.Sleep(100);
                                    Console.WriteLine(i);
                                }
                            }
                            else
                                SendMessage("coudnt send mail");
                        }
                        else
                        {
                            SendMessage("login not done");
                        }
                    }
                    else if (messageReceived.StartsWith("###mail+capcha###"))// window1- confirm email
                    {
                        String message = messageReceived.Remove(0, 17);
                        string[] data = message.Split('%');
                        for (int i = 1; i < data.Length; i++)
                        {
                            if (data[i] == "True")
                                data[i] = "pos";
                            else
                                data[i] = "neg";
                        }
                        if ((mailcode.Equals(data[0]) && data[1].Equals(captcha[0]) && data[2].Equals(captcha[1]) && data[3].Equals(captcha[2])) || true)
                        {
                            string[] today_date = DateTime.Now.ToString("dd/MM/yyyy").Split('/');
                            string[] last_date = DBH.get_last_password_change(this.client_id).Split('/');
                            if (!today_date[1].Equals(last_date[1]) && false)//every start of a new month new password need to be created.
                                SendMessage("code done - change password");
                            else
                                SendMessage("code done");
                        }
                        else
                            SendMessage("code or captcha incorect");
                    }
                    //
                    else if (messageReceived.StartsWith("###regist###"))// window2- registration
                    {
                        String userdatatmp = messageReceived.Remove(0, 12);
                        String[] userdata = userdatatmp.Split('&');

                        if (!DBH.IsUsernameExist(userdata[0]))
                        {
                            if (DBH.InsertNewUser(userdatatmp))
                            {
                                string message = "your registration completed!";
                                if (send_email("welcome to chessair", message, userdata[3]))
                                {
                                    SendMessage("regist complited"); //was able to create new user and send comfirmation mail
                                }
                                else
                                    SendMessage("regist mail incorrect");
                            }
                            else
                            {
                                SendMessage("there was a problem in the server, please try again later");//coudnt create new user, problem with the database
                            }
                        }
                        else
                        {
                            SendMessage("username already exist!");
                        }
                    }
                    else if (messageReceived.StartsWith("###change_password###"))// window3- change password
                    {
                        String userdatatmp = messageReceived.Remove(0, 21);
                        String[] userdata = userdatatmp.Split('$');
                        if (DBH.login(userdata))
                        {
                            if (Computediff(userdata[1], userdata[2]) <= 3)
                                SendMessage("bad password");
                            else
                            {
                                if (!userdata[2].Equals(userdata[3]))
                                    SendMessage("repeated password isn't correct");
                                else
                                {
                                    if (DBH.change_password(userdata[0], userdata[2]))
                                        SendMessage("password changed");
                                    else
                                        SendMessage("there is a problemo");
                                }
                            }
                        }
                        else
                        {
                            SendMessage("username or current password are incorect!");
                        }
                    }
                    else // window3- game
                    {
                        if (messageReceived == "###ready_to_play_vs_friend###")
                        {
                            Boolean search_player = true;
                            if (AllClients.Count > 1)
                            {
                                foreach (DictionaryEntry client in AllClients)
                                {
                                    if (search_player)
                                    {
                                        if (((ManageClient)(client.Value))._client != this._client && ((ManageClient)(client.Value)).ready_to_play)
                                        {
                                            ((ManageClient)(client.Value)).ready_to_play = false;//
                                            this.ready_to_play = false;// both players cant play right now, they are already in a game
                                            search_player = false;
                                            this.game = new _1v1(((ManageClient)(client.Value)), this);
                                            ((ManageClient)(client.Value)).game = this.game;
                                        }
                                    }
                                }
                            }
                            if (search_player == true) // didnt found a opponent to play with
                                ready_to_play = true;
                        }
                        else if (messageReceived.StartsWith("###ready_to_play_vs_ai###"))
                        {
                            this.ready_to_play = false;
                            this.game = new _1vsAI(this);
                        }
                        else if (messageReceived.StartsWith("###ai_vs_ai###"))
                        {
                            this.ready_to_play = false;
                        }
                        else if(this.game != null)//in the middle of a game..
                        {
                            if (messageReceived.Equals("###resignation###"))
                            {
                                this.game.ResignationHandler(this);
                            }
                            else if(this.my_turn) //if it is this clients turn
                                this.game.GameMessageHandler(messageReceived);
                        }
                    }
                }
                lock (_client.GetStream())
                {
                    // continue reading form the client
                    _client.GetStream().BeginRead(data, 0, System.Convert.ToInt32(_client.ReceiveBufferSize), ReceiveMessage, null);
                }
            }
            catch (Exception)
            {
                AllClients.Remove(_clientIP);
                Console.WriteLine(this.client_id.ToString() + " has left.");
            }
        }
        public void endgame(string message)
        {
            this.SendMessage("###endgame###"+message);
            this.game = null;
        }
        public TcpClient get_tcp()
        {
            return this._client;
        }
        public string get_nick()
        {
            return DBH.get_nickname(this.client_id);
        }
        public void changeturn()
        {
            my_turn = !my_turn;
        }

        public Boolean send_email(string subject, string message, string reciver_mail) //return if mail have been sent
        {
            try
            {
                MailMessage fmessage = new MailMessage();
                SmtpClient smtp = new SmtpClient();
                fmessage.From = new MailAddress("server.cyber123@gmail.com");
                fmessage.To.Add(new MailAddress(reciver_mail));
                fmessage.Subject = subject;
                fmessage.IsBodyHtml = true; //to make message body as html
                fmessage.Body = message;
                smtp.Port = 587;
                smtp.Host = "smtp.gmail.com"; //for gmail host 
                smtp.EnableSsl = true;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential("server.cyber123@gmail.com", "server123!");
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.Send(fmessage);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static int Computediff(string s, string t)
        {
            //LevenshteinDistance - how match characters we need to change in one of the strings to get the second string
            if (string.IsNullOrEmpty(s))
            {
                if (string.IsNullOrEmpty(t))
                    return 0;
                return t.Length;
            }

            if (string.IsNullOrEmpty(t))
            {
                return s.Length;
            }

            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];

            // initialize the top and right of the table to 0, 1, 2, ...
            for (int i = 0; i <= n; d[i, 0] = i++) ;
            for (int j = 1; j <= m; d[0, j] = j++) ;

            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                    int min1 = d[i - 1, j] + 1;
                    int min2 = d[i, j - 1] + 1;
                    int min3 = d[i - 1, j - 1] + cost;
                    d[i, j] = Math.Min(Math.Min(min1, min2), min3);
                }
            }
            return d[n, m];
        }

    }
}
