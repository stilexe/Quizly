using System.Collections.Generic;
using System.Data.SQLite;
using UnityEngine;
using UnityEngine.Windows;

namespace Quizly
{
    public static class DatabaseManager
    {
        private static string _databaseFileName; 
        private static string _databaseLocation;
        
        public enum Table
        {
            Users,
            Quizzes,
            Questions,
            Categories,
            Results,
            Weightings
        }

        /// <summary>
        /// Sets database location strings and creates the file if there is not one.
        /// </summary>
        private static void SetDatabaseLocation()
        {
            if (!Directory.Exists(Application.streamingAssetsPath))
            {
                Directory.CreateDirectory(Application.streamingAssetsPath);
            }

            _databaseLocation = Application.streamingAssetsPath + "/quizData.sqlite";

            if (!File.Exists(_databaseLocation))
            {
                CreateDatabase();
            }
        }

        /// <summary>
        /// Creates database.
        /// </summary>
        private static void CreateDatabase()
        {
#if UNITY_EDITOR
            Debug.Log("Creating database");
#endif
        
            SQLiteConnection.CreateFile(_databaseLocation);
        
            SQLiteConnection connection = new SQLiteConnection($"Data Source={_databaseLocation};Version=3;");
        
            connection.Open();
        
            SQLiteCommand command = connection.CreateCommand();
            command.CommandType = System.Data.CommandType.Text;
        
            command.CommandText = $"CREATE TABLE IF NOT EXISTS {Table.Users.ToString().ToLower()} (username TEXT PRIMARY KEY NOT NULL UNIQUE, password TEXT NOT NULL);";
            command.ExecuteNonQuery();
        
            command.CommandText = $"CREATE TABLE IF NOT EXISTS {Table.Quizzes.ToString().ToLower()} (id INTEGER PRIMARY KEY UNIQUE, name TEXT NOT NULL UNIQUE, time INTEGER, questions TEXT NOT NULL);";
            command.ExecuteNonQuery();

            command.CommandText = $"CREATE TABLE IF NOT EXISTS {Table.Categories.ToString().ToLower()} (id INTEGER PRIMARY KEY UNIQUE, name TEXT NOT NULL UNIQUE);";
            command.ExecuteNonQuery();
        
            command.CommandText = $"CREATE TABLE IF NOT EXISTS {Table.Results.ToString().ToLower()} (quiz_id INTEGER NOT NULL, username TEXT NOT NULL, result_date TEXT NOT NULL, score INTEGER NOT NULL, answers TEXT NOT NULL);";
            command.ExecuteNonQuery();
        
            command.CommandText = $"CREATE TABLE IF NOT EXISTS {Table.Questions.ToString().ToLower()} (id INTEGER NOT NULL PRIMARY KEY, question_text TEXT NOT NULL UNIQUE, answer_set TEXT NOT NULL, category_id INTEGER NOT NULL, difficulty INTEGER NOT NULL, tip TEXT NOT NULL);";
            command.ExecuteNonQuery();
        
            command.CommandText = $"CREATE TABLE IF NOT EXISTS {Table.Weightings.ToString().ToLower()} (quiz_id INTEGER NOT NULL, question_id INTEGER NOT NULL, weighting INTEGER NOT NULL);";
            command.ExecuteNonQuery();
        
            connection.Close(); 
        }

        /// <summary>
        /// Gets connection to database. 
        /// </summary>
        /// <returns>Connection to database</returns>
        public static SQLiteConnection GetConnection()
        {
            if (string.IsNullOrEmpty(_databaseLocation))
            {
                SetDatabaseLocation();
            }
        
            return new SQLiteConnection($"Data Source={_databaseLocation};Version=3;");
        }

