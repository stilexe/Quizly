using UnityEditor;

namespace Quizly
{
    public class EditQuestion : EditorWindow
    {
        private static Question _displayedQuestion; 
        
        public static void Create(Question question)
        {
            EditQuestion win = GetWindow<EditQuestion>();
            
            _displayedQuestion = question;
        }

        private void OnGUI()
        {
        
        }
    }
}
