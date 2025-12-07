using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using UnityEngine;

namespace Quizly
{
    public static class DBManager
    {
        private static string _databaseFolder = "/Databases/";
        private static string _loadedDatabasePath;
        
        public enum Table
        {
            None,
            Users,
            Quizzes,
            Questions,
            Categories,
            Results,
            Weightings
        }
        
        private static readonly Dictionary<Table, List<string>> TableHeaders = new Dictionary<Table, List<string>>()
        {
            { Table.Users, new List<string>()
            {
                "id INTEGER PRIMARY KEY UNIQUE", "username TEXT NOT NULL", "password TEXT NOT NULL"
            }},
            { Table.Questions, new List<string>()
            {
                "id INTEGER PRIMARY KEY UNIQUE", "question_text TEXT NOT NULL", "answer_set TEXT NOT NULL", "category_id INTEGER NOT NULL", "difficulty INTEGER NOT NULL", "tip TEXT NOT NULL"
            }},
            { Table.Results, new List<string>()
            {
                "id INTEGER PRIMARY KEY UNIQUE", "quiz_id INTEGER NOT NULL", "user_id INTEGER NOT NULL", "score INTEGER NOT NULL", "date TEXT NOT NULL", "answers TEXT NOT NULL"
            }},
            { Table.Categories, new List<string>()
            {
                "id INTEGER PRIMARY KEY UNIQUE", "name TEXT NOT NULL"
            }},
            { Table.Weightings, new List<string>()
            {
                "id INTEGER PRIMARY KEY UNIQUE", "question_id INTEGER NOT NULL", "quiz_id INTEGER NOT NULL", "weight INTEGER NOT NULL"
            }},
            { Table.Quizzes, new List<string>()
            {
                "id INTEGER PRIMARY KEY UNIQUE", "name TEXT NOT NULL", "time INTEGER", "questions TEXT NOT NULL"
            }}
        };
        
        #region Database Creation

        public static bool TryCreateDatabase(string name)
        {
            string savePath = Application.streamingAssetsPath + _databaseFolder;
            
            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
            }
            
            savePath += name + ".sqlite";
            
            if (File.Exists(savePath))
            {
                return false; 
            }
            
            CreateDatabase(savePath);
            return true; 
        }
        
        private static void CreateDatabase(string path)
        {
#if UNITY_EDITOR
            Debug.Log("Creating database");
#endif
            
            SQLiteConnection.CreateFile(path);
            
            string queryString = "";

            foreach (Table table in TableHeaders.Keys)
            {
                queryString += $"CREATE TABLE IF NOT EXISTS {table.ToString().ToLower()} (";
                int i = 0;

                foreach (string header in TableHeaders[table])
                {
                    queryString += $"{header}";
                    if (i < TableHeaders[table].Count - 1) //not at last header yet 
                    {
                        queryString += ", ";
                    }
                    i++;
                }

                queryString += "); ";
            }
            
            SendQuery(queryString, path);
 
        }

        #endregion

        #region Loading Database

        public static bool DatabaseLoaded()
        {
            if (string.IsNullOrEmpty(_loadedDatabasePath))
            {
                return false;
            }

            return true; 
        }

        public static string DatabaseName()
        {
            string name = Path.GetFileNameWithoutExtension(_loadedDatabasePath);
            name = name.Replace("_", " ");
            
            return name;
        }

        public static string DatabaseFilePath(bool full = false)
        {
            if (full)
            {
                return _loadedDatabasePath;
            }
            
            return Path.GetRelativePath(Application.dataPath, _loadedDatabasePath);
        }

        public static void LoadDatabase(string databaseName)
        {
            _loadedDatabasePath = Application.streamingAssetsPath + _databaseFolder + databaseName;

            if (!_loadedDatabasePath.Contains('.'))
            {
                _loadedDatabasePath += ".sqlite";
            }
        }

        private static SQLiteConnection GetConnection(string filePath)
        {
            return new SQLiteConnection($"Data Source={filePath};Version=3;");
        }

