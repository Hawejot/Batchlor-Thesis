using UnityEngine;
using System.Collections.Generic;
using Meta.XR.MRUtilityKit;

public class WallMeshGenerator : MonoBehaviour
{
    private List<GameObject> wallMeshObjects = new List<GameObject>();

    private bool meshrendererEnabled = false;

    public void GenerateWallsMeshes(MRUKRoom room)
    {
        if (room.WallAnchors == null || room.WallAnchors.Count == 0)
        {
            Debug.LogError("Wall anchors not found.");
            return;
        }

        foreach (var wallAnchor in room.WallAnchors)
        {
            GameObject wallMeshObject = CreateMeshObject("WallMesh");
            MeshFilter wallMeshFilter = wallMeshObject.GetComponent<MeshFilter>();
            GenerateMesh(wallMeshFilter, wallAnchor.PlaneBoundary2D, wallAnchor.transform);
            wallMeshObjects.Add(wallMeshObject);
        }
    }

    private GameObject CreateMeshObject(string name)
    {
        GameObject meshObject = new GameObject(name);
        meshObject.transform.SetParent(transform);
        meshObject.transform.localPosition = Vector3.zero;
        meshObject.transform.localRotation = Quaternion.identity;

        meshObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = meshObject.AddComponent<MeshRenderer>();
        meshRenderer.material = new Material(Shader.Find("Standard"));

        // Set the MeshRenderer to inactive to make the mesh invisible
        meshRenderer.enabled = meshrendererEnabled;

        return meshObject;
    }

    private void GenerateMesh(MeshFilter meshFilter, List<Vector2> boundary2D, Transform transform)
    {
        List<Vector3> vertices = new List<Vector3>();
        foreach (var point in boundary2D)
        {
            Vector3 worldPoint = transform.TransformPoint(new Vector3(point.x, point.y, 0));
            vertices.Add(worldPoint);
        }

        Mesh mesh = new Mesh();
        meshFilter.mesh = mesh;

        mesh.vertices = vertices.ToArray();

        // Triangulate the wall using a simple fan method
        List<int> triangles = new List<int>();
        for (int i = 1; i < vertices.Count - 1; i++)
        {
            triangles.Add(0);
            triangles.Add(i);
            triangles.Add(i + 1);
        }
        mesh.triangles = triangles.ToArray();

        // Correct orientation to ensure the mesh is correctly oriented
        Vector3[] normals = new Vector3[vertices.Count];
        for (int i = 0; i < normals.Length; i++)
        {
            normals[i] = Vector3.forward;
        }
        mesh.normals = normals;

        mesh.RecalculateBounds();
    }

    public List<GameObject> GetWallMeshes()
    {
        return wallMeshObjects;
    }

    public void setMeshRendererEnabled(bool enabled)
    {
        meshrendererEnabled = enabled;
        foreach (var wallMeshObject in wallMeshObjects)
        {
            MeshRenderer meshRenderer = wallMeshObject.GetComponent<MeshRenderer>();
            meshRenderer.enabled = enabled;
        }
    }
}
