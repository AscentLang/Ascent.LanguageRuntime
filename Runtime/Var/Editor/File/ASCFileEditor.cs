using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

[CustomEditor(typeof(ASCImporter))]
public class ASCFileEditor : ScriptedImporterEditor
{
    private string cachedString;
    private string cachedProcessedString;
    public override void OnEnable()
    {
        base.OnEnable();
    }
    public override void OnDisable()
    {
        base.OnDisable();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        ASCInspectorGUI();
    }

    public override bool showImportedObject => false;

    private void ASCInspectorGUI()
    {
        if (assetTarget == null || !(assetTarget is ASCAsset) || (assetTarget as ASCAsset) == null)
        {
            GUILayout.Label("Invalid ASCAsset! Please Reimport");
            if (GUILayout.Button("Reimport"))
            {
                AssetDatabase.ImportAsset((serializedObject.targetObject as ASCImporter).assetPath);
            }
            ApplyRevertGUI();
            return;
        }
        EditorGUILayout.Space(3);
        var style = new GUIStyle(GUI.skin.button);
        style.fontSize = 40;
        style.richText = true;
        style.fixedHeight = 50;
        EditorGUILayout.LabelField("<color=#E84855>Ascent</color> <color=#272635>Script  <size=15><i>Viewer</i></size></color>", style);

        EditorGUILayout.Space(60);

        style = new GUIStyle(GUI.skin.textField);
        style.fontSize = 15;
        style.padding = new RectOffset(10, 10, 10, 10);
        style.normal.textColor = Color.white;
        style.hover.textColor = Color.white;
        style.fontStyle = FontStyle.Normal;
        style.alignment = TextAnchor.UpperLeft;
        style.richText = true;
        var content = (assetTarget as ASCAsset).text;
        if (cachedString != content)
        {
            cachedString = content;
            cachedProcessedString = AscentProcessor.Process(cachedString);
        }
        style.fixedHeight = style.CalcHeight(new GUIContent(cachedProcessedString), 1000);
        EditorGUILayout.LabelField(cachedProcessedString, style);

        EditorGUILayout.Space(style.fixedHeight);

        style = new GUIStyle(GUI.skin.button);
        style.fontSize = 15;
        style.fixedWidth = 60;
        style.fontStyle = FontStyle.Italic;

        if (GUILayout.Button("Edit", style))
        {
            EditorUtility.OpenWithDefaultApp((serializedObject.targetObject as ASCImporter).assetPath);
        }

        ApplyRevertGUI();
    }

    public override Texture2D RenderStaticPreview(string assetPath, UnityEngine.Object[] subAssets, int width, int height)
    {
        Texture2D tex = EditorResourcesHolder.Instance.FileIcon;
        return tex;
    }

    protected override void OnHeaderGUI()
    {
    }
}