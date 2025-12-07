using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Quizly
{
    public class QuestionList : EditorWindow
    {
        private List<Question> _questionDisplay;
        private string _searchBar;
        private string _searchCategory;
        private string _errorMessage = "";

        private Vector2 _scrollPos;
        
        private Question _selectedQuestion;
        
        [MenuItem("Window/Quizly/List/Questions")]
        public static void Create()
        {
            QuestionList win = GetWindow<QuestionList>();
            
            win.titleContent = new GUIContent("Question List");
        }

        private void OnGUI()
        {
            GUILayout.Label("Questions", StyleLibrary.Header2LeftStyle);

            #region Search

            GUILayout.BeginHorizontal();
            GUILayout.Space(5);
            GUILayout.Label("Search: ", StyleLibrary.Header4Style);
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            GUILayout.Label("Question: ");
            _searchBar = GUILayout.TextArea(_searchBar);
            GUILayout.Space(10);
            GUILayout.EndHorizontal();
            
            GUILayout.Space(5);
            
            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            GUILayout.Label("Category: ");
            //category drop down 
            if (EditorGUILayout.DropdownButton(new GUIContent(_searchCategory), FocusType.Keyboard) && DBManager.DatabaseLoaded())
            {
                GenericMenu menu = new GenericMenu();
                
                menu.AddItem(new GUIContent("None"), false, ChangeCategory, "");

                foreach (object o in DBManager.FindMatching(DBManager.Table.Categories))
                {
                    menu.AddItem(new GUIContent((string)o), false, ChangeCategory, ((string)o));
                }
                
                menu.ShowAsContext();
            }
            else if(!DBManager.DatabaseLoaded())
            {
                _errorMessage = "No database loaded";
            }
            GUILayout.Space(10);
            GUILayout.EndHorizontal();
            
            GUILayout.Space(10);
            
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Search", GUILayout.Width(65), GUILayout.Height(25)))
            {
                UpdateQuestionDisplay();
                GUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.Space(10);
                GUILayout.EndHorizontal();
            }
            GUILayout.Space(10);

            #endregion

            #region Questions

            if (_questionDisplay == null) 
            {
                UpdateQuestionDisplay();
            } //might still be null after just need to make sure it's not from lack of updating
            
            //all questions 
            
            _scrollPos = GUILayout.BeginScrollView(_scrollPos);

            if (_questionDisplay is not null)
            {
                foreach (Question question in _questionDisplay)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(25);
                    GUILayout.Label(question.question);

                    if (GUILayout.Button("Expand", GUILayout.Width(55)))
                    {
                        _selectedQuestion = question;
                        GUILayout.EndHorizontal();
                    }
                    else if (GUILayout.Button("-", GUILayout.Width(20)))
                    {
                        DBManager.RemoveData(DBManager.Table.Questions, question.id);
                        UpdateQuestionDisplay();
                        GUILayout.EndHorizontal();
                    }
                    else
                    {
                        GUILayout.Space(15);
                        GUILayout.EndHorizontal();
                    }
                }
            }
            
            GUILayout.EndScrollView();

            #endregion

            GUILayout.FlexibleSpace();
            
            #region Selected

            if (_selectedQuestion is not null)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(5);
                GUILayout.Label("Details", StyleLibrary.Header4Style);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("-", GUILayout.Width(15)))
                {
                    _selectedQuestion = null;
                    GUILayout.EndHorizontal();
                }
                else
                {
                    GUILayout.Space(10);
                    GUILayout.EndHorizontal();
                
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(15);
                    GUILayout.Label(_selectedQuestion.question, StyleLibrary.Bold);
                    GUILayout.EndHorizontal();
                    
                    GUILayout.Space(5);
                    
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(20);
                    GUILayout.Label("Difficulty:");

                    for (int i = 0; i < _selectedQuestion.difficulty; i++)
                    {
                        GUILayout.Label("*", GUILayout.ExpandWidth(false));
                    }
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                    
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(20);
                    GUILayout.Label("Category: "); 
                    GUILayout.Label((string)DBManager.SearchWithID(DBManager.Table.Categories, _selectedQuestion.categoryID));
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                    
                    GUILayout.Space(5);
                    
                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    
                    GUILayout.BeginVertical();
                    GUILayout.Label("Correct answers");
                    foreach (string answer in _selectedQuestion.answerSet.correctAnswers)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(15);
                        GUILayout.Label("O");
                        GUILayout.Label(answer);
                        GUILayout.EndHorizontal();
                    }
                    GUILayout.EndVertical();
                    
                    GUILayout.FlexibleSpace();
                    
                    GUILayout.BeginVertical();
                    GUILayout.Label("Wrong answers");
                    foreach (string answer in _selectedQuestion.answerSet.wrongAnswers)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(15);
                        GUILayout.Label("X");
                        GUILayout.Label(answer);
                        GUILayout.EndHorizontal();
                    }
                    GUILayout.EndVertical();
                    
                    
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                    
                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    GUILayout.BeginVertical(GUILayout.Width(position.width * .85f));
                    GUILayout.Label("Tip: ");
                    GUILayout.Label(_selectedQuestion.tip);
                    GUILayout.EndVertical();
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                
                    GUILayout.FlexibleSpace();
                
                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Edit", GUILayout.Width(55)))
                    {
                        EditQuestion.Create(_selectedQuestion);
                        GUILayout.EndHorizontal();
                    }
                    else
                    {
                        GUILayout.Space(10);
                        GUILayout.EndHorizontal();
                    }
                }
            }

            #endregion
            
            GUILayout.Label(_errorMessage, StyleLibrary.BottomMessage);
        }

        private void ChangeCategory(object category)
        {
            _searchCategory = (string)category;
            UpdateQuestionDisplay();
        }

        private void UpdateQuestionDisplay()
        {
            if (!DBManager.DatabaseLoaded())
            {
                _errorMessage = "No database loaded";
                return; 
            }
            
            _questionDisplay = new List<Question>();
            
            List<object> toAdd = new List<object>();
            
            if (string.IsNullOrEmpty(_searchBar) && string.IsNullOrEmpty(_searchCategory))
            {
                toAdd = DBManager.FindMatching(DBManager.Table.Questions);
            }
            else
            {
                Dictionary<string, string> searchDict = new Dictionary<string, string>();

                if (!string.IsNullOrEmpty(_searchBar))
                {
                    searchDict.Add("question_text", _searchBar);
                }

                if (!string.IsNullOrEmpty(_searchCategory))
                {
                    searchDict.Add("category_id", DBManager.FindID(DBManager.Table.Categories, "name", _searchCategory).ToString());
                }

                toAdd = DBManager.FindMatching(DBManager.Table.Questions, searchDict, false);
            }

            foreach (object o in toAdd)
            {
                _questionDisplay.Add((Question)o);
            }

            _errorMessage = "";
        }
    }
}
