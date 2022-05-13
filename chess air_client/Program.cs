using connect4_client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace chessair_client
{
    public static class Program
    {
        private static readonly int portNo = 500;
        private static readonly string ipAddress = "127.0.0.1";
        public static TcpClient client;
        public static byte[] data;
        public static RSA rsa;
        public static bool[] forms = { false, false, false, false };//login, register, change password, game
        public static int login = 0, register = 1, changePassword = 2, game = 3;
        public static Action<string> receive_message_handler;
        
        [STAThread]
        // main function
        static void Main()
        {
          Application.EnableVisualStyles();
          Application.SetCompatibleTextRenderingDefault(false);
          ///

          //Application.Run(new chessair());
          Connect_server();
          
          //initialize RSA
          rsa = new RSA();
          forms[login] = true;
          client.GetStream().BeginRead(Program.data,
                                                       0,
                                                       System.Convert.ToInt32(Program.client.ReceiveBufferSize),
                                                       ReceiveMessage,
                                                       null);
            
          Application.Run(new Login());
          //new login().ShowDialog();
          System.Windows.Forms.Application.Exit();
          
          
        }
        /// <summary>
        /// send a message to server, the message is encrypted if needed
        /// </summary>
        /// <param name="message"></param>
        /// <param name="incripted"></param>
        public static void SendMessage(string message,bool incripted=true)
        {
            if (incripted)
            {
                //message = rsa.Encrypt(message);
            }
            //Thread.Sleep(300);
            try
            {
                // send message to the server
                NetworkStream ns = client.GetStream();
                byte[] data = System.Text.Encoding.ASCII.GetBytes(message);
                //Thread.Sleep(100);

                // send the text
                ns.Write(data, 0, data.Length);
                ns.Flush();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        /// <summary>
        /// create TCP connection with the server
        /// </summary>
        public static void Connect_server()
        {
            if (client == null)
            {
                client = new TcpClient();// יוצר חיבור חדש עם השרת
                client.Connect(ipAddress, portNo);
                data = new byte[client.ReceiveBufferSize];
            }
        }
        
        /// <summary>
        /// stops a runing TCP connection
        /// </summary>
        public static void Disconnect_server()
        {
            SendMessage("###quit###");
            if (client != null)
            {
                try
                {
                    // disconnect form the server
                    client.GetStream().Close();
                    client.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
        }

        /// <summary>
        /// asynronic function that gets messages from the server
        /// </summary>
        /// <param name="ar"></param>
        private static void ReceiveMessage(IAsyncResult ar)
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
                        receive_message_handler(textFromServer);
                    }
                    else if (textFromServer.StartsWith("public key:"))
                    {
                        Program.rsa.setPublicKey(textFromServer.Remove(0, 11));
                    }
                    else//incrypted message
                    {
                        textFromServer = rsa.Decrypt(textFromServer);
                        receive_message_handler(textFromServer);
                    }

                }
                Keep_reading();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        public static void Keep_reading()
        {
            client.GetStream().BeginRead(Program.data,
                                                       0,
                                                       System.Convert.ToInt32(Program.client.ReceiveBufferSize),
                                                       ReceiveMessage,
                                                       null);
        }

        public static void Close_form(Form f)
        {
            if(f != null)
            {
                try
                {
                    f.Hide();
                }
                catch (Exception)
                {
                    f.Invoke((MethodInvoker)delegate () { f.Hide(); });
                }
            }
        }
    }
}
