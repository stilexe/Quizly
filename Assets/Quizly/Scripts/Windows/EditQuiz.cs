using UnityEditor;

namespace Quizly
{
    public class EditQuiz : EditorWindow
    {
        public static void Create(int quizID)
        {
            EditQuiz win = GetWindow<EditQuiz>(); 
        }
    }
}
