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
                AddCategory();
            }
            
            //CATEGORY LIST 
            
            GUILayout.BeginHorizontal();
            
            GUILayout.Label("All Categories");
            
            if (GUILayout.Button("Refresh"))
            {
                LoadCategories();
                GUILayout.EndHorizontal();
            }
            
            GUILayout.EndHorizontal();
            
            foreach (string cat in categories)
            {
                GUILayout.BeginHorizontal();
                
                GUILayout.Label(cat);
                
                if (GUILayout.Button("-"))
                {
                    RemoveCategory(cat);
                    GUILayout.EndHorizontal();
                }
                
                GUILayout.EndHorizontal();
            }
        }

        private void AddCategory()
        {
            DatabaseManager.SaveQuery(newCategory);
            newCategory = "";
            LoadCategories();
        }

        private void LoadCategories()
        {
            categories = DatabaseManager.SearchQuery(DatabaseManager.Table.Categories);
        }

        private void RemoveCategory(string cat)
        {
            
        }
    }
}