        /// <summary>
        /// Get the ID of something stored in the database 
        /// </summary>
        /// <param name="table">Table its stored in</param>
        /// <param name="columnName">Name of a column</param>
        /// <param name="matchingValue">Value to search the column for</param>
        /// <returns></returns>
        public static int GetID(Table table, string columnName, string matchingValue)
        {
            int id = 0;
            
            SQLiteConnection connection = GetConnection();
        
            connection.Open();
        
            SQLiteCommand command = connection.CreateCommand();
            command.CommandType = System.Data.CommandType.Text;
            command.CommandText = $"SELECT * FROM {table.ToString().ToLower()} WHERE {columnName} = '{matchingValue}';";
        
            SQLiteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                id = int.Parse(reader["id"].ToString());
            }

            return id; 
        }

        /// <summary>
        /// Find item in table with certain id
        /// </summary>
        public static object FindWithID(Table table, int id)
        {
            SQLiteConnection connection = GetConnection();
            connection.Open();
            SQLiteCommand command = connection.CreateCommand();
            command.CommandType = System.Data.CommandType.Text;
            command.CommandText = $"SELECT * FROM {table.ToString().ToLower()} WHERE id = {id.ToString()}";
            SQLiteDataReader reader = command.ExecuteReader();
            
            while (reader.Read())
            {
                switch (table)
                {
                    case Table.Quizzes:
                        return new Quiz()
                        {
                            id = int.Parse(reader["id"].ToString()),
                            quizName = reader["name"].ToString(),
                            time = float.Parse(reader["time"].ToString()),
                            questionSet = JsonUtility.FromJson<QuestionSet>(reader["questions"].ToString())
                        };
                    
                     case Table.Categories:
                        return reader["category_name"].ToString();
                     
                     case Table.Questions:
                        return new Question()
                        {
                            id = int.Parse(reader["id"].ToString()),
                            question = reader["question_text"].ToString(),
                            categoryID = int.Parse(reader["category_id"].ToString()),
                            difficulty = int.Parse(reader["difficulty"].ToString()),
                            tip = reader["tip"].ToString(),
                            answerSet = JsonUtility.FromJson<AnswerSet>(reader["answer_set"].ToString())
                        };
                }
            }

            return null;
        }

        public static List<object> FindMatching(Table table, Dictionary<string, string> columnValues = null, bool exact = true)
        {
            string queryString = $"SELECT * FROM {table.ToString().ToLower()}";

            if (columnValues is not null)
            {
                queryString += " WHERE";
                int i = 0;
                
                foreach (KeyValuePair<string, string> pair in columnValues)
                {
                    if (i > 0)
                    {
                        queryString += " AND";
                    }
                
                    queryString += $" {pair.Key} ";
                
                    if(!exact)
                    {
                        queryString += $"LIKE '%{pair.Value}%'";
                    }
                    else
                    {
                        if(float.TryParse(pair.Value, out float f)) //if number dont put quotes around 
                        {
                            queryString += $"= {f}";
                        }
                        else
                        {
                            queryString += $"= '%{pair.Value}%'";
                        }
                    }

                    i++;
                }
            }

            queryString += ";";
            
            return FindQuery(table, queryString);
            
        }

        /// <summary>
        /// Finds everything in the table that has value that matches any of the column values in the specified column and returns it as the class object.
        /// </summary>
        public static List<object> FindMatching(Table table, string column, List<string> columnValues)
        {
            string queryString = $"SELECT * FROM {table.ToString().ToLower()} WHERE {column} IN (";

            foreach (string value in columnValues)
            {
                if (int.TryParse(value, out int i))
                {
                    queryString += $"{i},";
                }
                else
                {
                    queryString += $"'{value}',";
                }
            }

            queryString = queryString.TrimEnd();
            queryString = queryString.Trim(',');
            queryString += ");";
            
            return FindQuery(table, queryString);
        }

