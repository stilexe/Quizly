using System.IO;
using UnityEditor;
using UnityEngine;

namespace Quizly
{
    public class ImporterWindow : EditorWindow
    {
        private Object _toImport;
        private string _assetPath; 
        private string _errorMessage; 
        
        [MenuItem("Window/Quizly/Importer")]
        public static void Create()
        {
            ImporterWindow win = GetWindow<ImporterWindow>();
        }
        private void OnGUI()
        {
            //export template buttons 
            if (GUILayout.Button("Export Templates"))
            {
                CSVExporter.ExportTemplates();
            }

            GUILayout.Label("Import Data");
            GUILayout.BeginHorizontal();
            GUILayout.Label("File: ");
            
            //select file section 
            _toImport = EditorGUILayout.ObjectField(_toImport, typeof(Object) , false);
            
            GUILayout.EndHorizontal();
            
            _assetPath = AssetDatabase.GetAssetPath(_toImport);
            
            if (!string.IsNullOrEmpty(_assetPath))
            {
                if (_assetPath.Split('.')[1] != "csv")
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
                
            //import button 
            if (GUILayout.Button("Import"))
            {
                if (_toImport is not null)
                {
                    CSVImporter.ImportCsv(Directory.GetCurrentDirectory() +"/"+ AssetDatabase.GetAssetPath(_toImport));
                }
            }
            
            GUILayout.Label(_errorMessage);
        }
    }
}
