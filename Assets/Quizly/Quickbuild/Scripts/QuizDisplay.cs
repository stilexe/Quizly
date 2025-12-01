using System;
using Quizly;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class QuizDisplay : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI nameDisplay;
    [SerializeField] TextMeshProUGUI difficultyDisplay;
    [SerializeField] TextMeshProUGUI questionDisplay;
    [SerializeField] private Button startButton;

    private int _displayedID; 
    
    public void DisplayQuiz(Quiz quiz)
    {
        nameDisplay.text = quiz.quizName;

        questionDisplay.text = quiz.questionSet.questionIDs.Count.ToString();
        _displayedID = quiz.id;
    }

    private void OnDisable()
    {
        startButton.onClick.RemoveAllListeners();
    }

    public void StartQuiz()
    {
        #if UNITY_EDITOR
        Debug.Log("Starting quiz " + _displayedID);
        #endif
        
        QuizManager.LoadQuiz(_displayedID);
        SceneManager.LoadScene("QuizPlay");
    }
}