        #endregion

        #region Reference File

        private static void SaveReferenceFile()
        {
            string savePath = _databaseFolder + "reference_file.txt"; 
        }

        public static List<Table> GetTables()
        {
            List<Table> tables = new List<Table>();

            foreach (Table table in TableHeaders.Keys)
            {
                tables.Add(table);
            }
            
            return tables;
        }

        public static List<string> GetDatabaseNames()
        {
            List<string> databaseNames = new List<string>();
            
            string folderPath = Application.streamingAssetsPath + _databaseFolder;

            if (Directory.Exists(folderPath))
            {
                DirectoryInfo di = new DirectoryInfo(folderPath);
                foreach (FileInfo file in di.GetFiles())
                {
                    if (file.Extension == ".sqlite")
                    {
                        databaseNames.Add(file.Name);
                    }
                }
            }
            
            return databaseNames;
        }

        #endregion

        #region Managing Data

        public static void SaveObject(object toSave, bool overwrite = false, string filePath = null)
        {
            if (filePath is null && !DatabaseLoaded())
            {
                #if UNITY_EDITOR
                Debug.LogError("No database loaded.");
                #endif
                return; 
            }
            
            string queryString = "";

            if (overwrite)
            {
                queryString = "INSERT OR REPLACE INTO ";
            }
            else
            {
                queryString = "INSERT INTO ";
            }
            
            switch (toSave)
            {
                case Quiz save:
                    if (FindMatching(Table.Quizzes, new Dictionary<string, string>(){{"name", save.quizName}}).Count > 0 && !overwrite)
                    {
#if UNITY_EDITOR
                        Debug.LogError("Trying to save quiz with duplicate name.");
#endif
                        return; 
                    }
                    
                    queryString += $"{Table.Quizzes.ToString().ToLower()} (";

                    if (save.id != 0)
                    {
                        queryString += "'id',";
                    }
                    queryString += "'name', 'time', 'questions') VALUES (";
                    if (save.id != 0)
                    {
                        queryString += $"'{save.id}',";
                    }
                    queryString += $"'{save.quizName}', '{save.time}', '{JsonUtility.ToJson(save.questionSet)}');";
                    
                    //save weightings 
                    break;
                
                case Result results:
                    queryString += $"{Table.Results.ToString().ToLower()} ('quiz_id', 'user_id', 'date', 'score', 'answers') VALUES (";
                    queryString += $"{results.quizID}, {results.userID}, '{results.date}', '{results.score}', '{JsonUtility.ToJson(results.submissionSet)}');";
                    break;
                
                case Question question:
                    string answerJson = JsonUtility.ToJson(question.answerSet);
                    
                    if (!overwrite && FindMatching(Table.Questions, 
                            new Dictionary<string, string>(){{"question_text", question.question}, {"answer_set", answerJson}}).Count > 0)
                    {
#if UNITY_EDITOR
                        Debug.LogError("Trying to save question that already exists.");
#endif
                        return; 
                    }
                    queryString += $"{Table.Questions.ToString().ToLower()} (";
                    if (question.id != 0)
                    {
                        queryString += "'id',";
                    }
                    queryString += "'question_text', 'answer_set', 'category_id', 'difficulty', 'tip') VALUES (";
                    if (question.id != 0)
                    {
                        queryString += $"'{question.id}',";
                    }
                    queryString += $"'{question.question}', '{answerJson}', '{question.categoryID}', '{question.difficulty}', '{question.tip}');";
                    break;
                
                case QuestionWeighting weight:
                    if (!overwrite && FindMatching(Table.Weightings, 
                            new Dictionary<string, string>(){{"quiz_id", weight.quizID.ToString()}, {"question_id", weight.questionID.ToString()}}).Count > 0)
                    {
#if UNITY_EDITOR
                        Debug.LogError("Trying to save weighting that already exists.");
#endif
                        return; 
                    }
                    queryString += $"{Table.Weightings.ToString().ToLower()} ('quiz_id', 'question_id', 'weight') VALUES (";
                    queryString += $"'{weight.quizID}', '{weight.questionID}', '{weight.weight}');";
                    break;
                
                case User user:
                    if (!overwrite && FindMatching(Table.Users, new Dictionary<string, string>(){{"username", user.username}}).Count > 0)
                    {
#if UNITY_EDITOR
                        Debug.LogError("Trying to save user that already exists.");
#endif
                        return; 
                    }
                    queryString += $"{Table.Users.ToString().ToLower()} (";
                    if (user.id != 0)
                    {
                        queryString += "'id',";
                    }
                    queryString += "'username', 'password') VALUES (";
                    if (user.id != 0)
                    {
                        queryString += $"'{user.id}',";
                    }
                    queryString += $"'{user.username}', '{user.password}');";
                    break;
                
                case string category:
                    //check it doesnt already exist 
                    if (!overwrite && FindMatching(Table.Categories, new Dictionary<string, string>(){{"name", category}}).Count > 0)
                    {
#if UNITY_EDITOR
                        Debug.LogError("Trying to save category that already exists.");
#endif
                        return; 
                    }
                    queryString += $"{Table.Categories.ToString().ToLower()} ('name') VALUES ('{category}');";
                    break;
            }

            SendQuery(queryString, filePath);
        }
        
