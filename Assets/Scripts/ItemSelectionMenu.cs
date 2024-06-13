// ItemSelectionMenu.cs
// This script handles the UI for item selection and interaction in an AR environment.
// It populates a scroll view with items, manages hover effects, and handles item placement.
// Dependencies: UnityEngine, UnityEngine.UI, System.Collections

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Linq;

/// <summary>
/// Handles the UI for item selection and interaction in an AR environment.
/// </summary>
public class ItemSelectionMenu : MonoBehaviour
{
    /// <summary>
    /// Prefab for the RawImage to display item textures.
    /// </summary>
    public GameObject RawImagePrefab;

    /// <summary>
    /// Transform where the RawImage items will be instantiated.
    /// </summary>
    public Transform ContentTransform;

    /// <summary>
    /// Reference to the camera rig for positioning hover items.
    /// </summary>
    public GameObject CameraRig;

    /// <summary>
    /// Array of textures representing the items.
    /// </summary>
    public Texture[] ItemTextures;

    /// <summary>
    /// Array of prefabs corresponding to the items to be placed in the AR world.
    /// </summary>
    public GameObject[] ItemPrefabs;

    /// <summary>
    /// Reference to the InteractableAdder component.
    /// </summary>
    private InteractableAdder _interactableAdder;

    /// <summary>
    /// Currently selected prefab.
    /// </summary>
    private GameObject _selectedPrefab;

    /// <summary>
    /// Instance of the currently hovered prefab.
    /// </summary>
    private GameObject _hoverInstance;

    /// <summary>
    /// Coroutine for managing hover movement.
    /// </summary>
    private Coroutine _hoverCoroutine;

    /// <summary>
    /// Timestamp of the last hover event.
    /// </summary>
    private float _lastHoverTime;

    /// <summary>
    /// Cooldown period between hover events.
    /// </summary>
    private const float HoverCooldown = 1f;

    /// <summary>
    /// Initializes the InteractableAdder component and populates the scroll view with items.
    /// </summary>
    void Start()
    {
        // Get the InteractableAdder component attached to the same GameObject
        _interactableAdder = GetComponent<InteractableAdder>();
        if (_interactableAdder == null)
        {
            Debug.LogError("InteractableAdder component not found on the GameObject.");
            return;
        }

        // Populate the scroll view with items
        PopulateScrollView();
    }

    /// <summary>
    /// Populates the scroll view with RawImage items and sets up interaction callbacks.
    /// </summary>
    void PopulateScrollView()
    {
        // Create an array of indices
        int[] arr = Enumerable.Range(0, ItemTextures.Length).ToArray();

        for (int i = 0; i < ItemTextures.Length; i++)
        {
            // Instantiate a new RawImage item and set its texture
            GameObject newItem = Instantiate(RawImagePrefab, ContentTransform);
            newItem.GetComponentInChildren<RawImage>().texture = ItemTextures[i];

            int index = arr[i]; // Capture the index from the array

            // Add interactables to the new item
            _interactableAdder.AddInteractables(newItem, () => OnItemSelect(index), () => OnItemHover(index));
        }

        // Test: Log the size of the array
        Debug.Log("Array size: " + arr.Length);
    }

    /// <summary>
    /// Handles the hover event for an item.
    /// </summary>
    /// <param name="index">Index of the hovered item.</param>
    void OnItemHover(int index)
    {
        // Check if the cooldown period has elapsed
        if (Time.time - _lastHoverTime < HoverCooldown)
        {
            Debug.Log("Hover event ignored due to cooldown.");
            return;
        }

        // Update the last hover time
        _lastHoverTime = Time.time;

        // Destroy any existing hover instance
        DestroyHoverInstance();

        // Instantiate a new hover instance
        _selectedPrefab = ItemPrefabs[index];
        _hoverInstance = Instantiate(_selectedPrefab);
        _hoverInstance.transform.position = CameraRig.transform.position + CameraRig.transform.forward * 2f;
        SetHoverMode(_hoverInstance, true);
        Debug.Log("Hovering Item: " + ItemTextures[index].name);

        // Start the hover movement coroutine
        _hoverCoroutine = StartCoroutine(HoverMovementCoroutine());
    }

    /// <summary>
    /// Destroys the current hover instance and stops the hover coroutine.
    /// </summary>
    private void DestroyHoverInstance()
    {
        if (_hoverCoroutine != null)
        {
            StopCoroutine(_hoverCoroutine);
            _hoverCoroutine = null;
        }
        if (_hoverInstance != null)
        {
            Destroy(_hoverInstance);
            _hoverInstance = null;
        }
    }

    /// <summary>
    /// Coroutine to manage the movement of the hover instance.
    /// </summary>
    /// <returns>An IEnumerator for the coroutine.</returns>
    private IEnumerator HoverMovementCoroutine()
    {
        while (_hoverInstance != null)
        {
            // Update the hover instance position to follow the camera
            _hoverInstance.transform.position = CameraRig.transform.position + CameraRig.transform.forward * 2f;
            yield return null;
        }
    }

    /// <summary>
    /// Handles the selection event for an item.
    /// </summary>
    /// <param name="index">Index of the selected item.</param>
    void OnItemSelect(int index)
    {
        if (_hoverInstance != null)
        {
            // Place the hover instance in the AR world
            GameObject placedInstance = _hoverInstance;
            _hoverInstance = null;

            // Set the placed instance to its final state
            SetHoverMode(placedInstance, false);

            // Add interactables to the placed instance
            _interactableAdder.AddInteractables(placedInstance, () => Debug.Log("Object Selected"));
            Debug.Log("Placed Item: " + ItemTextures[index].name);

            // Destroy any remaining hover instance
            DestroyHoverInstance();
        }
    }

    /// <summary>
    /// Sets the hover mode for an object, making it semi-transparent if hovering.
    /// </summary>
    /// <param name="obj">The object to set hover mode on.</param>
    /// <param name="isHovering">Whether the object is in hover mode.</param>
    void SetHoverMode(GameObject obj, bool isHovering)
    {
        // Set the transparency of the object's renderers
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            Color color = renderer.material.color;
            color.a = isHovering ? 0.5f : 1f;
            renderer.material.color = color;
        }
    }
}
