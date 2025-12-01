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

        private string _searchBar;
        private List<Question> _questionDisplay = new List<Question>(); 
        
        private Quiz _newQuiz;
        private Dictionary<Question, int> _newQuestions = new Dictionary<Question, int>(); //question and the weighting
        
        [MenuItem("Window/Quizly/Create Quiz")]
        public static void Create()
        {
            CreateQuiz win = GetWindow<CreateQuiz>();
        }

        private void OnGUI()
        {
            GUILayout.Label("New Quiz");
            
            //quiz name 
            GUILayout.BeginHorizontal();
            
            GUILayout.Label("Quiz Name");

            if (_newQuiz is null)
            {
                _newQuiz = new Quiz();
                _newQuiz.quizName = "";
                _newQuiz.questionSet = new QuestionSet();
                _newQuestions = new Dictionary<Question, int>();
            }
            
            _newQuiz.quizName = GUILayout.TextField(_newQuiz.quizName);
            
            GUILayout.EndHorizontal();
            
            //timer
            GUILayout.BeginHorizontal();
            
            GUILayout.Label("Timer");
            _timed = EditorGUILayout.Toggle(_timed);

            if (_timed)
            {
                GUILayout.Label("Minutes");
                _newQuiz.time = EditorGUILayout.FloatField(_newQuiz.time);
            }
            
            GUILayout.EndHorizontal();
            
            //adding questions 
            GUILayout.Label("Add Questions");

            if (_questionDisplay.Count == 0 || _questionDisplay is null)
            {
                _updateDisplay = true;
            }

            //search bar 
            GUILayout.BeginHorizontal();
            GUILayout.Label("Search: ");
            _searchBar = GUILayout.TextField(_searchBar);
            if (GUILayout.Button("Search"))
            {
                _updateDisplay = true;
                GUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.EndHorizontal();
            }

            //show all questions in the display list
            //list them with an add button 
            if (_updateDisplay)
            {
                UpdateQuestionDisplay();    
            }
            
            foreach (Question question in _questionDisplay)
            {
                GUILayout.BeginHorizontal();
                
                GUILayout.Label(question.question);

                //add to quiz question set if button clicked
                if (GUILayout.Button("+"))
                {
                    _newQuestions.Add(question, 0);
                    _updateDisplay = true;
                    GUILayout.EndHorizontal();
                }
                else
                {
                    GUILayout.EndHorizontal();
                }
            }
            
            if (GUILayout.Button("New Question"))
            {
                CreateQuestion.Create();
            }
                
            //Display the current questions
            //headers
            GUILayout.BeginHorizontal();
            GUILayout.Label("Question");
            GUILayout.Label("Difficulty");
            GUILayout.Label("Weight");
            GUILayout.EndHorizontal();
            
            //display questions that have been added to quiz 
            List<Question> toIterate = new List<Question>();
            foreach (Question q in _newQuestions.Keys)
            {
                toIterate.Add(q);
            }
            foreach (Question question in toIterate)
            {
                GUILayout.BeginHorizontal();
                
                GUILayout.Label(question.question);
                GUILayout.Label(question.difficulty.ToString());
                
                _newQuestions[question] = EditorGUILayout.IntField(_newQuestions[question]);

                if (GUILayout.Button("-"))
                {
                    _newQuestions.Remove(question);
                    GUILayout.EndHorizontal();
                }
                else
                {
                    GUILayout.EndHorizontal();
                }
            }

            if (GUILayout.Button("Save Quiz"))
            {
                SaveQuiz();
            }
            
            GUILayout.Label(_errorMessage);

        }

        private void UpdateQuestionDisplay()
        {
            _questionDisplay.Clear();
            
            if (string.IsNullOrEmpty(_searchBar))
            {
                foreach (object o in DatabaseManager.FindMatching(DatabaseManager.Table.Questions))
                {
                    _questionDisplay.Add((Question)o);
                }
            }
            else
            {
                Dictionary<string, string> displaySearch = new Dictionary<string, string>();
                displaySearch.Add("question_text", _searchBar);

                foreach (object o in DatabaseManager.FindMatching(DatabaseManager.Table.Questions, displaySearch, false))
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
            
            DatabaseManager.SaveQuery(_newQuiz);
            
            //save weightings 
            //quiz now saved, get id 
            int id = DatabaseManager.GetID(DatabaseManager.Table.Quizzes, "name", _newQuiz.quizName);
            
            QuestionWeighting weight = new QuestionWeighting();
            weight.quizID = id;

            foreach (KeyValuePair<Question, int> question in _newQuestions)
            {
                weight.questionID = question.Key.id;
                weight.weight = question.Value;
                
                DatabaseManager.SaveQuery(weight);
            }

            _newQuiz = null;
        }
    }
}