        public static void RemoveData(Table table, int id, string filePath = null)
        {
            if (filePath is null && !DatabaseLoaded())
            {
#if UNITY_EDITOR
                Debug.LogError("No database loaded.");
#endif
                return; 
            }
            
            switch (table)
            {
                case Table.Quizzes:
                    SendQuery($"DELETE FROM {Table.Results.ToString().ToLower()} WHERE quiz_id = {id}", filePath); //delete any related results
                    SendQuery($"DELETE FROM {Table.Weightings.ToString().ToLower()} WHERE quiz_id = {id}", filePath); //delete any related tables 
                    break; 
                case Table.Users:
                    SendQuery($"DELETE FROM {Table.Results.ToString().ToLower()} WHERE username = {id}", filePath);
                    break; 
            }
            
            SendQuery($"DELETE FROM {table.ToString().ToLower()} WHERE id = {id}", filePath);
        }

        public static bool CanBeRemoved(Table table, int id, string filePath = null)
        {
            if (filePath is null && !DatabaseLoaded())
            {
#if UNITY_EDITOR
                Debug.LogError("No database loaded.");
#endif
                return false; 
            }
            
            string queryString;
            
            switch (table)
            {
                case Table.Categories: //check if any questions use the categories 
                    queryString = $"SELECT * FROM {Table.Questions.ToString().ToLower()} WHERE category_id = {id}";
                    //if any questions come back with the ids it cant be removed 
                    if (ResultTotal(queryString, filePath) > 0)
                    {
                        return false; 
                    }
                    break;
                case Table.Questions: //check if any quizzes use the questions 
                    queryString = $"SELECT questions FROM {Table.Quizzes.ToString().ToLower()};";
                    List<string> quizQuestions = SearchQuery(queryString, filePath); //get all quizzes questions 

                    foreach (string questions in quizQuestions)
                    {
                        Debug.Log(questions);
                    }
                    
                    break;
            }

            return true;
        }

        public static void ClearDatabase(string databaseName)
        {
            if (!databaseName.Contains('.'))
            {
                databaseName += ".sql";
            }

            string queryString = "";
            
            SendQuery(queryString, Application.streamingAssetsPath + _databaseFolder + databaseName);
        }

        #endregion

        #region Search Functions

