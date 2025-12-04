using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Quizly
{
    public static class CSVImporter
    {
        private static readonly Dictionary<DBManager.Table, string> TableHeaders = new Dictionary<DBManager.Table, string>()
        {
            {DBManager.Table.Questions, "Question,Difficulty,Category,CorrectAnswers,WrongAnswers,Tip"},
            {DBManager.Table.Categories, "Name"},
            {DBManager.Table.Users, "Username,Password"}
        };
        
        public static void ImportCsv(string filePath)
        {
         
#if UNITY_EDITOR
            Debug.Log("Importing CSV from " + filePath);
#endif
            
            //get all lines in csv 
            
            StreamReader reader = new StreamReader(filePath);
            List<string> lineStrings = new List<string>();

            while (!reader.EndOfStream)
            {
                lineStrings.Add(reader.ReadLine());
            }
            
            reader.Close();
            
            string debugString = "";
            for (int i = 0; i < lineStrings.Count; i++)
            {
                debugString += lineStrings[i] + "\n";
            }
            Debug.Log(debugString);
            
            //if no more than one line return because there is only the headers 

            if (lineStrings.Count == 1)
            {
#if UNITY_EDITOR
                Debug.Log("No data found in file");
#endif
                return; 
            }
            
            //read first line and turn each word into a list 
            string[] headers = lineStrings[0].Split(',');
            lineStrings.RemoveAt(0); //take headers out of line strings 

            switch (MatchTable(headers)) //depending on what table it is sending it to the database is different 
            {
                case DBManager.Table.Questions:
                    //headers = "0Question,1Difficulty,2Category,3CorrectAnswers,4WrongAnswers,5Tip";
#if UNITY_EDITOR
                    Debug.Log("Importing Questions");
#endif
                    foreach (string line in lineStrings)
                    {
                        string[] lineArray = line.Split(',');
                        
                        Question toSave = new Question()
                        {
                            question = lineArray[0],
                            difficulty = int.Parse(lineArray[1]),
                            categoryID = DBManager.FindID(DBManager.Table.Categories, "name", lineArray[2]),
                            answerSet = new AnswerSet()
                            {
                              correctAnswers  = new List<string>(),
                              wrongAnswers = new List<string>(),
                            },
                            tip = lineArray[5],
                        };

                        foreach (string answer in lineArray[3].Split(';')) //correct answers 
                        {
                            toSave.answerSet.correctAnswers.Add(answer.Trim());
                        }

                        foreach (string answer in lineArray[4].Split(';')) //wrong answers 
                        {
                            toSave.answerSet.wrongAnswers.Add(answer.Trim());
                        }
                        
                        DBManager.SaveObject(toSave);
                    }
                    
                    break;
                
                case DBManager.Table.Categories:
#if UNITY_EDITOR
                    Debug.Log("Importing Categories");
#endif
                    foreach (string line in lineStrings)
                    {
                        DBManager.SaveObject(line);
                    }
                    break;
                
                case DBManager.Table.Users:
                    //headers = "Username,Password";
#if UNITY_EDITOR
                    Debug.Log("Importing Users");
#endif
                    foreach (string line in lineStrings)
                    {
                        string[] lineArray = line.Split(',');
                        UserManager.CreateUser(lineArray[0], lineArray[1]);
                    }
                    
                    break;
                default:
#if UNITY_EDITOR
                    Debug.Log("Not a recognised table.");
#endif
                    return;
            }
        }

        private static DBManager.Table MatchTable(string[] headers)
        {
            int matches = 0;
            
            foreach (DBManager.Table table in TableHeaders.Keys)
            {
                string[] headerArray = TableHeaders[table].Split(',');

                if (headerArray.Length != headers.Length)
                {
                    continue;
                }
                
                for (int i = 0; i < headers.Length; i++)
                {
                    if (headers[i] == headerArray[i])
                    {
                        matches++;
                    }
                }

                if (matches == headers.Length)
                {
                    return table; 
                }
                
                matches = 0; 
            }

            return DBManager.Table.None;
        }
    }
}
