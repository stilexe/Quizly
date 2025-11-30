using System;
using System.Collections.Generic;
using Quizly;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UserForm : MonoBehaviour
{
    [SerializeField] private TMP_InputField userInput;
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private TextMeshProUGUI messageText;

    private void Start()
    {
        ResetMessage();
    }

    public void ResetMessage()
    {
        messageText.text = "";
    }

    public void ShowMessage(string message)
    {
        messageText.text = message;
    }

    public void CreateUser()
    {
        //check input in both fields 
        if (string.IsNullOrEmpty(userInput.text) || string.IsNullOrEmpty(passwordInput.text))
        {
            ShowMessage("Username or password is empty.");
            return; 
        }
        
        //check username is available 
        if (!UserManager.CheckUsername(userInput.text))
        {
            ShowMessage("Username is unavailable.");
            return;
        }
        
        UserManager.CreateUser(userInput.text, passwordInput.text);
    }

    public void Login()
    {
        if (string.IsNullOrEmpty(userInput.text) || string.IsNullOrEmpty(passwordInput.text))
        {
            ShowMessage("Username or password is empty.");
            return;
        }

        if (UserManager.CheckUsername(userInput.text))
        {
            ShowMessage("Username doesn't exist.");
            return; 
        }
        
        List<string> user = DatabaseManager.SearchQuery(DatabaseManager.Table.Users, new List<string>(){"username"}, new List<string>(){userInput.text});
        Debug.Log(user[0]);
    }
}
