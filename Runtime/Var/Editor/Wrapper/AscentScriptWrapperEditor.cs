using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AscentScriptWrapper))]
public class AscentScriptWrapperEditor : Editor
{
    private static IEnumerable<Type> GetBaseClasses(Type type, bool includeSelf = false)
    {
        if (type == null || type.BaseType == null)
        {
            yield break;
        }

        if (includeSelf)
        {
            yield return type;
        }

        var current = type.BaseType;

        while (current != null)
        {
            yield return current;
            current = current.BaseType;
        }
    }

    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();
        serializedObject.Update();

        var style = new GUIStyle(GUI.skin.button);
        style.fontSize = 40;
        style.richText = true;
        style.fixedHeight = 50;
        GUILayout.BeginHorizontal();
        GUILayout.Label("<color=#E84855>Ascent</color> <color=#272635>Script  <size=15><i>Wrapper</i></size></color>", style);
        GUILayout.EndHorizontal();

        GUILayout.Space(25);

        AscentScriptWrapper script = (AscentScriptWrapper)target;

        GUILayout.BeginHorizontal();

        // Label (Icon)
        GUILayout.BeginVertical();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button(EditorResourcesHolder.Instance.FileIcon, new GUIStyle(EditorStyles.label), GUILayout.Width(50), GUILayout.Height(50)))
        {
            Selection.SetActiveObjectWithContext(script.Asset, null);
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndVertical();

        // ObjectField
        GUILayout.BeginVertical();
        GUILayout.FlexibleSpace();
        script.Asset = EditorGUILayout.ObjectField(script.Asset, typeof(ASCAsset), false, GUILayout.Width(200), GUILayout.Height(20)) as ASCAsset;
        GUILayout.FlexibleSpace();
        GUILayout.EndVertical();

        // Label (Attached Script)
        GUILayout.BeginVertical();
        GUILayout.FlexibleSpace();
        GUILayout.Label("  <-- Attached Script", new GUIStyle(EditorStyles.boldLabel) { fontStyle = FontStyle.BoldAndItalic, fontSize = 15 });
        GUILayout.FlexibleSpace();
        GUILayout.EndVertical();

        GUILayout.FlexibleSpace();

        GUILayout.EndHorizontal();

        GUILayout.Space(25);

        if (script.Asset == null)
        {
            EditorGUILayout.LabelField("Please attach an ASCAsset to this script");
            return;
        }

        GUILayout.Label("Imports", new GUIStyle(EditorStyles.boldLabel) { fontStyle = FontStyle.BoldAndItalic, fontSize = 15 });

        var predicates = script.Asset.predicates;
        for (int i = 0; i < script.Asset.imports.Length; i++)
        {
            var import = script.Asset.imports[i].Split("^");
            var name = import[0];
            var type = import[1];

            var sysType = Matcher.GetType(type, predicates);

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(name, GUILayout.Width(200));

            if (sysType == null)
            {
                EditorGUILayout.LabelField("Type not found!");
                GUILayout.EndHorizontal();
                continue;
            }

            if (!GetBaseClasses(sysType).Contains(typeof(UnityEngine.Object)))
            {
                ImportVar importVar = null;

                for (int x = 0; x < script.ImportVars.Count; x++)
                {
                    var importVarTemp = script.ImportVars[x];
                    if (importVarTemp.name == name)
                    {
                        importVar = importVarTemp;
                        break;
                    }
                }

                ImportVar.ImportType importType;
                object defaultVal = null;
                switch (type)
                {
                    case "float":
                        importType = ImportVar.ImportType.Float;
                        defaultVal = 0f;
                        break;
                    case "string":
                        importType = ImportVar.ImportType.String;
                        defaultVal = "";
                        break;
                    case "bool":
                        importType = ImportVar.ImportType.Bool;
                        defaultVal = false;
                        break;
                    default:
                        importType = ImportVar.ImportType.String;
                        defaultVal = "";
                        break;
                };

                if (sysType == typeof(Vector2))
                {
                    importType = ImportVar.ImportType.Vector2;
                    defaultVal = new Vector2();
                }

                if (sysType == typeof(Vector3))
                {
                    importType = ImportVar.ImportType.Vector3;
                    defaultVal = new Vector3();
                }

                if (sysType == typeof(Color))
                {
                    importType = ImportVar.ImportType.Color;
                    defaultVal = new Color();
                }

                if (importVar == null || importVar.type != importType)
                {
                    var old = importVar;
                    script.ImportVars.Remove(importVar);
                    importVar = new ImportVar { name = name, type = importType };
                    importVar.Set(old == null ? defaultVal : old.value);
                    script.ImportVars.Add(importVar);
                }

                importVar.Set(DrawDynamicValuePicker("", sysType, importVar.Get()));
            }
            else
            {
                ImportVarUnity importVarUnity = null;

                for (int x = 0; x < script.UnityImportVars.Count; x++)
                {
                    var importVarTemp = script.UnityImportVars[x];
                    if (importVarTemp.name == name)
                    {
                        importVarUnity = importVarTemp;
                        break;
                    }
                }

                if (importVarUnity == null)
                {
                    importVarUnity = new ImportVarUnity { name = name };
                    importVarUnity.Set(null);
                    script.UnityImportVars.Add(importVarUnity);
                }

                importVarUnity.Set(EditorGUILayout.ObjectField(importVarUnity.value, sysType, true));
            }

            GUILayout.EndHorizontal();

            GUILayout.Space(5);
        }

        //Prune import vars that no longer exist
        for (int i = 0; i < script.ImportVars.Count; i++)
        {
            var importVar = script.ImportVars[i];
            var exists = false;
            for (int x = 0; x < script.Asset.imports.Length; x++)
            {
                var import = script.Asset.imports[x].Split("^");
                var name = import[0];
                if (importVar.name == name)
                {
                    exists = true;
                    break;
                }
            }
            if (!exists)
            {
                script.ImportVars.Remove(importVar);
                i--;
            }
        }

        serializedObject.ApplyModifiedProperties();
    }

    private object DrawDynamicValuePicker(string label, Type type, object value)
    {
        if (type == typeof(float))
        {
            return EditorGUILayout.FloatField(label, (value == null) ? 0f : (float)value);
        }
        else if (type == typeof(int))
        {
            return EditorGUILayout.IntField(label, (value == null) ? 0 : (int)value);
        }
        else if (type == typeof(bool))
        {
            return EditorGUILayout.Toggle(label, (value == null) ? false : (bool)value);
        }
        else if (type == typeof(string))
        {
            return EditorGUILayout.TextField(label, (value == null) ? "" : (string)value);
        }
        else if (type == typeof(Vector2))
        {
            return EditorGUILayout.Vector2Field(label, (value == null) ? new Vector2() : (Vector2)value);
        }
        else if (type == typeof(Vector3))
        {
            return EditorGUILayout.Vector3Field(label, (value == null) ? new Vector3() : (Vector3)value);
        }
        else if (type == typeof(Color))
        {
            return EditorGUILayout.ColorField(label, (value == null) ? new Color() : (Color)value);
        }
        else if (typeof(UnityEngine.Object).IsAssignableFrom(type))
        {
            return EditorGUILayout.ObjectField(label, (UnityEngine.Object)value, type, true);
        }
        else
        {
            EditorGUILayout.LabelField(label, "Unsupported type");
            return null;
        }
    }
}
