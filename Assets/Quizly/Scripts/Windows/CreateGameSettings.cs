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
    
    [MenuItem("Window/Quizly/Create/Game Settings")]
    public static void Create()
    {
        CreateGameSettings win = GetWindow<CreateGameSettings>();
        
        win.titleContent = new GUIContent("Create Settings");
    }

    private void OnGUI()
    {
        GUILayout.Label("Create Settings", StyleLibrary.Header2Style);
        
        GUILayout.BeginHorizontal();
        GUILayout.Space(10);
        GUILayout.Label("Name: ");
        _settingsName = EditorGUILayout.TextField(_settingsName);
        GUILayout.Space(10);
        GUILayout.EndHorizontal();
        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        GUILayout.Space(10);
        GUILayout.Label("Save Folder: ");
        _saveFolder = EditorGUILayout.ObjectField(_saveFolder, typeof(Object) , false);
        GUILayout.Space(10);
        GUILayout.EndHorizontal();
        
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
            GUILayout.Space(15);
            GUILayout.Label("File Path: ");
            GUILayout.Label(_saveFolderPath);
            GUILayout.EndHorizontal();
        }
        
        GUILayout.Space(10);
        
        GUILayout.BeginHorizontal();
        GUILayout.Space(10);
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
        GUILayout.Space(10);
        GUILayout.EndHorizontal();
        
        GUILayout.Space(15);
        
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Save Settings", GUILayout.Height(30), GUILayout.Width(position.width * .85f)))
        {
            SaveSettings();
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        
        GUILayout.Label(_errorMessage, StyleLibrary.BottomMessage);
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
