using System;
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

    private SpriteRenderer correctRenderer;

    public void ShowAnswer(AnswerSubmission toDisplay)
    {
        if (correctRenderer == null)
        {
            correctRenderer = correctMarker.GetComponent<SpriteRenderer>();
        }
        
        string answerString = "";

        foreach (string s in toDisplay.answers)
        {
            answerString += $"\n{s}";
        }
        
        answerText.text = answerString;
        
        weight.text = QuizManager.ScoreAnswerSubmission(toDisplay).ToString();

        if (toDisplay.isCorrect)
        {
            correctRenderer.color = Color.green;
            weight.color = Color.green;
            tipText.text = "";
        }
        else
        {
            tipText.text = DBManager.FindValues(DBManager.Table.Questions, "tip", 
                new Dictionary<string, List<string>>(){{"id", new List<string>() {toDisplay.questionID.ToString()}}})[0];
            if (int.Parse(weight.text) > 0)
            {
                weight.color = Color.yellow;
                correctRenderer.color = Color.yellow;
            }
            else
            {
                weight.color = Color.red;
                correctRenderer.color = Color.red;
            }
        }
    }
}
