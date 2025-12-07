using System.Collections.Generic;
using System.Data.SQLite;
using UnityEngine;

namespace Quizly
{
    public static class UserManager
    {
        private static User _loggedIn;
        public delegate void UserLogin();
        public static event UserLogin OnUserLogin;
        
        public delegate void UserLogout();
        public static event UserLogout OnUserLogout;

        public static User GetLoggedIn()
        {
            return _loggedIn;
        }

        public static bool IsLoggedIn()
        {
            return _loggedIn is not null;
        }

        /// <summary>
        /// Tries to login with the passed username and password. 
        /// </summary>
        /// <returns>True if able to login, false if not</returns>
        public static bool TryLogin(string username, string password)
        {
            string dataPassword = DBManager.FindValues(DBManager.Table.Users, "password", 
                new Dictionary<string, List<string>>(){{"username", new List<string>(){username}}})[0];

            if (dataPassword == password)
            {
                _loggedIn = (User)DBManager.FindMatching(DBManager.Table.Users, 
                    new Dictionary<string, string>(){{"username", username}})[0];
                OnUserLogin?.Invoke();
                return true; 
            }

            return false; 
        }
        
        /// <summary>
        /// Saves user to database, returns true if sent save query, otherwise returns false. 
        /// </summary>
        public static bool CreateUser(string username, string password)
        {
            // #if UNITY_EDITOR
            // Debug.Log($"Creating user {username}");
            // #endif
            
            if (!CheckUsername(username))
            {
                return false; 
            }
            
            User newUser = new User
            {
                username = username,
                password = password
            };

            DBManager.SaveObject(newUser);

            return true; 
        }

        /// <summary>
        /// Checks if username is already in use.
        /// </summary>
        /// <param name="username"></param>
        /// <returns>True if username is free, false if username taken</returns>
        public static bool CheckUsername(string username)
        {
            #if UNITY_EDITOR
            Debug.Log($"Checking user {username}");
            #endif
            
            if (DBManager.FindValues(DBManager.Table.Users, "*", 
                    new Dictionary<string, List<string>>(){{"username", new List<string>(){username}}}).Count > 0)
            {
                return false;
            }
            
            return true;
        }

        public static void Logout()
        {
            _loggedIn = null;
            OnUserLogout?.Invoke();
        }
    }
}
