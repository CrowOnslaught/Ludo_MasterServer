using System;
using System.Collections.Generic;
using Mono.Data.Sqlite;
using System.IO;

namespace Ludo_MasterServer
{
    public struct PlayerRankingInfo
    {
        public string m_playerName;
        public string m_playerScore;
        public string m_rankingPos;

        public PlayerRankingInfo(string name, string score, string pos)
        {
            m_playerName = name;
            m_playerScore = score;
            m_rankingPos = pos;
        }
    }


    public class SQLiteManager
    {
        public SqliteConnection m_connection;

        public SQLiteManager()
        {
            m_connection = new SqliteConnection("Data Source=database.sqlite3");

            if (!File.Exists("./database.sqlite3"))
            {
                SqliteConnection.CreateFile("database.sqlite3");
                InitDatabaseTables();
            }
        }
        private void OpenConnection()
        {
            if (m_connection.State != System.Data.ConnectionState.Open)
            {
                m_connection.Open();
            }
        }
        private void CloseConnection()
        {
            if (m_connection.State != System.Data.ConnectionState.Closed)
            {
                m_connection.Close();
            }
        }

        private void InitDatabaseTables()
        {
            OpenConnection();

            string query = "CREATE TABLE clients (id INTEGER PRIMARY KEY AUTOINCREMENT,name  TEXT,password  TEXT,score INTEGER DEFAULT 0)";
            SqliteCommand l_command = new SqliteCommand(query, m_connection);
            l_command.ExecuteNonQuery();

            string query2 = "CREATE TABLE matchs (id INTEGER PRIMARY KEY AUTOINCREMENT, " +
                "date  TEXT,id_red INTEGER, id_blue   INTEGER, " +
                "id_green  INTEGER,id_yellow INTEGER, " +
                "id_first  INTEGER,id_second INTEGER, " +
                "id_third  INTEGER,id_fourth INTEGER)";

            SqliteCommand l_command2 = new SqliteCommand(query2, m_connection);
            l_command2.ExecuteNonQuery();

            CloseConnection();
        }

        public void AddClient(string name, string password)
        {
            OpenConnection();

            string query = "INSERT INTO clients ('name', 'password') VALUES (@name, @password)";
            SqliteCommand l_command = new SqliteCommand(query, m_connection);
            l_command.Parameters.AddWithValue("@name", name);
            l_command.Parameters.AddWithValue("@password", password);
            var result = l_command.ExecuteNonQuery();

            CloseConnection();
        }

        public string GetClientName(int id)
        {
            OpenConnection();
            string l_name = "";

            string l_query = "SELECT name FROM clients WHERE id = @id";
            SqliteCommand l_command = new SqliteCommand(l_query, m_connection);
            l_command.Parameters.AddWithValue("@id", id);

            SqliteDataReader l_result = l_command.ExecuteReader();
            if (l_result.HasRows)
            {
                while(l_result.Read())
                {
                    l_name = l_result["name"].ToString();
                }
            }
            CloseConnection();

            return l_name;
        }

        public string GetClientPassword(int id)
        {
            OpenConnection();
            string l_pass = "";

            string l_query = "SELECT * FROM clients WHERE id = @id";
            SqliteCommand l_command = new SqliteCommand(l_query, m_connection);
            l_command.Parameters.AddWithValue("@id", id);


            SqliteDataReader l_result = l_command.ExecuteReader();
            if (l_result.HasRows)
            {
                while (l_result.Read())
                {
                    if ((int)(l_result["id"]) == id)
                        l_pass = l_result["password"].ToString();
                }
            }
            CloseConnection();

            return l_pass;
        }
        public string GetClientPassword(string name)
        {
            OpenConnection();
            string l_pass = "";

            string l_query = "SELECT * FROM clients WHERE name = @name";
            SqliteCommand l_command = new SqliteCommand(l_query, m_connection);
            l_command.Parameters.AddWithValue("@name", name);


            SqliteDataReader l_result = l_command.ExecuteReader();
            if (l_result.HasRows)
            {
                while (l_result.Read())
                {
                    if ((l_result["name"]).ToString() == name)
                        l_pass = l_result["password"].ToString();
                }
            }
            CloseConnection();

            return l_pass;
        }

        public int GetClientID(string name)
        {
            OpenConnection();
            int l_id = 0;

            string l_query = "SELECT id FROM clients WHERE name = @name";
            SqliteCommand l_command = new SqliteCommand(l_query, m_connection);
            l_command.Parameters.AddWithValue("@name", name);


            SqliteDataReader l_result = l_command.ExecuteReader();
            if (l_result.HasRows)
            {
                while (l_result.Read())
                {
                    l_id = l_result.GetInt32(0);
                }
            }
            CloseConnection();

            return l_id;
        }

