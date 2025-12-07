using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using UnityEngine;

namespace Quizly
{
    public static class CSVExporter
    {
        private static readonly Dictionary<DBManager.Table, string> TableHeaders = new Dictionary<DBManager.Table, string>()
        {
            {DBManager.Table.Questions, "ID,Question,Difficulty,Category,CorrectAnswers,WrongAnswers,Tip"},
            {DBManager.Table.Categories, "ID,Name"},
            {DBManager.Table.Users, "ID,Username,Password"},
            {DBManager.Table.Quizzes, "ID,Name,Time,Questions,Average"},
            {DBManager.Table.Results, "ID,Quiz,User,Date,Score,Answers"}
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
                
                WriteCsv(fileName, new List<string>(){headers});
                
            }
        
        }

        public static void ExportData(DBManager.Table table, List<object> toExport,  string savePath)
        {
            List<string> exportData = new List<string>()
            {
                TableHeaders[table],
            };

            string toAdd;

            if (toExport is null)
            {
                return; 
            }

            foreach (object o in toExport)
            {
                switch (o)
                {
                    case string category:
                        toAdd = DBManager.FindID(DBManager.Table.Categories, "name", category).ToString();
                        toAdd += string.Concat(",", category);
                        break;
                    case Question question:
                        toAdd = $"{question.id.ToString()},{question.question},{question.difficulty.ToString()},";
                        //category 
                        toAdd += DBManager.FindValueWithID(DBManager.Table.Categories, "name", question.categoryID) + ",";
                        //correct answers 
                        foreach (string answer in question.answerSet.correctAnswers)
                        {
                            toAdd += answer + ";";
                        }
                        toAdd += ",";
                        //wrong answers 
                        foreach (string answer in question.answerSet.wrongAnswers)
                        {
                            toAdd += answer + ";";
                        }
                        toAdd += ",";
                        //tip 
                        toAdd += question.tip;
                        break;
                    case User user:
                        toAdd = $"{user.id.ToString()},{user.username},";
                        toAdd += Convert.ToBase64String(Encoding.UTF8.GetBytes(user.password));
                        break;
                    case Result result:
                        //id
                        toAdd = DBManager.FindID(DBManager.Table.Results,
                            new Dictionary<string, string>()
                                { { "quiz_id", result.quizID.ToString() }, { "user_id", result.userID.ToString() } }) + ",";
                        //quiz
                        toAdd += DBManager.FindValues(DBManager.Table.Quizzes, "name", 
                            new Dictionary<string, List<string>>(){{"id", new List<string>(){result.quizID.ToString()}}})[0] + ",";
                        //user
                        toAdd += DBManager.FindValues(DBManager.Table.Users, "username", 
                            new Dictionary<string, List<string>>(){{"id", new List<string>(){result.userID.ToString()}}})[0] + ",";
                        //date and score 
                        toAdd += $"{result.date},{result.score},";
                        //answers
                        foreach (AnswerSubmission set in result.submissionSet.submissions)
                        {
                            foreach (string answer in set.answers)
                            {
                                toAdd += $"{answer} ";
                            }

                            if (set.isCorrect)
                            {
                                toAdd += "CORRECT + ";
                                toAdd += DBManager.FindValues(DBManager.Table.Weightings, "weight", 
                                    new Dictionary<string, List<string>>(){
                                            {"quiz_id", new List<string>(){result.quizID.ToString()}}, 
                                            {"question_id", new List<string>{set.questionID.ToString()}}
                                        })[0];
                            }
                            else
                            {
                                toAdd += "INCORRECT + 0";
                            }
                        }
                        break; 
                    case Quiz quiz:
                        toAdd = $"{quiz.id.ToString()},{quiz.quizName},{quiz.time},";
                        //question total 
                        toAdd += $"{quiz.questionSet.questionIDs.Count},";
                        //average score 
                        List<object> resultObjects = DBManager.FindMatching(DBManager.Table.Results, 
                            new Dictionary<string, string>() { {"quiz_id", quiz.id.ToString()}});

                        if (resultObjects is not null && resultObjects.Count > 0)
                        {
                            int totalScore = 0;
                            int scoreCount = 0; 
                            
                            foreach (object x in resultObjects)
                            {
                                Result result = (Result)x;
                                scoreCount++;
                            
                                totalScore += result.score;
                            }

                            toAdd += totalScore / scoreCount;
                            toAdd += $"{totalScore / scoreCount}/{QuizManager.QuizMaxScore(quiz.id)}";
                        }
                        
                        break; 
                    default:
                        #if UNITY_EDITOR
                        Debug.Log("Data passed not able to be exported");
                        #endif

                        toAdd = "";
                        break; 
                }

                exportData.Add(toAdd);
            }
            
            WriteCsv(savePath, exportData);
        }

        private static void WriteCsv(string savePath, List<string> contents)
        {
            #if UNITY_EDITOR
            Debug.Log("Writing CSV to " + savePath);
            #endif
            
            if (!File.Exists(savePath))
            {
#if UNITY_EDITOR
                Debug.Log("Creating file at: " + savePath);
#endif
                File.Create(savePath).Dispose();
            }

            StreamWriter writer = new StreamWriter(savePath);

            for (int i = 0; i < contents.Count; i++)
            {
                writer.WriteLine(contents[i]);
            }
            
            writer.Close();
            
        }
    }
}
