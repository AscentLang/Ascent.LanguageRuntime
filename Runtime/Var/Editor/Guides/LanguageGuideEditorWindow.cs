using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class LanguageGuideEditorWindow : EditorWindow
{
    private Vector2 scrollPos = Vector2.zero;

    [MenuItem("Ascent/Language Guide", false, 1)]
    public static void ShowWindow()
    {
        GetWindow<LanguageGuideEditorWindow>("Language Guide");
    }

    private void OnGUI()
    {
        minSize = new Vector2(350f, 50f);
        var style = new GUIStyle(GUI.skin.scrollView);
        //style.margin = new RectOffset(10, 0, 0, 0);
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, style);

        style = new GUIStyle(GUI.skin.button);
        style.fontSize = 40;
        style.richText = true;
        style.fixedHeight = 100;
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("<i><b>Welcome To</b></i>\n<color=#E84855>Ascent</color> <color=#272635>Script</color>", style);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(120);

        EditorGUILayout.LabelField("This guid will server as a reference for the language features of AscentScript.", new GUIStyle(EditorStyles.wordWrappedLabel) { fontStyle = FontStyle.Bold });

        EditorGUILayout.EndScrollView();
    }
}