        public bool ExistsName(string name)
        {
            OpenConnection();
            bool l_exists = false;

            string query = "SELECT * FROM clients WHERE name = @name";
            SqliteCommand l_command = new SqliteCommand(query, m_connection);
            l_command.Parameters.AddWithValue("@name", name);

            SqliteDataReader l_result = l_command.ExecuteReader();
            if (l_result.HasRows)
            {
                while (l_result.Read())
                {
                    if(name == l_result["name"].ToString())
                        l_exists = true;
                }
            }
            CloseConnection();

            return l_exists;
        }

        public int GetClientScore(int id)
        {
            OpenConnection();

            int l_score = 0;

            //Get actual score
            string l_query = "SELECT score FROM clients WHERE id = @id";
            SqliteCommand l_command = new SqliteCommand(l_query, m_connection);
            l_command.Parameters.AddWithValue("@id", id);

            SqliteDataReader l_result = l_command.ExecuteReader();
            if (l_result.HasRows)
            {
                while (l_result.Read())
                {
                    l_score = l_result.GetInt32(0);
                }
            }

            CloseConnection();
            return l_score;
        }

        public void AddScoreToClient(int id, int position)
        {
            int l_scoreToAdd = 4 - position;
           
            if(l_scoreToAdd > 0)
            {
                OpenConnection();

                int l_currentScore = GetClientScore(id);
                l_currentScore += l_scoreToAdd;

                string query = "UPDATE clients SET score = @score WHERE id = @id;";
                SqliteCommand l_command2 = new SqliteCommand(query, m_connection);
                l_command2.Parameters.AddWithValue("@score", l_scoreToAdd);
                l_command2.Parameters.AddWithValue("@id", id);
                var result = l_command2.ExecuteNonQuery();

                CloseConnection();
            }
        }

        public List<PlayerRankingInfo> GetRankingTop(int amount)
        {
            OpenConnection();

            List<PlayerRankingInfo> l_list = new List<PlayerRankingInfo>();
            //Get actual score
            string l_query = "SELECT id, name, score FROM clients ORDER BY score DESC LIMIT @amount";
            SqliteCommand l_command = new SqliteCommand(l_query, m_connection);
            l_command.Parameters.AddWithValue("@amount", amount);

            SqliteDataReader l_result = l_command.ExecuteReader();
            if (l_result.HasRows)
            {
                int l_pos = 1;
                while (l_result.Read())
                {
                    PlayerRankingInfo l_player = new PlayerRankingInfo(l_result["name"].ToString(), l_result["score"].ToString(), l_pos.ToString());
                    l_list.Add(l_player);
                    l_pos++;
                }
            }

            CloseConnection();
            return l_list;
        }

        public void AddMatch(List<GameServer.PlayerInfo> players)
        {
            OpenConnection();

            string query = "INSERT INTO matchs ('date', 'id_red', 'id_blue', 'id_green', 'id_yellow', 'id_first', 'id_second', 'id_third', 'id_fourth') " +
                            "VALUES (@date, @id_red, @id_blue,@id_green,@id_yellow,@id_first,@id_second,@id_third,@id_fourth)";

            SqliteCommand l_command = new SqliteCommand(query, m_connection);
            l_command.Parameters.AddWithValue("@date", String.Format( "dd/MM/yy HH:mm", DateTime.Now));

            l_command.Parameters.AddWithValue("@id_red", players.Find(x=> x.m_color == Enums.Colors.red).m_id);
            l_command.Parameters.AddWithValue("@id_blue", players.Find(x => x.m_color == Enums.Colors.blue).m_id);
            l_command.Parameters.AddWithValue("@id_green", players.Find(x => x.m_color == Enums.Colors.green).m_id);
            l_command.Parameters.AddWithValue("@id_yellow", players.Find(x => x.m_color == Enums.Colors.yellow).m_id);

            l_command.Parameters.AddWithValue("@id_first", players[0].m_id);
            l_command.Parameters.AddWithValue("@id_second", players[1].m_id);
            l_command.Parameters.AddWithValue("@id_third", players[2].m_id);
            l_command.Parameters.AddWithValue("@id_fourth", players[3].m_id);

            var result = l_command.ExecuteNonQuery();

            CloseConnection();
        }
    }
}
