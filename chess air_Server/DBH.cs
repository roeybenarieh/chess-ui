using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace chess_air_Server
{
    class DBH
    {
        private static String connectionString = get_local_db_Filename("DB.mdf");
        private static SqlConnection sqlConnection = new SqlConnection(connectionString);
        private static SqlCommand cmd = new SqlCommand();
        private static SHA512 shaM = new SHA512Managed();

        //username and password are hashed in the database

        //select statements in DB
        public static Boolean login(String[] username_password) //if their is a account with that username and password
        {
            String stm = "select count(username) from users where username='" + gethash(username_password[0]) + "' and password='" + gethash(username_password[1]) + "'";
            cmd.Connection = sqlConnection;
            cmd.CommandText = stm;
            sqlConnection.Open();
            int count = (int)cmd.ExecuteScalar();
            sqlConnection.Close();
            if (count == 1) { return true; }
            else { return false; }
        }
        
        public static int get_id(String[] username_password) //get the id for future comunication with the DB- PK!
        {
            String stm = "select Id from users where username='" + gethash(username_password[0]) + "' and password='" + gethash(username_password[1]) + "'";
            cmd.Connection = sqlConnection;
            cmd.CommandText = stm;
            sqlConnection.Open();
            int id = (int)cmd.ExecuteScalar();
            sqlConnection.Close();
            return id;
        }

        public static Boolean IsUsernameExist(String username) //if their is a username with that name
        {
            String stm = "select count(username) from users where username='" + gethash(username) + "'";
            cmd.Connection = sqlConnection;
            cmd.CommandText = stm;
            sqlConnection.Open();
            int count = (int)cmd.ExecuteScalar();//Executenonquery();
            sqlConnection.Close();
            if (count >= 1) { return true; }
            else { return false; }
        }

        public static String get_nickname(int id)
        {
            String stm = "select nickname from users where Id='" + id + "'";
            cmd.Connection = sqlConnection;
            cmd.CommandText = stm;
            sqlConnection.Open();
            String nickname = (string)cmd.ExecuteScalar();
            sqlConnection.Close();
            return nickname;
        }

        public static String get_mail(int id)
        {
            String stm = "select mail from users where Id='" + id + "'";
            cmd.Connection = sqlConnection;
            cmd.CommandText = stm;
            sqlConnection.Open();
            String mail = (string)cmd.ExecuteScalar();
            sqlConnection.Close();
            return mail;
        }

        public static string get_last_password_change(int id)
        {
            String stm = "select change_password from users where Id='" + id + "'";
            cmd.Connection = sqlConnection;
            cmd.CommandText = stm;
            sqlConnection.Open();
            DateTime datetime = (DateTime)cmd.ExecuteScalar();
            sqlConnection.Close();
            return datetime.ToShortDateString();
        }

        public static void load_games(string player_id)
        {
            //string stm = String.Format("insert into Games (wplayerid,bplayerid,moves) values ({0},{1},{2})", white_player_id, black_player_id, game);
            cmd.Connection = sqlConnection;
            //cmd.CommandText = stm;
            sqlConnection.Open();
            cmd.ExecuteNonQuery();
            sqlConnection.Close();
        }

        //update DB
        public static Boolean change_password(String username, String new_password)
        {
            String stm = "UPDATE users SET password = '" + gethash(new_password) + "', change_password = getdate() WHERE username = '" + gethash(username) + "'; ";
            cmd.Connection = sqlConnection;
            cmd.CommandText = stm;
            sqlConnection.Open();
            int rows = cmd.ExecuteNonQuery();
            sqlConnection.Close();
            if (rows == 1)
                return true;
            return false;
        }

        // backstage opperations
        private static string get_local_db_Filename(string database_name)
        {
            if (!database_name.EndsWith(".mdf"))
                database_name += ".mdf";
            //
            string directory ="";
            // System.IO.Directory.GetCurrentDirectory() == C:\Users\roey2\OneDrive\Desktop\chessair\chess air_Server\bin\Debug
            string[] positions = System.IO.Directory.GetCurrentDirectory().Split('\\');
            foreach(string position in positions)
            {
                if(position.Equals("bin"))
                {
                    directory += database_name;
                    /*if (!String.Format(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename={0};Integrated Security=True", directory).Equals(
                        @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C: \Users\roey2\OneDrive\Desktop\chessair\chess air_Server\DB.mdf;Integrated Security=True"))
                        Console.Write(String.Format(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename={0};Integrated Security=True", directory));*/
                    return String.Format(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename={0};Integrated Security=True", directory);
                    //return @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C: \Users\roey2\OneDrive\Desktop\chessair\chess air_Server\DB.mdf;Integrated Security=True";
                }
                directory += position + "\\";
            }
            return "something went wrong";
        }

        private static byte[] GetHashbytes(string inputString)
        {
            HashAlgorithm algorithm = SHA512.Create(); ;
            return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
        }

        public static string gethash(string inputString)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in GetHashbytes(inputString))
                sb.Append(b.ToString("X2"));

            return sb.ToString();
        }

        // insert statments in DB
        public static Boolean InsertNewUser(String client_data) // inserting client data if he doesnt exist already // return if did the action
        {
            String[] usepass = client_data.Split('&');
            if (!IsUsernameExist(usepass[0]) && usepass.Length >= 2)
            {
                usepass[0] = gethash(usepass[0]);
                usepass[1] = gethash(usepass[1]);
                for (int i = 0; i < usepass.Length; i++)
                {
                    if (usepass[i] == "" || usepass[i] == null)
                    {
                        usepass[i] = "null";
                        if (i == 2)
                            usepass[i] = "'anonymous'";
                    }
                    else
                        usepass[i] = "'" + usepass[i] + "'";
                }
                string stm = "insert into users (username,password,nickname,mail,age,country,city) values (" + usepass[0] + "," + usepass[1] + "," + usepass[2] + "," + usepass[3] + "," + usepass[4] + "," + usepass[5] + "," + usepass[6] + ")";

                cmd.Connection = sqlConnection;
                cmd.CommandText = stm;
                sqlConnection.Open();
                cmd.ExecuteNonQuery();
                sqlConnection.Close();
                return true;
            }
            return false;
        }

        public static void insert_game(string white_player_id, string black_player_id, string game)
        {
            string stm = String.Format("insert into Games (wplayerid,bplayerid,moves) values ({0},{1},'{2}')", white_player_id, black_player_id, game);
            cmd.Connection = sqlConnection;
            cmd.CommandText = stm;
            sqlConnection.Open();
            cmd.ExecuteNonQuery();
            sqlConnection.Close();
        }
    }
}
