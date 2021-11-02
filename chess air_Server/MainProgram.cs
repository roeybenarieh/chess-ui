using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace Server
{
    class MainProgram
    {
        private const int portNo = 500;
        private const string ipAddress = "127.0.0.1";


        public static void Main(string[] args)
        {
            string[] str = get_random_captcha();
            System.Net.IPAddress localAdd = System.Net.IPAddress.Parse(ipAddress);

            TcpListener listener = new TcpListener(localAdd, portNo);

            Console.WriteLine("Simple TCP Server");
            Console.WriteLine("Listening to ip {0} port: {1}", ipAddress, portNo);
            Console.WriteLine("Server is ready.");

            // Start listen to incoming connection requests
            listener.Start();
            // infinit loop.
            while (true)
            {
                // AcceptTcpClient - Blocking call
                // Execute will not continue until a connection is established

                // create an instance of ChatClient so the server will be able to 
                // serve multiple client at the same time.
                //AcceptTcpClient open new socket for the new client
                ManageClient user = new ManageClient(listener.AcceptTcpClient());
            }
        }

        public static void Log_new_event(string logMessage, string ipAddress)
        { //f is thenth of a second
            using (StreamWriter w = File.AppendText("log.txt"))
            {
                w.WriteLine( logMessage + "%" + ipAddress + "%" + DateTime.Now.ToString("dd/MM/yyyy :: HH:mm:ss.f"));
            }
        }

        public static Boolean not_spufing(string ipAddress)
        {
            using (StreamReader r = File.OpenText("log.txt"))
            {
                string current_time = DateTime.Now.ToString("dd/MM/yyyy :: HH:mm:ss.f");
                string line;
                while ((line = r.ReadLine()) != null)
                {
                    string[] info = line.Split('%');
                    if(info[1].Equals(ipAddress) && info[2].Equals(current_time))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        
        public static string[] get_random_captcha()
        {
            //str[0] = the words
            //str[1] = the Sentiment Analysis answer
            string[] str = new string[2];
            var random = new Random();
            string sourceDirectory = "";
            if (random.Next(2) == 0)
            {//half a chace to be positive and half a chance to be negative
                sourceDirectory = System.IO.Directory.GetCurrentDirectory() + "\\captcha\\pos";
                str[1] = "pos";
            }
            else
            {
                sourceDirectory = System.IO.Directory.GetCurrentDirectory() + "\\captcha\\neg";
                str[1] = "neg";
            }

            try
            {
                var txtFiles = Directory.EnumerateFiles(sourceDirectory, "*.txt");
                int filenum = random.Next(12500);
                int count = 0;
                foreach (string currentFile in txtFiles)
                {
                    if (count == filenum)
                    {
                        string fileName = currentFile.Substring(sourceDirectory.Length + 1);

                        using (StreamReader r = File.OpenText(currentFile))
                        {
                            string line;
                            while ((line = r.ReadLine()) != null)
                            {
                                string[] info = line.Split('.');
                                str[0] = info[0] + "." + info[1];
                                return str;
                            }
                        }
                    }
                    else
                        count ++;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return null;
        }


    }
}
