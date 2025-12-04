using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Quizly;

public class AnswerSubmissionDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI answerText;
    [SerializeField] private TextMeshProUGUI weight;
    [SerializeField] private GameObject correctMarker;
    [SerializeField] private TextMeshProUGUI tipText; 
    public void ShowAnswer(AnswerSubmission toDisplay)
    {
        string answerString = "";

        foreach (string s in toDisplay.answers)
        {
            answerString += $"\n{s}";
        }
        
        answerText.text = answerString;

        if (toDisplay.isCorrect)
        {
            correctMarker.SetActive(true);
            tipText.text = "";
        }
        else
        {
            correctMarker.SetActive(false);
            tipText.text = DBManager.FindValues(DBManager.Table.Questions, "tip", 
                new Dictionary<string, List<string>>(){{"id", new List<string>() {toDisplay.questionID.ToString()}}})[0];
        }
    }
}