        private static List<object> FindQuery(Table table, string queryString)
        {
            #if UNITY_EDITOR
            Debug.Log(queryString);
            #endif
            
            //connect and send query 
            SQLiteConnection connection = GetConnection();
            connection.Open();
            SQLiteCommand command = connection.CreateCommand();
            command.CommandType = System.Data.CommandType.Text;
            command.CommandText = queryString;
            SQLiteDataReader reader = command.ExecuteReader();
            List<object> results = new List<object>();

            while (reader.Read())
            {
                switch (table)
                {
                    case Table.Quizzes:
                        Quiz quizAdd = new Quiz()
                        {
                            id = int.Parse(reader["id"].ToString()),
                            quizName = reader["name"].ToString(),
                            time = float.Parse(reader["time"].ToString()),
                            questionSet = JsonUtility.FromJson<QuestionSet>(reader["questions"].ToString())
                        };
                        
                        results.Add(quizAdd);
                        break;
                    
                    case Table.Categories:
                        string stringAdd = reader["name"].ToString();
                        results.Add(stringAdd);
                        break;
                     
                    case Table.Questions:
                        Question questionAdd = new Question()
                        {
                            id = int.Parse(reader["id"].ToString()),
                            question = reader["question_text"].ToString(),
                            categoryID = int.Parse(reader["category_id"].ToString()),
                            difficulty = int.Parse(reader["difficulty"].ToString()),
                            tip = reader["tip"].ToString(),
                            answerSet = JsonUtility.FromJson<AnswerSet>(reader["answer_set"].ToString())
                        };
                        
                        results.Add(questionAdd);
                        break;
                    
                    case Table.Weightings:
                        QuestionWeighting weightAdd = new QuestionWeighting()
                        {
                            questionID = int.Parse(reader["question_id"].ToString()),
                            quizID = int.Parse(reader["quiz_id"].ToString()),
                            weight = int.Parse(reader["weighting"].ToString()),
                        };
                        
                        results.Add(weightAdd);
                        break;
                    
                    case Table.Users:
                        User userAdd = new User()
                        {
                            username = reader["username"].ToString(),
                            password = reader["password"].ToString(),
                        };
                        
                        results.Add(userAdd);
                        break; 
                    
                    case Table.Results:
                        break; 
                }
            }
            
            connection.Close();
            
            return results;
        }

        /// <summary>
        /// Returns values found in the database as a list of strings. 
        /// </summary>
        /// <param name="table">Table searched</param>
        /// <param name="columnToReturn">Column to return values from</param>
        /// <param name="columnToSearch">Column searched</param>
        /// <param name="columnValues">Values searched for in column</param>
        /// <returns></returns>
        public static List<string> ValuesQuery(Table table, string columnToReturn, string columnToSearch, List<string> columnValues)
        {
            List<string> results = new List<string>();

            string queryString = $"SELECT {columnToReturn} FROM {table.ToString().ToLower()} WHERE {columnToSearch} ";

            if (columnValues.Count > 1)
            {
                queryString += $" IN (";

                foreach (string value in columnValues)
                {
                    queryString += $"{value},";
                }
                
                queryString = queryString.Trim();
                queryString = queryString.TrimEnd(',');
                queryString += ");";

            }
            else
            {
                queryString += $" = '{columnValues[0]}';";
            }
            
#if UNITY_EDITOR
            Debug.Log(queryString);
#endif
            
            //connect and send query 
            SQLiteConnection connection = GetConnection();
            connection.Open();
            SQLiteCommand command = connection.CreateCommand();
            command.CommandType = System.Data.CommandType.Text;
            command.CommandText = queryString;
            SQLiteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                results.Add(reader.GetString(0));
            }
            
            connection.Close();

