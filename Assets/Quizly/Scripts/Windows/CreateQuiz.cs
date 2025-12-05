using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace Quizly
{
    public class CreateQuiz : EditorWindow
    {
        private string _errorMessage; 
        private bool _timed;
        private bool _updateDisplay;

        private Vector2 _scrollPos, _searchScroll, _questionScroll;

        private string _searchBar;
        private List<Question> _questionDisplay = new List<Question>(); 
        
        private Quiz _newQuiz;
        private Dictionary<Question, int> _newQuestions = new Dictionary<Question, int>(); //question and the weighting
        
        [MenuItem("Window/Quizly/Create Quiz")]
        public static void Create()
        {
            CreateQuiz win = GetWindow<CreateQuiz>();
            
            win.titleContent = new GUIContent("Create Quiz");
        }

        private void OnGUI()
        {
            GUILayout.Label("New Quiz", StyleLibrary.Header2Style);
            
            _scrollPos = GUILayout.BeginScrollView(_scrollPos);
            GUILayout.Label("Details", StyleLibrary.Header3Style);
            //quiz name 
            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            GUILayout.Label("Quiz Name");
            if (_newQuiz is null)
            {
                _newQuiz = new Quiz();
                _newQuiz.quizName = "";
                _newQuiz.questionSet = new QuestionSet();
                _newQuestions = new Dictionary<Question, int>();
            }
            _newQuiz.quizName = GUILayout.TextField(_newQuiz.quizName);
            GUILayout.Space(10);
            GUILayout.EndHorizontal();
            
            //timer
            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            GUILayout.Label("Timer");
            _timed = EditorGUILayout.Toggle(_timed);

            if (_timed)
            {
                GUILayout.Label("Minutes");
                _newQuiz.time = EditorGUILayout.FloatField(_newQuiz.time, GUILayout.Width(60));
            }
            GUILayout.Space(10);
            GUILayout.EndHorizontal();
            
            GUILayout.Label("Questions", StyleLibrary.Header3Style);
            //adding questions 
            
            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            GUILayout.Label("Add Questions", StyleLibrary.Header4Style);
            
            if (GUILayout.Button("Create New", GUILayout.Width(100)))
            {
                CreateQuestion.Create();
            }
            GUILayout.Space(10);
            GUILayout.EndHorizontal();
            GUILayout.Space(5);

            if (_questionDisplay.Count == 0 || _questionDisplay is null)
            {
                _updateDisplay = true;
            }

            //search bar 
            GUILayout.BeginHorizontal();
            GUILayout.Space(25);
            GUILayout.Label("Search: ");
            _searchBar = GUILayout.TextField(_searchBar);
            GUILayout.Space(5);
            if (GUILayout.Button("Search", GUILayout.Width(85)))
            {
                _updateDisplay = true;
                GUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.Space(10);
                GUILayout.EndHorizontal();
            }
            
            GUILayout.Space(15);

            //show all questions in the display list
            //list them with an add button 
            if (_updateDisplay)
            {
                UpdateQuestionDisplay();    
            }

            if (_questionDisplay is not null && _questionDisplay.Count > 0)
            {
                _searchScroll = GUILayout.BeginScrollView(_searchScroll, GUILayout.Height(75));
                foreach (Question question in _questionDisplay)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(35);
                
                    GUILayout.Label(question.question);

                    //add to quiz question set if button clicked
                    if (GUILayout.Button("+", GUILayout.Width(20)))
                    {
                        _newQuestions.Add(question, 0);
                        _updateDisplay = true;
                        GUILayout.EndHorizontal();
                    }
                    else
                    {
                        GUILayout.Space(35);
                        GUILayout.EndHorizontal();
                    }
                    
                    GUILayout.Space(5);
                }
                GUILayout.EndScrollView();
            }
                
            //Display the current questions
            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            GUILayout.Label("Added Questions", StyleLibrary.Header4Style);
            GUILayout.EndHorizontal();
            //headers
            GUILayout.BeginHorizontal();
            GUILayout.Space(25);
            GUILayout.Label("Question", StyleLibrary.Bold);
            GUILayout.Label("Difficulty", StyleLibrary.Bold);
            GUILayout.Label("Weight", StyleLibrary.Bold);
            GUILayout.EndHorizontal();
            
            //display questions that have been added to quiz 
            List<Question> toIterate = new List<Question>();
            foreach (Question q in _newQuestions.Keys)
            {
                toIterate.Add(q);
            }

            if (toIterate.Count > 0)
            {
                _questionScroll = GUILayout.BeginScrollView(_questionScroll, GUILayout.Height(85));
                foreach (Question question in toIterate)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(30);
                    GUILayout.Label(question.question);
                    GUILayout.Label(question.difficulty.ToString());
                
                    _newQuestions[question] = EditorGUILayout.IntField(_newQuestions[question], GUILayout.Width(65));

                    GUILayout.Space(15);
                    
                    if (GUILayout.Button("-", GUILayout.Width(20)))
                    {
                        _newQuestions.Remove(question);
                        GUILayout.EndHorizontal();
                    }
                    else
                    {
                        GUILayout.Space(35);
                        GUILayout.EndHorizontal();
                    }
                }
                
                GUILayout.EndScrollView();
            }
            
            GUILayout.Space(15);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Save Quiz", GUILayout.Width(position.width * .85f), GUILayout.Height(30)))
            {
                SaveQuiz();
                GUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
            
            GUILayout.EndScrollView();
            GUILayout.Label(_errorMessage, StyleLibrary.BottomMessage);

        }

        private void UpdateQuestionDisplay()
        {
            _questionDisplay.Clear();

            if (!DBManager.DatabaseLoaded())
            {
                _errorMessage = "No database loaded";
                return;
            }
            
            List<object> toDisplay = new List<object>();
            
            if (string.IsNullOrEmpty(_searchBar))
            {
                toDisplay = DBManager.FindMatching(DBManager.Table.Questions);
            }
            else
            {
                Dictionary<string, string> displaySearch = new Dictionary<string, string>();
                displaySearch.Add("question_text", _searchBar);
                
                toDisplay = DBManager.FindMatching(DBManager.Table.Questions, displaySearch, false);
            }

            if (toDisplay is not null && toDisplay.Count > 0)
            {
                foreach (object o in toDisplay)
                {
                    _questionDisplay.Add((Question)o);
                }
            }
            
            List<Question> toRemove = new List<Question>();

            foreach (Question q in _questionDisplay)
            {
                foreach (Question includedQs in _newQuestions.Keys)
                {
                    if (includedQs.id == q.id)
                    {
                        toRemove.Add(q);
                    }
                }
            }

            foreach (Question q in toRemove)
            {
                _questionDisplay.Remove(q);
            }

            _updateDisplay = false; 
        }

        private void SaveQuiz()
        {
            //check everything is filled out 
            if (string.IsNullOrEmpty(_newQuiz.quizName.Trim()) || _newQuestions.Count == 0 || _newQuestions is null)
            {
                _errorMessage = "Enter quiz name, and include at least one question";
                return; 
            }
            
            
            _newQuiz.questionSet.questionIDs = new List<int>();
            
            foreach (Question question in _newQuestions.Keys)
            {
                if (_newQuestions[question] == 0)
                {
                    _errorMessage = "Add weighting to all questions";
                    _newQuiz.questionSet.questionIDs = new List<int>();
                    return;
                }
                
                _newQuiz.questionSet.questionIDs.Add(question.id); //add id to question set 
            }
            
            DBManager.SaveObject(_newQuiz);
            
            //save weightings 
            //quiz now saved, get id 
            int id = DBManager.FindID(DBManager.Table.Quizzes, "name", _newQuiz.quizName);
            
            Debug.Log($"New quiz questions {_newQuestions.Count}");

            foreach (KeyValuePair<Question, int> question in _newQuestions)
            {
                QuestionWeighting weight = new QuestionWeighting();
                weight.quizID = id;
                weight.questionID = question.Key.id;
                weight.weight = question.Value;
                
                DBManager.SaveObject(weight);
            }

            _newQuiz = null;
        }
    }
}
