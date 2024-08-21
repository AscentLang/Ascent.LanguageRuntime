using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class AscentInstaller
{
    private const string InstalledFlag = "AscentInstalled";

    static AscentInstaller()
    {
        // Check if the package has been installed before
        if (!EditorPrefs.HasKey(InstalledFlag))
        {
            // Mark the package as installed
            EditorPrefs.SetBool(InstalledFlag, true);

            // Open the editor window
            EditorApplication.update += ShowPopupWindow;
        }
    }

    private static void ShowPopupWindow()
    {
        EditorApplication.update -= ShowPopupWindow;

        // Show your custom editor window
        GreetingEditorWindow.ShowWindow();
    }
}