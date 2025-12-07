using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Quizly
{
    public class EditQuestion : EditorWindow
    {
        private static Question _displayedQuestion;

        private static Question _questionOverwrite;

        private Vector2 _scrollPos;

        private static string _newCategoryName; 
        private static Dictionary<string, bool> _newAnswers = new Dictionary<string, bool>();
        private string _newAnswer;

        private string _errorMessage;
        
        
        public static void Create(Question question)
        {
            EditQuestion win = GetWindow<EditQuestion>();
            
            win.titleContent = new GUIContent("Edit Question");
            
            _displayedQuestion = question;
            _questionOverwrite = new Question()
            {
                id = question.id,
                categoryID = question.categoryID,
                question = question.question,
                answerSet = question.answerSet,
                difficulty = question.difficulty,
                tip = question.tip,
            };
            
            _newAnswers.Clear();

            foreach (string answer in question.answerSet.correctAnswers)
            {
                _newAnswers.Add(answer, true);
            }

            foreach (string answer in question.answerSet.wrongAnswers)
            {
                _newAnswers.Add(answer, false);
            }

            _newCategoryName = DBManager.FindValueWithID(DBManager.Table.Categories, "name", question.categoryID);
        }

        private void OnGUI()
        {
            GUILayout.Label("Edit Question", StyleLibrary.Header3Style);
            
            _scrollPos = GUILayout.BeginScrollView(_scrollPos);
            
            GUILayout.Space(5);
            
            //category drop down
            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            GUILayout.Label("Category");
            if (EditorGUILayout.DropdownButton(new GUIContent(_newCategoryName), FocusType.Keyboard))
            {
                GenericMenu categoryMenu = new GenericMenu();
                List<string> categories = DBManager.FindValues(DBManager.Table.Categories, "name");

                foreach (string category in categories)
                {
                    categoryMenu.AddItem(new GUIContent(category), false, () => { _newCategoryName = category; });
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
            _questionOverwrite.difficulty = EditorGUILayout.IntSlider(_questionOverwrite.difficulty, 1, 5);
            GUILayout.Space(10);
            GUILayout.EndHorizontal();
            
            GUILayout.Space(15);
            
            //question text 
            GUILayout.Label("Question", StyleLibrary.Header3Style);
            
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            _questionOverwrite.question = EditorGUILayout.TextField(_questionOverwrite.question, GUILayout.Width(position.width *.85f));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();


            GUILayout.Label("Answer", StyleLibrary.Header3Style);

            GUILayout.Space(15);

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

                _newAnswers.Add(_newAnswer, false);
                _newAnswer = "";
            }

            GUILayout.Space(10);
            GUILayout.EndHorizontal();

            GUILayout.Space(15);

            List<string> toRemove = new List<string>();
            Dictionary<string, bool> toggleChanges = new Dictionary<string, bool>();

            foreach (KeyValuePair<string, bool> answer in _newAnswers)
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
                _newAnswers.Remove(key);
                toggleChanges.Remove(key);
            }

            foreach (KeyValuePair<string, bool> change in toggleChanges)
            {
                _newAnswers[change.Key] = change.Value;
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
            _questionOverwrite.tip = EditorGUILayout.TextField(_questionOverwrite.tip, GUILayout.Width(position.width *.85f));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            
            GUILayout.Space(25);

            #region SaveButtons

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Save", GUILayout.Width(position.width * .45f), GUILayout.Height(25)))
            {
                OverwriteQuestion();
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

        private void OverwriteQuestion()
        {
            _questionOverwrite.categoryID = DBManager.FindID(DBManager.Table.Categories, "name", _newCategoryName);
            
            _questionOverwrite.answerSet.correctAnswers.Clear();
            _questionOverwrite.answerSet.wrongAnswers.Clear();
            
            foreach (string answer in _newAnswers.Keys)
            {
                if (_newAnswers[answer])
                {
                    _questionOverwrite.answerSet.correctAnswers.Add(answer);
                }
                else
                {
                    _questionOverwrite.answerSet.wrongAnswers.Add(answer);
                }
            }
            
            DBManager.SaveObject(_questionOverwrite, true);
        }
    }
}
