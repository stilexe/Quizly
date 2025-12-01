using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Quizly;
using Random = UnityEngine.Random;

public class QuestionDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI questionText;
    [SerializeField] private GameObject answerHolder;
    [SerializeField] private GameObject answerDisplayPrefab;
    [SerializeField] private TextMeshProUGUI errorMessage; 
    
    private List<GameObject> _answerDisplays = new List<GameObject>();

    private void OnEnable()
    {
        QuizManager.OnNewQuestion += ShowQuestion;
    }

    private void OnDisable()
    {
        QuizManager.OnNewQuestion -= ShowQuestion;
    }

    private void Start()
    {
        ShowQuestion();
    }

    private void ShowQuestion()
    {
        //show question 
        Question toShow = QuizManager.GetCurrentQuestion();

        questionText.text = toShow.question;

        //get all the possible answers 
        List<string> answers = new List<string>();
        foreach (string answer in toShow.answerSet.correctAnswers)
        {
            answers.Add(answer);
        }
        foreach (string answer in toShow.answerSet.wrongAnswers)
        {
            answers.Add(answer);
        }
        //randomise list 
        for (int i = 0; i < answers.Count - 1; i++)
        {
            int replace = Random.Range(i, answers.Count);
            (answers[i], answers[replace]) = (answers[replace], answers[i]); 
        }
        
        //clear unnecessary displays 
        for (int i = 0; i < _answerDisplays.Count; i++)
        {
            if (i < answers.Count)
            {
                _answerDisplays[i].SetActive(true);
            }
            else
            {
                _answerDisplays[i].SetActive(false);
            }
        }

        //show answers 
        for (int i = 0; i < answers.Count; i++)
        {
            if (i > _answerDisplays.Count - 1)
            {
                GameObject answerDisplay = Instantiate(answerDisplayPrefab, answerHolder.transform); 
                _answerDisplays.Add(answerDisplay);
            }
            
            _answerDisplays[i].GetComponent<AnswerDisplay>().SetAnswer(answers[i]);
        }

    }

    public void SubmitQuestion()
    {
        List<string> answers = new List<string>();

        foreach (GameObject display in _answerDisplays)
        {
            if (!display.activeSelf) continue; 

            if (!display.TryGetComponent(out AnswerDisplay displayComp)) continue;
            
            if (displayComp.ToggleState())
            {
                answers.Add(displayComp.DisplayedAnswer());
            }
        }

        if (answers.Count == 0)
        {
            errorMessage.text = "Need at least one answer.";
            return;
        }
        
        QuizManager.SubmitAnswer(answers);
    }
}
