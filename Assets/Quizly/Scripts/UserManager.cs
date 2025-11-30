using System.Data.SQLite;
using UnityEngine;

namespace Quizly
{
    public static class UserManager
    {
        public static void CreateUser(string username, string password)
        {
            if (!CheckUsername(username))
            {
                Debug.Log("Username already in use");
                return;
            }
        
            SQLiteConnection connection = DatabaseManager.GetConnection();
        
            connection.Open();
        
            SQLiteCommand command = connection.CreateCommand();
            command.CommandType = System.Data.CommandType.Text;
            command.CommandText = "INSERT INTO users (username, password) VALUES ('" + username + "','" + password + "');";
            command.ExecuteNonQuery();
        
            connection.Close();
        }

        /// <summary>
        /// Checks if username is already in use.
        /// </summary>
        /// <param name="username"></param>
        /// <returns>True if username is free, false if username taken</returns>
        public static bool CheckUsername(string username)
        {
            SQLiteConnection connection = DatabaseManager.GetConnection();
        
            connection.Open();
        
            SQLiteCommand command = connection.CreateCommand();
            command.CommandType = System.Data.CommandType.Text;
            command.CommandText = "SELECT * FROM users";
            var reader = command.ExecuteReader();

            while (reader.Read())
            {
                if (reader["username"].Equals(username))
                {
                    connection.Close();
                    return false; 
                }
            }
        
            connection.Close();
            return true;
        }
    }
}
