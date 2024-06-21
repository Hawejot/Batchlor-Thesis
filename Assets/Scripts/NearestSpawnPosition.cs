using UnityEngine;
using Meta.XR.MRUtilityKit;

public class NearestSpawnPosition : MonoBehaviour
{
    #region Fields

    [Tooltip("Prefab to be placed into the scene, or object in the scene to be moved around.")]
    public GameObject spawnObject;

    [Tooltip("Maximum number of times to attempt finding a valid position before giving up.")]
    public int maxIterations = 5;

    [SerializeField, Tooltip("Check for overlaps with other colliders.")]
    private bool checkOverlaps = true;

    [SerializeField, Tooltip("Override bounds for free space.")]
    private float overrideBounds = -1;

    [SerializeField, Tooltip("Layer mask for bounding box checks.")]
    private LayerMask layerMask = -1;

    [SerializeField, Tooltip("Clearance distance for valid spawn positions.")]
    private float surfaceClearanceDistance = 0.1f;

    #endregion

    #region Public Methods

    /// <summary>
    /// Finds the nearest valid position to spawn the object within the room.
    /// </summary>
    /// <param name="targetPosition">The target position for spawning.</param>
    /// <param name="targetObject">The target object to align the rotation towards.</param>
    /// <returns>The nearest valid position if found; otherwise, null.</returns>
    public Vector3? FindNearestValidPosition(Vector3 targetPosition, GameObject targetObject)
    {
        // Ensure the MRUK instance and current room are available
        if (MRUK.Instance == null) return null;

        MRUKRoom room = MRUK.Instance.GetCurrentRoom();
        if (room == null) return null;

        // Get the bounds of the prefab
        var prefabBounds = Utilities.GetPrefabBounds(spawnObject);
        float baseOffset = -prefabBounds?.min.y ?? 0.0f;
        float centerOffset = prefabBounds?.center.y ?? 0.0f;
        Bounds adjustedBounds = new();

        // Adjust the bounds based on the prefab size and clearance distance
        if (prefabBounds.HasValue)
        {
            var min = prefabBounds.Value.min;
            var max = prefabBounds.Value.max;
            min.y += surfaceClearanceDistance;
            max.y = Mathf.Max(max.y, min.y);

            adjustedBounds.SetMinMax(min, max);

            // Override bounds if specified
            if (overrideBounds > 0)
            {
                Vector3 center = new Vector3(0f, surfaceClearanceDistance, 0f);
                Vector3 size = new Vector3(overrideBounds * 2f, surfaceClearanceDistance * 2f, overrideBounds * 2f); // OverrideBounds represents the extents, not the size
                adjustedBounds = new Bounds(center, size);
            }
        }

        // Check for valid spawn positions around the target position
        for (int j = 0; j < maxIterations; ++j)
        {
            if (TryFindValidPositionOnFloor(targetPosition, targetObject, baseOffset, centerOffset, adjustedBounds, out Vector3 validPosition, out Quaternion validRotation))
            {
                spawnObject.transform.position = validPosition;
                spawnObject.transform.rotation = validRotation;
                return validPosition;
            }
        }

        return null;
    }

    /// <summary>
    /// Sets the spawn object to be used.
    /// </summary>
    /// <param name="obj">The object to be set as the spawn object.</param>
    public void SetSpawnObject(GameObject obj)
    {
        spawnObject = obj;
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Attempts to find a valid spawn position on the floor near the target position.
    /// </summary>
    private bool TryFindValidPositionOnFloor(Vector3 targetPosition, GameObject targetObject, float baseOffset, float centerOffset, Bounds adjustedBounds, out Vector3 validPosition, out Quaternion validRotation)
    {
        validPosition = Vector3.zero;
        validRotation = Quaternion.identity;

        MRUKRoom room = MRUK.Instance.GetCurrentRoom();
        if (room == null || room.FloorAnchor == null)
        {
            return false;
        }

        var floorBounds = room.FloorAnchor.PlaneRect;
        if (!floorBounds.HasValue)
        {
            return false;
        }

        Vector3 floorPosition = room.FloorAnchor.transform.TransformPoint(new Vector3(
            Mathf.Clamp(targetPosition.x, floorBounds.Value.min.x, floorBounds.Value.max.x),
            0,
            Mathf.Clamp(targetPosition.z, floorBounds.Value.min.y, floorBounds.Value.max.y)
        ));

        Vector3 directionToTarget = (targetObject.transform.position - floorPosition).normalized;
        Quaternion rotationToTarget = Quaternion.LookRotation(directionToTarget, Vector3.up);

        if (IsValidSpawnPosition(floorPosition, rotationToTarget, adjustedBounds))
        {
            validPosition = floorPosition;
            validRotation = rotationToTarget;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Checks if the spawn position is valid.
    /// </summary>
    private bool IsValidSpawnPosition(Vector3 position, Quaternion rotation, Bounds bounds)
    {
        if (!MRUK.Instance.GetCurrentRoom().IsPositionInRoom(position)) return false;
        if (checkOverlaps && Physics.CheckBox(position + rotation * bounds.center, bounds.extents, rotation, layerMask, QueryTriggerInteraction.Ignore)) return false;
        return true;
    }

    #endregion
}
