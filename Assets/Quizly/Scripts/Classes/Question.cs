using System.Collections.Generic;
using UnityEngine;

namespace Quizly
{
    public class Question
    {
        public int id;
        public string question;
        public AnswerSet answerSet;
        public int categoryID;
        public int difficulty;
        public string tip;
    }

    public class Category
    {
        public int catID;
        public string catName;
    }
    
    [System.Serializable]
    public class AnswerSet
    {
        public List<string> correctAnswers = new List<string>();
        public List<string> wrongAnswers = new List<string>();
    }
}
