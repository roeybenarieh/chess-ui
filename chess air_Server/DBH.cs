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

        /// <summary>
        /// check if the username and password are correct
        /// </summary>
        /// <param name="username_password"></param>
        /// <returns></returns>
        public static Boolean login(String[] username_password) //if their is a account with that username and password
        {
            for (int i = 0; i < username_password.Length; i++)
                if (checkForSQLInjection(username_password[i]))
                    return false;
            String stm = "select count(username) from users where username='" + gethash(username_password[0]) + "' and password='" + gethash(username_password[1]) + "'";
            cmd.Connection = sqlConnection;
            cmd.CommandText = stm;
            sqlConnection.Open();
            int count = (int)cmd.ExecuteScalar();
            sqlConnection.Close();
            if (count == 1) { return true; }
            else { return false; }
        }
        /// <summary>
        /// check if the username is already in the database and if so return it's ID
        /// </summary>
        /// <param name="username_password"></param>
        /// <returns></returns>
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

        /// <summary>
        /// return a string array of all of the latest 15 games from a specific game id
        /// </summary>
        /// <param name="client_id"></param>
        /// <param name="first_game_id"></param>
        internal static string[] getgamesinfo(int client_id, int gameswatched)
        {
            //retrieve the SQL Server instance version
            string stm = @"select * from Games where wplayerid = " + client_id + " or bplayerid = " + client_id + " ORDER BY game_id DESC";
            
            cmd.CommandText = stm;

            //open connection
            sqlConnection.Open();

            //execute the SQLCommand
            SqlDataReader dr = cmd.ExecuteReader();

            string[][] tmp = new string[15][];//15 games, for each one 3 peaces of information
            for (int i = 0; i < tmp.Length; i++)
            {
                tmp[i] = new string[4];

            }
            //check if there are records
            if (dr.HasRows)
            {
                int count = 0;
                while (count != gameswatched && dr.Read())//getting red of the unwanted games
                {
                    count++;
                }
                count = 0;
                while (count != 15 && dr.Read())//reading the wanted games
                {
                    int gameid = dr.GetInt32(0);
                    var gamedate = dr.GetDateTime(1);
                    int wplayerid = dr.GetInt32(2);
                    int bplayerid = dr.GetInt32(3);

                    tmp[count][0] = gamedate.ToString();
                    tmp[count][1] = wplayerid.ToString();
                    tmp[count][2] = bplayerid.ToString();
                    tmp[count][3] = gameid.ToString();

                    count++;
                }
            }

            //close data reader
            dr.Close();

            //close connection
            sqlConnection.Close();

            string[] result = new string[15];
            for(int i=0; i < tmp.Length; i++)
            {
                if(tmp[i][0] != null)
                    result[i] = tmp[i][3] + "$" + get_nickname(Int32.Parse(tmp[i][1])) + " VS " + get_nickname(Int32.Parse(tmp[i][2])) + " AT " + tmp[i][0];

            }
            return result;
        }

        /// <summary>
        /// gets a game information according to its id
        /// </summary>
        /// <param name="client_id"></param>
        /// <param name="gameid"></param>
        /// <returns></returns>
        public static string[] getgame(int client_id, int gameid)
        {
            String stm = @"select date_time, wplayerid, bplayerid, moves from Games where game_id=" + gameid+ " AND (wplayerid=" + client_id + " OR bplayerid=" + client_id + ")";
            cmd.CommandText = stm;

            //open connection
            sqlConnection.Open();

            //execute the SQLCommand
            SqlDataReader dr = cmd.ExecuteReader();

            string[] result = new string[4];
            //check if there are records
            if (dr.HasRows)
            {
                while (dr.Read())
                {
                    result[0]= dr.GetDateTime(0).ToString();//var gamedate 
                    result[1] = dr.GetInt32(1).ToString();  //int wplayerid
                    result[2] = dr.GetInt32(2).ToString();  //int bplayerid
                    result[3]= dr.GetString(3);             //string moves
                }
            }

            //close data reader
            dr.Close();

            //close connection
            sqlConnection.Close();

            return result;
        }

        /// <summary>
        /// check if the username is already in the database
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public static Boolean IsUsernameExist(String username) //if their is a username with that name
        {
            if (checkForSQLInjection(username))
                return false;
            String stm = "select count(username) from users where username='" + gethash(username) + "'";
            cmd.Connection = sqlConnection;
            cmd.CommandText = stm;
            sqlConnection.Open();
            int count = (int)cmd.ExecuteScalar();//Executenonquery();
            sqlConnection.Close();
            if (count >= 1) { return true; }
            else { return false; }
        }
        /// <summary>
        /// gets a player ID and returns it's nickname
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static String get_nickname(int id)
        {
            if (id == -1)
                return "AI";
            String stm = "select nickname from users where Id='" + id + "'";
            cmd.Connection = sqlConnection;
            cmd.CommandText = stm;
            sqlConnection.Open();
            String nickname = (string)cmd.ExecuteScalar();
            sqlConnection.Close();
            return nickname;
        }
        
        /// <summary>
        /// check if the email is already in the database
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
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
        /// <summary>
        /// get player's ID and return when was the last time he changed password
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
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

        /// <summary>
        /// update a username's password
        /// </summary>
        /// <param name="username"></param>
        /// <param name="new_password"></param>
        /// <returns></returns>
        public static Boolean change_password(String username, String new_password)
        {
            if (checkForSQLInjection(username) || checkForSQLInjection(new_password))
                return false;
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

        /// <summary>
        /// automaticly finds the local database directory for the connection string
        /// </summary>
        /// <param name="database_name"></param>
        /// <returns></returns>
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
                    return String.Format(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename={0};Integrated Security=True", directory);
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
        /// <summary>
        /// get the hash of a string
        /// </summary>
        /// <param name="inputString"></param>
        /// <returns></returns>
        public static string gethash(string inputString)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in GetHashbytes(inputString))
                sb.Append(b.ToString("X2"));

            return sb.ToString();
        }

        /// <summary>
        /// insert new user data to the database
        /// </summary>
        /// <param name="client_data"></param>
        /// <returns></returns>
        public static Boolean InsertNewUser(String client_data) // inserting client data if he doesnt exist already // return if did the action
        {
            String[] usepass = client_data.Split('&');
            if (!IsUsernameExist(usepass[0]) && usepass.Length >= 2)
            {
                usepass[0] = gethash(usepass[0]);
                usepass[1] = gethash(usepass[1]);
                for (int i = 0; i < usepass.Length; i++)
                {
                    if (checkForSQLInjection(usepass[i]))
                        return false;
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

        /// <summary>
        /// inser new game data to the database
        /// </summary>
        /// <param name="white_player_id"></param>
        /// <param name="black_player_id"></param>
        /// <param name="game"></param>
        public static void insert_game(string white_player_id, string black_player_id, string game)
        {
            string stm = String.Format("insert into Games (wplayerid,bplayerid,moves) values ({0},{1},'{2}')", white_player_id, black_player_id, game);
            cmd.Connection = sqlConnection;
            cmd.CommandText = stm;
            sqlConnection.Open();
            cmd.ExecuteNonQuery();
            sqlConnection.Close();
        }
        /// <summary>
        /// gets a string and check if it contain sql saved statments, if so it might can be sql injection
        /// </summary>
        /// <param name="userInput"></param>
        /// <returns></returns>
        public static Boolean checkForSQLInjection(string userInput)
        {

            bool isSQLInjection = false;

            string[] sqlCheckList = { "--",

                                       ";--",

                                       ";",

                                       "/*",

                                       "*/",

                                        "@@",

                                        "@",

                                        "char",

                                       "nchar",

                                       "varchar",

                                       "nvarchar",

                                       "alter",

                                       "begin",

                                       "cast",

                                       "create",

                                       "cursor",

                                       "declare",

                                       "delete",

                                       "drop",

                                       "end",

                                       "exec",

                                       "execute",

                                       "fetch",

                                            "insert",

                                          "kill",

                                             "select",

                                           "sys",

                                            "sysobjects",

                                            "syscolumns",

                                           "table",

                                           "update"

                                       };

            string CheckString = userInput.Replace("'", "''");

            for (int i = 0; i <= sqlCheckList.Length - 1; i++)
                if ((CheckString.IndexOf(sqlCheckList[i], StringComparison.OrdinalIgnoreCase) >= 0))
                    isSQLInjection = true;

            return isSQLInjection;
        }
    }
}
