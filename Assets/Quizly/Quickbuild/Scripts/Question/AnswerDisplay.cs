using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AnswerDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI answerText;
    [SerializeField] private Toggle toggle;

    private void Start()
    {
        toggle.isOn = false;
    }

    public bool ToggleState()
    {
        return toggle.isOn;
    }

    public string DisplayedAnswer()
    {
        return answerText.text;
    }

    public void SetAnswer(string answer)
    {
        answerText.text = answer;
    }
}
