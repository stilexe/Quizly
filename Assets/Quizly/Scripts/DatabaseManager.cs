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
        /// Creates database
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

        public static void SaveQuery(Question question)
        {
            string queryString = $"INSERT INTO {Table.Questions.ToString().ToLower()} ('question_text', 'answer_set', 'category_id', 'difficulty', 'tip') VALUES (";

            queryString += $"'{question.question}', '{JsonUtility.ToJson(question.answerSet)}', '{question.categoryID}', '{question.difficulty}', '{question.tip}');";

            SendQuery(queryString);
        }

        public static List<Question> SearchQuestions(Dictionary<string, string> columnValues = null)
        {
            List<Question> questions = new List<Question>();
            
            Question toAdd;
            
            SQLiteConnection connection = GetConnection();
            connection.Open();
            SQLiteCommand command = connection.CreateCommand();
            command.CommandType = System.Data.CommandType.Text;
            command.CommandText = $"SELECT * FROM {Table.Questions.ToString().ToLower()}";

            if (columnValues != null)
            {
                command.CommandText += " WHERE ";
                
                int i = 0;
                
                foreach (KeyValuePair<string, string> pair in columnValues)
                {
                    if (i > 0)
                    {
                        command.CommandText += " AND ";
                    }

                    command.CommandText += $"{pair.Key}";

                    if (pair.Key == "question_text")
                    {
                        command.CommandText += $" LIKE '%{pair.Value}%'";
                    }
                    else
                    {
                        command.CommandText += $"= '{pair.Value}'";
                    }
                    
                    i++;
                }
            } 
            
            command.CommandText += ";";
            
#if UNITY_EDITOR
            Debug.Log(command.CommandText);
#endif
            
            SQLiteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                toAdd = new Question();
                toAdd.id = int.Parse(reader["id"].ToString());
                toAdd.question = reader["question_text"].ToString();
                toAdd.answerSet = JsonUtility.FromJson<AnswerSet>(reader["answer_set"].ToString());
                toAdd.categoryID = int.Parse(reader["category_id"].ToString());
                toAdd.difficulty = int.Parse(reader["difficulty"].ToString());
                toAdd.tip = reader["tip"].ToString();
                
                questions.Add(toAdd);
            }
            
            connection.Close();

            return questions; 
        }

        public static void SaveQuery(Quiz quiz)
        {
            string queryString = $"INSERT INTO {Table.Quizzes.ToString().ToLower()} ('name', 'time', 'questions') VALUES (";

            queryString += $"'{quiz.quizName}', '{quiz.time}', '{JsonUtility.ToJson(quiz.questionSet)}');";
            
            SendQuery(queryString);
        }

        public static void SaveQuery(QuestionWeighting weight)
        {
            string queryString = $"INSERT INTO {Table.Weightings.ToString().ToLower()} ('quiz_id', 'question_id', 'weighting') VALUES (";

            queryString += $"'{weight.quizID}', '{weight.questionID}', '{weight.weight}');";
            
            SendQuery(queryString);
        }

        public static void SaveQuery(string category)
        {
            string queryString = $"INSERT INTO {Table.Categories.ToString().ToLower()} ('name') VALUES ('{category}');";
            
            SendQuery(queryString);
        }

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

        public static List<string> SearchQuery(Table table, List<string> columns = null, List<string> values = null)
        {
            string queryString = $"SELECT * FROM {table.ToString().ToLower()}";

            if (columns is not null && values is not null)
            {
                queryString += " WHERE";
                for (int i = 0; i < columns.Count; i++)
                {
                    if (i > 0)
                    {
                        queryString += " AND";
                    }
                    
                    queryString += $" '{columns[i]}' = '{values[i]}'";
                }
            }
            
#if UNITY_EDITOR
            Debug.Log(queryString);
#endif
            
            List<string> results = new List<string>();

            SQLiteConnection connection = GetConnection();
        
            connection.Open();
        
            SQLiteCommand command = connection.CreateCommand();
            command.CommandType = System.Data.CommandType.Text;
            command.CommandText = queryString;

            string toAdd;
        
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                toAdd = "";

                if (columns is not null && values is null)
                {
                    foreach (string column in columns)
                    {
                        toAdd += reader[column] + "|";
                    }
                }
                else
                {
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        toAdd += reader[i] + "|";
                    }
                }
                
                toAdd = toAdd.TrimEnd();
                toAdd = toAdd.TrimEnd('|');
            
                results.Add(toAdd);
            }

            connection.Close();
            
            return results;
        }
        
    }
}
