using System;
using Quizly;
using UnityEditor;
using UnityEngine;

public class MainWindow : EditorWindow
{
    private Vector2 scrollPos;

    private GUILayoutOption[] _windowButtonOptions;
    
    [MenuItem("Window/Quizly/Quizly Hub", false, 1)]
    public static void Create()
    {
        MainWindow win = GetWindow<MainWindow>();
        
        win.titleContent = new GUIContent("Quizly Hub");
    }

    private void OnGUI()
    {
        _windowButtonOptions = new GUILayoutOption[]
        {
            GUILayout.Width(position.width * .45f), //needs to update for the width 
            GUILayout.Height(25)
        };
        
        GUILayout.Label("Quizly", StyleLibrary.HeaderStyle);
        
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        #region Loaded DB Information

        if (DBManager.DatabaseLoaded())
        {
            string dbName = DBManager.DatabaseName();
            dbName = string.Concat(dbName.ToUpper()[0], dbName[1..]);
            
            GUILayout.Label("Open Database", StyleLibrary.Header2LeftStyle);
            
            GUILayout.BeginHorizontal();
            GUILayout.Space(45);
            GUILayout.Label("Database: ", StyleLibrary.Bold);
            GUILayout.Space(10);
            GUILayout.Label(dbName);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            GUILayout.Space(45);
            GUILayout.Label("File Path: ", StyleLibrary.Bold);
            GUILayout.Space(10);
            GUILayout.Label(DBManager.DatabaseFilePath());
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
        else
        {
            GUILayout.Label("No loaded database.", StyleLibrary.WarningStyle);
        }

        #endregion
        
        GUILayout.Space(15);
        
        //OPEN DATABASE BUTTON 
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Load Database", GUILayout.Height(30), GUILayout.Width(position.width * .65f)))
        {
            DatabaseCreator.Create();
            GUILayout.EndHorizontal();
        }
        else
        {
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
        
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Create Game Settings", GUILayout.Height(25), GUILayout.Width(position.width * .55f)))
        {
            CreateGameSettings.Create();
            GUILayout.EndHorizontal();
        }
        else
        {
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
        
        GUILayout.Space(15);

        #region Quiz Management Buttons

        GUILayout.Label("Data Management", StyleLibrary.Header2Style);
        
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("All Questions", _windowButtonOptions))
        {
            QuestionList.Create();
            GUILayout.EndHorizontal();
        }
        else if (GUILayout.Button("All Quizzes", _windowButtonOptions))
        {
            QuizList.Create();
            GUILayout.EndHorizontal();
        }
        else
        {
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
        
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Create Question", _windowButtonOptions))
        {
            CreateQuestion.Create();
            GUILayout.EndHorizontal();
        }
        else if (GUILayout.Button("Create Quiz", _windowButtonOptions))
        {
            CreateQuiz.Create();
            GUILayout.EndHorizontal();
        }
        else
        {
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
        
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        
        if (GUILayout.Button("Categories", _windowButtonOptions))
        {
            CategoryManager.Create();
            GUILayout.EndHorizontal();
        }
        else
        {
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        #endregion

        #region CSV Management

        GUILayout.Label("CSV Management", StyleLibrary.Header2Style);
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Import CSVs", _windowButtonOptions))
        {
            ImporterWindow.Create();
            GUILayout.EndHorizontal();
        }
        else if (GUILayout.Button("Export CSVs", _windowButtonOptions))
        {
            ExporterWindow.Create(); 
            GUILayout.EndHorizontal();
        }
        else
        {
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        #endregion
        
        GUILayout.EndScrollView();
    }
}
