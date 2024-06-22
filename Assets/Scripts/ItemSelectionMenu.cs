using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles the UI for item selection and interaction in an AR environment.
/// </summary>
public class ItemSelectionMenu : MonoBehaviour
{
    [Tooltip("Prefab for the RawImage to display item textures.")]
    public GameObject rawImagePrefab;

    [Tooltip("Transform where the RawImage items will be instantiated.")]
    public Transform contentTransform;

    [Tooltip("Reference to the camera rig for positioning hover items.")]
    public GameObject cameraRig;

    [Tooltip("Reference to the nearest spawn position component.")]
    public PrefabPositionFinder prefabPositionFinder;

    [Tooltip("Array of textures representing the items.")]
    public Texture[] itemTextures;

    [Tooltip("Array of prefabs corresponding to the items to be placed in the AR world.")]
    public GameObject[] itemPrefabs;

    public RaycastHandler _raycastHandler;

    private InteractableAdder _interactableAdder;

    private bool raycastActive = false;

    private int selectedIndex = -1;

    void Start()
    {
        // Get the InteractableAdder component attached to the GameObject
        _interactableAdder = GetComponent<InteractableAdder>();
        if (_interactableAdder == null)
        {
            Debug.LogError("InteractableAdder component not found on the GameObject.");
            return;
        }

        // Populate the scroll view with items
        PopulateScrollView();

        _raycastHandler.DeactivateRaycast();
    }

    void PopulateScrollView()
    {
        int[] indices = Enumerable.Range(0, itemTextures.Length).ToArray();

        for (int i = 0; i < itemTextures.Length; i++)
        {
            GameObject newItem = Instantiate(rawImagePrefab, contentTransform);
            newItem.GetComponentInChildren<RawImage>().texture = itemTextures[i];
            int index = indices[i];

            _interactableAdder.AddInteractables(newItem, () => OnItemSelect(index), null);
        }

        Debug.Log("Array size: " + indices.Length);
    }

    void OnItemSelect(int index)
    {
        if (raycastActive)
        {
            _raycastHandler.DeactivateRaycast();
            raycastActive = false;
            selectedIndex = -1;
        }
        else
        {
            _raycastHandler.ActivateRaycast();
            raycastActive = true;
            selectedIndex = index;
        }
    }

    public void OnButtonClick()
    {
        if (raycastActive && selectedIndex != -1)
        {
            _raycastHandler.DeactivateRaycast();
            raycastActive = false; // Added this line to set raycastActive to false after deactivating the raycast

            Vector3 targetPoint = _raycastHandler.GetTargetPoint();

            var (validPosition, rotation) = prefabPositionFinder.FindNearestPosition(itemPrefabs[selectedIndex], targetPoint, cameraRig);

            Instantiate(itemPrefabs[selectedIndex], validPosition, rotation);

            selectedIndex = -1; // Reset selectedIndex after instantiation
        }
    }
}
