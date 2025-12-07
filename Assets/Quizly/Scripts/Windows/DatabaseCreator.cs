using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Quizly
{
    public class DatabaseCreator : EditorWindow
    {
        private string _newDBName = "";
        private List<string> _allDBs = new List<string>();

        private string _errorMessage;

        private Vector2 _scrollPos, _listPos;
        
        [MenuItem("Window/Quizly/Database Hub", false, 2)]
        public static void Create()
        {
            DatabaseCreator win = GetWindow<DatabaseCreator>();
            
            win.titleContent = new GUIContent("Database Hub");
        }

        private void OnGUI()
        {
            GUILayout.Label("Database Hub", StyleLibrary.Header2Style);

            #region Database Creation

            GUILayout.Label("Create Database", StyleLibrary.Header3Style);
            
            //NEW DATABASE NAME FIELD
            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            GUILayout.Space(15);
            GUILayout.Label("Name: ");
            GUILayout.Space(25);
            _newDBName = EditorGUILayout.TextField(_newDBName.Replace('_', ' '));
            GUILayout.Space(15);
            GUILayout.EndHorizontal();
            GUILayout.Space(10);
            
            //make name appropriate for file name
            _newDBName = _newDBName.Replace("/", "_");
            _newDBName = _newDBName.Replace(".", "_");
            _newDBName = _newDBName.Replace(" ", "_");
            _newDBName = _newDBName.ToLower();
            
            // CREATE DATABASE 
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Create", GUILayout.Height(25), GUILayout.Width(65)))
            {
                if (string.IsNullOrEmpty(_newDBName.Trim()))
                {
                    _errorMessage = "Name cannot be empty.";
                    GUILayout.EndHorizontal();
                    return; 
                }
                
                if (DBManager.TryCreateDatabase(_newDBName))
                {
                    _errorMessage = "Database created";
                    _newDBName = "";
                }
                else
                {
                    _errorMessage = "Could not create database";
                }
            }
            
            GUILayout.Space(5);
            GUILayout.EndHorizontal();

                #endregion

            #region Database List

            _allDBs = DBManager.GetDatabaseNames();

            if (_allDBs is not null && _allDBs.Count != 0)
            {
                GUILayout.Space(10);
                GUILayout.Label("All Databases", StyleLibrary.Header3Style);
                GUILayout.BeginHorizontal();
                GUILayout.Label("Name ", StyleLibrary.Bold);
                GUILayout.EndHorizontal();
                
                _listPos = EditorGUILayout.BeginScrollView(_listPos);
                
                foreach (string db in _allDBs)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(15);
                    GUILayout.Label(db.Split('.')[0].Replace('_',' '));
                    
                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button("Load", GUILayout.Width(55)))
                    {
                        DBManager.LoadDatabase(db);
                        _errorMessage = $"Loaded: {db.Remove(db.IndexOf('.'))}";
                        GUILayout.EndHorizontal();
                    }
                    else if (GUILayout.Button("Clear", GUILayout.Width(55)))
                    {
                        DBManager.ClearDatabase(db);
                        _errorMessage = $"Cleared: {db.Remove(db.IndexOf('.'))}";
                        GUILayout.EndHorizontal();
                    }
                    else
                    {
                        GUILayout.Space(10);
                        GUILayout.EndHorizontal();
                    }
                    
                    GUILayout.Space(15);
                }
                
                EditorGUILayout.EndScrollView();
            }
            else
            {
                GUILayout.Label("No databases found.", StyleLibrary.WarningStyle);
            }

            #endregion
            
            GUILayout.Label(_errorMessage, StyleLibrary.BottomMessage);
        }
    }
}