        public static int TotalMatchingResults(Table table, Dictionary<string, string> columnValues = null, bool exact = true, string filePath = null)
        {
            if (filePath is null && !DatabaseLoaded())
            {
#if UNITY_EDITOR
                Debug.LogError("No database loaded.");
#endif
                return 0; 
            }
            
            string queryString = $"SELECT * FROM {table.ToString().ToLower()}";

            if (columnValues is not null && columnValues.Count > 0)
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
                            queryString += $"= '{pair.Value}'";
                        }
                    }

                    i++;
                }
            }

            queryString += ";";
            
            return ResultTotal(queryString, filePath);
        }

        public static object SearchWithID(Table table, int id, string filePath = null)
        {
            if (filePath is null && !DatabaseLoaded())
            {
#if UNITY_EDITOR
                Debug.LogError("No database loaded.");
#endif
                return null; 
            }
            
            string queryString = $"SELECT * FROM {table.ToString().ToLower()} WHERE id = {id.ToString()}";
            
            return ObjectQuery(table, queryString, filePath)[0];
        }

        public static string FindValueWithID(Table table, string columnToReturn, int id, string filePath = null)
        {
            if (filePath is null && !DatabaseLoaded())
            {
#if UNITY_EDITOR
                Debug.LogError("No database loaded.");
#endif
                return null; 
            }
            
            string queryString = $"SELECT {columnToReturn} FROM {table.ToString().ToLower()} WHERE id = {id.ToString()}";
            
            return SearchQuery(queryString, filePath)[0];
        }

        public static int FindID(Table table, string columnToSearch, string columnValue, string filePath = null)
        {
            if (filePath is null && !DatabaseLoaded())
            {
#if UNITY_EDITOR
                Debug.LogError("No database loaded.");
#endif
                return 0; 
            }
            
            string queryString = $"SELECT id FROM {table.ToString().ToLower()} WHERE {columnToSearch} =";

            if (int.TryParse(columnValue, out int i))
            {
                queryString += $"{i};";
            }
            else
            {
                queryString += $"'{columnValue}';";
            }
            
            int id = int.Parse(SearchQuery(queryString, filePath)[0]);

            return id; 
        }

        public static int FindID(Table table, Dictionary<string,string> searchValues, string filePath = null)
        {
            if (filePath is null && !DatabaseLoaded())
            {
#if UNITY_EDITOR
                Debug.LogError("No database loaded.");
#endif
                return 0; 
            }
            
            string queryString = $"SELECT id FROM {table.ToString().ToLower()} WHERE ";

            int i = 0;

            foreach (string key in searchValues.Keys)
            {
                if (i > 0)
                {
                    queryString += " AND ";
                }
                
                queryString += $"{key} = '{searchValues[key]}'";
                i++;
            }

            queryString += ";";

            return int.Parse(SearchQuery(queryString, filePath)[0]); 
        }
        
        public static List<object> FindMatching(Table table, Dictionary<string, string> columnValues = null, bool exact = true, string filePath = null)
        {
            if (filePath is null && !DatabaseLoaded())
            {
#if UNITY_EDITOR
                Debug.LogError("No database loaded.");
#endif
                return null; 
            }
            
            string queryString = $"SELECT * FROM {table.ToString().ToLower()}";

            if (columnValues is not null && columnValues.Count > 0)
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
                            queryString += $"= '{pair.Value}'";
                        }
                    }

                    i++;
                }
            }

            queryString += ";";
            
            return ObjectQuery(table, queryString, filePath);
        }
        
        public static List<string> FindValues(Table table, string columnToReturn = "*", Dictionary<string, List<string>> columnValues = null, bool exact = true, string filePath = null)
        {
            string queryString = $"SELECT {columnToReturn} FROM {table.ToString().ToLower()}";

            if (columnValues is null)
            {
                queryString += ";";
                return SearchQuery(queryString, filePath); 
            }

            queryString += " WHERE ";

            int i = 0;

            foreach (KeyValuePair<string, List<string>> pair in columnValues) //for every column to search 
            {
                if (i > 0) 
                {
                    queryString += " AND ";
                }

                queryString += pair.Key; //column name 

                if (pair.Value.Count > 1) //if more than one value to search for 
                {
                    queryString += " IN (";
                    
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
                else if (pair.Value.Count == 1)
                {
                    if (exact)
                    {
                        if (float.TryParse(pair.Value[0], out float f))
                        {
                            queryString += $" = {f}";
                        }
                        else
                        {
                            queryString += $" = '{pair.Value[0]}'";
                        }
                    }
                    else
                    {
                        queryString += $" LIKE '%{pair.Value[0]}%'";
                    }
                }
                
                i++;
            }
            
            queryString += ";";

            return SearchQuery(queryString, filePath);
        }

        #endregion

        #region Query Handlers

        /// <summary>
        /// Sends query you don't need anything back from
        /// </summary>
        private static void SendQuery(string query, string filePath)
        {
// #if UNITY_EDITOR
//             Debug.Log(query);
// #endif

            if (filePath is null)
            {
                filePath = _loadedDatabasePath;
            }

            SQLiteConnection connection = GetConnection(filePath);
            connection.Open();
            
            SQLiteCommand command = connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = query;
            command.ExecuteNonQuery();
            
            connection.Close();
        }

        /// <summary>
        /// Send query you just want the text values back from
        /// </summary>
        private static List<string> SearchQuery(string query, string filePath)
        {
// #if UNITY_EDITOR
//             Debug.Log(query);
// #endif
            
            if (filePath is null)
            {
                filePath = _loadedDatabasePath;
            }
            
            List<string> results = new List<string>();
            
            SQLiteConnection connection = GetConnection(filePath);
            connection.Open();
            SQLiteCommand command = connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = query;
            SQLiteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                string toAdd = "";
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    toAdd += reader[i].ToString();

                    if (i > 1)
                    {
                        toAdd += ",";
                    }
                }
                results.Add(toAdd);
            }
            
            connection.Close();
            return results;
        }
        
        /// <summary>
        /// Send queries you want to get the objects back from. 
        /// </summary>
        private static List<object> ObjectQuery(Table table, string queryString, string filePath)
        {
// #if UNITY_EDITOR
//             Debug.Log(queryString);
// #endif

            if (filePath is null)
            {
                filePath = _loadedDatabasePath;
            }
            
            //connect and send query 
            SQLiteConnection connection = GetConnection(filePath);
            connection.Open();
            SQLiteCommand command = connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = queryString;
            SQLiteDataReader reader = command.ExecuteReader();
            List<object> results = new List<object>();

            if (reader.FieldCount.Equals(0))
            {
                return results;
            }

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
                            weight = int.Parse(reader["weight"].ToString()),
                        };
                        
                        results.Add(weightAdd);
                        break;
                    
                    case Table.Users:
                        User userAdd = new User()
                        {
                            id = int.Parse(reader["id"].ToString()),
                            username = reader["username"].ToString(),
                            password = reader["password"].ToString(),
                        };
                        
                        results.Add(userAdd);
                        break; 
                    
                    case Table.Results:
                        Result resultsAdd = new Result()
                        {
                            quizID = int.Parse(reader["quiz_id"].ToString()),
                            userID = int.Parse(reader["user_id"].ToString()),
                            score = int.Parse(reader["score"].ToString()),
                            date = reader["date"].ToString(),
                            submissionSet = JsonUtility.FromJson<SubmissionSet>(reader["answers"].ToString())
                        };
                        results.Add(resultsAdd);
                        break; 
                }
            }
            
            connection.Close();
            
            return results;
        }

        private static int ResultTotal(string queryString, string filePath)
        {
            if (filePath is null)
            {
                filePath = _loadedDatabasePath;
            }
            
            SQLiteConnection connection = GetConnection(filePath);
            connection.Open();
            SQLiteCommand command = connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = queryString;
            SQLiteDataReader reader = command.ExecuteReader();

            int results = 0;
            while (reader.Read())
            {
                results++;
            }
                
                
            connection.Close();
            
            return results;
        }

        #endregion
        
    }
}
