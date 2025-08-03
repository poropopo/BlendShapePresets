using UnityEngine;
using UnityEditor;

public class BlendShapePresets : EditorWindow
{
    private bool includeChildObjects = true;
    private int selectedTab = 0;
    private string[] tabNames = { "Export", "Import" };

    [MenuItem("Tools/BlendShape Presets")]
    public static void ShowWindow()
    {
        GetWindow<BlendShapePresets>("BlendShape Presets");
    }

    void OnGUI()
    {
        GUILayout.Label("BlendShape Presets", EditorStyles.boldLabel);

        selectedTab = GUILayout.Toolbar(selectedTab, tabNames);

        EditorGUILayout.Space();

        if (selectedTab == 0)
        {
            DrawExportTab();
        }
        else
        {
            DrawImportTab();
        }
    }

    void DrawExportTab()
    {
        GUILayout.Label("JSON Export", EditorStyles.boldLabel);

        includeChildObjects = EditorGUILayout.Toggle("Include Child Objects", includeChildObjects);

        EditorGUILayout.Space();

        if (GUILayout.Button("Export Selected Object(s)"))
        {
            BlendShapeExporter.ExportBlendShapeValues(includeChildObjects);
        }

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "Save blend shape values from all SkinnedMeshRenderers in the selected object" +
            (includeChildObjects ? " and its child objects" : "") + ".",
            MessageType.Info);
    }

    void DrawImportTab()
    {
        GUILayout.Label("JSON Import", EditorStyles.boldLabel);

        includeChildObjects = EditorGUILayout.Toggle("Include Child Objects", includeChildObjects);

        EditorGUILayout.Space();

        if (GUILayout.Button("Import JSON to Selected Object(s)"))
        {
            BlendShapeImporter.ImportBlendShapeValues(includeChildObjects);
        }

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "Select a JSON file and apply blend shape values to SkinnedMeshRenderers in the selected object" +
            (includeChildObjects ? " and its child objects" : "") + ".\n\n" +
            "Values will be automatically applied to meshes with matching object names or paths.",
            MessageType.Info);
    }
}
