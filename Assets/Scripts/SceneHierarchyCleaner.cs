using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;

public class SceneHierarchyCleaner : EditorWindow
{
    [MenuItem("Tools/Clean Up Scene Hierarchy")]
    public static void ShowWindow()
    {
        //EditorWindow.GetWindow(typeof(SceneHierarchyCleaner));
        GetWindow<SceneHierarchyCleaner>("Scene Hierarchy Cleaner");
    }

    void OnGUI()
    {
        if (GUILayout.Button("Clean Up Hierarchy"))
        {
            CleanHierarchy();
        }
    }

    void CleanHierarchy()
    {
        // Get the active scene
        Scene currentScene = EditorSceneManager.GetActiveScene();

        // Get all the root game objects
        GameObject[] rootGameObjects = currentScene.GetRootGameObjects();

        List<GameObject> rootGameObjectsList = rootGameObjects.ToList();

        rootGameObjectsList.Sort((x, y) => x.name.CompareTo(y.name));
        foreach (GameObject rootGameObject in rootGameObjectsList)
        {

            rootGameObject.transform.SetAsLastSibling();
            
        }

        // Loop through each root game object
        foreach (GameObject rootGameObject in rootGameObjects)
        {
            CleanHierarchy(rootGameObject);
            
        }

        // Mark the scene as dirty
        EditorSceneManager.MarkSceneDirty(currentScene);
    }

    void CleanHierarchy(GameObject root)
    {
        // Dictionary to store game objects under the same parent
        Dictionary<string, List<Transform>> objectsByParent = new Dictionary<string, List<Transform>>();

        // Organize objects based on their parent names
        OrganizeChildren(root.transform, objectsByParent);

        // Sort and rename objects under the same parent
        foreach (var kvp in objectsByParent)
        {
            kvp.Value.Sort((x, y) => x.name.CompareTo(y.name));
            RenameDuplicateGameObjects(kvp.Value);
        }

        // Rearrange the hierarchy based on the sorted order
        foreach (var kvp in objectsByParent)
        {
            foreach (Transform child in kvp.Value)
            {
                child.SetAsLastSibling();
            }
        }
    }

    void OrganizeChildren(Transform parent, Dictionary<string, List<Transform>> objectsByParent)
    {
        if (!PrefabUtility.IsPartOfAnyPrefab(parent.gameObject))
        {
            foreach (Transform child in parent)
            {
                if (child == parent) continue;
                // recursive
                if (child.childCount > 0)
                {
                    OrganizeChildren(child, objectsByParent);
                }

                string parentName = parent != null ? parent.name : "Root";
                if (!objectsByParent.ContainsKey(parentName))
                {
                    objectsByParent[parentName] = new List<Transform>();
                }
                objectsByParent[parentName].Add(child);
            }
        }
    }

    void RenameDuplicateGameObjects(List<Transform> objects)
    {
        Dictionary<string, int> nameCount = new Dictionary<string, int>();

        foreach (Transform obj in objects)
        {
            string originalName = obj.name;
            int count = 0;
            string newName = "";
            
            // delete the number after names
            if (obj.gameObject.name.Contains("("))
            {

                int textNumber = obj.gameObject.name.IndexOf("(");
                int textLength = obj.gameObject.name.Length;


                newName = obj.gameObject.name.Remove(textNumber, textLength - textNumber);
                newName = newName.TrimEnd();
                obj.gameObject.name = newName;
            }
            else
            {
                newName = originalName.TrimEnd();
            }





            // If the name already exists, add a number to make it unique
            if (nameCount.ContainsKey(newName))
            {
                //Debug.Log(newName);
                count = nameCount[newName] + 1;
                obj.name = $"{newName} ({count})";
            }
            else
            {
                obj.name = newName;
            }

            nameCount[newName] = count;

        }
    }
}
