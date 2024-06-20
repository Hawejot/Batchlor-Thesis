using UnityEngine;
using Meta.XR.MRUtilityKit;

public class NearestSpawnPosition : MonoBehaviour
{
    [Tooltip("Prefab to be placed into the scene, or object in the scene to be moved around.")]
    public GameObject spawnObject;

    [Tooltip("Maximum number of times to attempt finding a valid position before giving up.")]
    public int maxIterations = 1000;

    public enum SpawnLocation
    {
        Floating,
        AnySurface,
        VerticalSurfaces,
        OnTopOfSurfaces,
        HangingDown
    }

    [SerializeField, Tooltip("Attach content to scene surfaces.")]
    private SpawnLocation spawnLocation = SpawnLocation.Floating;

    [SerializeField, Tooltip("Filter for surface spawning.")]
    private MRUKAnchor.SceneLabels labels = ~(MRUKAnchor.SceneLabels)0;

    [SerializeField, Tooltip("Check for overlaps with other colliders.")]
    private bool checkOverlaps = true;

    [SerializeField, Tooltip("Override bounds for free space.")]
    private float overrideBounds = -1;

    [SerializeField, Tooltip("Layer mask for bounding box checks.")]
    private LayerMask layerMask = -1;

    [SerializeField, Tooltip("Clearance distance for valid spawn positions.")]
    private float surfaceClearanceDistance = 0.1f;

    /// <summary>
    /// Finds the nearest valid position to spawn the object within the room.
    /// </summary>
    /// <param name="targetPosition">The target position for spawning.</param>
    /// <param name="targetObject">The target object to align the rotation towards.</param>
    /// <returns>The nearest valid position if found; otherwise, null.</returns>
    public Vector3? FindNearestValidPosition(Vector3 targetPosition, GameObject targetObject)
    {
        if (MRUK.Instance == null) return null;

        MRUKRoom room = MRUK.Instance.GetCurrentRoom();
        if (room == null) return null;

        var prefabBounds = Utilities.GetPrefabBounds(spawnObject);
        float minRadius = 0.0f;
        const float clearanceDistance = 0.01f;
        float baseOffset = -prefabBounds?.min.y ?? 0.0f;
        float centerOffset = prefabBounds?.center.y ?? 0.0f;
        Bounds adjustedBounds = new();

        if (prefabBounds.HasValue)
        {
            minRadius = Mathf.Min(-prefabBounds.Value.min.x, -prefabBounds.Value.min.z, prefabBounds.Value.max.x, prefabBounds.Value.max.z);
            minRadius = Mathf.Max(minRadius, 0f);

            var min = prefabBounds.Value.min;
            var max = prefabBounds.Value.max;
            min.y += clearanceDistance;
            max.y = Mathf.Max(max.y, min.y);

            adjustedBounds.SetMinMax(min, max);

            if (overrideBounds > 0)
            {
                Vector3 center = new Vector3(0f, clearanceDistance, 0f);
                Vector3 size = new Vector3(overrideBounds * 2f, clearanceDistance * 2f, overrideBounds * 2f); // OverrideBounds represents the extents, not the size
                adjustedBounds = new Bounds(center, size);
            }
        }

        Vector3? nearestValidPosition = null;
        Quaternion? nearestValidRotation = null;
        float nearestDistance = float.MaxValue;

        for (int j = 0; j < maxIterations; ++j)
        {
            Vector3 spawnPosition = Vector3.zero;
            Vector3 spawnNormal = Vector3.zero;

            if (spawnLocation == SpawnLocation.Floating)
            {
                var randomPos = room.GenerateRandomPositionInRoom(minRadius, true);
                if (!randomPos.HasValue) break;

                spawnPosition = randomPos.Value;
            }
            else
            {
                MRUK.SurfaceType surfaceType = 0;

                switch (spawnLocation)
                {
                    case SpawnLocation.AnySurface:
                        surfaceType |= MRUK.SurfaceType.FACING_UP;
                        surfaceType |= MRUK.SurfaceType.VERTICAL;
                        surfaceType |= MRUK.SurfaceType.FACING_DOWN;
                        break;
                    case SpawnLocation.VerticalSurfaces:
                        surfaceType |= MRUK.SurfaceType.VERTICAL;
                        break;
                    case SpawnLocation.OnTopOfSurfaces:
                        surfaceType |= MRUK.SurfaceType.FACING_UP;
                        break;
                    case SpawnLocation.HangingDown:
                        surfaceType |= MRUK.SurfaceType.FACING_DOWN;
                        break;
                }

                if (room.GenerateRandomPositionOnSurface(surfaceType, minRadius, LabelFilter.FromEnum(labels), out var pos, out var normal))
                {
                    spawnPosition = pos + normal * baseOffset;
                    spawnNormal = normal;
                    var center = spawnPosition + normal * centerOffset;

                    if (!room.IsPositionInRoom(center)) continue;
                    if (room.IsPositionInSceneVolume(center)) continue;
                    if (room.Raycast(new Ray(pos, normal), surfaceClearanceDistance, out _)) continue;
                }
            }

            Vector3 directionToTarget = (targetObject.transform.position - spawnPosition).normalized;
            Quaternion spawnRotation = Quaternion.LookRotation(directionToTarget, spawnNormal);

            if (checkOverlaps && prefabBounds.HasValue)
            {
                if (Physics.CheckBox(spawnPosition + spawnRotation * adjustedBounds.center, adjustedBounds.extents, spawnRotation, layerMask, QueryTriggerInteraction.Ignore))
                {
                    continue;
                }
            }

            float distance = Vector3.Distance(targetPosition, spawnPosition);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestValidPosition = spawnPosition;
                nearestValidRotation = spawnRotation;
            }
        }

        if (nearestValidPosition.HasValue)
        {
            spawnObject.transform.position = nearestValidPosition.Value;
            if (nearestValidRotation.HasValue)
            {
                spawnObject.transform.rotation = nearestValidRotation.Value;
            }
        }

        return nearestValidPosition;
    }

    /// <summary>
    /// Sets the spawn object to be used.
    /// </summary>
    /// <param name="obj">The object to be set as the spawn object.</param>
    public void SetSpawnObject(GameObject obj)
    {
        spawnObject = obj;
    }
}
