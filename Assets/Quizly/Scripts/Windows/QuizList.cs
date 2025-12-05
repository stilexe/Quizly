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
        
        [MenuItem("Window/Quizly/View Quizzes")]
        public static void Create()
        {
            QuizList win = GetWindow<QuizList>();
        }

        private void OnGUI()
        {
            GUILayout.Label("Quiz List");
            
            //get all quizzes 

            if (_quizDisplay is null)
            {
                RefreshDisplay();    
            }
            
            //search bar 
            
            GUILayout.BeginHorizontal();
            
            GUILayout.Label("Search: ");
            _searchBar = GUILayout.TextField(_searchBar);

            if (GUILayout.Button("Search"))
            {
                RefreshDisplay();
                GUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.EndHorizontal();
            }

            _scrollPos = GUILayout.BeginScrollView(_scrollPos);
            
            //show each quiz in a list
            foreach (KeyValuePair<int, string> pair in _quizDisplay)
            {
                GUILayout.BeginHorizontal();
                
                GUILayout.Label(pair.Value);

                if (GUILayout.Button("Edit"))
                {
                    EditQuiz.Create(pair.Key);
                    GUILayout.EndHorizontal();
                }
                else if (GUILayout.Button("-"))
                {
                    DBManager.RemoveData(DBManager.Table.Quizzes, pair.Key);
                    RefreshDisplay();
                    GUILayout.EndHorizontal();
                }
                else
                {
                    GUILayout.EndHorizontal();
                }
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
            
        }
    }
}
