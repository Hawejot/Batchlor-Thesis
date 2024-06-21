using UnityEngine;
using System.Collections.Generic;
using Meta.XR.MRUtilityKit;

namespace Meta.XR.MRUtilityKit
{
    public class ObjectMeshGenerator : MonoBehaviour
    {
        private List<GameObject> objectMeshObjects = new List<GameObject>();

        public void GenerateObjectsMeshes(MRUKRoom room)
        {
            if (room.Anchors == null || room.Anchors.Count == 0)
            {
                Debug.LogError("Room anchors not found.");
                return;
            }

            foreach (var anchor in room.Anchors)
            {
                // Skip floor, wall, and ceiling anchors
                if (room.FloorAnchor == anchor || room.CeilingAnchor == anchor || room.WallAnchors.Contains(anchor))
                {
                    Debug.Log($"Skipping anchor {anchor.name} as it is classified as floor, wall, or ceiling.");
                    continue;
                }

                GameObject objectMeshObject = CreateMeshObject("ObjectMesh_" + anchor.name);
                MeshFilter objectMeshFilter = objectMeshObject.GetComponent<MeshFilter>();

                if (anchor.PlaneBoundary2D != null && anchor.PlaneBoundary2D.Count > 0)
                {
                    GenerateMesh(objectMeshFilter, anchor.PlaneBoundary2D, anchor.transform);
                }
                else if (anchor.VolumeBounds.HasValue)
                {
                    GenerateBoxMesh(objectMeshFilter, anchor.VolumeBounds.Value, anchor.transform);
                }
                else
                {
                    Debug.LogWarning($"Anchor {anchor.name} has no boundary data and no volume bounds.");
                    Destroy(objectMeshObject);
                    continue;
                }

                objectMeshObjects.Add(objectMeshObject);
                Debug.Log($"Created mesh for anchor {anchor.name}.");
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

            return meshObject;
        }

        private void GenerateMesh(MeshFilter meshFilter, List<Vector2> boundary2D, Transform transform)
        {
            if (boundary2D == null)
            {
                Debug.LogError("Boundary2D is null.");
                return;
            }

            if (transform == null)
            {
                Debug.LogError("Transform is null.");
                return;
            }

            List<Vector3> vertices = new List<Vector3>();
            foreach (var point in boundary2D)
            {
                Vector3 worldPoint = transform.TransformPoint(new Vector3(point.x, point.y, 0));
                vertices.Add(worldPoint);
            }

            if (vertices.Count < 3)
            {
                Debug.LogWarning("Not enough vertices to create a mesh.");
                return;
            }

            Mesh mesh = new Mesh();
            meshFilter.mesh = mesh;

            mesh.vertices = vertices.ToArray();

            // Triangulate the object using a simple fan method
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
                normals[i] = Vector3.up;
            }
            mesh.normals = normals;

            mesh.RecalculateBounds();
        }

        private void GenerateBoxMesh(MeshFilter meshFilter, Bounds bounds, Transform transform)
        {
            Mesh mesh = new Mesh();
            meshFilter.mesh = mesh;

            Vector3 size = bounds.size;
            Vector3 center = bounds.center;

            Vector3[] vertices = new Vector3[8];

            vertices[0] = transform.TransformPoint(center + new Vector3(-size.x, -size.y, -size.z) * 0.5f);
            vertices[1] = transform.TransformPoint(center + new Vector3(size.x, -size.y, -size.z) * 0.5f);
            vertices[2] = transform.TransformPoint(center + new Vector3(size.x, -size.y, size.z) * 0.5f);
            vertices[3] = transform.TransformPoint(center + new Vector3(-size.x, -size.y, size.z) * 0.5f);

            vertices[4] = transform.TransformPoint(center + new Vector3(-size.x, size.y, -size.z) * 0.5f);
            vertices[5] = transform.TransformPoint(center + new Vector3(size.x, size.y, -size.z) * 0.5f);
            vertices[6] = transform.TransformPoint(center + new Vector3(size.x, size.y, size.z) * 0.5f);
            vertices[7] = transform.TransformPoint(center + new Vector3(-size.x, size.y, size.z) * 0.5f);

            mesh.vertices = vertices;

            int[] triangles = {
                // Bottom
                0, 2, 1, 0, 3, 2,
                // Top
                4, 5, 6, 4, 6, 7,
                // Front
                0, 1, 5, 0, 5, 4,
                // Back
                2, 3, 7, 2, 7, 6,
                // Left
                0, 4, 7, 0, 7, 3,
                // Right
                1, 2, 6, 1, 6, 5
            };
            mesh.triangles = triangles;

            Vector3[] normals = new Vector3[8];
            for (int i = 0; i < normals.Length; i++)
            {
                normals[i] = Vector3.up;
            }
            mesh.normals = normals;

            mesh.RecalculateBounds();
        }

        public List<GameObject> GetObjectMeshes()
        {
            return objectMeshObjects;
        }
    }
}
