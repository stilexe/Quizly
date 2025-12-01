using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Quizly
{
    public class CategoryManager : EditorWindow
    {
        private string newCategory = "";
        private List<string> categories = new List<string>();
    
        [MenuItem("Window/Quizly/Category Manager")]
        public static void Create()
        {
            CategoryManager win = GetWindow<CategoryManager>();
        }

        private void OnGUI()
        {
            GUILayout.Label("Category Manager");

            // ADD CATEGORY 
            GUILayout.Label("Add Category");
            
            GUILayout.BeginHorizontal();
            
            GUILayout.Label("Name: ");
            newCategory = GUILayout.TextField(newCategory);
            
            GUILayout.EndHorizontal();
            
            if (GUILayout.Button("Add"))
            {
                DatabaseManager.SaveQuery(newCategory);
                newCategory = "";
                LoadCategories();
            }
            
            //CATEGORY LIST 
            
            GUILayout.BeginHorizontal();
            
            GUILayout.Label("All Categories");
            
            if (GUILayout.Button("Refresh"))
            {
                LoadCategories();
                GUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.EndHorizontal();
            }
            
            foreach (string cat in categories)
            {
                GUILayout.BeginHorizontal();
                
                GUILayout.Label(cat);
                
                if (GUILayout.Button("-"))
                {
                    DatabaseManager.RemoveQuery(cat);
                    GUILayout.EndHorizontal();
                }
                
                GUILayout.EndHorizontal();
            }
        }

        private void LoadCategories()
        {
            categories.Clear();
            
            foreach (object o in DatabaseManager.FindMatching(DatabaseManager.Table.Categories))
            {
                categories.Add((string)o);
            }
        }
    }
}

