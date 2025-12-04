using System.IO;
using Quizly;
using UnityEditor;
using UnityEngine;

public class CreateGameSettings : EditorWindow
{
    private string _chosenDatabase = "";
    private string _errorMessage = "";
    private Object _saveFolder; 
    private string _saveFolderPath = "";
    private string _settingsName = "";
    
    [MenuItem("Window/Quizly/Create Game Settings")]
    public static void Create()
    {
        CreateGameSettings win = GetWindow<CreateGameSettings>();
    }

    private void OnGUI()
    {
        GUILayout.Label("Create Game Settings");
        
        GUILayout.BeginHorizontal();
        GUILayout.Label("Name: ");
        _settingsName = EditorGUILayout.TextField(_settingsName);
        GUILayout.EndHorizontal();
        
        _saveFolder = EditorGUILayout.ObjectField(_saveFolder, typeof(Object) , false);
        
        _saveFolderPath = AssetDatabase.GetAssetPath(_saveFolder);
            
        if (!string.IsNullOrEmpty(_saveFolderPath))
        {
            if (!Directory.Exists(Directory.GetCurrentDirectory() + "/" + _saveFolderPath))
            {
                _errorMessage = "Must be a folder.";
                _saveFolderPath = "";
                _saveFolder = null;
            }
            else
            {
                _errorMessage = "";
            }
                
            GUILayout.BeginHorizontal();
            GUILayout.Label("File Path: ");
            GUILayout.Label(_saveFolderPath);
            GUILayout.EndHorizontal();
        }
        
        GUILayout.BeginHorizontal();
        
        GUILayout.Label("Database: ");

        if (EditorGUILayout.DropdownButton(new GUIContent(_chosenDatabase), FocusType.Keyboard))
        {
            GenericMenu menu = new GenericMenu();
            
            foreach (string db in DBManager.GetDatabaseNames())
            { 
                menu.AddItem(new GUIContent(db.Remove(db.IndexOf('.')).Replace('_', ' ')), false, () => _chosenDatabase = db);
            }
            
            menu.ShowAsContext();
        }
        
        GUILayout.EndHorizontal();
        
        if (GUILayout.Button("Save Settings"))
        {
            SaveSettings();
        }
        
        GUILayout.Label(_errorMessage);
    }

    private void SaveSettings()
    {
        if (string.IsNullOrEmpty(_chosenDatabase))
        {
            _errorMessage = "Please select a database";
            return;
        }

        _errorMessage = "";
        
        GameSettings gameSettings = CreateInstance<GameSettings>();
        gameSettings.databaseName = _chosenDatabase;
        
        AssetDatabase.CreateAsset(gameSettings, _saveFolderPath + "/" + _settingsName + ".asset" );
    }
}
