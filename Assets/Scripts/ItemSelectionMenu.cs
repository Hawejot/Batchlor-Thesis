// ItemSelectionMenu.cs
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles the UI for item selection and interaction in an AR environment.
/// </summary>
public class ItemSelectionMenu : MonoBehaviour
{
    /// <summary>
    /// Prefab for the RawImage to display item textures.
    /// </summary>
    [Tooltip("Prefab for the RawImage to display item textures.")]
    public GameObject rawImagePrefab;

    /// <summary>
    /// Transform where the RawImage items will be instantiated.
    /// </summary>
    [Tooltip("Transform where the RawImage items will be instantiated.")]
    public Transform contentTransform;

    /// <summary>
    /// Reference to the camera rig for positioning hover items.
    /// </summary>
    [Tooltip("Reference to the camera rig for positioning hover items.")]
    public GameObject cameraRig;

    /// <summary>
    /// Reference to the nearest spawn position component.
    /// </summary>
    [Tooltip("Reference to the nearest spawn position component.")]
    public NearestSpawnPosition nearestSpawnPosition;

    /// <summary>
    /// Array of textures representing the items.
    /// </summary>
    [Tooltip("Array of textures representing the items.")]
    public Texture[] itemTextures;

    /// <summary>
    /// Array of prefabs corresponding to the items to be placed in the AR world.
    /// </summary>
    [Tooltip("Array of prefabs corresponding to the items to be placed in the AR world.")]
    public GameObject[] itemPrefabs;

    private InteractableAdder _interactableAdder;
    private ItemHoverHandler _hoverHandler;
    private ItemSelectHandler _selectHandler;

    /// <summary>
    /// Initializes the InteractableAdder component and populates the scroll view with items.
    /// </summary>
    void Start()
    {
        // Get the InteractableAdder component attached to the GameObject
        _interactableAdder = GetComponent<InteractableAdder>();
        if (_interactableAdder == null)
        {
            Debug.LogError("InteractableAdder component not found on the GameObject.");
            return;
        }

        // Initialize hover and select handlers
        _hoverHandler = new ItemHoverHandler(cameraRig, nearestSpawnPosition);
        _selectHandler = new ItemSelectHandler(nearestSpawnPosition);

        // Populate the scroll view with items
        PopulateScrollView();
    }

    /// <summary>
    /// Populates the scroll view with RawImage items and sets up interaction callbacks.
    /// </summary>
    void PopulateScrollView()
    {
        // Create an array of indices based on the length of itemTextures
        int[] indices = Enumerable.Range(0, itemTextures.Length).ToArray();

        // Iterate through each texture and create a UI item for it
        for (int i = 0; i < itemTextures.Length; i++)
        {
            // Instantiate a new RawImage item and set its texture
            GameObject newItem = Instantiate(rawImagePrefab, contentTransform);
            newItem.GetComponentInChildren<RawImage>().texture = itemTextures[i];

            // Store the current index for use in the callback
            int index = indices[i];

            // Add interaction callbacks for hover and selection
            _interactableAdder.AddInteractables(newItem, () => OnItemSelect(index), () => OnItemHover(index));
        }

        Debug.Log("Array size: " + indices.Length);
    }

    /// <summary>
    /// Handles the hover event for an item.
    /// </summary>
    /// <param name="index">Index of the hovered item.</param>
    void OnItemHover(int index)
    {
        _hoverHandler.HandleItemHover(itemPrefabs[index], itemTextures[index].name);
    }

    /// <summary>
    /// Handles the selection event for an item.
    /// </summary>
    /// <param name="index">Index of the selected item.</param>
    void OnItemSelect(int index)
    {
        _hoverHandler.StopHover(); // Stop the hover effect
        _selectHandler.HandleItemSelect(itemPrefabs[index], _hoverHandler.LastHoverPosition, itemTextures[index].name);
    }
}
