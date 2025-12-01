using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace Quizly
{
    public class CreateQuestion : EditorWindow
    {
        public enum QuestionType
        {
            None,
            TrueOrFalse,
            MultipleChoice
        }

        private Question _newQuestion = new Question(); 
        private string _questionText, _explanationText, _newAnswer, _categoryName;
        private Dictionary<string, bool> _answers = new Dictionary<string, bool>();
        private bool _true, _false;
        private QuestionType _questionType;
        private int _categoryID;
        private float _difficulty;

        private string _errorMessage; 

        private GenericMenu _answerMenu; 
        
        [MenuItem("Window/Quizly/Create Question")]
        public static void Create()
        {
            CreateQuestion win = GetWindow<CreateQuestion>();
        }

        private void OnGUI()
        {
            GUILayout.Label("New Question");
            
            //category drop down
            GUILayout.BeginHorizontal();
            GUILayout.Label("Category");


            if (EditorGUILayout.DropdownButton(new GUIContent(_categoryName), FocusType.Keyboard))
            {
                GenericMenu categoryMenu = new GenericMenu();

                foreach (object o in DatabaseManager.FindMatching(DatabaseManager.Table.Categories))
                {
                    categoryMenu.AddItem(new GUIContent((string)o), false, SetQuestionCategory, o);
                }
                
                categoryMenu.ShowAsContext();
            }
            
            GUILayout.EndHorizontal();
            
            //difficulty slider
            GUILayout.BeginHorizontal();
            GUILayout.Label("Difficulty");
            _difficulty = EditorGUILayout.Slider(_difficulty, 1, 5);
            _difficulty = Mathf.Round(_difficulty);
            GUILayout.EndHorizontal();
            
            //question text 
            GUILayout.Label("Question");
            _questionText = EditorGUILayout.TextField(_questionText);

            //answer type dropdown 
            GUILayout.BeginHorizontal();
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
            
            GUILayout.EndHorizontal();

            //answer section
            switch (_questionType)
            {
                //if true or false question
                case QuestionType.TrueOrFalse:
                    
                    GUILayout.BeginHorizontal();

                    _true = EditorGUILayout.Toggle("True", _true);
                    _false = EditorGUILayout.Toggle("False", _false);
                
                    GUILayout.EndHorizontal();
                    
                    break;
                
                //if multiple choice question
                case QuestionType.MultipleChoice:
                    
                    GUILayout.BeginHorizontal();

                    GUILayout.Label("Answer");
                    _newAnswer = EditorGUILayout.TextField(_newAnswer);
                    
                    GUILayout.EndHorizontal();

                    if (GUILayout.Button("Add"))
                    {
                        _newAnswer = _newAnswer.Trim();
                        
                        if (string.IsNullOrEmpty(_newAnswer))
                        {
                            GUILayout.Label("Answer is empty.");
                        }
                        
                        _answers.Add(_newAnswer, false);
                        _newAnswer = "";
                    }
                    
                    List<string> toRemove = new List<string>();
                    Dictionary<string, bool> toggleChanges = new Dictionary<string, bool>();

                    foreach (KeyValuePair<string, bool> answer in _answers)
                    {
                        GUILayout.BeginHorizontal();

                        toggleChanges.Add(answer.Key, answer.Value);
                        
                        toggleChanges[answer.Key] = EditorGUILayout.Toggle(toggleChanges[answer.Key]);
                        GUILayout.Label(answer.Key);

                        if (GUILayout.Button("-"))
                        {
                            toRemove.Add(answer.Key);
                        }
                        
                        GUILayout.EndHorizontal();
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

            //tip
            GUILayout.Label("Answer Explanation");
            _explanationText = EditorGUILayout.TextField(_explanationText);

            //buttons 
            GUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Add to Open Quiz"))
            {
                SaveQuestion();
                
                //check if a quiz is open 
                //send to quiz window
            }

            if (GUILayout.Button("Save Question"))
            {
                SaveQuestion();
            }
            
            GUILayout.EndHorizontal();
            
            GUILayout.Label(_errorMessage);
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
            
            newQuestion.categoryID = DatabaseManager.GetID(DatabaseManager.Table.Categories, "name", _categoryName);
            
            DatabaseManager.SaveQuery(newQuestion);

            _questionText = "";
            _explanationText = "";
            _difficulty = 1;
            _categoryName = "";
        }
    }
}
