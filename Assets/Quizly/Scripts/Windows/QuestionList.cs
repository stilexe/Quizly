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
        
        [MenuItem("Window/Quizly/View Questions")]
        public static void Create()
        {
            QuestionList win = GetWindow<QuestionList>();
        }

        private void OnGUI()
        {
            GUILayout.Label("All Questions"); 
            
            GUILayout.BeginHorizontal();
            
            GUILayout.Label("Search: ");
            _searchBar = GUILayout.TextArea(_searchBar);
            
            if (GUILayout.Button("Search"))
            {
                UpdateQuestionDisplay();
                GUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.EndHorizontal();
            }
            
            //category drop down 
            if (EditorGUILayout.DropdownButton(new GUIContent(_searchCategory), FocusType.Keyboard))
            {
                GenericMenu menu = new GenericMenu();
                
                menu.AddItem(new GUIContent("None"), false, ChangeCategory, "");

                foreach (object o in DBManager.FindMatching(DBManager.Table.Categories))
                {
                    menu.AddItem(new GUIContent((string)o), false, ChangeCategory, ((string)o));
                }
                
                menu.ShowAsContext();
            }

            if (_questionDisplay == null)
            {
                UpdateQuestionDisplay();
            }
            
            GUILayout.Label("Questions");
            
            //all questions 

            foreach (Question question in _questionDisplay)
            {
                GUILayout.BeginHorizontal();
                
                GUILayout.Label(question.question);
                GUILayout.Label(question.difficulty.ToString());

                if (GUILayout.Button("Edit"))
                {
                    EditQuestion.Create(question);
                    GUILayout.EndHorizontal();
                }
                else if (GUILayout.Button("-"))
                {
                    GUILayout.EndHorizontal();
                }
                else
                {
                    GUILayout.EndHorizontal();
                }
            }
        }

        private void ChangeCategory(object category)
        {
            _searchCategory = (string)category;
            UpdateQuestionDisplay();
        }

        private void UpdateQuestionDisplay()
        {
            _questionDisplay = new List<Question>();
            
            if (string.IsNullOrEmpty(_searchBar) && string.IsNullOrEmpty(_searchCategory))
            {
                foreach (object o in DBManager.FindMatching(DBManager.Table.Questions))
                {
                    _questionDisplay.Add((Question)o);
                }
                return;
            }
            
            Dictionary<string, string> searchDict = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(_searchBar))
            {
                searchDict.Add("question_text", _searchBar);
            }

            if (!string.IsNullOrEmpty(_searchCategory))
            {
                searchDict.Add("category_id", DBManager.FindID(DBManager.Table.Categories, "name", _searchCategory).ToString());
            }

            foreach (object o in DBManager.FindMatching(DBManager.Table.Questions, searchDict, false))
            {
                _questionDisplay.Add((Question)o);
            }
        }
    }
}
