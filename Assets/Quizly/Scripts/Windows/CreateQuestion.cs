using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace Quizly
{
    public class CreateQuestion : EditorWindow
    {
        private Vector2 _scrollPos;
        public enum QuestionType
        {
            None,
            TrueOrFalse,
            MultipleChoice
        }

        private Question _newQuestion = new Question(); 
        private string _questionText, _explanationText, _newAnswer, _categoryName = "";
        private Dictionary<string, bool> _answers = new Dictionary<string, bool>();
        private bool _true, _false;
        private QuestionType _questionType;
        private int _categoryID;
        private float _difficulty;

        private string _errorMessage; 

        private GenericMenu _answerMenu; 
        
        [MenuItem("Window/Quizly/Create/Question")]
        public static void Create()
        {
            CreateQuestion win = GetWindow<CreateQuestion>();
            
            win.titleContent = new GUIContent("Create Question");
        }

        private void OnGUI()
        {
            GUILayout.Label("New Question", StyleLibrary.Header2Style);
            
            _scrollPos = GUILayout.BeginScrollView(_scrollPos);
            
            GUILayout.Space(5);
            
            //category drop down
            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            GUILayout.Label("Category");
            if (EditorGUILayout.DropdownButton(new GUIContent(_categoryName), FocusType.Keyboard))
            {
                GenericMenu categoryMenu = new GenericMenu();
                List<object> categories = DBManager.FindMatching(DBManager.Table.Categories);

                if (categories is not null)
                {
                    foreach (object o in categories)
                    {
                        categoryMenu.AddItem(new GUIContent((string)o), false, SetQuestionCategory, o);
                    }
                }
                
                categoryMenu.ShowAsContext();
            }
            GUILayout.Space(10);
            GUILayout.EndHorizontal();
            
            GUILayout.Space(15);
            
            //difficulty slider
            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            GUILayout.Label("Difficulty");
            _difficulty = EditorGUILayout.Slider(_difficulty, 1, 5);
            _difficulty = Mathf.Round(_difficulty);
            GUILayout.Space(10);
            GUILayout.EndHorizontal();
            
            GUILayout.Space(15);
            
            //question text 
            GUILayout.Label("Question", StyleLibrary.Header3Style);
            
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            _questionText = EditorGUILayout.TextField(_questionText, GUILayout.Width(position.width *.85f));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            
            
            GUILayout.Label("Answer", StyleLibrary.Header3Style);

            //answer type dropdown 
            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            GUILayout.Label("Answer Type");

            if (EditorGUILayout.DropdownButton(new GUIContent(_questionType.ToString()), FocusType.Keyboard))
            {
                if (_answerMenu is null)
                {
                    SetQuestionType(QuestionType.None);
                    _answerMenu = new GenericMenu();
                    _answerMenu.AddItem(new GUIContent("True or False"), false, SetQuestionType, QuestionType.TrueOrFalse);
                    _answerMenu.AddItem(new GUIContent("Multiple Choice"), false, SetQuestionType, QuestionType.MultipleChoice);
                }
                
                _answerMenu.ShowAsContext();
            }
            GUILayout.Space(10);
            GUILayout.EndHorizontal();
            
            GUILayout.Space(15);

            //answer section
            switch (_questionType)
            {
                //if true or false question
                case QuestionType.TrueOrFalse:
                    
                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    _true = EditorGUILayout.Toggle("True", _true);
                    GUILayout.Space(25);
                    _false = EditorGUILayout.Toggle("False", _false);
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                    
                    break;
                
                //if multiple choice question
                case QuestionType.MultipleChoice:
                    
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(10);
                    GUILayout.Label("Answer");
                    _newAnswer = EditorGUILayout.TextField(_newAnswer);
                    GUILayout.Space(10);
                    GUILayout.EndHorizontal();
                    GUILayout.Space(5);
                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Add", GUILayout.Width(45), GUILayout.Height(25)))
                    {
                        _newAnswer = _newAnswer.Trim();
                        
                        if (string.IsNullOrEmpty(_newAnswer))
                        {
                            GUILayout.Label("Answer is empty.");
                        }
                        
                        _answers.Add(_newAnswer, false);
                        _newAnswer = "";
                    }
                    GUILayout.Space(10);
                    GUILayout.EndHorizontal();
                    
                    GUILayout.Space(15);
                    
                    List<string> toRemove = new List<string>();
                    Dictionary<string, bool> toggleChanges = new Dictionary<string, bool>();

                    foreach (KeyValuePair<string, bool> answer in _answers)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(35);
                        toggleChanges.Add(answer.Key, answer.Value);
                        
                        toggleChanges[answer.Key] = EditorGUILayout.Toggle(toggleChanges[answer.Key]);
                        GUILayout.Label(answer.Key);

                        if (GUILayout.Button("-", GUILayout.Width(25)))
                        {
                            toRemove.Add(answer.Key);
                            GUILayout.EndHorizontal();
                        }
                        else
                        {
                            GUILayout.Space(35);
                            GUILayout.EndHorizontal();
                        }
                        
                        GUILayout.Space(5);
                    }

                    foreach (string key in toRemove)
                    {
                        _answers.Remove(key);
                        toggleChanges.Remove(key);
                    }

                    foreach (KeyValuePair<string, bool> change in toggleChanges)
                    {
                        _answers[change.Key] = change.Value;
                    }
                    
                    break;
            }

            GUILayout.Space(15);
            //tip
            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            GUILayout.Label("Answer Explanation");
            GUILayout.EndHorizontal();
            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            _explanationText = EditorGUILayout.TextField(_explanationText, GUILayout.Width(position.width *.85f));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            
            GUILayout.Space(25);

            #region SaveButtons

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Add to Open Quiz", GUILayout.Width(position.width * .45f), GUILayout.Height(25)))
            {
                SaveQuestion();

                //check if a quiz is open 
                if (HasOpenInstances<CreateQuiz>())
                {
                    int questionID = int.Parse(DBManager.FindValues(DBManager.Table.Questions, "id")[^1]);
                    GetWindow<CreateQuiz>().AddQuestion((Question)DBManager.SearchWithID(DBManager.Table.Questions, questionID));
                }
                else
                {
                    _errorMessage = "No quiz open.";
                }
                
                GUILayout.EndHorizontal();
            }
            else if (GUILayout.Button("Save Question", GUILayout.Width(position.width * .45f), GUILayout.Height(25)))
            {
                SaveQuestion();
                GUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }

            #endregion
            
            GUILayout.EndScrollView();
            
            GUILayout.Label(_errorMessage, StyleLibrary.BottomMessage);
        }

        private void SetQuestionType(object type)
        {
            if (type is QuestionType questionType)
            {
                _questionType = questionType;
            }
        }

        private void SetQuestionCategory(object category)
        {
            _categoryName = (string)category;
        }

        private void SaveQuestion()
        {
            //check everything is filled out 
            if (string.IsNullOrEmpty(_questionText.Trim()) || string.IsNullOrEmpty(_explanationText.Trim()) || string.IsNullOrEmpty(_categoryName) || _difficulty < 1)
            {
                _errorMessage = "Fill out all fields.";
                return;
            }
            
            //create new question and fill with what is filled out 
            Question newQuestion = new Question();

            newQuestion.question = _questionText.Trim();
            newQuestion.tip = _explanationText.Trim();
            newQuestion.difficulty = Mathf.RoundToInt(_difficulty);
            newQuestion.answerSet = new AnswerSet();

            switch (_questionType)
            {
                case QuestionType.TrueOrFalse:

                    switch (_true)
                    {
                        case false when !_false:
                            _errorMessage = "Set a correct answer";
                            return;
                        case true:
                            newQuestion.answerSet.correctAnswers.Add("True");
                            newQuestion.answerSet.wrongAnswers.Add("False");
                            break;
                        case false:
                            newQuestion.answerSet.correctAnswers.Add("False");
                            newQuestion.answerSet.wrongAnswers.Add("True");
                            break;
                    }

                    break;
                
                case QuestionType.MultipleChoice:

                    if (_answers.Count == 0)
                    {
                        _errorMessage = "There are no answers.";
                        return; 
                    }
                    
                    foreach (KeyValuePair<string, bool> answer in _answers)
                    {
                        if (answer.Value) newQuestion.answerSet.correctAnswers.Add(answer.Key);
                        if (!answer.Value) newQuestion.answerSet.wrongAnswers.Add(answer.Key);
                    }

                    if (newQuestion.answerSet.correctAnswers.Count == 0)
                    {
                        _errorMessage = "No correct answers set.";
                        return; 
                    }
                    
                    break;
            }
            
            newQuestion.categoryID = DBManager.FindID(DBManager.Table.Categories, "name", _categoryName);
            
            DBManager.SaveObject(newQuestion);

            _questionText = "";
            _explanationText = "";
            _difficulty = 1;
            _categoryName = "";
            _answers.Clear();
        }
    }
}
