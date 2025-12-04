using UnityEditor;
using UnityEngine;

namespace Quizly
{
    public class DatabaseCreator : EditorWindow
    {
        private string _newDBName = "";

        private string _errorMessage; 
        
        [MenuItem("Window/Quizly/Database Creator")]
        public static void Create()
        {
            DatabaseCreator win = GetWindow<DatabaseCreator>();
        }

        private void OnGUI()
        {
            GUILayout.Label("Database Manager");
            
            GUILayout.Label("Create Database");
            
            GUILayout.BeginHorizontal();
            
            GUILayout.Label("Name: ");
            _newDBName = EditorGUILayout.TextField(_newDBName);
            
            GUILayout.EndHorizontal();
            
            _newDBName = _newDBName.Replace("/", "_");
            _newDBName = _newDBName.Replace(".", "_");
            _newDBName = _newDBName.Replace(" ", "_");
            _newDBName = _newDBName.ToLower();

            if (GUILayout.Button("Create"))
            {
                if (string.IsNullOrEmpty(_newDBName.Trim()))
                {
                    _errorMessage = "Name cannot be empty.";
                    return; 
                }
                
                if (DBManager.TryCreateDatabase(_newDBName))
                {
                    _errorMessage = "Database created";
                    _newDBName = "";
                }
                else
                {
                    _errorMessage = "Could not create database";
                }
            }
            
            GUILayout.BeginHorizontal();
            GUILayout.Label("Name ");
            GUILayout.EndHorizontal();

            foreach (string db in DBManager.GetDatabaseNames())
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(db.Split('.')[0]);

                if (GUILayout.Button("Load"))
                {
                    DBManager.LoadDatabase(db.Split('.')[0]);
                    GUILayout.EndHorizontal();
                }
                else
                {
                    GUILayout.EndHorizontal();
                }
            }
            
            GUILayout.Label(_errorMessage);
        }
    }
}
