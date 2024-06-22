using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Responsible for finding the nearest valid position on a floor mesh
/// to place a prefab without overlapping with wall and object meshes,
/// and ensuring the prefab faces a given target GameObject.
/// </summary>
public class PrefabPositionFinder : MonoBehaviour
{
    // Reference to the RoomMeshManager which provides the meshes.
    public RoomMeshManager roomMeshManager;
    public GameObject targetLook;  // The target GameObject that the prefab should face
    public float gridSpacing = 0.2f;  // The spacing between grid points

    private GameObject floorObject;
    private List<GameObject> wallAndObjectGameObjects;

    /// <summary>
    /// Initializes the floor and wall/object meshes by retrieving them from the RoomMeshManager.
    /// </summary>
    void Start()
    {
        floorObject = roomMeshManager.GetFloorMesh();
        wallAndObjectGameObjects = roomMeshManager.GetWallMeshes();
        List<GameObject> temp = roomMeshManager.GetObjectMeshes();
        wallAndObjectGameObjects.AddRange(temp);
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
    public (Vector3, Quaternion) FindNearestPosition(GameObject prefab, Vector3 targetPoint, GameObject targetLook)
    {
        Bounds floorBounds = floorObject.GetComponent<Renderer>().bounds;
        Bounds prefabBounds = prefab.GetComponent<Renderer>().bounds;

        Vector3 nearestPosition = Vector3.zero;
        Quaternion requiredRotation = Quaternion.identity;
        float shortestDistance = Mathf.Infinity;

        float prefabHeight = prefabBounds.size.y;  // Get the height of the prefab

        // Iterate through each point in the grid within the bounds of the floor object.
        for (float x = floorBounds.min.x; x <= floorBounds.max.x; x += gridSpacing)
        {
            for (float z = floorBounds.min.z; z <= floorBounds.max.z; z += gridSpacing)
            {
                Vector3 testPosition = new Vector3(x, floorBounds.min.y, z);
                testPosition.y += prefabHeight / 2;  // Adjust the test position to account for the prefab height

                // Check if the test position is valid and the prefab faces the target.
                if (IsPositionValid(testPosition, prefabBounds, wallAndObjectGameObjects))
                {
                    // Calculate the distance between the target point and the test position
                    float distance = Vector3.Distance(targetPoint, testPosition);
                    // If this distance is shorter than the previously found shortest distance, update the nearest position and shortest distance
                    if (distance < shortestDistance)
                    {
                        shortestDistance = distance;
                        nearestPosition = testPosition;
                        // Calculate the required rotation for the prefab to face the target
                        Vector3 directionToTarget = targetLook.transform.position - testPosition;
                        Quaternion lookRotation = Quaternion.LookRotation(directionToTarget);
                        // Extract the Y rotation and subtract 180 degrees
                        float yRotation = lookRotation.eulerAngles.y - 180f;
                        requiredRotation = Quaternion.Euler(0, yRotation, 0);
                    }
                }
            }
        }

        // Return the nearest valid position found and the required rotation
        return (nearestPosition, requiredRotation);
    }

    /// <summary>
    /// Checks if the given position is valid by ensuring the prefab does not overlap
    /// with any wall or object meshes.
    /// </summary>
    /// <param name="position">The position to be checked.</param>
    /// <param name="prefabBounds">The bounds of the prefab.</param>
    /// <param name="wallAndObjectGameObjects">A list of game objects representing walls and objects.</param>
    /// <returns>True if the position is valid, false otherwise.</returns>
    private bool IsPositionValid(Vector3 position, Bounds prefabBounds, List<GameObject> wallAndObjectGameObjects)
    {
        Vector3 prefabMin = position + prefabBounds.min;
        Vector3 prefabMax = position + prefabBounds.max;

        // Iterate through each wall and object game object to check for overlap.
        foreach (GameObject wallObject in wallAndObjectGameObjects)
        {
            MeshFilter wallMeshFilter = wallObject.GetComponent<MeshFilter>();
            if (wallMeshFilter == null) continue;

            Mesh wallMesh = wallMeshFilter.sharedMesh;
            foreach (Vector3 vertex in wallMesh.vertices)
            {
                Vector3 worldVertex = wallObject.transform.TransformPoint(vertex);

                // Check if any vertex of the wall or object mesh overlaps with the prefab bounds.
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
