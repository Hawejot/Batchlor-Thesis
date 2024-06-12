using UnityEngine;
using UnityEngine.UI;
using System;

public class ItemSelectionMenu : MonoBehaviour
{
    public GameObject rawImagePrefab; // Reference to the RawImage prefab
    public Transform contentTransform; // Reference to the Content GameObject
    public GameObject cameraRig; // Reference to the camera rig
    public Texture[] itemTextures; // Array of item textures
    public GameObject[] itemPrefabs; // Array of prefabs to be placed in the AR world


    private InteractableAdder interactableAdder; // Reference to the InteractableAdder script
    private GameObject selectedPrefab; // The currently selected prefab
    private GameObject hoverInstance; // Instance of the hovered prefab
    private GameObject placedInstance; // Instance of the placed prefab

    void Start()
    {
        // Get the InteractableAdder component on the same GameObject
        interactableAdder = GetComponent<InteractableAdder>();

        if (interactableAdder == null)
        {
            Debug.LogError("InteractableAdder component not found on the GameObject.");
            return;
        }

        PopulateScrollView();
    }

    void PopulateScrollView()
    {
        for (int i = 0; i < itemTextures.Length; i++)
        {
            Texture itemTexture = itemTextures[i];
            GameObject newItem = Instantiate(rawImagePrefab, contentTransform);
            newItem.GetComponentInChildren<RawImage>().texture = itemTexture; // Assuming the RawImage prefab has a RawImage component in its children
            int index = i; // Capture the current index for the listener

            // Add interactables to the new item
           interactableAdder.AddInteractables(newItem, () => OnItemSelect(index), () => OnItemHover(index));
        }
    }

    void OnItemHover(int index)
    {
        if (hoverInstance != null)
        {
            Destroy(hoverInstance);
        }

        selectedPrefab = itemPrefabs[index];
        hoverInstance = Instantiate(selectedPrefab);
        hoverInstance.transform.position = cameraRig.transform.position + cameraRig.transform.forward * 2f;
        SetHoverMode(hoverInstance, true);
        Debug.Log("Hovering Item: " + itemTextures[index].name);

        // Additional debug message
        Debug.Log("Hovering over item with index: " + index);
    }

    void OnItemSelect(int index)
    {       
        if (hoverInstance != null)
        {
            placedInstance = hoverInstance;
            hoverInstance = null;
            SetHoverMode(placedInstance, false);
            // Add interactables to the placed instance
            interactableAdder.AddInteractables(placedInstance, () => Debug.Log("Object Selected"), () => Debug.Log("Object Hovered"));
            Debug.Log("Placed Item: " + itemTextures[index].name);

            // Additional debug message
            Debug.Log("Selected item with index: " + index);
        }
    }

    void SetHoverMode(GameObject obj, bool isHovering)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            Color color = renderer.material.color;
            color.a = isHovering ? 0.5f : 1f; // Semi-transparent when hovering
            renderer.material.color = color;
        }
    }
}
