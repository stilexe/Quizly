using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Quizly
{
    public class QuestionList : EditorWindow
    {
        private List<Question> _questionDisplay;
        
        [MenuItem("Window/Quizly/View Questions")]
        public static void Create()
        {
            QuestionList win = GetWindow<QuestionList>();
        }

        private void OnGUI()
        {
            //search bar 
            //category drop down 

            _questionDisplay = DatabaseManager.SearchQuestions();
            
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
    }
}
