using System.Collections.Generic;

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