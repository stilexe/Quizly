using System;
using System.Collections.Generic;
using UnityEngine;
using Quizly; 

public class QuizDisplayHolder : MonoBehaviour
{
    private List<Quiz> _all; 
    private int _pageNumber; 

    [SerializeField] private GameObject displayHolder;
    [SerializeField] private GameObject displayPrefab;
    [SerializeField] private int quizPerPage; 

    private void Start()
    {
        _pageNumber = 0; 
        
        LoadQuizzes();
        UpdateDisplay();
    }

    public void ChangePage(int page)
    {
        _pageNumber += page;

        if (_pageNumber < 0)
        {
            _pageNumber = 0;
        }
    }

    public void LoadQuizzes()
    {
        _all = new List<Quiz>();
        
        foreach (object o in DatabaseManager.FindMatching(DatabaseManager.Table.Quizzes))
        {
            _all.Add((Quiz)o);
        }
    }

    private void UpdateDisplay()
    {
        int count;
        //if not enough left in all to display max per page 
        if ((_pageNumber + 1) * quizPerPage > _all.Count)
        {
            count = _all.Count - _pageNumber * quizPerPage;
        }
        else
        {
            count = quizPerPage;
        }
        
        foreach (Quiz quiz in _all.GetRange(_pageNumber, count))
        {
            GameObject newDisplay = Instantiate(displayPrefab, displayHolder.transform);
            newDisplay.GetComponent<QuizDisplay>().DisplayQuiz(quiz);
        }
    }
}
