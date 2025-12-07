using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Quizly
{
    public class CategoryManager : EditorWindow
    {
        private Vector2 _scrollPos; 
        
        private string newCategory = "";
        private List<string> categories = new List<string>();
        private string _errorMessage = "";
    
        [MenuItem("Window/Quizly/Create/Category")]
        public static void Create()
        {
            CategoryManager win = GetWindow<CategoryManager>();
        }

        private void OnGUI()
        {
            GUILayout.Label("Category Manager", StyleLibrary.Header2Style);

            #region AddCategory

            GUILayout.Label("Add Category", StyleLibrary.Header3Style);
            
            GUILayout.BeginHorizontal();
            GUILayout.Space(15);
            GUILayout.Label("Name: ");
            newCategory = GUILayout.TextField(newCategory, GUILayout.ExpandWidth(true));
            GUILayout.Space(10);
            GUILayout.EndHorizontal();
            
            GUILayout.Space(15);
            
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Add", GUILayout.Width(65), GUILayout.Height(25)))
            {
                DBManager.SaveObject(newCategory);
                newCategory = "";
                LoadCategories();
            }
            GUILayout.Space(10);
            GUILayout.EndHorizontal();

            #endregion
            
            GUILayout.Space(25);

            #region CategoryList

            GUILayout.BeginHorizontal();
            
            GUILayout.Label("All Categories", StyleLibrary.Header3Style);
            
            if (GUILayout.Button("Refresh", GUILayout.Width(85), GUILayout.Height(23)))
            {
                LoadCategories();
                GUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.EndHorizontal();
            }
            
            GUILayout.Space(10);
            _scrollPos = GUILayout.BeginScrollView(_scrollPos);
            
            foreach (string cat in categories)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(25);
                GUILayout.Label(cat);
                
                if (GUILayout.Button("-", GUILayout.Width(35)))
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
                    GUILayout.Space(15);
                    GUILayout.EndHorizontal();
                }
                
                GUILayout.Space(5);
            }
            
            GUILayout.EndScrollView();

            #endregion
            
            GUILayout.Label(_errorMessage, StyleLibrary.BottomMessage);
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

