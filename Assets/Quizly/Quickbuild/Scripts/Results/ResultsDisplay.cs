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

    private void Start()
    {
        ShowResults();
    }

    private void Update()
    {
        if (_displaying is null)
        {
            ShowResults();
        }
    }

    private void ShowResults()
    {
        _displaying = QuizManager.GetResults();

        if (_displaying is null)
        {
            return;
        }
        
        quizName.text = QuizManager.QuizName();

        int maxScoreInt = QuizManager.QuizMaxScore();
        score.text = _displaying.score.ToString();
        maxScore.text = maxScoreInt.ToString();

        if (_displaying.score > (maxScoreInt * .75f))
        {
            score.color = Color.magenta;
        }
        else if(_displaying.score > (maxScoreInt * .5f))
        {
            score.color = Color.green;
        }
        else if (_displaying.score > (maxScoreInt * .3f))
        {
            score.color = Color.yellow;
        }
        else
        {
            score.color = Color.red;
        }

        for (int i = 0; i < answerDisplays.Count; i++)
        {
            if (i > _displaying.submissionSet.submissions.Count)
            {
                answerDisplays[i].SetActive(false);
            }
            else
            {
                answerDisplays[i].SetActive(true);
            }
        }
        
        //show submitted answers 

        for (int i = 0; i < _displaying.submissionSet.submissions.Count; i++)
        {
            if (i > answerDisplays.Count - 1)
            {
                answerDisplays.Add(Instantiate(answerPrefab, answerKeyHolder.transform));
            }
            
            answerDisplays[i].GetComponent<AnswerSubmissionDisplay>().ShowAnswer(_displaying.submissionSet.submissions[i]);
        }
        
        RefreshPastResults();
    }

    public void SaveResults()
    {
        DBManager.SaveObject(_displaying);
    }

    public void RefreshPastResults(int userID = 0)
    {
        Dictionary<string,string> searchDict = new Dictionary<string, string>()
        {
            {"quiz_id", QuizManager.QuizID().ToString()},
        };
        
        if (userID > 0)
        {
            searchDict.Add("user_id", userID.ToString());    
        }
        
        _pastResults.Clear();
        
        List<object> pastObjects = DBManager.FindMatching(DBManager.Table.Results,searchDict);

        foreach (object o in pastObjects)
        {
            _pastResults.Add((Result)o);
        }
        
        for (int i = 0; i < _pastResultDisplays.Count; i++)
        {
            if (i > _pastResults.Count - 1)
            {
                _pastResultDisplays[i].SetActive(false);
            }
            else
            {
                _pastResultDisplays[i].SetActive(true);
            }
        }
        
        for (int i = 0; i < _pastResults.Count; i++)
        {
            if (i > _pastResultDisplays.Count - 1)
            {
                _pastResultDisplays.Add(Instantiate(pastResultPrefab, pastResultHolder.transform));
            }
            
            _pastResultDisplays[i].GetComponent<PastResultDisplay>().DisplayResult(_pastResults[i]);
        }
    }

    public void DisplayMyResults()
    {
        RefreshPastResults(UserManager.GetLoggedIn().id);
    }
}
