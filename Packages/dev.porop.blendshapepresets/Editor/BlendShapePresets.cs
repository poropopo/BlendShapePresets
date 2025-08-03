using System;
using UnityEngine;
using UnityEditor;

namespace BlendShapePresets.Editor
{
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
            try
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
            catch (Exception ex)
            {
                BlendShapeLogger.LogException("OnGUI: Unexpected error in UI rendering", ex);
                EditorGUILayout.HelpBox($"UI Error: {ex.Message}", MessageType.Error);
            }
        }

        void DrawExportTab()
        {
            try
            {
                GUILayout.Label("JSON Export", EditorStyles.boldLabel);

                includeChildObjects = EditorGUILayout.Toggle("Include Child Objects", includeChildObjects);

                EditorGUILayout.Space();

                // Validate selection before showing buttons
                GameObject selectedObject = Selection.activeGameObject;
                bool hasValidSelection = selectedObject != null;

                if (!hasValidSelection)
                {
                    EditorGUILayout.HelpBox("Please select a GameObject in the scene to export blend shapes.", MessageType.Warning);
                }

                EditorGUI.BeginDisabledGroup(!hasValidSelection);

                if (GUILayout.Button("Export to File"))
                {
                    try
                    {
                        BlendShapeLogger.Log($"DrawExportTab: Export to File button clicked for '{selectedObject?.name}'");
                        BlendShapeExporter.ExportBlendShapeValues(includeChildObjects);
                    }
                    catch (Exception ex)
                    {
                        BlendShapeLogger.LogException("DrawExportTab: Error during file export", ex);
                        EditorUtility.DisplayDialog("Export Error", $"Failed to export to file: {ex.Message}", "OK");
                    }
                }

                if (GUILayout.Button("Copy to Clipboard"))
                {
                    try
                    {
                        BlendShapeLogger.Log($"DrawExportTab: Copy to Clipboard button clicked for '{selectedObject?.name}'");
                        BlendShapeExporter.CopyBlendShapeValuesToClipboard(includeChildObjects);
                    }
                    catch (Exception ex)
                    {
                        BlendShapeLogger.LogException("DrawExportTab: Error during clipboard copy", ex);
                        EditorUtility.DisplayDialog("Export Error", $"Failed to copy to clipboard: {ex.Message}", "OK");
                    }
                }

                EditorGUI.EndDisabledGroup();

                EditorGUILayout.Space();
                EditorGUILayout.HelpBox(
                    "Save blend shape values from all SkinnedMeshRenderers in the selected object" +
                    (includeChildObjects ? " and its child objects" : "") + ".\n\n" +
                    "Export to File: Save as JSON file\n" +
                    "Copy to Clipboard: Copy JSON data to clipboard",
                    MessageType.Info);

                if (hasValidSelection)
                {
                    // Show additional info about the selected object
                    var renderers = BlendShapeUtility.CollectSkinnedMeshRenderers(selectedObject, includeChildObjects);
                    if (renderers != null && renderers.Count > 0)
                    {
                        EditorGUILayout.Space();
                        EditorGUILayout.HelpBox($"Selected: '{selectedObject.name}'\nFound {renderers.Count} SkinnedMeshRenderer(s)", MessageType.None);
                    }
                }
            }
            catch (Exception ex)
            {
                BlendShapeLogger.LogException("DrawExportTab: Unexpected error in export tab", ex);
                EditorGUILayout.HelpBox($"Export Tab Error: {ex.Message}", MessageType.Error);
            }
        }

        void DrawImportTab()
        {
            try
            {
                GUILayout.Label("JSON Import", EditorStyles.boldLabel);

                includeChildObjects = EditorGUILayout.Toggle("Include Child Objects", includeChildObjects);

                EditorGUILayout.Space();

                // Validate selection before showing buttons
                GameObject selectedObject = Selection.activeGameObject;
                bool hasValidSelection = selectedObject != null;

                if (!hasValidSelection)
                {
                    EditorGUILayout.HelpBox("Please select a GameObject in the scene to import blend shapes.", MessageType.Warning);
                }

                EditorGUI.BeginDisabledGroup(!hasValidSelection);

                if (GUILayout.Button("Import from File"))
                {
                    try
                    {
                        BlendShapeLogger.Log($"DrawImportTab: Import from File button clicked for '{selectedObject?.name}'");
                        BlendShapeImporter.ImportBlendShapeValues(includeChildObjects);
                    }
                    catch (Exception ex)
                    {
                        BlendShapeLogger.LogException("DrawImportTab: Error during file import", ex);
                        EditorUtility.DisplayDialog("Import Error", $"Failed to import from file: {ex.Message}", "OK");
                    }
                }

                if (GUILayout.Button("Paste from Clipboard"))
                {
                    try
                    {
                        BlendShapeLogger.Log($"DrawImportTab: Paste from Clipboard button clicked for '{selectedObject?.name}'");
                        BlendShapeImporter.ImportBlendShapeValuesFromClipboard(includeChildObjects);
                    }
                    catch (Exception ex)
                    {
                        BlendShapeLogger.LogException("DrawImportTab: Error during clipboard import", ex);
                        EditorUtility.DisplayDialog("Import Error", $"Failed to import from clipboard: {ex.Message}", "OK");
                    }
                }

                EditorGUI.EndDisabledGroup();

                EditorGUILayout.Space();
                EditorGUILayout.HelpBox(
                    "Apply blend shape values to SkinnedMeshRenderers in the selected object" +
                    (includeChildObjects ? " and its child objects" : "") + ".\n\n" +
                    "Import from File: Select and load JSON file\n" +
                    "Paste from Clipboard: Load JSON data from clipboard\n\n" +
                    "Values will be automatically applied to meshes with matching object names or paths.",
                    MessageType.Info);

                if (hasValidSelection)
                {
                    // Show additional info about the selected object
                    var renderers = BlendShapeUtility.CollectSkinnedMeshRenderers(selectedObject, includeChildObjects);
                    if (renderers != null && renderers.Count > 0)
                    {
                        EditorGUILayout.Space();
                        EditorGUILayout.HelpBox($"Selected: '{selectedObject.name}'\nFound {renderers.Count} SkinnedMeshRenderer(s)", MessageType.None);
                    }
                }
            }
            catch (Exception ex)
            {
                BlendShapeLogger.LogException("DrawImportTab: Unexpected error in import tab", ex);
                EditorGUILayout.HelpBox($"Import Tab Error: {ex.Message}", MessageType.Error);
            }
        }
    }
}
