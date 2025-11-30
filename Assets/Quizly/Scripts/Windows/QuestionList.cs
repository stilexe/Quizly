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

                foreach (string s in DatabaseManager.SearchQuery(DatabaseManager.Table.Categories,
                             new List<string>() { "name" }))
                {
                    menu.AddItem(new GUIContent(s), false, ChangeCategory, s);
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
            if (string.IsNullOrEmpty(_searchBar) && string.IsNullOrEmpty(_searchCategory))
            {
                _questionDisplay = DatabaseManager.SearchQuestions();
                return;
            }
            
            Dictionary<string, string> searchDict = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(_searchBar))
            {
                searchDict.Add("question_text", _searchBar);
            }

            if (!string.IsNullOrEmpty(_searchCategory))
            {
                searchDict.Add("category_id", DatabaseManager.GetID(DatabaseManager.Table.Categories, "name", _searchCategory).ToString());
            }
            
            _questionDisplay = DatabaseManager.SearchQuestions(searchDict);
        }
    }
}
