using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ASCCallbacks : Editor
{
    [InitializeOnLoadMethod]
    static void OnProjectLoadedInEditor()
    {
        EditorApplication.projectWindowItemOnGUI -= IconGUI;
        EditorApplication.projectWindowItemOnGUI += IconGUI;

        EditorApplication.hierarchyWindowItemOnGUI -= HierarchyWindowItemCallback;
        EditorApplication.hierarchyWindowItemOnGUI += HierarchyWindowItemCallback;

        var dropHandler = (DragAndDrop.InspectorDropHandler)OnInspectorDrop;

        if (!DragAndDrop.HasHandler("Inspector".GetHashCode(), dropHandler))
            DragAndDrop.AddDropHandler(dropHandler);
    }

    private static DragAndDropVisualMode OnInspectorDrop(object[] targets, bool perform)
    {
        if (perform)
        {
            foreach (var objectRef in DragAndDrop.objectReferences)
            {
                if (objectRef is ASCAsset asset)
                {
                    var path = AssetDatabase.GetAssetPath(asset);
                    // Check if .asc
                    if (path.EndsWith(".asc"))
                    {
                        foreach (var target in targets)
                        {
                            if (target is GameObject gameObject)
                            {
                                var wrapper = gameObject.AddComponent<AscentScriptWrapper>();

                                wrapper.Asset = asset;
                            }
                        }
                    }
                }
            }
        }
        return DragAndDropVisualMode.Generic;
    }

    static void HierarchyWindowItemCallback(int pID, Rect pRect)
    {
        if (Event.current.type == EventType.DragUpdated)
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
            DragAndDrop.AcceptDrag();
            Event.current.Use();

        }
        if (Event.current.type == EventType.DragPerform)
        {
            DragAndDrop.AcceptDrag();

            var selectedObjects = new List<GameObject>();

            foreach (var objectRef in DragAndDrop.objectReferences)
            {
                if (objectRef is ASCAsset asset)
                {
                    var path = AssetDatabase.GetAssetPath(asset);
                    // Check if .asc
                    if (path.EndsWith(".asc"))
                    {
                        var hoveredObject = HierarchyInitialization.GetHoveredObject();
                        GameObject obj;
                        if (hoveredObject is GameObject gameObject)
                        {
                            obj = gameObject;
                        }
                        else
                        {
                            // we create a new GameObject using the asset's name.
                            obj = new GameObject(objectRef.name);
                        }

                        var wrapper = obj.AddComponent<AscentScriptWrapper>();
                        wrapper.Asset = asset;

                        selectedObjects.Add(obj);
                    }
                }
            }

            // we didn't drag any assets of type AssetX, so do nothing.
            if (selectedObjects.Count == 0) return;

            // emulate selection of newly created objects.
            Selection.objects = selectedObjects.ToArray();

            // make sure this call is the only one that processes the event.
            Event.current.Use();
        }
    }

    static void IconGUI(string s, Rect r)
    {
        string fileName = AssetDatabase.GUIDToAssetPath(s);
        int index = fileName.LastIndexOf('.');
        if (index == -1) return;
        string fileType = fileName.Substring(fileName.LastIndexOf(".") + 1);
        r.width = r.height;
        switch (fileType)
        {
            case "asc":
                Texture2D tex = EditorResourcesHolder.Instance.FileIcon;
                r.width -= 10;
                r.height -= 10;
                GUI.DrawTexture(r, tex);
                break;
        }
    }
}