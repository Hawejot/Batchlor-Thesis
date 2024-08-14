using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Responsible for finding the nearest valid position on a floor mesh
/// to place a prefab without overlapping with wall and object meshes,
/// based on the user's gaze direction.
/// </summary>
public class GazeBasedPrefabPlacer : MonoBehaviour
{
    public RoomMeshManager roomMeshManager;  // Reference to the RoomMeshManager
    public GameObject cameraRig;             // The camera rig representing the user's gaze
    public float gridSpacing = 0.2f;         // The spacing between grid points
    public LayerMask layerMask;              // Layer mask to specify which layers the raycast should hit

    private GameObject floorObject;
    private List<GameObject> wallAndObjectGameObjects;

    void Start()
    {
        // Initialize the floor and wall/object meshes by retrieving them from the RoomMeshManager.
        floorObject = roomMeshManager.GetFloorMesh();
        wallAndObjectGameObjects = roomMeshManager.GetWallMeshes();
        List<GameObject> temp = roomMeshManager.GetObjectMeshes();
        wallAndObjectGameObjects.AddRange(temp);
    }

    /// <summary>
    /// Finds the nearest valid position on the floor mesh to place the prefab,
    /// based on the user's gaze direction, without overlapping with wall and object meshes.
    /// </summary>
    /// <param name="prefab">The prefab to be placed.</param>
    /// <returns>A tuple containing the nearest valid position as a Vector3 and the required rotation as a Quaternion.</returns>
    public (Vector3, Quaternion) FindNearestValidPosition(GameObject prefab)
    {
        Vector3 targetPoint = GetGazeHitPoint();
        return FindNearestPosition(prefab, targetPoint, cameraRig);
    }

    /// <summary>
    /// Casts a ray from the camera rig in the forward direction to find the point where the gaze hits the floor.
    /// </summary>
    /// <returns>The point where the gaze hits the floor.</returns>
    private Vector3 GetGazeHitPoint()
    {
        Ray ray = new Ray(cameraRig.transform.position, cameraRig.transform.forward);
        RaycastHit hit;

        // Perform the raycast without using a layer mask
        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            // Check if the hit object is the specified floorObject
            if (hit.collider.gameObject == floorObject)
            {
                return hit.point;  // Return the hit point if it's the floorObject
            }
        }

        // If nothing is hit or the hit object is not the floorObject, return a point 2 meters forward from the camera
        return cameraRig.transform.position + cameraRig.transform.forward * 2f;
    }


    /// <summary>
    /// Finds the nearest valid position on the floor mesh to place the prefab,
    /// given a target point, without overlapping with wall and object meshes,
    /// and ensuring the prefab faces a given target GameObject.
    /// </summary>
    /// <param name="prefab">The prefab to be placed.</param>
    /// <param name="targetPoint">The target point for placement.</param>
    /// <param name="targetLook">The target GameObject that the prefab should face.</param>
    /// <returns>A tuple containing the nearest valid position as a Vector3 and the required rotation as a Quaternion.</returns>
    private (Vector3, Quaternion) FindNearestPosition(GameObject prefab, Vector3 targetPoint, GameObject targetLook)
    {
        Bounds floorBounds = floorObject.GetComponent<Renderer>().bounds;
        Bounds prefabBounds = prefab.GetComponent<Renderer>().bounds;

        Vector3 nearestPosition = Vector3.zero;
        Quaternion requiredRotation = Quaternion.identity;
        float shortestDistance = Mathf.Infinity;

        float prefabHeight = prefabBounds.size.y;

        for (float x = floorBounds.min.x; x <= floorBounds.max.x; x += gridSpacing)
        {
            for (float z = floorBounds.min.z; z <= floorBounds.max.z; z += gridSpacing)
            {
                Vector3 testPosition = new Vector3(x, floorBounds.min.y, z);
                testPosition.y += prefabHeight / 2;

                if (IsPositionValid(testPosition, prefabBounds))
                {
                    float distance = Vector3.Distance(targetPoint, testPosition);
                    if (distance < shortestDistance)
                    {
                        shortestDistance = distance;
                        nearestPosition = testPosition;
                        Vector3 directionToTarget = targetLook.transform.position - testPosition;
                        Quaternion lookRotation = Quaternion.LookRotation(directionToTarget);
                        float yRotation = lookRotation.eulerAngles.y - 180f;
                        requiredRotation = Quaternion.Euler(0, yRotation, 0);
                    }
                }
            }
        }

        return (nearestPosition, requiredRotation);
    }

    /// <summary>
    /// Checks if the given position is valid by ensuring the prefab does not overlap
    /// with any wall or object meshes.
    /// </summary>
    /// <param name="position">The position to be checked.</param>
    /// <param name="prefabBounds">The bounds of the prefab.</param>
    /// <returns>True if the position is valid, false otherwise.</returns>
    private bool IsPositionValid(Vector3 position, Bounds prefabBounds)
    {
        Vector3 prefabMin = position + prefabBounds.min;
        Vector3 prefabMax = position + prefabBounds.max;

        foreach (GameObject wallObject in wallAndObjectGameObjects)
        {
            MeshFilter wallMeshFilter = wallObject.GetComponent<MeshFilter>();
            if (wallMeshFilter == null) continue;

            Mesh wallMesh = wallMeshFilter.sharedMesh;
            foreach (Vector3 vertex in wallMesh.vertices)
            {
                Vector3 worldVertex = wallObject.transform.TransformPoint(vertex);

                if (worldVertex.x >= prefabMin.x && worldVertex.x <= prefabMax.x &&
                    worldVertex.y >= prefabMin.y && worldVertex.y <= prefabMax.y &&
                    worldVertex.z >= prefabMin.z && worldVertex.z <= prefabMax.z)
                {
                    return false;
                }
            }
        }

        return true;
    }
}
