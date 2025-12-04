using System;
using Quizly;
using UnityEditor;
using UnityEngine;

public class MainWindow : EditorWindow
{
    [MenuItem("Window/Quizly/Quizly Hub")]
    public static void Create()
    {
        MainWindow win = GetWindow<MainWindow>();
        
        win.titleContent = new GUIContent("Quizly Hub");
    }

    private void OnGUI()
    {
        GUILayout.Label("Quizly");
        
        GUILayout.Label("Loaded Database");

        if (DBManager.DatabaseLoaded())
        {
            GUILayout.Label("Database is open " + DBManager.DatabaseName());
        }
        else
        {
            GUILayout.Label("Database is not open");
        }

        if (GUILayout.Button("Open Database"))
        {
            DatabaseCreator.Create();
        }
        
        GUILayout.Label("Features");
        
        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Create Question"))
        {
            CreateQuestion.Create();
            GUILayout.EndHorizontal();
        }
        else if (GUILayout.Button("Create Category"))
        {
            CategoryManager.Create();
            GUILayout.EndHorizontal();
        }
        else
        {
            GUILayout.EndHorizontal();
        }
    }
}
