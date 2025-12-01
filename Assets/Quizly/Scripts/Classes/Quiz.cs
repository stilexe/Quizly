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

    public class Result
    {
        public int quizID;
        public string username;
        public string date;
        public int score; 
        public SubmissionSet submissionSet;
    }

    [System.Serializable]
    public class SubmissionSet
    {
        public List<AnswerSubmission> submissions;
    }

    public class AnswerSubmission
    {
        public int questionID;
        public List<string> answers;
        public bool isCorrect;
    }
}
