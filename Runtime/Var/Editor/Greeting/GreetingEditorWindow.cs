using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GreetingEditorWindow : EditorWindow
{
    [MenuItem("Ascent/Welcome", false, 0)]
    public static void ShowWindow()
    {
        GetWindow<GreetingEditorWindow>("Welcome to Ascent");
    }

    private void OnGUI()
    {
        GUILayout.Label("Welcome to My Package!", EditorStyles.boldLabel);
        GUILayout.Label("This window is shown the first time the package is installed.");
        if (GUILayout.Button("Close"))
        {
            this.Close();
        }
    }
}
