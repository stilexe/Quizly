using System;
using TMPro;
using UnityEngine;
using Quizly;
using UnityEngine.UI;

public class UserDisplay : MonoBehaviour
{
    [SerializeField] private GameObject displayPanel; 
    [SerializeField] private TextMeshProUGUI username;
    
    [SerializeField] private GameObject loginPanel;
    [SerializeField] private TMP_InputField userInput;
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private TextMeshProUGUI messageText;
    
    [SerializeField] private Button loginButton;
    [SerializeField] private Button createUserButton;
    [SerializeField] private Button logoutButton;
    private void OnEnable()
    {
        loginButton.onClick.AddListener(Login);
        createUserButton.onClick.AddListener(CreateUser);
        logoutButton.onClick.AddListener(Logout);
        UserManager.OnUserLogin += UserLoggedIn;
        UserManager.OnUserLogout += UserLoggedOut;
    }

    private void OnDisable()
    {
        loginButton.onClick.RemoveAllListeners();
        createUserButton.onClick.RemoveAllListeners();
        logoutButton.onClick.RemoveAllListeners();
        UserManager.OnUserLogin -= UserLoggedIn;
        UserManager.OnUserLogout -= UserLoggedOut;
    }

    private void UserLoggedIn()
    {
        loginPanel.SetActive(false);
        displayPanel.SetActive(true);

        username.text = UserManager.GetLoggedIn().username;
    }

    private void UserLoggedOut()
    {
        loginPanel.SetActive(true);
        displayPanel.SetActive(false);
    }

    private void ShowMessage(string message)
    {
        messageText.text = message;
    }

    private void CreateUser()
    {
        //check input in both fields 
        if (string.IsNullOrEmpty(userInput.text) || string.IsNullOrEmpty(passwordInput.text))
        {
            ShowMessage("Username or password is empty.");
            return; 
        }

        if (UserManager.CreateUser(userInput.text, passwordInput.text))
        {
            ShowMessage("User created.");
            Login();
        }
        else
        {
            ShowMessage("Username is unavailable.");
        }
    }

    private void Login()
    {
        ShowMessage("");
        
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
        
        UserManager.TryLogin(userInput.text, passwordInput.text);
    }

    private void Logout()
    {
        ShowMessage("");
        userInput.text = "";
        passwordInput.text = "";
        
        UserManager.Logout();
    }
}
