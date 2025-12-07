using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Quizly
{
    public class QuizList : EditorWindow
    {
        private Dictionary<int, string> _quizDisplay;
        private string _searchBar;
        private Vector2 _scrollPos;
        private string _errorMessage; 
        
        [MenuItem("Window/Quizly/List/Quizzes")]
        public static void Create()
        {
            QuizList win = GetWindow<QuizList>();
            
            win.titleContent = new GUIContent("Quizzes");
        }

        private void OnGUI()
        {
            GUILayout.Label("Quiz List", StyleLibrary.Header2LeftStyle);
            
            //get all quizzes 

            if (_quizDisplay is null)
            {
                RefreshDisplay();    
            }
            
            //search bar 
            
            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            GUILayout.Label("Search: ");
            _searchBar = GUILayout.TextField(_searchBar);
            GUILayout.Space(5);
            if (GUILayout.Button("Search", GUILayout.Width(65)))
            {
                RefreshDisplay();
                GUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.Space(10);
                GUILayout.EndHorizontal();
            }
            
            GUILayout.Space(15);
            
            _scrollPos = GUILayout.BeginScrollView(_scrollPos); 

            if (_quizDisplay is not null && _quizDisplay.Count > 0)
            {
            
                //show each quiz in a list
                foreach (KeyValuePair<int, string> pair in _quizDisplay)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(20);
                    GUILayout.Label(string.Concat(pair.Value.ToUpper()[0], pair.Value[1..]));

                    if (GUILayout.Button("-", GUILayout.Width(20)))
                    {
                        DBManager.RemoveData(DBManager.Table.Quizzes, pair.Key);
                        RefreshDisplay();
                        GUILayout.EndHorizontal();
                    }
                    else
                    {
                        GUILayout.Space(20);
                        GUILayout.EndHorizontal();
                    }
                }
            }
            else if (_quizDisplay is null)
            {
                _errorMessage = "No database loaded.";
            }
            else
            {
                _errorMessage = "No quizzes in database.";
            }
            
            GUILayout.EndScrollView();
            
            GUILayout.Label(_errorMessage, StyleLibrary.BottomMessage);
        }

        private void RefreshDisplay()
        {
            if (!DBManager.DatabaseLoaded())
            {
                _errorMessage = "No database loaded";
                return; 
            }
            
            _quizDisplay = new Dictionary<int, string>();
            
            List<string> quizIDs = new List<string>();

            //get ids of all quizzes to display 
            if (string.IsNullOrEmpty(_searchBar))
            {
                foreach (string id in DBManager.FindValues(DBManager.Table.Quizzes, "id")) //every quiz id
                {
                    quizIDs.Add(id);
                }
            }
            else
            {
                Dictionary<string, List<string>> dict = new Dictionary<string, List<string>>()
                {
                    {"name", new List<string>() {_searchBar}}
                };
                
                foreach (string id in DBManager.FindValues(DBManager.Table.Quizzes, "id", dict, false)) //every quiz id that matches the search bar
                {
                    quizIDs.Add(id);
                }
            }

            string quizName; 
            foreach (string id in quizIDs)
            {
                //get quiz name 
                quizName = DBManager.FindValues(DBManager.Table.Quizzes, "name", new Dictionary<string,List<string>>() {{"id", new List<string>(){id}}})[0]; 
                
                _quizDisplay.Add(int.Parse(id), quizName);
            }

            _errorMessage = "";

        }
    }
}
