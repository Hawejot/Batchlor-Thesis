using UnityEngine;
using System.Collections.Generic;
using Meta.XR.MRUtilityKit;

public class FloorMeshGenerator : MonoBehaviour
{
    private GameObject floorMeshObject;
    public string floorLayerName = "Floor"; // Ensure this layer exists in the Unity project

    private bool meshrendererEnabled = true;

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

        // Set the MeshRenderer to inactive to make the mesh invisible
        meshRenderer.enabled = meshrendererEnabled;

        // Set the layer to "Floor"
        int floorLayer = LayerMask.NameToLayer(floorLayerName);
        if (floorLayer == -1)
        {
            Debug.LogError("Layer '" + floorLayerName + "' not found. Please ensure it exists in the project.");
        }
        else
        {
            meshObject.layer = floorLayer;
        }

        MeshCollider meshCollider = meshObject.AddComponent<MeshCollider>();
        meshCollider.sharedMesh = meshObject.GetComponent<MeshFilter>().mesh;
        meshCollider.convex = false;  // Ensure it's not convex, unless you have a specific need for it

        return meshObject;
    }

    private void GenerateMesh(MeshFilter meshFilter, List<Vector2> boundary2D, Transform transform)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> indices = new List<int>();

        // Add the boundary vertices
        foreach (var point in boundary2D)
        {
            Vector3 worldPoint = transform.TransformPoint(new Vector3(point.x, point.y, 0));
            vertices.Add(worldPoint);
        }

        // Triangulate the vertices
        List<int> triangles = Triangulate(vertices);

        Mesh mesh = new Mesh();
        meshFilter.mesh = mesh;

        mesh.vertices = vertices.ToArray();
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

    private List<int> Triangulate(List<Vector3> vertices)
    {
        List<int> triangles = new List<int>();

        // Triangulate using a simple fan method
        for (int i = 1; i < vertices.Count - 1; i++)
        {
            triangles.Add(0);
            triangles.Add(i);
            triangles.Add(i + 1);
        }

        return triangles;
    }

    public GameObject GetFloorMesh()
    {
        return floorMeshObject;
    }

    public void setMeshRendererEnabled(bool enabled)
    {
        meshrendererEnabled = enabled;
        floorMeshObject.GetComponent<MeshRenderer>().enabled = enabled;
    }
}
