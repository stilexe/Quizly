using System.Collections.Generic;
using UnityEngine;

namespace Quizly
{
    public class Quiz
    {
        public int id;
        public string quizName;
        public float time;
        public QuestionSet questionSet;
    }

    [System.Serializable]
    public class QuestionSet
    {
        public List<int> questionIDs;
    }
    
    public class QuestionWeighting
    {
        public int quizID;
        public int questionID;
        public int weight;
    }
}
