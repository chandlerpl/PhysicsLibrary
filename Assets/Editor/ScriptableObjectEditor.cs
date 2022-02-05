using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

[CustomPropertyDrawer(typeof(ScriptableObject), true)]
public class ScriptableObjectEditor : PropertyDrawer
{
    private static string defaultPath = "Assets/Data/";

   // Cached scriptable object editor
    private Editor editor = null;

    bool rename = false;
    string name = defaultPath;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Draw label
        Rect lab = EditorGUI.PrefixLabel(position, label);
        position.width -= 100;

        if (rename)
        {
            name = EditorGUI.TextField(position, name);
        }
        else
        {
            EditorGUI.PropertyField(position, property);
        }

        bool clonePressed = false;

        bool createPressed;
        // Draw foldout arrow
        if (property.objectReferenceValue != null)
        {
            Rect buttonRect = new Rect(position.x + position.width, position.y, 50, position.height);
            createPressed = GUI.Button(buttonRect, new GUIContent("Create"), EditorStyles.miniButton);
            buttonRect.x += 50;
            clonePressed = GUI.Button(buttonRect, new GUIContent("Clone"), EditorStyles.miniButton);

            property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, GUIContent.none);
        }
        else
        {
            Rect buttonRect = new Rect(position.x + position.width, position.y, 100, position.height);
            createPressed = GUI.Button(buttonRect, new GUIContent("Create"), EditorStyles.miniButton);
        }

        if (clonePressed)
        {
            if (rename)
            {
                property.objectReferenceValue = CloneAsset(AssetDatabase.GetAssetPath(property.objectReferenceValue), name + ".asset");
                name = defaultPath;
                rename = false;
            }
            else
            {
                string path = AssetDatabase.GetAssetPath(property.objectReferenceValue).Replace(".asset", "-Clone");
                name = path;
                rename = true;
            }
        }
        if (createPressed)
        {
            if (rename)
            {
                if (name.Equals(defaultPath))
                {
                    Debug.Log("The name cannot be a folder");
                    name = defaultPath;
                    rename = false;
                    return;
                }
                property.objectReferenceValue = CreateAsset(Type.GetType(property.type.Replace("PPtr<$", "").Replace(">", "")), name);
                name = defaultPath;
                rename = false;
            }
            else
            {
                rename = true;
            }
        }

        // Draw foldout properties
        if (property.isExpanded)
        {
            // Make child fields be indented
            EditorGUI.indentLevel++;

            if (!editor)
                Editor.CreateCachedEditor(property.objectReferenceValue, null, ref editor);

            // Draw object properties
            if (editor) // catches empty property 
            {
                editor.OnInspectorGUI();
            }

            // Set indent back to what it was
            EditorGUI.indentLevel--;
        }
    }

    public static ScriptableObject CreateAsset(Type obj, string name)
    {
        ScriptableObject asset = ScriptableObject.CreateInstance(obj);
        string path = name.Substring(0, name.LastIndexOf('/'));
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        Debug.Log(path);
        Debug.Log(name);
        string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(name + ".asset");
        AssetDatabase.CreateAsset(asset, assetPathAndName);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    
        return asset;
    }

    public static ScriptableObject CloneAsset(string path, string newFile)
    {
        ScriptableObject so = null;
        if(AssetDatabase.CopyAsset(path, newFile))
        {
            so = (ScriptableObject)AssetDatabase.LoadAssetAtPath(newFile, typeof(ScriptableObject));
        }
    
        return so;
    }
}

/*
 * This Editor class is important to the use of the ScriptableObjectEditor, this is due to a bug in Unity which causes an error when repainting the ScriptableObject inspector.
 * In theory this should work exactly like the default Editor and therefore have the same error, but whatever is happening in the background of Unity, that just is not the case.
 * 
 * This is a temporary solution, I am hoping to find a better fix soon.
 */
[CustomEditor(typeof(MonoBehaviour), true, isFallback = true)]
public class MonoBehaviourEditor : Editor { }