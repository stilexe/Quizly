using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Quizly
{
    public class ImporterWindow : EditorWindow
    {
        private Object _toImport;
        private string _assetPath; 
        private string _errorMessage; 
        
        [MenuItem("Window/Quizly/CSV/Importer")]
        public static void Create()
        {
            ImporterWindow win = GetWindow<ImporterWindow>();
            win.titleContent = new GUIContent("CSV Importer");
        }
        
        private void OnGUI()
        {
            GUILayout.Label("Import CSVs", StyleLibrary.Header2Style);
            GUILayout.Space(10);
            
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            //export template buttons 
            if (GUILayout.Button("Export Templates", GUILayout.Width(position.width * .85f)))
            {
                CSVExporter.ExportTemplates();
                GUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
            
            GUILayout.Space(15);

            GUILayout.Label("Import Data", StyleLibrary.Header3Style);
            
            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            GUILayout.Label("File: ");
            
            //select file section 
            _toImport = EditorGUILayout.ObjectField(_toImport, typeof(Object) , false);
            GUILayout.Space(15);
            GUILayout.EndHorizontal();
            
            _assetPath = AssetDatabase.GetAssetPath(_toImport);
            
            if (!string.IsNullOrEmpty(_assetPath))
            {
                if (!_assetPath.Contains('.') || _assetPath.Split('.')[1] != "csv")
                {
                    _errorMessage = "Must be CSV file.";
                    _assetPath = "";
                    _toImport = null;
                }
                else
                {
                    _errorMessage = "";
                }
                
                GUILayout.BeginHorizontal();
                GUILayout.Label("File Path: ");
                GUILayout.Label(_assetPath);
                GUILayout.EndHorizontal();
            }
            
            GUILayout.Space(20);
            
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
                
            //import button 
            if (GUILayout.Button("Import", GUILayout.Width(position.width * .85f), GUILayout.Height(25)))
            {
                if (_toImport is not null)
                {
                    CSVImporter.ImportCsv(Directory.GetCurrentDirectory() +"/"+ AssetDatabase.GetAssetPath(_toImport));
                }
                
                GUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
            
            GUILayout.Label(_errorMessage, StyleLibrary.BottomMessage);
        }
    }
}
