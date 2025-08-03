using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;

namespace BlendShapePresets.Editor
{
    /// <summary>
    /// Utility class for common BlendShape operations
    /// </summary>
    public static class BlendShapeUtility
    {
        /// <summary>
        /// Gets the full hierarchy path of a GameObject
        /// </summary>
        /// <param name="obj">The GameObject to get the path for</param>
        /// <returns>The full hierarchy path as a string</returns>
        public static string GetObjectPath(GameObject obj)
        {
            if (obj == null)
            {
                BlendShapeLogger.LogError("GetObjectPath: GameObject is null");
                return string.Empty;
            }

            try
            {
                string path = obj.name;
                Transform parent = obj.transform.parent;

                while (parent != null)
                {
                    path = parent.name + "/" + path;
                    parent = parent.parent;
                }

                return path;
            }
            catch (Exception ex)
            {
                BlendShapeLogger.LogErrorFormat("GetObjectPath: Failed to get object path for '{0}'. Error: {1}", obj.name, ex.Message);
                return obj.name; // Fallback to just the object name
            }
        }

        /// <summary>
        /// Collects all SkinnedMeshRenderer components from the selected object and optionally its children
        /// </summary>
        /// <param name="selectedObject">The root GameObject to search from</param>
        /// <param name="includeChildObjects">Whether to include child objects in the search</param>
        /// <returns>List of SkinnedMeshRenderer components found</returns>
        public static List<SkinnedMeshRenderer> CollectSkinnedMeshRenderers(GameObject selectedObject, bool includeChildObjects)
        {
            if (selectedObject == null)
            {
                BlendShapeLogger.LogError("CollectSkinnedMeshRenderers: selectedObject is null");
                return new List<SkinnedMeshRenderer>();
            }

            List<SkinnedMeshRenderer> renderers = new List<SkinnedMeshRenderer>();

            try
            {
                // Add the selected object's renderer if it exists
                SkinnedMeshRenderer selectedRenderer = selectedObject.GetComponent<SkinnedMeshRenderer>();
                if (selectedRenderer != null)
                {
                    if (selectedRenderer.sharedMesh != null)
                    {
                        renderers.Add(selectedRenderer);
                        BlendShapeLogger.LogFormat("Found SkinnedMeshRenderer on '{0}' with {1} blend shapes", selectedObject.name, selectedRenderer.sharedMesh.blendShapeCount);
                    }
                    else
                    {
                        BlendShapeLogger.LogWarningFormat("SkinnedMeshRenderer on '{0}' has no shared mesh", selectedObject.name);
                    }
                }

                // Add child renderers if requested
                if (includeChildObjects)
                {
                    SkinnedMeshRenderer[] childRenderers = selectedObject.GetComponentsInChildren<SkinnedMeshRenderer>();
                    foreach (SkinnedMeshRenderer renderer in childRenderers)
                    {
                        if (renderer != null && renderer.gameObject != selectedObject && !renderers.Contains(renderer))
                        {
                            if (renderer.sharedMesh != null)
                            {
                                renderers.Add(renderer);
                                BlendShapeLogger.LogFormat("Found child SkinnedMeshRenderer on '{0}' with {1} blend shapes", renderer.name, renderer.sharedMesh.blendShapeCount);
                            }
                            else
                            {
                                BlendShapeLogger.LogWarningFormat("Child SkinnedMeshRenderer on '{0}' has no shared mesh", renderer.name);
                            }
                        }
                    }
                }

                BlendShapeLogger.LogFormat("CollectSkinnedMeshRenderers: Found {0} valid SkinnedMeshRenderer(s)", renderers.Count);
            }
            catch (Exception ex)
            {
                BlendShapeLogger.LogErrorFormat("CollectSkinnedMeshRenderers: Error collecting renderers from '{0}'. Error: {1}", selectedObject.name, ex.Message);
            }

            return renderers;
        }

