using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System;

namespace BlendShapePresets.Editor
{
    public static class BlendShapeExporter
    {
        public static void ExportBlendShapeValues(bool includeChildObjects)
        {
            try
            {
                var startTime = System.DateTime.Now;
                GameObject selectedObject = Selection.activeGameObject;

                BlendShapeLogger.LogFormat("ExportBlendShapeValues: Starting export for '{0}' (includeChildren: {1})", selectedObject?.name, includeChildObjects);

                if (!BlendShapeUtility.ValidateBlendShapeTarget(selectedObject, includeChildObjects))
                {
                    BlendShapeLogger.LogWarningFormat("ExportBlendShapeValues: Validation failed for '{0}'", selectedObject?.name);
                    return;
                }

                // Collect blend shape data from multiple meshes
                MultiMeshBlendShapeData exportData = BlendShapeUtility.CollectAllBlendShapeData(selectedObject, includeChildObjects);

                if (exportData == null)
                {
                    BlendShapeLogger.LogError("ExportBlendShapeValues: Failed to collect blend shape data");
                    EditorUtility.DisplayDialog("Error", "Failed to collect blend shape data", "OK");
                    return;
                }

                if (exportData.meshDataList == null || exportData.meshDataList.Count == 0)
                {
                    BlendShapeLogger.LogError("ExportBlendShapeValues: No renderers found");
                    EditorUtility.DisplayDialog("Error", "No SkinnedMeshRenderer found", "OK");
                    return;
                }

                if (exportData.meshDataList.Count == 0)
                {
                    BlendShapeLogger.LogWarning("ExportBlendShapeValues: No blend shape data collected");
                    EditorUtility.DisplayDialog("Warning", "No SkinnedMeshRenderer with BlendShapes found", "OK");
                    return;
                }

                string json;
                try
                {
                    json = JsonUtility.ToJson(exportData, true);
                    if (string.IsNullOrEmpty(json))
                    {
                        throw new Exception("JSON serialization resulted in empty string");
                    }
                    BlendShapeLogger.LogFormat("ExportBlendShapeValues: JSON serialization successful ({0} characters)", json.Length);
                }
                catch (Exception ex)
                {
                    BlendShapeLogger.LogException("ExportBlendShapeValues: JSON serialization failed", ex);
                    EditorUtility.DisplayDialog("Error", $"Failed to serialize data: {ex.Message}", "OK");
                    return;
                }

                string path = EditorUtility.SaveFilePanel(
                    "Save Blend Shape Data",
                    "",
                    selectedObject.name + "_blendshapes.json",
                    "json");

                if (string.IsNullOrEmpty(path))
                {
                    BlendShapeLogger.Log("ExportBlendShapeValues: Export cancelled by user");
                    return;
                }

                try
                {
                    // Validate path before writing
                    string directory = Path.GetDirectoryName(path);
                    if (!Directory.Exists(directory))
                    {
                        BlendShapeLogger.LogErrorFormat("ExportBlendShapeValues: Directory does not exist: {0}", directory);
                        EditorUtility.DisplayDialog("Error", $"Directory does not exist: {directory}", "OK");
                        return;
                    }

                    File.WriteAllText(path, json);
                    
                    // Verify file was written successfully
                    if (!File.Exists(path))
                    {
                        throw new Exception("File was not created successfully");
                    }

                    var fileInfo = new FileInfo(path);
                    var duration = System.DateTime.Now - startTime;

                    BlendShapeLogger.LogCompletion("ExportBlendShapeValues", duration.TotalMilliseconds);
                    BlendShapeLogger.LogFormat("ExportBlendShapeValues: File saved: {0} ({1} bytes)", path, fileInfo.Length);
                    BlendShapeLogger.LogFormat("ExportBlendShapeValues: Data collected from {0} meshes", exportData.meshDataList.Count);

                    int totalBlendShapes = BlendShapeUtility.GetTotalBlendShapeCount(exportData);
                    EditorUtility.DisplayDialog("Complete",
                        $"Blend shape data saved\n{path}\n\n" +
                        $"Mesh count: {exportData.meshDataList.Count}\n" +
                        $"Blend shape count: {totalBlendShapes}\n" +
                        $"File size: {fileInfo.Length} bytes", "OK");
                }
                catch (UnauthorizedAccessException ex)
                {
                    BlendShapeLogger.LogException($"ExportBlendShapeValues: Access denied when writing to {path}", ex);
                    EditorUtility.DisplayDialog("Error", $"Access denied. Please check file permissions: {ex.Message}", "OK");
                }
                catch (DirectoryNotFoundException ex)
                {
                    BlendShapeLogger.LogException("ExportBlendShapeValues: Directory not found", ex);
                    EditorUtility.DisplayDialog("Error", $"Directory not found: {ex.Message}", "OK");
                }
                catch (IOException ex)
                {
                    BlendShapeLogger.LogException("ExportBlendShapeValues: IO error when writing file", ex);
                    EditorUtility.DisplayDialog("Error", $"Failed to save file (IO Error): {ex.Message}", "OK");
                }
                catch (Exception ex)
                {
                    BlendShapeLogger.LogException("ExportBlendShapeValues: Unexpected error when saving file", ex);
                    EditorUtility.DisplayDialog("Error", $"Failed to save file: {ex.Message}", "OK");
                }
            }
            catch (Exception ex)
            {
                BlendShapeLogger.LogException("ExportBlendShapeValues: Unexpected error during export", ex);
                EditorUtility.DisplayDialog("Error", $"Export failed: {ex.Message}", "OK");
            }
        }

