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
        private static int portNo = 500;
        private static string ipAddress = "127.0.0.1";
        public static TcpClient client;
        public static byte[] data;
        [STAThread]
        static void Main()
        {
            for (int i = 0; i < 100; i++)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                ///
                //Application.Run(new chessair());
                Application.Run(new login());
                System.Windows.Forms.Application.Exit();
            }
        }

        public static void SendMessage(string message)
        {
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

        public static void connect_server()
        {
            if (client == null)
            {
                client = new TcpClient();// יוצר חיבור חדש עם השרת
                client.Connect(ipAddress, portNo);
                data = new byte[client.ReceiveBufferSize];
            }
        }
        
        public static void disconnect_server()
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
