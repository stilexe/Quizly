using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Quizly;
using UnityEngine.UI;

public class QuestionDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI questionText;
    [SerializeField] private GameObject answerHolder;
    [SerializeField] private GameObject answerDisplayPrefab;
    [SerializeField] private TextMeshProUGUI errorMessage; 
    
    private List<GameObject> _answerDisplays = new List<GameObject>();
    private List<Toggle> _answerToggles = new List<Toggle>();
    private List<Toggle> _onToggles = new List<Toggle>();
    private int _maxAnswers;

    private void OnEnable()
    {
        QuizManager.OnNewQuestion += ShowQuestion;
    }

    private void OnDisable()
    {
        QuizManager.OnNewQuestion -= ShowQuestion;

        foreach (Toggle toggle in _answerToggles)
        {
            toggle.onValueChanged.RemoveAllListeners();
        }
    }

    private void Start()
    {
        ShowQuestion();
    }

    private void ShowQuestion()
    {
        _onToggles.Clear();
        
        //show question 
        Question toShow = QuizManager.GetCurrentQuestion();
        _maxAnswers = toShow.answerSet.correctAnswers.Count;

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
            
            //clear toggle 
            _answerDisplays[i].GetComponentInChildren<Toggle>().isOn = false;
        }

        //show answers 
        for (int i = 0; i < answers.Count; i++)
        {
            if (i > _answerDisplays.Count - 1)
            {
                GameObject answerDisplay = Instantiate(answerDisplayPrefab, answerHolder.transform); 
                _answerDisplays.Add(answerDisplay);
                _answerToggles.Add(answerDisplay.GetComponentInChildren<Toggle>());
            }
            
            _answerDisplays[i].GetComponent<AnswerDisplay>().SetAnswer(answers[i]);
        }

        foreach (Toggle toggle in _answerToggles)
        {
            toggle.onValueChanged.AddListener(ToggleClicked);
        }
        
        Debug.Log($"New question on toggles {_onToggles.Count}, answer toggles {_answerToggles.Count}");

    }

    private void ToggleClicked(bool value)
    {
        if (value && _onToggles.Count == _maxAnswers) //toggle changed to true but max answers chosen
        {
            #if UNITY_EDITOR
            Debug.Log("Max answers chosen");
            #endif
            
            errorMessage.text = "Max answers chosen";

            foreach (Toggle toggle in _answerToggles)
            {
                if (toggle.isOn && !_onToggles.Contains(toggle)) //if toggle is on and not in on toggles 
                {
                    toggle.isOn = false; //turn toggle off
                }
            }
        }
        else if(value) //changed to true but max answers not chosen 
        {
            errorMessage.text = "";
            
            foreach (Toggle toggle in _answerToggles)
            {
                if (toggle.isOn && !_onToggles.Contains(toggle)) //if toggle on but not in on toggles 
                {
                    _onToggles.Add(toggle); //add to on toggles 
                }
            }
        }
        else //toggle changed to false 
        {
            errorMessage.text = "";
            
            foreach (Toggle toggle in _answerToggles)
            {
                if (!toggle.isOn && _onToggles.Contains(toggle)) //if toggle off but in on toggles 
                {
                    _onToggles.Remove(toggle); //remove from on toggles 
                }
            }
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

        if (!CheckToggles())
        {
            errorMessage.text = $"Question requires {_maxAnswers} answer";

            if (_maxAnswers > 1)
            {
                errorMessage.text += "s.";
            }
            else
            {
                errorMessage.text += ".";
            }
            
            return;
        }
        
        QuizManager.SubmitAnswer(answers);
    }

    /// <summary>
    /// Checks if the max number of answers have been selected. 
    /// </summary>
    /// <returns></returns>
    private bool CheckToggles()
    {
        if (_onToggles.Count == _maxAnswers)
        {
            return true;
        }

        return false; 
    }
}
