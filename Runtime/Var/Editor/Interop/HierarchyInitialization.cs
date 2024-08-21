using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using static System.Linq.Expressions.Expression;

public static class HierarchyInitialization
{
    private static PropertyInfo lastInteractedHierarchyWindow;
    private static PropertyInfo hoveredObject;

    private static Func<object> getLastHierarchyWindowFunc;


    [InitializeOnLoadMethod]
    private static void OnInitialize()
    {
        DoReflection();
    }

    private static void DoReflection()
    {
        var sceneHierarchyWindowType = typeof(Editor).Assembly.GetType("UnityEditor.SceneHierarchyWindow");

        lastInteractedHierarchyWindow = sceneHierarchyWindowType
            .GetProperty("lastInteractedHierarchyWindow", BindingFlags.Public | BindingFlags.Static);

        hoveredObject = sceneHierarchyWindowType
            .GetProperty("hoveredObject");

        getLastHierarchyWindowFunc = Lambda<Func<object>>(Property(null, lastInteractedHierarchyWindow)).Compile();
    }

    internal static object GetLastHierarchy()
    {
        var lastHierarchyWindow = getLastHierarchyWindowFunc();

        return lastHierarchyWindow;
    }

    internal static UnityEngine.Object GetHoveredObject()
    {
        var lastHierarchy = GetLastHierarchy();

        var hoveredObj = hoveredObject.GetValue(lastHierarchy);

        return hoveredObj as UnityEngine.Object;
    }
}