using System.Collections.Generic;
using UnityEngine;

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

        public delegate void QuizComplete();
        public static event QuizComplete OnQuizComplete;

        private static Result _quizResults;
        private static bool _quizScored;

        /// <summary>
        /// Returns to loaded results, if quiz not scored yet returns null
        /// </summary>
        /// <returns></returns>
        public static Result GetResults()
        {
            if (_quizScored)
            {
                return _quizResults;
            }

            return null;
        }

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
            List<string> qIDs = new List<string>();
            searchDict.Add("quiz_id", new List<string> { _loadedQuiz.id.ToString() });

            foreach (int id in _loadedQuiz.questionSet.questionIDs)
            {
                qIDs.Add(id.ToString());
            }
            searchDict.Add("question_id", qIDs);

            foreach (string weight in DatabaseManager.ValuesQuery(DatabaseManager.Table.Weightings, "weighting", searchDict))
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
            _quizScored = false;
            
            #if UNITY_EDITOR
            Debug.Log($"Loaded Quiz: {_loadedQuiz.quizName}"); 
            #endif
            
            NextQuestion(); 
        }
        
        public static void SubmitAnswer(List<string> answers)
        {
            _submittedAnswers.Add(_loadedQuestion.id, answers);
            
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

            //check each question's answers 
            foreach (int key in _submittedAnswers.Keys)
            {
                answerSubmission = new AnswerSubmission() { questionID = key, answers = _submittedAnswers[key] }; 
                
                // get the correct answers
                List<string> correctAnswers = DatabaseManager.ValuesQuery(DatabaseManager.Table.Questions,
                    "answer_set", "id", new List<string>() { key.ToString() });
                correctAnswers = JsonUtility.FromJson<AnswerSet>(correctAnswers[0]).correctAnswers;

                int weight = int.Parse(DatabaseManager.ValuesQuery(DatabaseManager.Table.Weightings, "weighting" , 
                    new Dictionary<string, List<string>>(){{"quiz_id", new List<string>(){_loadedQuiz.id.ToString()}}, 
                        {"question_id", new List<string>(){key.ToString()}}})[0]);
                
                #if UNITY_EDITOR
                Debug.Log($"Checking question: {key.ToString()}. Correct answers: {correctAnswers.Count.ToString()}. Submitted answers: {_submittedAnswers[key].Count.ToString()}.");
                #endif
                
                //see if answer matches 
                //if only one correct answer 
                if (correctAnswers.Count == 1 && correctAnswers[0] == _submittedAnswers[key][0]) //and it's right 
                {
                    score += weight;
                    answerSubmission.isCorrect = true; 
                }
                else if (correctAnswers.Count == 1)
                {
                    answerSubmission.isCorrect = false; 
                }
                else //if more than one correct answer 
                {
                    int correctCount = 0; 
                    
                    foreach (string answer in _submittedAnswers[key]) //every answer submitted for the question
                    {
                        if (correctAnswers.Contains(answer))
                        {
                            score += weight / correctAnswers.Count; //add division of weight based on correct answers to score 
                            correctCount++;
                        }
                    }

                    if (correctCount == correctAnswers.Count)
                    {
                        answerSubmission.isCorrect = true;
                    }
                    else
                    {
                        answerSubmission.isCorrect = false;
                    }
                }
                
                submissionObjects.Add(answerSubmission);
            }
            
            _quizResults = new Result
            {
                quizID = _loadedQuiz.id,
                username = UserManager.GetLoggedIn().username,
                date = System.DateTime.Today.ToString("DD/MM/YYYY"),
                score = score,
                submissionSet = new SubmissionSet() {submissions = submissionObjects}
            };
            
            _quizScored = true;
            OnQuizComplete?.Invoke();
        }
    }
}
