using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Quizly
{
    public class CategoryManager : EditorWindow
    {
        private string newCategory = "";
        private List<string> categories = new List<string>();
        private string _errorMessage = "";
    
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
                DBManager.SaveObject(newCategory);
                newCategory = "";
                LoadCategories();
            }
            
            GUILayout.Label(_errorMessage);
            
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
                    int id = DBManager.FindID(DBManager.Table.Categories, "name", cat); 
                    
                    if(!DBManager.CanBeRemoved(DBManager.Table.Categories, id))
                    {
                        _errorMessage = "Category being used in questions.";
                    }
                    else
                    {
                        DBManager.RemoveData(DBManager.Table.Categories, id);
                    }
                    
                    GUILayout.EndHorizontal();
                }
                else
                {
                    GUILayout.EndHorizontal();
                }
            }
        }

        private void LoadCategories()
        {
            categories.Clear();
            
            List<object> toAdd = DBManager.FindMatching(DBManager.Table.Categories);

            if (toAdd is null) return; 
            
            foreach (object o in DBManager.FindMatching(DBManager.Table.Categories))
            {
                categories.Add((string)o); 
            }
        }
    }
}

