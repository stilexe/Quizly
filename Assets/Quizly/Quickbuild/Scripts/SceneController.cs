using System;
using Quizly;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    private void OnEnable()
    {
        QuizManager.OnQuizComplete += LoadResults;
    }

    private void OnDisable()
    {
        QuizManager.OnQuizComplete -= LoadResults;
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    private void LoadResults()
    {
        SceneManager.LoadScene("QuizResults");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
