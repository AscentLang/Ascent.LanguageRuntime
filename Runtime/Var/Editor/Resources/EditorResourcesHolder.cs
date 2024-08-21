using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EditorResources", menuName = "Ascent/EditorResources", order = 1)]
public class EditorResourcesHolder : ScriptableObject
{
    public Texture2D FileIcon;

    public static EditorResourcesHolder Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<EditorResourcesHolder>("EditorResources");
            }
            return _instance;
        }
    }

    private static EditorResourcesHolder _instance;
}