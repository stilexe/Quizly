using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Quizly
{
    public class ExporterWindow : EditorWindow
    {
        private DBManager.Table _exportTable = DBManager.Table.None;
        private Object _saveFolder;
        
        private string _errorMessage;
        private Dictionary<string, string> _exportConditions = new Dictionary<string, string>();

        private string _searchName, _searchCategory, _searchQuiz, _searchUser, _fileName;
        private int _searchCatID, _searchDifficulty;

        private Vector2 _resultsPos;
        
        [MenuItem("Window/Quizly/CSV/Exporter")]
        public static void Create()
        {
            ExporterWindow win = GetWindow<ExporterWindow>();
            
            win.titleContent = new GUIContent("Exporter");
        }

        private void OnGUI()
        {
            GUILayout.Label("CSV Exporter", StyleLibrary.Header2Style);
            
            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            GUILayout.BeginVertical();

            #region ExportOptions

            GUILayout.Label("Export Settings", StyleLibrary.Header4Style);
            
            GUILayout.BeginHorizontal();
            GUILayout.Label("File name: ");
            _fileName = GUILayout.TextField(_fileName);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Folder: ");
            _saveFolder = EditorGUILayout.ObjectField(_saveFolder, typeof(Object), false);
            GUILayout.EndHorizontal();
            GUILayout.Space(5);

            if (AssetDatabase.GetAssetPath(_saveFolder).Contains('.'))
            {
                _errorMessage = "Must put a folder in folder field."; 
                _saveFolder = null; 
            }
            
            GUILayout.BeginHorizontal();
            GUILayout.Label("Table: ");
            if (EditorGUILayout.DropdownButton(new GUIContent(_exportTable.ToString()), FocusType.Keyboard))
            {
                GenericMenu menu = new GenericMenu();

                foreach (DBManager.Table table in DBManager.GetTables())
                {
                    if (table == DBManager.Table.Weightings)
                    {
                        continue;
                    }
                    menu.AddItem(new GUIContent(table.ToString()), false, () => { _exportTable = table; });
                }
                
                menu.ShowAsContext();
            }
            GUILayout.EndHorizontal();

            #endregion
            
            GUILayout.Space(5);

            if (DBManager.DatabaseLoaded())
            {
                switch (_exportTable)
                {
                    case DBManager.Table.Categories:
                        GUILayout.Label("Category Search", StyleLibrary.Header4Style);
                        _searchName = EditorGUILayout.TextField("Name: ", _searchName);
                        break; 
                    case DBManager.Table.Questions:
                        GUILayout.Label("Question Search", StyleLibrary.Header4Style);
                        //question search 
                        _searchName = EditorGUILayout.TextField("Question: ", _searchName);
                        //category select 
                        if (EditorGUILayout.DropdownButton(new GUIContent(_searchCategory), FocusType.Keyboard))
                        {
                            GenericMenu menu = new GenericMenu();

                            foreach (string category in DBManager.FindValues(DBManager.Table.Categories, "name"))
                            {
                                menu.AddItem(new GUIContent(category), false, () => { _searchCategory = category; });
                            }
                        
                            menu.ShowAsContext();
                        }
                        GUILayout.Space(5);
                        //difficulty slider 
                        _searchDifficulty = EditorGUILayout.IntSlider(new GUIContent("Difficulty: "), _searchDifficulty, 0, 5);
                        break;
                    case DBManager.Table.Results:
                        GUILayout.Label("Results Search", StyleLibrary.Header4Style);
                    
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(10);
                        GUILayout.Label("User: ");
                        if (EditorGUILayout.DropdownButton(new GUIContent(_searchUser), FocusType.Keyboard))
                        {
                            GenericMenu menu = new GenericMenu();
                            
                            menu.AddItem(new GUIContent("None"), false, () => { _searchUser = ""; });

                            foreach (string s in DBManager.FindValues(DBManager.Table.Users, "username"))
                            {
                                menu.AddItem(new GUIContent(string.Concat(s.ToUpper()[0], s[1..])), false, () => { _searchUser = s; });
                            }
                            menu.ShowAsContext();
                        }
                        GUILayout.Space(10);
                        GUILayout.EndHorizontal();
                    
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(10);
                        GUILayout.Label("Quiz: ");
                        if (EditorGUILayout.DropdownButton(new GUIContent(_searchQuiz), FocusType.Keyboard))
                        {
                            GenericMenu menu = new GenericMenu();
                            
                            menu.AddItem(new GUIContent("None"), false, () => { _searchQuiz = ""; });

                            foreach (string s in DBManager.FindValues(DBManager.Table.Quizzes, "name"))
                            {
                                menu.AddItem(new GUIContent(string.Concat(s.ToUpper()[0], s[1..])), false, () => { _searchQuiz = s; });
                            }
                        
                            menu.ShowAsContext();
                        }
                        GUILayout.Space(10);
                        GUILayout.EndHorizontal();
                    
                        break; 
                    case DBManager.Table.Users:
                        GUILayout.Label("User Search", StyleLibrary.Header4Style);
                        _searchName = EditorGUILayout.TextField("Username: ", _searchName);
                        break; 
                    case DBManager.Table.Quizzes:
                        GUILayout.Label("Quiz Search", StyleLibrary.Header4Style);
                        _searchName = EditorGUILayout.TextField("Name: ", _searchName);
                        break;
                }
            }
            
            GUILayout.EndVertical();
            GUILayout.Space(10);
            GUILayout.EndHorizontal();

            #region DataPreview 

            _resultsPos = GUILayout.BeginScrollView(_resultsPos);
            
            GUILayout.EndScrollView();

            #endregion

            GUILayout.Space(15);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Export", GUILayout.Height(30), GUILayout.Width(position.width * .85f)))
            {
                if (_saveFolder is null)
                {
                    _errorMessage = "Please select a folder first.";
                    GUILayout.EndHorizontal();
                }
                else if (_exportTable == DBManager.Table.None)
                {
                    _errorMessage = "Please select a table to export.";
                    GUILayout.EndHorizontal();
                }
                else if (string.IsNullOrEmpty(_fileName))
                {
                    _errorMessage = "Enter a file name to export.";
                    GUILayout.EndHorizontal();
                }
                else
                {
                    ExportTable();
                    GUILayout.EndHorizontal();
                }
                
                //GUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
            
            GUILayout.Label(_errorMessage, StyleLibrary.BottomMessage);
        }

        private void ExportTable()
        {
            _exportConditions = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(_searchName))
            {
                switch (_exportTable)
                {
                    case DBManager.Table.Categories:
                        _exportConditions.Add("name", _searchName);
                        break;
                    case DBManager.Table.Results:
                        _exportConditions.Add("user_id", _searchName);
                        break;
                    case DBManager.Table.Users:
                        _exportConditions.Add("username", _searchUser);
                        break;
                    case DBManager.Table.Quizzes:
                        _exportConditions.Add("name", _searchName);
                        break;
                    case DBManager.Table.Questions:
                        _exportConditions.Add("question_text", _searchName);
                        break;
                }
            }

            if (!string.IsNullOrEmpty(_searchCategory))
            {
                switch (_exportTable)
                {
                    case DBManager.Table.Questions:
                        _exportConditions.Add("category_id", DBManager.FindID(DBManager.Table.Categories, "name", _searchCategory).ToString());
                        break; 
                }
            }

            if (_searchDifficulty > 0)
            {
                switch (_exportTable)
                {
                    case DBManager.Table.Questions:
                        _exportConditions.Add("difficulty", _searchDifficulty.ToString());
                        break;
                }
            }

            if (!string.IsNullOrEmpty(_searchQuiz))
            {
                switch (_exportTable)
                {
                    case DBManager.Table.Results:
                        _exportConditions.Add("quiz_id", DBManager.FindID(DBManager.Table.Quizzes, "name", _searchQuiz).ToString());
                        break;
                }
            }
            
            List<object> toExport = DBManager.FindMatching(_exportTable, _exportConditions, false);

            string savePath = AssetDatabase.GetAssetPath(_saveFolder);
            savePath = savePath[savePath.IndexOf('/')..];
            savePath = string.Concat(Application.dataPath, savePath, "/", _fileName + ".csv");
            
            CSVExporter.ExportData(_exportTable, toExport, savePath);
        }
    }
}
