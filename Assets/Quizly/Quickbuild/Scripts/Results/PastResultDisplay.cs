using Quizly;
using TMPro;
using UnityEngine;

public class PastResultDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI userText;
    [SerializeField] private TextMeshProUGUI dateText;

    public void DisplayResult(Result result)
    {
        scoreText.text = result.score.ToString();
        userText.text = DBManager.FindValueWithID(DBManager.Table.Users, "username", result.userID);
        dateText.text = result.date;
    }
    
}