            return results;
        }

        public static List<string> ValuesQuery(Table table, string columnToReturn, Dictionary<string, List<string>> columnValues)
        {
            string queryString = $"SELECT {columnToReturn} FROM {table.ToString().ToLower()} WHERE ";

            int i = 0;

            foreach (KeyValuePair<string, List<string>> pair in columnValues)
            {
                if (i > 0)
                {
                    queryString += " AND ";
                }

                queryString += pair.Key; //column name 

                if (pair.Value.Count > 1) //the values to search for
                {
                    queryString += "IN (";
                    
                    foreach (string s in pair.Value)
                    {
                        if (int.TryParse(s, out int value))
                        {
                            queryString += $"{value},";
                        }
                        else
                        {
                            queryString += $"'{s}',";
                        }
                    }
                    
                    queryString = queryString.Trim();
                    queryString = queryString.TrimEnd(',');
                    queryString += ")";
                }
                else
                {
                    if (int.TryParse(pair.Value[0], out int value))
                    {
                        queryString += $" = {value}";
                    }
                    else
                    {
                        queryString += $" = '{pair.Value[0]}'";
                    }
                }
                
                i++;
            }
            
            queryString += ";";
            
            List<string> results = new List<string>();
            
#if UNITY_EDITOR
            Debug.Log(queryString);
#endif
            
            //connect and send query 
            SQLiteConnection connection = GetConnection();
            connection.Open();
            SQLiteCommand command = connection.CreateCommand();
            command.CommandType = System.Data.CommandType.Text;
            command.CommandText = queryString;
            SQLiteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                results.Add(reader.GetString(0));
            }
            
            connection.Close();
            
#if UNITY_EDITOR
            if (results.Count > 0)
            {
                string debug = "Values Query Results: ";
                foreach (string s in results)
                {
                    debug += $"{s} ,";
                }
                Debug.Log(debug);
            }
#endif

            return results;
        }

        public static void SaveQuery(object toSave, bool overwrite = false)
        {
            string queryString = "";

            if (overwrite)
            {
                
            }
            else
            {
                queryString = "INSERT INTO ";
            }
            
            switch (toSave)
            {
                case Quiz save:
                    queryString += $"{Table.Quizzes.ToString().ToLower()} ('name', 'time', 'questions') VALUES (";
                    queryString += $"'{save.quizName}', '{save.time}', '{JsonUtility.ToJson(save.questionSet)}');";
                    break;
                
                case Result results:
                    queryString += $"{Table.Results.ToString().ToLower()} ('quiz_id', 'username', 'result_date', 'score', 'answers') VALUES (";
                    queryString += $"{results.quizID}, '{results.username}', '{results.date}', '{results.score}', '{JsonUtility.ToJson(results.submissionSet)});";
                    break;
                
                case Question question:
                    queryString += $"{Table.Questions.ToString().ToLower()} ('question_text', 'answer_set', 'category_id', 'difficulty', 'tip') VALUES (";
                    queryString += $"'{question.question}', '{JsonUtility.ToJson(question.answerSet)}', '{question.categoryID}', '{question.difficulty}', '{question.tip}');";
                    break;
                
                case QuestionWeighting weight:
                    queryString += $"{Table.Weightings.ToString().ToLower()} ('quiz_id', 'question_id', 'weighting') VALUES (";
                    queryString += $"'{weight.quizID}', '{weight.questionID}', '{weight.weight}');";
                    break;
                
                case User user:
                    queryString += $"{Table.Users.ToString().ToLower()} ('username', 'password') VALUES (";
                    queryString += $"'{user.username}', '{user.password}');";
                    break;
                
                case string category:
                    queryString += $"{Table.Categories.ToString().ToLower()} ('name') VALUES ('{category}');";
                    break;
            }

            if (!string.IsNullOrEmpty(queryString))
            {
                SendQuery(queryString);
            }
            else
            {
                #if UNITY_EDITOR
                Debug.Log("Object sent not able to be searched for");
                #endif 
            }
        }

        public static void RemoveQuery(object toRemove)
        {
            
        }

        /// <summary>
        /// Sends a query to database with no return.
        /// </summary>
        private static void SendQuery(string query)
        {
            
#if UNITY_EDITOR
            Debug.Log(query);
#endif
            
            //connect to database
            SQLiteConnection connection = GetConnection();
            
            connection.Open();
            
            SQLiteCommand command = connection.CreateCommand();
            command.CommandType = System.Data.CommandType.Text;
            command.CommandText = query;
            command.ExecuteNonQuery();
            
            connection.Close();
        }
        
    }
}
