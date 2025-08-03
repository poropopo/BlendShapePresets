using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

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
            ExportBlendShapeValues();
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
            ImportBlendShapeValues();
        }

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "Select a JSON file and apply blend shape values to SkinnedMeshRenderers in the selected object" +
            (includeChildObjects ? " and its child objects" : "") + ".\n\n" +
            "Values will be automatically applied to meshes with matching object names or paths.",
            MessageType.Info);
    }

    void ExportBlendShapeValues()
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

    void ImportBlendShapeValues()
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

    SkinnedMeshRenderer FindMatchingRenderer(List<SkinnedMeshRenderer> renderers, BlendShapeData meshData)
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

    int ApplyBlendShapesToRenderer(SkinnedMeshRenderer renderer, BlendShapeData meshData)
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

    BlendShapeData CreateBlendShapeData(SkinnedMeshRenderer renderer)
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

    void CollectBlendShapeData(GameObject obj, List<BlendShapeData> dataList)
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

    string GetObjectPath(GameObject obj)
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


[System.Serializable]
public class MultiMeshBlendShapeData
{
    public string rootObjectName;
    public List<BlendShapeData> meshDataList;
}

[System.Serializable]
public class BlendShapeData
{
    public string objectName;
    public string objectPath;
    public List<BlendShapeValue> blendShapes;
}

[System.Serializable]
public class BlendShapeValue
{
    public string name;
    public int index;
    public float weight;
}
