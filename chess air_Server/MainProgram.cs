using chess;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace chess_air_Server
{
    internal class MainProgram
    {
        private const int portNo = 500;
        private const string ipAddress = "127.0.0.1";//127.0.0.1 for debugging purposes


        public static void Main(string[] args)
        {
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
        /// <summary>
        /// add new even message in the log file
        /// </summary>
        /// <param name="logMessage"></param>
        /// <param name="ipAddress"></param>
        public static void Log_new_event(string logMessage, string ipAddress)
        { //f is thenth of a second
            using (StreamWriter w = File.AppendText("log.txt"))
            {
                w.WriteLine(logMessage + "%" + ipAddress + "%" + DateTime.Now.ToString("dd/MM/yyyy :: HH:mm:ss.f"));
            }
        }

        /// <summary>
        /// checking wheter the user is trying to spuf the server
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
        public static Boolean Not_spufing(string ipAddress)
        {
            using (StreamReader r = File.OpenText("log.txt"))
            {
                string current_time = DateTime.Now.ToString("dd/MM/yyyy :: HH:mm:ss.f");
                string line;
                while ((line = r.ReadLine()) != null)
                {
                    string[] info = line.Split('%');
                    if (info[1].Equals(ipAddress) && info[2].Equals(current_time))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// gets a random captach text
        /// </summary>
        /// <returns></returns>
        public static string[] Get_random_captcha()
        {
            //str[0] = the words
            //str[1] = the Sentiment Analysis answer
            string[] str = new string[2];
            Random random = new Random();
            string sourceDirectory;
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
                int filenum = random.Next(150);//12500 txt files exist, only 300 are used in this project
                int count = 0;
                foreach (string currentFile in txtFiles)
                {
                    if (count == filenum)
                    {
                        //string fileName = currentFile.Substring(sourceDirectory.Length + 1);

                        using (StreamReader r = File.OpenText(currentFile))
                        {
                            string line;
                            while ((line = r.ReadLine()) != null)
                            {
                                line = line.Replace("<br /><br />", "     ");
                                string[] info = line.Split('.');
                                if (line.Length < 50)// the overall text is less than 50 characters
                                    return Get_random_captcha();
                                else
                                {
                                    int charcount = 0;
                                    for (int i = 0; i < info.Length; i++)
                                    {
                                        charcount += info[i].Length;
                                        str[0] += info[i] + ".";
                                        if (charcount > 230) //when the captcha should stop getting more letters
                                        {
                                            if (str[0] == null)
                                            {
                                                Console.WriteLine("something went wrong - capcha didnt perform well");
                                                return Get_random_captcha();
                                            }
                                            return str;
                                        }
                                    }
                                    return Get_random_captcha();
                                }
                            }
                        }
                    }
                    else
                        count++;
                }
            }
            catch (Exception)
            {
                return Get_random_captcha();
            }
            return Get_random_captcha();
        }

    }
}
