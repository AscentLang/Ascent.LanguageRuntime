using AscentLanguage.Splitter;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

public class AscentTypeMatcherEditorWindow : EditorWindow
{
    private string matchTesterPredicates = string.Empty;
    private string matchTester = string.Empty;
    private Vector2 scrollPos = Vector2.zero;

    [MenuItem("Ascent/Tools/Type Matching Debugger", false, 1)]
    public static void ShowExample()
    {
        AscentTypeMatcherEditorWindow wnd = GetWindow<AscentTypeMatcherEditorWindow>();
        wnd.titleContent = new GUIContent("Ascent Type Matching Debugger");
    }

    private void OnGUI()
    {
        var style = new GUIStyle(GUI.skin.scrollView);
        scrollPos = GUILayout.BeginScrollView(scrollPos, style);

        style = new GUIStyle(GUI.skin.button);
        style.fontSize = 40;
        style.richText = true;
        style.fixedHeight = 50;
        GUILayout.BeginHorizontal();
        GUILayout.Label("<color=#E84855>Ascent</color> <color=#272635>Script  <size=15><i>Type Match Debugger</i></size></color>", style);
        GUILayout.EndHorizontal();

        var textFieldstyle = new GUIStyle(GUI.skin.textField);
        textFieldstyle.fontSize = 15;
        textFieldstyle.padding = new RectOffset(10, 10, 10, 10);
        textFieldstyle.normal.textColor = Color.white;
        textFieldstyle.hover.textColor = Color.white;
        textFieldstyle.fontStyle = FontStyle.Normal;
        textFieldstyle.alignment = TextAnchor.UpperLeft;
        textFieldstyle.richText = true;

        StringBuilder matchedTypeBuilder = new StringBuilder();

        var matchedTypes = Matcher.ReadOnlyMatchedQualifiedTypes;

        foreach (var matchedType in matchedTypes)
        {
            matchedTypeBuilder.AppendLine(matchedType);
        }

        GUILayout.Label(matchedTypeBuilder.ToString().Trim(), textFieldstyle);

        style = new GUIStyle(GUI.skin.label);
        GUILayout.Label("Total Matched Types: " + matchedTypes.Count, style);

        GUILayout.Space(20);

        GUILayout.Label("Match Tester", new GUIStyle(EditorStyles.boldLabel) { fontStyle = FontStyle.BoldAndItalic, fontSize = 20 });

        matchTesterPredicates = EditorGUILayout.TextField("Predicates (, Seperated)", matchTesterPredicates);

        matchTester = EditorGUILayout.TextField("Type Name", matchTester);

        var types = Matcher.GetTypesFromPredicates(matchTesterPredicates.Split(','));

        var type = Matcher.GetType(matchTester, matchTesterPredicates.Split(","));

        if (type != null)
        {
            GUILayout.Label("Matched Type", new GUIStyle(EditorStyles.boldLabel) { fontStyle = FontStyle.BoldAndItalic, fontSize = 15 });

            GUILayout.Label(type.FullName, textFieldstyle);
        }

        StringBuilder testingTypeBuilder = new StringBuilder();

        foreach (var matchedType in types)
        {
            testingTypeBuilder.AppendLine(matchedType);
        }

        GUILayout.Label("Matched Types", new GUIStyle(EditorStyles.boldLabel) { fontStyle = FontStyle.BoldAndItalic, fontSize = 20 });

        GUILayout.Label(testingTypeBuilder.ToString().Trim(), textFieldstyle);

        GUILayout.EndScrollView();
    }
}