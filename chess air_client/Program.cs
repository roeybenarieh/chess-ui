using connect4_client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
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
            if(incripted)
                message = rsa.Encrypt(message);
            try
            {
                // send message to the server
                NetworkStream ns = client.GetStream();
                byte[] data = System.Text.Encoding.ASCII.GetBytes(message);

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

    }
}
