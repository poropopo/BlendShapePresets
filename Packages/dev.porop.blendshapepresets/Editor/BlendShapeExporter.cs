using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System;

public static class BlendShapeExporter
{
    public static void ExportBlendShapeValues(bool includeChildObjects)
    {
        GameObject selectedObject = Selection.activeGameObject;
        if (selectedObject == null)
        {
            EditorUtility.DisplayDialog("Error", "No object selected", "OK");
            return;
        }

        // Collect blend shape data from multiple meshes
        MultiMeshBlendShapeData exportData = new MultiMeshBlendShapeData();
        exportData.rootObjectName = selectedObject.name;
        exportData.meshDataList = new List<BlendShapeData>();

        CollectBlendShapeData(selectedObject, exportData.meshDataList);

        if (includeChildObjects)
        {
            SkinnedMeshRenderer[] childRenderers = selectedObject.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (SkinnedMeshRenderer renderer in childRenderers)
            {
                if (renderer.gameObject != selectedObject)
                {
                    CollectBlendShapeData(renderer.gameObject, exportData.meshDataList);
                }
            }
        }

        if (exportData.meshDataList.Count == 0)
        {
            EditorUtility.DisplayDialog("Warning", "No SkinnedMeshRenderer found", "OK");
            return;
        }

        string json = JsonUtility.ToJson(exportData, true);

        string path = EditorUtility.SaveFilePanel(
            "Save Blend Shape Data",
            "",
            selectedObject.name + "_blendshapes.json",
            "json");

        if (string.IsNullOrEmpty(path))
        {
            return;
        }
        try
        {
            File.WriteAllText(path, json);
            Debug.Log($"Blend shape data saved: {path}");
            Debug.Log($"Data collected from {exportData.meshDataList.Count} meshes");

            EditorUtility.DisplayDialog("Complete",
                $"Blend shape data saved\n{path}\n\n" +
                $"Mesh count: {exportData.meshDataList.Count}", "OK");
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("Error", $"Failed to save file: {e.Message}", "OK");
        }
    }

    private static BlendShapeData CreateBlendShapeData(SkinnedMeshRenderer renderer)
    {
        BlendShapeData data = new BlendShapeData();
        data.objectName = renderer.name;
        data.objectPath = GetObjectPath(renderer.gameObject);
        data.blendShapes = new List<BlendShapeValue>();

        Mesh mesh = renderer.sharedMesh;
        for (int i = 0; i < mesh.blendShapeCount; i++)
        {
            BlendShapeValue blendShape = new BlendShapeValue();
            blendShape.name = mesh.GetBlendShapeName(i);
            blendShape.index = i;
            blendShape.weight = renderer.GetBlendShapeWeight(i);
            data.blendShapes.Add(blendShape);
        }

        return data;
    }

    private static void CollectBlendShapeData(GameObject obj, List<BlendShapeData> dataList)
    {
        SkinnedMeshRenderer renderer = obj.GetComponent<SkinnedMeshRenderer>();
        if (renderer == null || renderer.sharedMesh == null)
            return;

        Mesh mesh = renderer.sharedMesh;
        if (mesh.blendShapeCount == 0)
            return;

        Debug.Log($"Collected {mesh.blendShapeCount} shape keys from mesh '{obj.name}'");

        BlendShapeData meshData = CreateBlendShapeData(renderer);
        dataList.Add(meshData);
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