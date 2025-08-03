using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

namespace BlendShapePresets.Editor
{
    public static class BlendShapeImporter
    {
        public static void ImportBlendShapeValues(bool includeChildObjects)
        {
            try
            {
                var startTime = System.DateTime.Now;
                GameObject selectedObject = Selection.activeGameObject;

                BlendShapeLogger.LogFormat("ImportBlendShapeValues: Starting import for '{0}' (includeChildren: {1})", selectedObject?.name, includeChildObjects);

                if (!BlendShapeUtility.ValidateBlendShapeTarget(selectedObject, includeChildObjects))
                {
                    BlendShapeLogger.LogWarningFormat("ImportBlendShapeValues: Validation failed for '{0}'", selectedObject?.name);
                    return;
                }

                string filePath = EditorUtility.OpenFilePanel("Import BlendShape Values", "", "json");
                if (string.IsNullOrEmpty(filePath))
                {
                    BlendShapeLogger.Log("ImportBlendShapeValues: User cancelled file selection");
                    return;
                }

                BlendShapeLogger.LogFormat("ImportBlendShapeValues: Selected file: {0}", filePath);

                // Validate file exists and is readable
                if (!File.Exists(filePath))
                {
                    BlendShapeLogger.LogErrorFormat("ImportBlendShapeValues: File does not exist: {0}", filePath);
                    EditorUtility.DisplayDialog("Error", "Selected file does not exist", "OK");
                    return;
                }

                string json;
                try
                {
                    json = File.ReadAllText(filePath);
                    if (string.IsNullOrEmpty(json))
                    {
                        throw new Exception("File is empty or contains no readable content");
                    }
                    BlendShapeLogger.LogFormat("ImportBlendShapeValues: File read successfully ({0} characters)", json.Length);
                }
                catch (UnauthorizedAccessException ex)
                {
                    BlendShapeLogger.LogException("ImportBlendShapeValues: Access denied reading file", ex);
                    EditorUtility.DisplayDialog("Error", $"Access denied. Please check file permissions: {ex.Message}", "OK");
                    return;
                }
                catch (IOException ex)
                {
                    BlendShapeLogger.LogException("ImportBlendShapeValues: IO error reading file", ex);
                    EditorUtility.DisplayDialog("Error", $"Failed to read file (IO Error): {ex.Message}", "OK");
                    return;
                }
                catch (Exception ex)
                {
                    BlendShapeLogger.LogException("ImportBlendShapeValues: Unexpected error reading file", ex);
                    EditorUtility.DisplayDialog("Error", $"Failed to read file: {ex.Message}", "OK");
                    return;
                }

                MultiMeshBlendShapeData importData;
                try
                {
                    importData = JsonUtility.FromJson<MultiMeshBlendShapeData>(json);
                    if (importData == null)
                    {
                        throw new Exception("JSON deserialization returned null");
                    }
                    BlendShapeLogger.Log("ImportBlendShapeValues: JSON deserialization successful");
                }
                catch (Exception ex)
                {
                    BlendShapeLogger.LogException("ImportBlendShapeValues: JSON parsing failed", ex);
                    EditorUtility.DisplayDialog("Error", $"Invalid JSON format: {ex.Message}", "OK");
                    return;
                }

                if (importData.meshDataList == null || importData.meshDataList.Count == 0)
                {
                    BlendShapeLogger.LogWarning("ImportBlendShapeValues: No valid blend shape data found in file");
                    EditorUtility.DisplayDialog("Warning", "No valid blend shape data found in the file", "OK");
                    return;
                }

                BlendShapeLogger.LogFormat("ImportBlendShapeValues: Found {0} mesh data entries", importData.meshDataList.Count);

                List<SkinnedMeshRenderer> renderers = BlendShapeUtility.CollectSkinnedMeshRenderers(selectedObject, includeChildObjects);

                if (renderers == null || renderers.Count == 0)
                {
                    BlendShapeLogger.LogError("ImportBlendShapeValues: No renderers found on target object");
                    EditorUtility.DisplayDialog("Error", "No SkinnedMeshRenderer found on target object", "OK");
                    return;
                }

                BlendShapeLogger.LogFormat("ImportBlendShapeValues: Found {0} target renderers", renderers.Count);

                int appliedCount = 0;
                int skippedCount = 0;
                foreach (BlendShapeData meshData in importData.meshDataList)
                {
                    if (meshData == null)
                    {
                        BlendShapeLogger.LogWarning("ImportBlendShapeValues: Null mesh data encountered, skipping");
                        skippedCount++;
                        continue;
                    }

                    try
                    {
                        SkinnedMeshRenderer targetRenderer = BlendShapeUtility.FindMatchingRenderer(renderers, meshData);
                        if (targetRenderer != null)
                        {
                            BlendShapeUtility.ApplyBlendShapesToRenderer(targetRenderer, meshData);
                            appliedCount++;
                            BlendShapeLogger.LogFormat("ImportBlendShapeValues: Applied blend shapes to renderer: {0}", meshData.objectName);
                        }
                        else
                        {
                            BlendShapeLogger.LogWarningFormat("ImportBlendShapeValues: No matching renderer found for mesh: {0}", meshData.objectName);
                            skippedCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        BlendShapeLogger.LogException($"ImportBlendShapeValues: Error applying blend shapes to mesh '{meshData.objectName}'", ex);
                        skippedCount++;
                    }
                }

                var duration = System.DateTime.Now - startTime;

                BlendShapeLogger.LogCompletion("ImportBlendShapeValues", duration.TotalMilliseconds);
                BlendShapeLogger.LogFormat("ImportBlendShapeValues: Applied: {0}, Skipped: {1}, Total: {2}", appliedCount, skippedCount, importData.meshDataList.Count);

                string resultMessage = $"Blend shape values imported\n\n" +
                    $"Applied to {appliedCount} out of {importData.meshDataList.Count} meshes";
                
                if (skippedCount > 0)
                {
                    resultMessage += $"\nSkipped: {skippedCount} meshes";
                }

                EditorUtility.DisplayDialog("Complete", resultMessage, "OK");
            }
            catch (Exception ex)
            {
                BlendShapeLogger.LogException("ImportBlendShapeValues: Unexpected error during import", ex);
                EditorUtility.DisplayDialog("Error", $"Import failed: {ex.Message}", "OK");
            }
        }

        public static void ImportBlendShapeValuesFromClipboard(bool includeChildObjects)
        {
            try
            {
                var startTime = System.DateTime.Now;
                GameObject selectedObject = Selection.activeGameObject;

                BlendShapeLogger.LogFormat("ImportBlendShapeValuesFromClipboard: Starting clipboard import for '{0}' (includeChildren: {1})", selectedObject?.name, includeChildObjects);

                if (!BlendShapeUtility.ValidateBlendShapeTarget(selectedObject, includeChildObjects))
                {
                    BlendShapeLogger.LogWarningFormat("ImportBlendShapeValuesFromClipboard: Validation failed for '{0}'", selectedObject?.name);
                    return;
                }

                string json;
                try
                {
                    json = EditorGUIUtility.systemCopyBuffer;
                    if (string.IsNullOrEmpty(json))
                    {
                        BlendShapeLogger.LogWarning("ImportBlendShapeValuesFromClipboard: Clipboard is empty");
                        EditorUtility.DisplayDialog("Warning", "Clipboard is empty", "OK");
                        return;
                    }
                    BlendShapeLogger.LogFormat("ImportBlendShapeValuesFromClipboard: Clipboard content retrieved ({0} characters)", json.Length);
                }
                catch (Exception ex)
                {
                    BlendShapeLogger.LogException("ImportBlendShapeValuesFromClipboard: Failed to access clipboard", ex);
                    EditorUtility.DisplayDialog("Error", "Failed to access clipboard", "OK");
                    return;
                }

                // Validate JSON format before parsing
                if (!json.Trim().StartsWith("{") && !json.Trim().StartsWith("["))
                {
                    BlendShapeLogger.LogWarning("ImportBlendShapeValuesFromClipboard: Clipboard content does not appear to be JSON");
                    EditorUtility.DisplayDialog("Warning", "Clipboard content does not appear to be valid JSON", "OK");
                    return;
                }

                MultiMeshBlendShapeData importData;
                try
                {
                    importData = JsonUtility.FromJson<MultiMeshBlendShapeData>(json);
                    if (importData == null)
                    {
                        throw new Exception("JSON deserialization returned null");
                    }
                    BlendShapeLogger.Log("ImportBlendShapeValuesFromClipboard: JSON deserialization successful");
                }
                catch (Exception ex)
                {
                    BlendShapeLogger.LogException("ImportBlendShapeValuesFromClipboard: JSON parsing failed", ex);
                    EditorUtility.DisplayDialog("Error", $"Invalid JSON format in clipboard: {ex.Message}", "OK");
                    return;
                }

                if (importData.meshDataList == null || importData.meshDataList.Count == 0)
                {
                    BlendShapeLogger.LogWarning("ImportBlendShapeValuesFromClipboard: No valid blend shape data found in clipboard");
                    EditorUtility.DisplayDialog("Warning", "No valid blend shape data found in clipboard", "OK");
                    return;
                }

                BlendShapeLogger.LogFormat("ImportBlendShapeValuesFromClipboard: Found {0} mesh data entries", importData.meshDataList.Count);

                List<SkinnedMeshRenderer> renderers = BlendShapeUtility.CollectSkinnedMeshRenderers(selectedObject, includeChildObjects);

                if (renderers == null || renderers.Count == 0)
                {
                    BlendShapeLogger.LogError("ImportBlendShapeValuesFromClipboard: No renderers found on target object");
                    EditorUtility.DisplayDialog("Error", "No SkinnedMeshRenderer found on target object", "OK");
                    return;
                }

                BlendShapeLogger.LogFormat("ImportBlendShapeValuesFromClipboard: Found {0} target renderers", renderers.Count);

                int appliedCount = 0;
                int skippedCount = 0;
                foreach (BlendShapeData meshData in importData.meshDataList)
                {
                    if (meshData == null)
                    {
                        BlendShapeLogger.LogWarning("ImportBlendShapeValuesFromClipboard: Null mesh data encountered, skipping");
                        skippedCount++;
                        continue;
                    }

                    try
                    {
                        SkinnedMeshRenderer targetRenderer = BlendShapeUtility.FindMatchingRenderer(renderers, meshData);
                        if (targetRenderer != null)
                        {
                            BlendShapeUtility.ApplyBlendShapesToRenderer(targetRenderer, meshData);
                            appliedCount++;
                            BlendShapeLogger.LogFormat("ImportBlendShapeValuesFromClipboard: Applied blend shapes to renderer: {0}", meshData.objectName);
                        }
                        else
                        {
                            BlendShapeLogger.LogWarningFormat("ImportBlendShapeValuesFromClipboard: No matching renderer found for mesh: {0}", meshData.objectName);
                            skippedCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        BlendShapeLogger.LogException($"ImportBlendShapeValuesFromClipboard: Error applying blend shapes to mesh '{meshData.objectName}'", ex);
                        skippedCount++;
                    }
                }

                var duration = System.DateTime.Now - startTime;

                BlendShapeLogger.LogCompletion("ImportBlendShapeValuesFromClipboard", duration.TotalMilliseconds);
                BlendShapeLogger.LogFormat("ImportBlendShapeValuesFromClipboard: Applied: {0}, Skipped: {1}, Total: {2}", appliedCount, skippedCount, importData.meshDataList.Count);

                string resultMessage = $"Blend shape values imported from clipboard\n\n" +
                    $"Applied to {appliedCount} out of {importData.meshDataList.Count} meshes";
                
                if (skippedCount > 0)
                {
                    resultMessage += $"\nSkipped: {skippedCount} meshes";
                }

                EditorUtility.DisplayDialog("Complete", resultMessage, "OK");
            }
            catch (Exception ex)
            {
                BlendShapeLogger.LogException("ImportBlendShapeValuesFromClipboard: Unexpected error during clipboard import", ex);
                EditorUtility.DisplayDialog("Error", $"Clipboard import failed: {ex.Message}", "OK");
            }
        }


    }
}