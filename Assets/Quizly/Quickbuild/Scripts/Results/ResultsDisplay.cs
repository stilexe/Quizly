using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Quizly;

public class ResultsDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI quizName;
    [SerializeField] private TextMeshProUGUI score;
    [SerializeField] private TextMeshProUGUI maxScore;
    [SerializeField] private GameObject answerKeyHolder;
    [SerializeField] private GameObject answerPrefab;
    [SerializeField] private GameObject pastResultHolder;
    [SerializeField] private GameObject pastResultPrefab;

    private List<GameObject> answerDisplays = new List<GameObject>();
    private Result _displaying; 
    private List<GameObject> _pastResultDisplays = new List<GameObject>();
    private List<Result> _pastResults = new List<Result>();

    private void OnEnable()
    {
        QuizManager.OnQuizComplete += ShowResults;
    }

    private void OnDisable()
    {
        QuizManager.OnQuizComplete -= ShowResults;
    }

    private void ShowResults(Result toShow)
    {
        _displaying = toShow;
        quizName.text = QuizManager.QuizName();
        score.text = toShow.score.ToString();
        maxScore.text = QuizManager.QuizMaxScore().ToString();

        for (int i = 0; i < answerDisplays.Count; i++)
        {
            if (i > toShow.submissionSet.submissions.Count)
            {
                answerDisplays[i].SetActive(false);
            }
            else
            {
                answerDisplays[i].SetActive(true);
            }
        }

        for (int i = 0; i < toShow.submissionSet.submissions.Count; i++)
        {
            if (i > answerDisplays.Count)
            {
                answerDisplays.Add(Instantiate(answerPrefab, answerKeyHolder.transform));
            }
            
            answerDisplays[i].GetComponent<AnswerSubmissionDisplay>().ShowAnswer(toShow.submissionSet.submissions[i]);
        }
    }

    public void SaveResults()
    {
        DatabaseManager.SaveQuery(_displaying);
    }

    public void SeePastResults()
    {
        
    }

    public void HidePastResults()
    {
        foreach (GameObject pastResult in _pastResultDisplays)
        {
            pastResult.SetActive(false);
        }
    }
}