        public static void CopyBlendShapeValuesToClipboard(bool includeChildObjects)
        {
            try
            {
                var startTime = System.DateTime.Now;
                GameObject selectedObject = Selection.activeGameObject;

                BlendShapeLogger.LogFormat("CopyBlendShapeValuesToClipboard: Starting clipboard copy for '{0}' (includeChildren: {1})", selectedObject?.name, includeChildObjects);

                if (!BlendShapeUtility.ValidateBlendShapeTarget(selectedObject, includeChildObjects))
                {
                    BlendShapeLogger.LogWarningFormat("CopyBlendShapeValuesToClipboard: Validation failed for '{0}'", selectedObject?.name);
                    return;
                }

                // Collect blend shape data from multiple meshes
                MultiMeshBlendShapeData exportData = BlendShapeUtility.CollectAllBlendShapeData(selectedObject, includeChildObjects);

                if (exportData == null)
                {
                    BlendShapeLogger.LogError("CopyBlendShapeValuesToClipboard: Failed to collect blend shape data");
                    EditorUtility.DisplayDialog("Error", "Failed to collect blend shape data", "OK");
                    return;
                }

                if (exportData.meshDataList == null || exportData.meshDataList.Count == 0)
                {
                    BlendShapeLogger.LogError("CopyBlendShapeValuesToClipboard: No renderers found");
                    EditorUtility.DisplayDialog("Error", "No SkinnedMeshRenderer found", "OK");
                    return;
                }

                if (exportData.meshDataList.Count == 0)
                {
                    BlendShapeLogger.LogWarning("CopyBlendShapeValuesToClipboard: No blend shape data collected");
                    EditorUtility.DisplayDialog("Warning", "No SkinnedMeshRenderer with BlendShapes found", "OK");
                    return;
                }

                string json;
                try
                {
                    json = JsonUtility.ToJson(exportData, true);
                    if (string.IsNullOrEmpty(json))
                    {
                        throw new Exception("JSON serialization resulted in empty string");
                    }
                    BlendShapeLogger.LogFormat("CopyBlendShapeValuesToClipboard: JSON serialization successful ({0} characters)", json.Length);
                }
                catch (Exception ex)
                {
                    BlendShapeLogger.LogException("CopyBlendShapeValuesToClipboard: JSON serialization failed", ex);
                    EditorUtility.DisplayDialog("Error", $"Failed to serialize data: {ex.Message}", "OK");
                    return;
                }

                try
                {
                    EditorGUIUtility.systemCopyBuffer = json;
                    
                    // Verify clipboard content
                    string clipboardContent = EditorGUIUtility.systemCopyBuffer;
                    if (string.IsNullOrEmpty(clipboardContent) || clipboardContent != json)
                    {
                        throw new Exception("Clipboard verification failed - content was not copied correctly");
                    }

                    var duration = System.DateTime.Now - startTime;

                    BlendShapeLogger.LogCompletion("CopyBlendShapeValuesToClipboard", duration.TotalMilliseconds);
                    BlendShapeLogger.LogFormat("CopyBlendShapeValuesToClipboard: Data copied to clipboard ({0} characters)", json.Length);
                    BlendShapeLogger.LogFormat("CopyBlendShapeValuesToClipboard: Data collected from {0} meshes", exportData.meshDataList.Count);

                    int totalBlendShapes = BlendShapeUtility.GetTotalBlendShapeCount(exportData);
                    EditorUtility.DisplayDialog("Complete",
                        $"Blend shape data copied to clipboard\n\n" +
                        $"Mesh count: {exportData.meshDataList.Count}\n" +
                        $"Blend shape count: {totalBlendShapes}\n" +
                        $"Data size: {json.Length} characters", "OK");
                }
                catch (Exception ex)
                {
                    BlendShapeLogger.LogException("CopyBlendShapeValuesToClipboard: Failed to copy to clipboard", ex);
                    EditorUtility.DisplayDialog("Error", $"Failed to copy to clipboard: {ex.Message}", "OK");
                }
            }
            catch (Exception ex)
            {
                BlendShapeLogger.LogException("CopyBlendShapeValuesToClipboard: Unexpected error during clipboard copy", ex);
                EditorUtility.DisplayDialog("Error", $"Clipboard copy failed: {ex.Message}", "OK");
            }
        }

    }
}