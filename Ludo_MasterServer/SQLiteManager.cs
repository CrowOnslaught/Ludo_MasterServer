﻿using System.Collections.Generic;
using System.Data.SQLite;
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
        public SQLiteConnection m_connection;

        public SQLiteManager()
        {
            m_connection = new SQLiteConnection("Data Source=database.sqlite3");

            if (!File.Exists("./database.sqlite3"))
            {
                SQLiteConnection.CreateFile("database.sqlite3");
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
            SQLiteCommand l_command = new SQLiteCommand(query, m_connection);
            l_command.ExecuteNonQuery();

            //string query2 = "CREATE TABLE clients (id INTEGER PRIMARY KEY AUTOINCREMENT,name  TEXT,password  TEXT,score INTEGER DEFAULT 0)";
            //SQLiteCommand l_command2 = new SQLiteCommand(query2, m_connection);
            //l_command2.ExecuteNonQuery();

            CloseConnection();
        }

        public void AddClient(string name, string password)
        {
            OpenConnection();

            string query = "INSERT INTO clients ('name', 'password') VALUES (@name, @password)";
            SQLiteCommand l_command = new SQLiteCommand(query, m_connection);
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
            SQLiteCommand l_command = new SQLiteCommand(l_query, m_connection);
            l_command.Parameters.AddWithValue("@id", id);

            SQLiteDataReader l_result = l_command.ExecuteReader();
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
            SQLiteCommand l_command = new SQLiteCommand(l_query, m_connection);
            l_command.Parameters.AddWithValue("@id", id);


            SQLiteDataReader l_result = l_command.ExecuteReader();
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
            SQLiteCommand l_command = new SQLiteCommand(l_query, m_connection);
            l_command.Parameters.AddWithValue("@name", name);


            SQLiteDataReader l_result = l_command.ExecuteReader();
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
            SQLiteCommand l_command = new SQLiteCommand(l_query, m_connection);
            l_command.Parameters.AddWithValue("@name", name);


            SQLiteDataReader l_result = l_command.ExecuteReader();
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
            SQLiteCommand l_command = new SQLiteCommand(query, m_connection);
            l_command.Parameters.AddWithValue("@name", name);

            SQLiteDataReader l_result = l_command.ExecuteReader();
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
            SQLiteCommand l_command = new SQLiteCommand(l_query, m_connection);
            l_command.Parameters.AddWithValue("@id", id);

            SQLiteDataReader l_result = l_command.ExecuteReader();
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
                SQLiteCommand l_command2 = new SQLiteCommand(query, m_connection);
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
            SQLiteCommand l_command = new SQLiteCommand(l_query, m_connection);
            l_command.Parameters.AddWithValue("@amount", amount);

            SQLiteDataReader l_result = l_command.ExecuteReader();
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
    }
}
