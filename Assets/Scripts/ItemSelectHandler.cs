// ItemSelectHandler.cs
// This script handles the selection functionality for items in an AR environment.

using UnityEngine;

/// <summary>
/// Handles the selection functionality for items in an AR environment.
/// </summary>
public class ItemSelectHandler
{
    private NearestSpawnPosition _nearestSpawnPosition;

    /// <summary>
    /// Initializes a new instance of the <see cref="ItemSelectHandler"/> class.
    /// </summary>
    /// <param name="nearestSpawnPosition">The nearest spawn position component.</param>
    public ItemSelectHandler(NearestSpawnPosition nearestSpawnPosition)
    {
        _nearestSpawnPosition = nearestSpawnPosition;
    }

    /// <summary>
    /// Handles the selection event for an item.
    /// </summary>
    /// <param name="itemPrefab">The prefab of the selected item.</param>
    /// <param name="lastHoverPosition">The last valid hover position, if any.</param>
    /// <param name="itemName">The name of the selected item.</param>
    public void HandleItemSelect(GameObject itemPrefab, Vector3? lastHoverPosition, string itemName)
    {
        _nearestSpawnPosition.SetSpawnObject(itemPrefab);

        // Use the last hover position if it exists, otherwise find a new position
        Vector3? spawnPosition = lastHoverPosition ?? FindAndSetValidPosition(itemPrefab);

        // If a valid position is found, instantiate the spawn object there
        if (spawnPosition.HasValue)
        {
            Object.Instantiate(itemPrefab, spawnPosition.Value, Quaternion.identity);
            Debug.Log("Placed Item: " + itemName);
        }
        else
        {
            Debug.LogWarning("No valid spawn position found for: " + itemName);
        }
    }

    /// <summary>
    /// Finds and sets a valid position for the given object using NearestSpawnPosition.
    /// </summary>
    /// <param name="obj">The object to find a valid position for.</param>
    /// <returns>The valid position if found, otherwise null.</returns>
    private Vector3? FindAndSetValidPosition(GameObject obj)
    {
        _nearestSpawnPosition.SetSpawnObject(obj);

        // Find a valid position
        Vector3 targetPosition = _nearestSpawnPosition.transform.position + _nearestSpawnPosition.transform.forward * 2f;
        Vector3? validPosition = _nearestSpawnPosition.FindNearestValidPosition(targetPosition, _nearestSpawnPosition.gameObject);

        // Return the valid position if found, otherwise null
        return validPosition;
    }
}
