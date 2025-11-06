using UnityEngine;
using System.Data.SQLite; 

public static class SaveManager
{
    private static string _databaseFileName = "quizlyData.sqlite";
    
    private static void ConnectToDatabase()
    {
        SQLiteConnection connection = new SQLiteConnection();
    }
}
