using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public static class BlendShapeImporter
{
    public static void ImportBlendShapeValues(bool includeChildObjects)
    {
        GameObject selectedObject = Selection.activeGameObject;
        if (selectedObject == null)
        {
            EditorUtility.DisplayDialog("Error", "No object selected", "OK");
            return;
        }

        string path = EditorUtility.OpenFilePanel(
            "Select Blend Shape JSON File",
            "",
            "json");

        if (string.IsNullOrEmpty(path))
        {
            return;
        }
        MultiMeshBlendShapeData importData;
        try
        {
            string json = File.ReadAllText(path);
            importData = JsonUtility.FromJson<MultiMeshBlendShapeData>(json);
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("Error", $"Failed to load JSON file: {e.Message}", "OK");
            return;
        }

        if (importData == null || importData.meshDataList == null || importData.meshDataList.Count == 0)
        {
            EditorUtility.DisplayDialog("Error", "Invalid JSON file", "OK");
            return;
        }

        // Collect target meshes for applying blend shapes
        List<SkinnedMeshRenderer> targetRenderers = new List<SkinnedMeshRenderer>();

        SkinnedMeshRenderer selectedRenderer = selectedObject.GetComponent<SkinnedMeshRenderer>();
        if (selectedRenderer != null)
        {
            targetRenderers.Add(selectedRenderer);
        }

        if (includeChildObjects)
        {
            SkinnedMeshRenderer[] childRenderers = selectedObject.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (SkinnedMeshRenderer renderer in childRenderers)
            {
                if (renderer.gameObject != selectedObject && !targetRenderers.Contains(renderer))
                {
                    targetRenderers.Add(renderer);
                }
            }
        }

        if (targetRenderers.Count == 0)
        {
            EditorUtility.DisplayDialog("Warning", "No SkinnedMeshRenderer found", "OK");
            return;
        }

        // Apply blend shape values
        int appliedMeshes = 0;
        int totalAppliedShapes = 0;

        foreach (BlendShapeData meshData in importData.meshDataList)
        {
            SkinnedMeshRenderer targetRenderer = FindMatchingRenderer(targetRenderers, meshData);

            if (targetRenderer == null)
            {
                Debug.LogWarning($"No matching mesh found: {meshData.objectName} (Path: {meshData.objectPath})");
                continue;
            }

            int appliedShapes = ApplyBlendShapesToRenderer(targetRenderer, meshData);
            if (appliedShapes > 0)
            {
                appliedMeshes++;
                totalAppliedShapes += appliedShapes;
                Debug.Log($"Applied {appliedShapes} blend shapes to '{targetRenderer.name}'");
            }
        }
        if (appliedMeshes > 0)
        {
            EditorUtility.DisplayDialog("Complete",
                $"Blend shape application completed\n\n" +
                $"Applied meshes: {appliedMeshes}\n" +
                $"Total applied shape keys: {totalAppliedShapes}", "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("Warning", "No applicable meshes found", "OK");
        }
    }

    private static SkinnedMeshRenderer FindMatchingRenderer(List<SkinnedMeshRenderer> renderers, BlendShapeData meshData)
    {
        // 1. Prioritize exact name match
        foreach (SkinnedMeshRenderer renderer in renderers)
        {
            if (renderer.name == meshData.objectName)
            {
                return renderer;
            }
        }

        // 2. Check path match if path is provided
        if (!string.IsNullOrEmpty(meshData.objectPath))
        {
            foreach (SkinnedMeshRenderer renderer in renderers)
            {
                string currentPath = GetObjectPath(renderer.gameObject);
                if (currentPath.EndsWith(meshData.objectPath))
                {
                    return renderer;
                }
            }
        }

        return null;
    }

    private static int ApplyBlendShapesToRenderer(SkinnedMeshRenderer renderer, BlendShapeData meshData)
    {
        if (renderer.sharedMesh == null)
        {
            return 0;
        }

        Mesh mesh = renderer.sharedMesh;
        int appliedCount = 0;

        foreach (BlendShapeValue blendShape in meshData.blendShapes)
        {
            // Try to find by index first, then fallback to name search
            if (blendShape.index >= 0 && blendShape.index < mesh.blendShapeCount)
            {
                string meshShapeName = mesh.GetBlendShapeName(blendShape.index);

                if (meshShapeName == blendShape.name)
                {
                    renderer.SetBlendShapeWeight(blendShape.index, blendShape.weight);
                    appliedCount++;
                }
                else
                {
                    // Index mismatch, search by name
                    for (int i = 0; i < mesh.blendShapeCount; i++)
                    {
                        if (mesh.GetBlendShapeName(i) == blendShape.name)
                        {
                            renderer.SetBlendShapeWeight(i, blendShape.weight);
                            appliedCount++;
                            break;
                        }
                    }
                }
            }
            else
            {
                // Invalid index, search by name
                for (int i = 0; i < mesh.blendShapeCount; i++)
                {
                    if (mesh.GetBlendShapeName(i) == blendShape.name)
                    {
                        renderer.SetBlendShapeWeight(i, blendShape.weight);
                        appliedCount++;
                        break;
                    }
                }
            }
        }

        return appliedCount;
    }

    private static string GetObjectPath(GameObject obj)
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
}