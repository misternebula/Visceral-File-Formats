using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class TSGMesh : MonoBehaviour
{
    public bool SwapUV;
    public bool Split;
    public List<int> SplitSizes = new List<int>();

    [SerializeField]
    private Mesh _referenceMesh;

    private MeshFilter _filter;

    void Start() 
        => _filter = GetComponent<MeshFilter>();

    void OnValidate()
    {
        if (Split)
        {
            SplitMesh();
        }
        Split = false;

        if (SwapUV)
        {
            SwapUVs();
        }
        SwapUV = false;
    }

    public void SetReferenceMesh(Mesh mesh) 
        => _referenceMesh = mesh;

    private void SplitMesh()
    {
        var triangles = _referenceMesh.GetTriangles(0);
        var newTriList = new List<List<int>>();
        for (var i = 0; i < SplitSizes.Count; i++)
        {
            var getTriangles = i == SplitSizes.Count - 1
                ? triangles.Skip(SplitSizes[i] * 3).ToList()
                : triangles.Skip(SplitSizes[i] * 3).Take((SplitSizes[i + 1] - SplitSizes[i]) * 3).ToList();
            newTriList.Add(getTriangles);
            var vertOne = _referenceMesh.vertices[getTriangles[0]];
            var vertTwo = _referenceMesh.vertices[getTriangles[1]];
            var vertThree = _referenceMesh.vertices[getTriangles[2]];
            Debug.Log($"First triangle of split {i} is {vertOne}, {vertTwo}, {vertThree}");
        }

        var newMesh = new Mesh();
        newMesh.SetVertices(_referenceMesh.vertices);
        newMesh.subMeshCount = SplitSizes.Count + 1;
        for (var i = 0; i < newTriList.Count; i++)
        {
            newMesh.SetTriangles(newTriList[i], i);
        }
        newMesh.SetUVs(0, _referenceMesh.uv);
        newMesh.SetUVs(1, _referenceMesh.uv2);
        newMesh.RecalculateNormals();
        _filter.sharedMesh = newMesh;
    }

    private void SwapUVs()
    {
        var uv1 = _filter.sharedMesh.uv;
        var uv2 = _filter.sharedMesh.uv2;
        _filter.sharedMesh.uv = uv2;
        _filter.sharedMesh.uv2 = uv1;
        _filter.sharedMesh.RecalculateTangents();
        Debug.Log($"uv is now {_filter.sharedMesh.uv}, uv2 is now {_filter.sharedMesh.uv2}");
    }
}
