using System.Collections.Generic;
using System.Data;
using System.IO;
using UnityEngine;

namespace Quizly
{
    public static class CSVExporter
    {
        private static readonly Dictionary<DBManager.Table, string> TableHeaders = new Dictionary<DBManager.Table, string>()
        {
            {DBManager.Table.Questions, "ID, Question,Difficulty,Category,CorrectAnswers,WrongAnswers,Tip"},
            {DBManager.Table.Categories, "ID, Name"},
            {DBManager.Table.Users, "ID, Username,Password"},
            {DBManager.Table.Quizzes, "ID,name,time,questions"},
            {DBManager.Table.Results, "ID, Quiz,User,Date,Score,Answers"}
        };
        
        private static string _templateFilePath = "";
        private static string _dataFilePath = "";
        private static List<DBManager.Table> _templateTables = new List<DBManager.Table>(){DBManager.Table.Questions, DBManager.Table.Categories, DBManager.Table.Users};

        private static void SetExportFilePath()
        {
            _templateFilePath = Application.streamingAssetsPath + "/Templates/";
            _dataFilePath = Application.streamingAssetsPath + "/Data/";
        }

        public static void ExportTemplates()
        {

            if (string.IsNullOrEmpty(_templateFilePath))
            {
                SetExportFilePath();
            }
            
#if UNITY_EDITOR
            Debug.Log("Exporting template CSV files to " + _templateFilePath);
#endif

            //check folder exists and create it if not 
            if (!Directory.Exists(_templateFilePath))
            {
                Directory.CreateDirectory(_templateFilePath);
            }

            string fileName;
            string headers = "";
            //go through all tables that can be exported as a template 
            foreach (DBManager.Table table in _templateTables) 
            {
                fileName = _templateFilePath + table.ToString().ToLower() + "_template.csv";
                
                switch (table)
                {
                    case DBManager.Table.Questions:
                        headers = "Question,Difficulty,Category,CorrectAnswers,WrongAnswers,Tip";
                        break;
                    case DBManager.Table.Categories:
                        headers = "Name";
                        break;
                    case DBManager.Table.Users:
                        headers = "Username,Password";
                        break;
                }
                
                WriteCSV(fileName, new List<string>(){headers});
                
            }
        
        }

        /// <summary>
        /// Export a CSV file with data from the database 
        /// </summary>
        public static void ExportQuery(DBManager.Table table, string fileName)
        {
            List<string> linesToWrite = new List<string>();

            switch (table)
            {
                case DBManager.Table.Questions: 
                    break;
                case DBManager.Table.Categories:
                    break;
                case DBManager.Table.Users:
                    break;
            }
            
        }

        private static void WriteCSV(string fileName, List<string> contents)
        {
            #if UNITY_EDITOR
            Debug.Log("Writing CSV to " + fileName);
            #endif
            
            if (!File.Exists(fileName))
            {
#if UNITY_EDITOR
                Debug.Log("Creating file at: " + fileName);
#endif
                File.Create(fileName).Dispose();
            }

            StreamWriter writer = new StreamWriter(fileName);

            for (int i = 0; i < contents.Count; i++)
            {
                writer.WriteLine(contents[i]);
            }
            
            writer.Close();
            
        }
    }
}
