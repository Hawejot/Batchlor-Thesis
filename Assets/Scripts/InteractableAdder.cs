using UnityEngine;
using UnityEngine.Events;
using Oculus.Interaction;
using Oculus.Interaction.Surfaces;
using System;
using System.Collections;

public class InteractableAdder : MonoBehaviour
{
    public GameObject surfacePrefab; // Assign the Surface prefab in the Inspector

    private Action onSelectAction;

    // Function to add both interactables to a GameObject
    public void AddInteractables(GameObject targetGameObject, Action onSelect)
    {
        onSelectAction = onSelect;

        PokeInteractable pokeInteractable = AddPokeInteractable(targetGameObject);
        RayInteractable rayInteractable = AddRayInteractable(targetGameObject);

        // Add the Surface prefab as a child to the target GameObject
        if (surfacePrefab != null)
        {
            GameObject surfaceInstance = Instantiate(surfacePrefab, targetGameObject.transform);
            surfaceInstance.name = "Surface";

            // Adjust the BoundsClipper size to match the target GameObject's size
            AdjustBoundsClipperSize(surfaceInstance, targetGameObject);

            // Inject the Surface into the interactables
            ISurfacePatch surfacePatch = surfaceInstance.GetComponent<ISurfacePatch>();
            if (surfacePatch != null)
            {
                InjectSurfacePatch(targetGameObject, surfacePatch);
            }
            else
            {
                Debug.LogError("Surface instance does not have a component that implements ISurfacePatch.");
            }
        }
        else
        {
            Debug.LogError("Surface prefab is not assigned in the InteractableAdder script.");
        }

        // Add the UnityEvent wrappers
        AddInteractableUnityEventWrapper(targetGameObject, pokeInteractable);
        AddInteractableUnityEventWrapper(targetGameObject, rayInteractable);
    }

    #region AddInteractables

    private PokeInteractable AddPokeInteractable(GameObject targetGameObject)
    {
        PokeInteractable pokeInteractable = targetGameObject.GetComponent<PokeInteractable>();
        if (pokeInteractable == null)
        {
            pokeInteractable = targetGameObject.AddComponent<PokeInteractable>();
        }
        else
        {
            Debug.LogWarning("The GameObject already has a PokeInteractable component.");
        }

        return pokeInteractable;
    }

    private RayInteractable AddRayInteractable(GameObject targetGameObject)
    {
        RayInteractable rayInteractable = targetGameObject.GetComponent<RayInteractable>();
        if (rayInteractable == null)
        {
            rayInteractable = targetGameObject.AddComponent<RayInteractable>();
        }
        else
        {
            Debug.LogWarning("The GameObject already has a RayInteractable component.");
        }

        return rayInteractable;
    }

    #endregion

    private void AddInteractableUnityEventWrapper(GameObject targetGameObject, IInteractableView interactableView)
    {
        if (targetGameObject == null)
        {
            Debug.LogError("targetGameObject is null.");
            return;
        }

        var eventWrapper = targetGameObject.AddComponent<InteractableUnityEventWrapper>();
        if (eventWrapper == null)
        {
            Debug.LogError("Failed to add InteractableUnityEventWrapper component.");
            return;
        }
        Debug.Log("Added InteractableUnityEventWrapper component.");

        // Ensure UnityEvents are initialized
        InitializeUnityEvents(eventWrapper);

        eventWrapper.InjectAllInteractableUnityEventWrapper(interactableView);
        Debug.Log("Injected InteractableView.");

        UnityEvent whenSelect = eventWrapper.WhenSelect;
        if (whenSelect == null)
        {
            Debug.LogError("whenSelect is null.");
            return;
        }
        Debug.Log("Retrieved WhenSelect event.");

        whenSelect.AddListener(OnInteractableSelected);
        Debug.Log("Added event listener for WhenSelect event.");
    }

    void InitializeUnityEvents(InteractableUnityEventWrapper eventWrapper)
    {
        if (eventWrapper.WhenHover == null)
        {
            eventWrapper.GetType().GetField("_whenHover", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(eventWrapper, new UnityEvent());
        }
        if (eventWrapper.WhenUnhover == null)
        {
            eventWrapper.GetType().GetField("_whenUnhover", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(eventWrapper, new UnityEvent());
        }
        if (eventWrapper.WhenSelect == null)
        {
            eventWrapper.GetType().GetField("_whenSelect", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(eventWrapper, new UnityEvent());
        }
        if (eventWrapper.WhenUnselect == null)
        {
            eventWrapper.GetType().GetField("_whenUnselect", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(eventWrapper, new UnityEvent());
        }
        if (eventWrapper.WhenInteractorViewAdded == null)
        {
            eventWrapper.GetType().GetField("_whenInteractorViewAdded", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(eventWrapper, new UnityEvent());
        }
        if (eventWrapper.WhenInteractorViewRemoved == null)
        {
            eventWrapper.GetType().GetField("_whenInteractorViewRemoved", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(eventWrapper, new UnityEvent());
        }
        if (eventWrapper.WhenSelectingInteractorViewAdded == null)
        {
            eventWrapper.GetType().GetField("_whenSelectingInteractorViewAdded", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(eventWrapper, new UnityEvent());
        }
        if (eventWrapper.WhenSelectingInteractorViewRemoved == null)
        {
            eventWrapper.GetType().GetField("_whenSelectingInteractorViewRemoved", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(eventWrapper, new UnityEvent());
        }
    }

    private void OnInteractableSelected()
    {
        if (onSelectAction != null)
        {
            onSelectAction();
        }
        else
        {
            Debug.Log("Interactable selected, but no action was provided.");
        }
    }

    #region Surface

    private void InjectSurfacePatch(GameObject targetGameObject, ISurfacePatch surfacePatch)
    {
        PokeInteractable pokeInteractable = targetGameObject.GetComponent<PokeInteractable>();
        if (pokeInteractable != null)
        {
            pokeInteractable.InjectSurfacePatch(surfacePatch);
        }

        RayInteractable rayInteractable = targetGameObject.GetComponent<RayInteractable>();
        if (rayInteractable != null)
        {
            rayInteractable.InjectSurface(surfacePatch as ISurface);
        }
    }

    private void AdjustBoundsClipperSize(GameObject surfaceInstance, GameObject targetGameObject)
    {
        Renderer targetRenderer = targetGameObject.GetComponent<Renderer>();
        if (targetRenderer != null)
        {
            Vector3 targetSize = targetRenderer.bounds.size;

            BoundsClipper boundsClipper = surfaceInstance.GetComponent<BoundsClipper>();
            if (boundsClipper != null)
            {
                boundsClipper.Size = new Vector3(targetSize.x, targetSize.y, targetSize.z);
            }
            else
            {
                Debug.LogWarning("Surface instance does not have a BoundsClipper component.");
            }
        }
        else
        {
            Debug.LogWarning("Target GameObject does not have a Renderer component.");
        }
    }

    #endregion
}