        /// <summary>
        /// Validates if a GameObject has any SkinnedMeshRenderer components with BlendShapes
        /// </summary>
        /// <param name="obj">The GameObject to validate</param>
        /// <param name="includeChildObjects">Whether to check child objects as well</param>
        /// <returns>True if valid SkinnedMeshRenderer with BlendShapes is found</returns>
        public static bool ValidateBlendShapeTarget(GameObject obj, bool includeChildObjects)
        {
            if (obj == null)
            {
                BlendShapeLogger.LogError("ValidateBlendShapeTarget: No object selected");
                EditorUtility.DisplayDialog("Error", "No object selected", "OK");
                return false;
            }

            BlendShapeLogger.LogFormat("ValidateBlendShapeTarget: Validating '{0}' (includeChildren: {1})", obj.name, includeChildObjects);

            try
            {
                List<SkinnedMeshRenderer> renderers = CollectSkinnedMeshRenderers(obj, includeChildObjects);

                if (renderers.Count == 0)
                {
                    BlendShapeLogger.LogWarningFormat("ValidateBlendShapeTarget: No SkinnedMeshRenderer found on '{0}'", obj.name);
                    EditorUtility.DisplayDialog("Warning", "No SkinnedMeshRenderer found", "OK");
                    return false;
                }

                // Check if any renderer has blend shapes
                int totalBlendShapes = 0;
                foreach (var renderer in renderers)
                {
                    if (renderer.sharedMesh != null)
                    {
                        totalBlendShapes += renderer.sharedMesh.blendShapeCount;
                    }
                }

                if (totalBlendShapes == 0)
                {
                    BlendShapeLogger.LogWarningFormat("ValidateBlendShapeTarget: No blend shapes found in any SkinnedMeshRenderer on '{0}'", obj.name);
                    EditorUtility.DisplayDialog("Warning", "No blend shapes found in any SkinnedMeshRenderer", "OK");
                    return false;
                }

                BlendShapeLogger.LogFormat("ValidateBlendShapeTarget: Validation successful. Found {0} renderer(s) with {1} total blend shapes", renderers.Count, totalBlendShapes);
                return true;
            }
            catch (Exception ex)
            {
                BlendShapeLogger.LogErrorFormat("ValidateBlendShapeTarget: Error during validation of '{0}'. Error: {1}", obj.name, ex.Message);
                EditorUtility.DisplayDialog("Error", $"Validation failed: {ex.Message}", "OK");
                return false;
            }
        }

