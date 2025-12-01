using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Quizly
{
    public static class QuizManager
    {
        private static Quiz _loadedQuiz;
        private static List<int> _unusedQuestions = new List<int>();
        private static Question _loadedQuestion; 
        private static Dictionary<int, List<string>> _submittedAnswers = new Dictionary<int, List<string>>();

        public delegate void NewQuestion();
        public static event NewQuestion OnNewQuestion;

        public delegate void QuizComplete(Result results);
        public static event QuizComplete OnQuizComplete;

        private static Result _quizResults;

        public static Question GetCurrentQuestion()
        {
            return _loadedQuestion;
        }

        public static string QuizName()
        {
            return _loadedQuiz.quizName; 
        }

        public static int QuizMaxScore()
        {
            int score = 0;
            Dictionary<string, List<string>> searchDict   = new Dictionary<string, List<string>>();
            searchDict["quiz_id"] = new List<string>(){_loadedQuiz.id.ToString()};
            searchDict["question_id"] = new List<string>();
            
            foreach (int id in _loadedQuiz.questionSet.questionIDs)
            {
                searchDict["question_id"].Add(id.ToString());
            }

            foreach (string weight in DatabaseManager.ValuesQuery(DatabaseManager.Table.Weightings, "weight",searchDict))
            {
                score += int.Parse(weight);
            }

            return score; 
        }

        public static void LoadQuiz(int id)
        {
            _loadedQuiz = (Quiz)DatabaseManager.FindWithID(DatabaseManager.Table.Quizzes, id);
            _submittedAnswers = new Dictionary<int, List<string>>();
            _unusedQuestions = _loadedQuiz.questionSet.questionIDs;
            _quizResults = null; 
            
            #if UNITY_EDITOR
            Debug.Log($"Loaded Quiz: {_loadedQuiz.quizName}"); 
            #endif
            
            NextQuestion(); 
        }
        
        public static void SubmitAnswer(List<string> answers)
        {
            _submittedAnswers.Add(_loadedQuestion.id, new List<string>());
            
            NextQuestion();
        }

        private static void NextQuestion()
        {
            if (_unusedQuestions.Count == 0)
            {
                //end of quiz 
                ScoreQuiz();
                return;
            }
            
            _loadedQuestion = (Question)DatabaseManager.FindWithID(DatabaseManager.Table.Questions, _unusedQuestions[Random.Range(0, _unusedQuestions.Count)]);
            _unusedQuestions.Remove(_loadedQuestion.id);
            
            OnNewQuestion?.Invoke();
        }

        private static void ScoreQuiz()
        {
            int score = 0;
            List<AnswerSubmission> submissionObjects = new List<AnswerSubmission>();
            
            AnswerSubmission answerSubmission = new AnswerSubmission();

            //check each answer and create a answer submission for it 
            foreach (int key in _submittedAnswers.Keys)
            {
                answerSubmission = new AnswerSubmission() { questionID = key, answers = _submittedAnswers[key] };
                
                // get the correct answers
                List<string> correctAnswers = DatabaseManager.ValuesQuery(DatabaseManager.Table.Questions,
                    "answer_set", "id", new List<string>() { key.ToString() });
                correctAnswers = JsonUtility.FromJson<AnswerSet>(correctAnswers[0]).correctAnswers;
                
                //get question from database 
                
                //see if answer matches 
                
                
                submissionObjects.Add(answerSubmission);
            }
            
            //check answers 
            foreach (KeyValuePair<int, List<string>> answer in _submittedAnswers)
            {

                checkingWeight = (QuestionWeighting)DatabaseManager.FindMatching(DatabaseManager.Table.Weightings,
                    new Dictionary<string, string>() { { "question_id", checking.id.ToString() }, {"quiz_id", _loadedQuiz.id.ToString()} })[0];

                if (checking.answerSet.correctAnswers.Count > 1)
                {
                    int allCorrect = checking.answerSet.correctAnswers.Count;
                    int allGotten = 0;
                    
                    foreach (string correctAnswer in checking.answerSet.correctAnswers)
                    {
                        foreach (string givenAnswer in answer.Value)
                        {
                            if (correctAnswer == givenAnswer)
                            {
                                score += checkingWeight.weight / correctAnswer.Length; //divide weight per possible correct answers 
                                allGotten++;
                            }
                        }
                    }

                    if (allGotten == allCorrect)
                    {
                        answerSubmission.isCorrect = true;
                    }
                    else
                    {
                        answerSubmission.isCorrect = false;
                    }
                }
                else
                {
                    if (answer.Value[0] == checking.answerSet.correctAnswers[0])
                    {
                        score += checkingWeight.weight; 
                        answerSubmission.isCorrect = true;
                    }
                    else
                    {
                        answerSubmission.isCorrect = false;
                    }
                }
                
                submissionSet.submissions.Add(answerSubmission);
            }
            
            _quizResults = new Result
            {
                quizID = _loadedQuiz.id,
                username = UserManager.GetLoggedIn().username,
                date = DateTime.Today.ToString("DD/MM/YYYY"),
                score = score,
                submissionSet = submissionSet
            };
            
            OnQuizComplete?.Invoke(_quizResults);
        }
    }
}
