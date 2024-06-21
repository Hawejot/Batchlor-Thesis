using UnityEngine;
using System.Collections.Generic;
using Meta.XR.MRUtilityKit;

public class FloorMeshGenerator : MonoBehaviour
{
    private GameObject floorMeshObject;

    public void GenerateFloorMesh(MRUKRoom room)
    {
        if (room.FloorAnchor == null)
        {
            Debug.LogError("Floor anchor not found.");
            return;
        }

        floorMeshObject = CreateMeshObject("FloorMesh");
        MeshFilter floorMeshFilter = floorMeshObject.GetComponent<MeshFilter>();
        GenerateMesh(floorMeshFilter, room.FloorAnchor.PlaneBoundary2D, room.FloorAnchor.transform);
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

        // Triangulate the floor using a simple fan method
        List<int> triangles = new List<int>();
        for (int i = 1; i < vertices.Count - 1; i++)
        {
            triangles.Add(0);
            triangles.Add(i);
            triangles.Add(i + 1);
        }
        mesh.triangles = triangles.ToArray();

        // Correct orientation to ensure the mesh is flat and correctly oriented
        Vector3[] normals = new Vector3[vertices.Count];
        for (int i = 0; i < normals.Length; i++)
        {
            normals[i] = Vector3.up;
        }
        mesh.normals = normals;

        mesh.RecalculateBounds();
    }

    public GameObject GetFloorMesh()
    {
        return floorMeshObject;
    }
}