        /// <summary>
        /// Creates BlendShapeData from a SkinnedMeshRenderer
        /// </summary>
        /// <param name="renderer">The SkinnedMeshRenderer to extract data from</param>
        /// <returns>BlendShapeData containing the renderer's blend shape information</returns>
        public static BlendShapeData CreateBlendShapeData(SkinnedMeshRenderer renderer)
        {
            if (renderer == null)
            {
                BlendShapeLogger.LogError("CreateBlendShapeData: SkinnedMeshRenderer is null");
                return null;
            }

            if (renderer.sharedMesh == null)
            {
                BlendShapeLogger.LogErrorFormat("CreateBlendShapeData: SkinnedMeshRenderer '{0}' has no shared mesh", renderer.name);
                return null;
            }

            try
            {
                BlendShapeData data = new BlendShapeData();
                data.objectName = renderer.name;
                data.objectPath = GetObjectPath(renderer.gameObject);
                data.blendShapes = new List<BlendShapeValue>();

                Mesh mesh = renderer.sharedMesh;
                BlendShapeLogger.LogFormat("CreateBlendShapeData: Processing '{0}' with {1} blend shapes", renderer.name, mesh.blendShapeCount);

                for (int i = 0; i < mesh.blendShapeCount; i++)
                {
                    try
                    {
                        BlendShapeValue blendShape = new BlendShapeValue();
                        blendShape.name = mesh.GetBlendShapeName(i);
                        blendShape.index = i;
                        blendShape.weight = renderer.GetBlendShapeWeight(i);

                        if (string.IsNullOrEmpty(blendShape.name))
                        {
                            BlendShapeLogger.LogWarningFormat("CreateBlendShapeData: Blend shape at index {0} has empty name", i);
                        }

                        // Log unusual weight values for debugging
                        if (blendShape.weight < 0 || blendShape.weight > 100)
                        {
                            BlendShapeLogger.LogWarningFormat("CreateBlendShapeData: Blend shape '{0}' has unusual weight value: {1}", blendShape.name, blendShape.weight);
                        }

                        data.blendShapes.Add(blendShape);
                    }
                    catch (Exception ex)
                    {
                        BlendShapeLogger.LogErrorFormat("CreateBlendShapeData: Error processing blend shape at index {0} for '{1}'. Error: {2}", i, renderer.name, ex.Message);
                    }
                }

                BlendShapeLogger.LogFormat("CreateBlendShapeData: Successfully created data for '{0}' with {1} blend shapes", renderer.name, data.blendShapes.Count);
                return data;
            }
            catch (Exception ex)
            {
                BlendShapeLogger.LogErrorFormat("CreateBlendShapeData: Error creating blend shape data for '{0}'. Error: {1}", renderer.name, ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Finds a matching SkinnedMeshRenderer from a list based on BlendShapeData
        /// </summary>
        /// <param name="renderers">List of available SkinnedMeshRenderer components</param>
        /// <param name="meshData">The BlendShapeData to match against</param>
        /// <returns>Matching SkinnedMeshRenderer or null if not found</returns>
        public static SkinnedMeshRenderer FindMatchingRenderer(List<SkinnedMeshRenderer> renderers, BlendShapeData meshData)
        {
            if (renderers == null || renderers.Count == 0)
            {
                BlendShapeLogger.LogError("FindMatchingRenderer: Renderer list is null or empty");
                return null;
            }

            if (meshData == null)
            {
                BlendShapeLogger.LogError("FindMatchingRenderer: BlendShapeData is null");
                return null;
            }

            BlendShapeLogger.LogFormat("FindMatchingRenderer: Looking for renderer matching '{0}' (Path: '{1}')", meshData.objectName, meshData.objectPath);

            try
            {
                // Create lookup dictionaries for better performance
                var nameToRenderer = new Dictionary<string, SkinnedMeshRenderer>();
                var pathToRenderer = new Dictionary<string, SkinnedMeshRenderer>();

                foreach (var renderer in renderers)
                {
                    if (renderer != null)
                    {
                        // Build name lookup
                        if (!nameToRenderer.ContainsKey(renderer.name))
                        {
                            nameToRenderer[renderer.name] = renderer;
                        }

                        // Build path lookup
                        string rendererPath = GetObjectPath(renderer.gameObject);
                        if (!string.IsNullOrEmpty(rendererPath) && !pathToRenderer.ContainsKey(rendererPath))
                        {
                            pathToRenderer[rendererPath] = renderer;
                        }
                    }
                }

                // First, try to find by exact name match
                if (nameToRenderer.TryGetValue(meshData.objectName, out SkinnedMeshRenderer nameMatch))
                {
                    BlendShapeLogger.LogFormat("FindMatchingRenderer: Found exact name match for '{0}'", meshData.objectName);
                    return nameMatch;
                }

                // If no exact name match, try to find by path
                if (!string.IsNullOrEmpty(meshData.objectPath) && pathToRenderer.TryGetValue(meshData.objectPath, out SkinnedMeshRenderer pathMatch))
                {
                    BlendShapeLogger.LogFormat("FindMatchingRenderer: Found path match for '{0}' -> '{1}'", meshData.objectPath, pathMatch.name);
                    return pathMatch;
                }

                BlendShapeLogger.LogWarningFormat("FindMatchingRenderer: No matching renderer found for '{0}' (Path: '{1}')", meshData.objectName, meshData.objectPath);
                return null;
            }
            catch (Exception ex)
            {
                BlendShapeLogger.LogErrorFormat("FindMatchingRenderer: Error finding matching renderer for '{0}'. Error: {1}", meshData.objectName, ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Collects blend shape data from multiple renderers
        /// </summary>
        /// <param name="selectedObject">The root GameObject to collect data from</param>
        /// <param name="includeChildObjects">Whether to include child objects</param>
        /// <returns>MultiMeshBlendShapeData containing all collected data</returns>
        public static MultiMeshBlendShapeData CollectAllBlendShapeData(GameObject selectedObject, bool includeChildObjects)
        {
            if (selectedObject == null)
            {
                BlendShapeLogger.LogError("CollectAllBlendShapeData: selectedObject is null");
                return null;
            }

            try
            {
                MultiMeshBlendShapeData exportData = new MultiMeshBlendShapeData();
                exportData.rootObjectName = selectedObject.name;
                exportData.meshDataList = new List<BlendShapeData>();

                List<SkinnedMeshRenderer> renderers = CollectSkinnedMeshRenderers(selectedObject, includeChildObjects);

                if (renderers == null || renderers.Count == 0)
                {
                    BlendShapeLogger.LogWarning("CollectAllBlendShapeData: No renderers found");
                    return exportData;
                }

                foreach (SkinnedMeshRenderer renderer in renderers)
                {
                    if (renderer != null)
                    {
                        BlendShapeData meshData = CreateBlendShapeData(renderer);
                        if (meshData != null && meshData.blendShapes != null && meshData.blendShapes.Count > 0)
                        {
                            exportData.meshDataList.Add(meshData);
                            BlendShapeLogger.LogFormat("CollectAllBlendShapeData: Collected {0} blend shapes from '{1}'", meshData.blendShapes.Count, renderer.name);
                        }
                    }
                    else
                    {
                        BlendShapeLogger.LogWarning("CollectAllBlendShapeData: Null renderer encountered");
                    }
                }

                BlendShapeLogger.LogFormat("CollectAllBlendShapeData: Successfully collected data from {0} meshes", exportData.meshDataList.Count);
                return exportData;
            }
            catch (Exception ex)
            {
                BlendShapeLogger.LogException("CollectAllBlendShapeData: Error collecting blend shape data", ex);
                return null;
            }
        }

        /// <summary>
        /// Validates and serializes MultiMeshBlendShapeData to JSON
        /// </summary>
        /// <param name="exportData">The data to serialize</param>
        /// <returns>JSON string or null if validation fails</returns>
        public static string ValidateAndSerializeData(MultiMeshBlendShapeData exportData)
        {
            if (exportData == null)
            {
                BlendShapeLogger.LogError("ValidateAndSerializeData: Export data is null");
                return null;
            }

            if (exportData.meshDataList == null || exportData.meshDataList.Count == 0)
            {
                BlendShapeLogger.LogError("ValidateAndSerializeData: No mesh data found");
                return null;
            }

            try
            {
                string json = JsonUtility.ToJson(exportData, true);
                if (string.IsNullOrEmpty(json))
                {
                    throw new Exception("JSON serialization resulted in empty string");
                }
                BlendShapeLogger.LogFormat("ValidateAndSerializeData: JSON serialization successful ({0} characters)", json.Length);
                return json;
            }
            catch (Exception ex)
            {
                BlendShapeLogger.LogException("ValidateAndSerializeData: JSON serialization failed", ex);
                return null;
            }
        }

        /// <summary>
        /// Calculates the total number of blend shapes in MultiMeshBlendShapeData
        /// </summary>
        /// <param name="data">The MultiMeshBlendShapeData to count blend shapes from</param>
        /// <returns>Total number of blend shapes across all meshes</returns>
        public static int GetTotalBlendShapeCount(MultiMeshBlendShapeData data)
        {
            if (data == null || data.meshDataList == null)
            {
                return 0;
            }

            int totalCount = 0;
            foreach (var meshData in data.meshDataList)
            {
                if (meshData != null && meshData.blendShapes != null)
                {
                    totalCount += meshData.blendShapes.Count;
                }
            }

            return totalCount;
        }

        /// <summary>
        /// Validates and deserializes JSON to MultiMeshBlendShapeData
        /// </summary>
        /// <param name="json">The JSON string to deserialize</param>
        /// <returns>MultiMeshBlendShapeData or null if validation fails</returns>
        public static MultiMeshBlendShapeData ValidateAndDeserializeData(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                BlendShapeLogger.LogError("ValidateAndDeserializeData: JSON string is null or empty");
                return null;
            }

            try
            {
                MultiMeshBlendShapeData importData = JsonUtility.FromJson<MultiMeshBlendShapeData>(json);
                if (importData == null)
                {
                    throw new Exception("JSON deserialization returned null");
                }

                if (importData.meshDataList == null || importData.meshDataList.Count == 0)
                {
                    BlendShapeLogger.LogWarning("ValidateAndDeserializeData: No valid blend shape data found");
                    return null;
                }

                BlendShapeLogger.LogFormat("ValidateAndDeserializeData: Successfully deserialized {0} mesh data entries", importData.meshDataList.Count);
                return importData;
            }
            catch (Exception ex)
            {
                BlendShapeLogger.LogException("ValidateAndDeserializeData: JSON parsing failed", ex);
                return null;
            }
        }

        /// <summary>
        /// Applies blend shape values to a SkinnedMeshRenderer
        /// </summary>
        /// <param name="renderer">The target SkinnedMeshRenderer</param>
        /// <param name="meshData">The BlendShapeData containing values to apply</param>
        /// <returns>Number of blend shapes successfully applied</returns>
        public static int ApplyBlendShapesToRenderer(SkinnedMeshRenderer renderer, BlendShapeData meshData)
        {
            if (renderer == null)
            {
                BlendShapeLogger.LogError("ApplyBlendShapesToRenderer: SkinnedMeshRenderer is null");
                return 0;
            }

            if (renderer.sharedMesh == null)
            {
                BlendShapeLogger.LogErrorFormat("ApplyBlendShapesToRenderer: SkinnedMeshRenderer '{0}' has no shared mesh", renderer.name);
                return 0;
            }

            if (meshData == null || meshData.blendShapes == null)
            {
                BlendShapeLogger.LogError("ApplyBlendShapesToRenderer: BlendShapeData or blendShapes list is null");
                return 0;
            }

            try
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                int appliedCount = 0;
                int skippedCount = 0;

                BlendShapeLogger.LogFormat("ApplyBlendShapesToRenderer: Applying {0} blend shapes to '{1}'", meshData.blendShapes.Count, renderer.name);

                foreach (var blendShape in meshData.blendShapes)
                {
                    if (blendShape == null)
                    {
                        BlendShapeLogger.LogWarning("ApplyBlendShapesToRenderer: Null blend shape value encountered");
                        skippedCount++;
                        continue;
                    }

                    if (string.IsNullOrEmpty(blendShape.name))
                    {
                        BlendShapeLogger.LogWarningFormat("ApplyBlendShapesToRenderer: Blend shape with empty name encountered at index {0}", blendShape.index);
                        skippedCount++;
                        continue;
                    }

                    try
                    {
                        Mesh mesh = renderer.sharedMesh;
                        int targetIndex = -1;

                        // First try to find by the stored index
                        if (blendShape.index >= 0 && blendShape.index < mesh.blendShapeCount)
                        {
                            string meshShapeName = mesh.GetBlendShapeName(blendShape.index);
                            if (meshShapeName == blendShape.name)
                            {
                                targetIndex = blendShape.index;
                            }
                            else
                            {
                                BlendShapeLogger.LogWarningFormat("ApplyBlendShapesToRenderer: Index mismatch for '{0}' (expected at {1}, found '{2}')", blendShape.name, blendShape.index, meshShapeName);
                            }
                        }

                        // If index doesn't match, search by name
                        if (targetIndex == -1)
                        {
                            for (int i = 0; i < mesh.blendShapeCount; i++)
                            {
                                if (mesh.GetBlendShapeName(i) == blendShape.name)
                                {
                                    targetIndex = i;
                                    BlendShapeLogger.LogFormat("ApplyBlendShapesToRenderer: Found '{0}' at index {1} instead of {2}", blendShape.name, i, blendShape.index);
                                    break;
                                }
                            }
                        }

                        if (targetIndex != -1)
                        {
                            // Apply the blend shape weight
                            renderer.SetBlendShapeWeight(targetIndex, blendShape.weight);
                            appliedCount++;
                        }
                        else
                        {
                            BlendShapeLogger.LogWarningFormat("ApplyBlendShapesToRenderer: Blend shape '{0}' not found in mesh '{1}'", blendShape.name, renderer.name);
                            skippedCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        BlendShapeLogger.LogErrorFormat("ApplyBlendShapesToRenderer: Error applying blend shape '{0}' to '{1}'. Error: {2}", blendShape.name, renderer.name, ex.Message);
                        skippedCount++;
                    }
                }

                stopwatch.Stop();
                BlendShapeLogger.LogCompletion("ApplyBlendShapesToRenderer", stopwatch.Elapsed.TotalMilliseconds, $"Applied: {appliedCount}, Skipped: {skippedCount}");
                return appliedCount;
            }
            catch (Exception ex)
            {
                BlendShapeLogger.LogErrorFormat("ApplyBlendShapesToRenderer: Error applying blend shapes to '{0}'. Error: {1}", renderer.name, ex.Message);
                return 0;
            }
        }
    }
}