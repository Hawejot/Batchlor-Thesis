// ItemHoverHandler.cs
// This script handles the hover functionality for items in an AR environment.

using System.Collections;
using UnityEngine;

/// <summary>
/// Handles the hover functionality for items in an AR environment.
/// </summary>
public class ItemHoverHandler
{
    private GameObject _cameraRig;
    private NearestSpawnPosition _nearestSpawnPosition;
    private GameObject _hoverInstance;
    private Coroutine _hoverCoroutine;
    private float _lastHoverTime;
    private const float HoverCooldown = 1f;

    /// <summary>
    /// Gets the last valid hover position.
    /// </summary>
    public Vector3? LastHoverPosition { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ItemHoverHandler"/> class.
    /// </summary>
    /// <param name="cameraRig">The camera rig for positioning hover items.</param>
    /// <param name="nearestSpawnPosition">The nearest spawn position component.</param>
    public ItemHoverHandler(GameObject cameraRig, NearestSpawnPosition nearestSpawnPosition)
    {
        _cameraRig = cameraRig;
        _nearestSpawnPosition = nearestSpawnPosition;
    }

    /// <summary>
    /// Handles the hover event for an item.
    /// </summary>
    /// <param name="itemPrefab">The prefab of the item to hover.</param>
    /// <param name="itemName">The name of the item to hover.</param>
    public void HandleItemHover(GameObject itemPrefab, string itemName)
    {
        // Check for hover cooldown to avoid too frequent hover events
        if (Time.time - _lastHoverTime < HoverCooldown)
        {
            Debug.Log("Hover event ignored due to cooldown.");
            return;
        }

        _lastHoverTime = Time.time; // Update the last hover time

        StopHover(); // Ensure any existing hover is stopped

        // Instantiate the hover instance
        _hoverInstance = Object.Instantiate(itemPrefab);
        LastHoverPosition = FindAndSetValidPosition(_hoverInstance);

        // Set the hover mode (e.g., semi-transparent)
        SetHoverMode(_hoverInstance, true);
        Debug.Log("Hovering Item: " + itemName);

        // Start the coroutine to manage hover movement
        _hoverCoroutine = _nearestSpawnPosition.StartCoroutine(HoverMovementCoroutine());
    }

    /// <summary>
    /// Stops the hover effect and destroys the hover instance.
    /// </summary>
    public void StopHover()
    {
        DestroyHoverInstance();
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
        Vector3 targetPosition = _cameraRig.transform.position + _cameraRig.transform.forward * 2f;
        Vector3? validPosition = _nearestSpawnPosition.FindNearestValidPosition(targetPosition, _cameraRig);

        // Set the object's position if a valid position is found
        if (validPosition.HasValue)
        {
            obj.transform.position = validPosition.Value;
        }
        else
        {
            obj.transform.position = targetPosition; // Fallback to default position
        }

        return validPosition;
    }

    /// <summary>
    /// Coroutine to manage the movement of the hover instance.
    /// </summary>
    /// <returns>An IEnumerator for the coroutine.</returns>
    private IEnumerator HoverMovementCoroutine()
    {
        // Continuously update the hover instance position while it exists
        while (_hoverInstance != null)
        {
            // Update hover position only if needed to reduce jittering
            Vector3 targetPosition = _cameraRig.transform.position + _cameraRig.transform.forward * 2f;
            if (Vector3.Distance(_hoverInstance.transform.position, targetPosition) > 0.01f)
            {
                LastHoverPosition = FindAndSetValidPosition(_hoverInstance);
            }

            yield return null; // Wait until the next frame
        }
    }

    /// <summary>
    /// Destroys the current hover instance and stops the hover coroutine.
    /// </summary>
    private void DestroyHoverInstance()
    {
        // Stop the hover coroutine if it's running
        if (_hoverCoroutine != null)
        {
            _nearestSpawnPosition.StopCoroutine(_hoverCoroutine);
            _hoverCoroutine = null;
        }

        // Destroy the hover instance if it exists
        if (_hoverInstance != null)
        {
            Object.Destroy(_hoverInstance);
            _hoverInstance = null;
        }
    }

    /// <summary>
    /// Sets the hover mode for an object, making it semi-transparent if hovering.
    /// </summary>
    /// <param name="obj">The object to set hover mode on.</param>
    /// <param name="isHovering">Whether the object is in hover mode.</param>
    private void SetHoverMode(GameObject obj, bool isHovering)
    {
        // Get all renderers in the object and its children
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();

        // Iterate through each renderer and set the alpha value based on hover state
        foreach (Renderer renderer in renderers)
        {
            Color color = renderer.material.color;
            color.a = isHovering ? 0.5f : 1f; // Set alpha to 0.5 if hovering, otherwise 1
            renderer.material.color = color; // Apply the color change to the material
        }
    }
}
