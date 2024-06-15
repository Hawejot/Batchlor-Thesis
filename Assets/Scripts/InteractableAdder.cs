using UnityEngine;
using UnityEngine.Events;
using Oculus.Interaction;
using Oculus.Interaction.Surfaces;
using System;

/// <summary>
/// Class responsible for adding interactables to a GameObject.
/// </summary>
public class InteractableAdder : MonoBehaviour
{
    /// <summary>
    /// Prefab for the surface to be assigned in the Inspector.
    /// </summary>
    public GameObject surfacePrefab;

    #region Public Methods

    /// <summary>
    /// Adds interactables to a GameObject with a select action.
    /// </summary>
    /// <param name="targetGameObject">The target GameObject.</param>
    /// <param name="onSelect">Action to be called on select.</param>
    public void AddInteractables(GameObject targetGameObject, Action onSelect)
    {
        AddInteractables(targetGameObject, onSelect, null, null);
    }

    /// <summary>
    /// Adds interactables to a GameObject with select and hover actions.
    /// </summary>
    /// <param name="targetGameObject">The target GameObject.</param>
    /// <param name="onSelect">Action to be called on select.</param>
    /// <param name="onHover">Action to be called on hover.</param>
    public void AddInteractables(GameObject targetGameObject, Action onSelect, Action onHover)
    {
        AddInteractables(targetGameObject, onSelect, onHover, null);
    }

    /// <summary>
    /// Adds interactables to a GameObject with select, hover, and unhover actions.
    /// </summary>
    /// <param name="targetGameObject">The target GameObject.</param>
    /// <param name="onSelectAction">Action to be called on select.</param>
    /// <param name="onHoverAction">Action to be called on hover.</param>
    /// <param name="onUnhoverAction">Action to be called on unhover.</param>
    public void AddInteractables(GameObject targetGameObject, Action onSelectAction, Action onHoverAction, Action onUnhoverAction)
    {
        Debug.Log($"Adding interactables to: {targetGameObject.name}");

        // Create and add poke interactable
        PokeInteractable pokeInteractable = AddPokeInteractable(targetGameObject);

        // Create and add ray interactable
        RayInteractable rayInteractable = AddRayInteractable(targetGameObject);

        // Instantiate and configure the surface prefab
        if (surfacePrefab != null)
        {
            GameObject surfaceInstance = Instantiate(surfacePrefab, targetGameObject.transform);
            surfaceInstance.name = "Surface";

            AdjustBoundsClipperSize(surfaceInstance, targetGameObject);

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

        // Add UnityEvent wrappers to handle interactions
        AddInteractableUnityEventWrapper(targetGameObject, pokeInteractable, onSelectAction, onHoverAction, onUnhoverAction);
        AddInteractableUnityEventWrapper(targetGameObject, rayInteractable, onSelectAction, onHoverAction, onUnhoverAction);
    }

    #endregion

    #region AddInteractables

    /// <summary>
    /// Adds a PokeInteractable to the target GameObject.
    /// </summary>
    /// <param name="targetGameObject">The target GameObject.</param>
    /// <returns>The added PokeInteractable component.</returns>
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

    /// <summary>
    /// Adds a RayInteractable to the target GameObject.
    /// </summary>
    /// <param name="targetGameObject">The target GameObject.</param>
    /// <returns>The added RayInteractable component.</returns>
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

    #region Interaction Logic

    /// <summary>
    /// Adds a UnityEvent wrapper to handle interactable events.
    /// </summary>
    /// <param name="targetGameObject">The target GameObject.</param>
    /// <param name="interactableView">The interactable view.</param>
    /// <param name="onSelect">Action to be called on select.</param>
    /// <param name="onHover">Action to be called on hover.</param>
    /// <param name="onUnhover">Action to be called on unhover.</param>
    private void AddInteractableUnityEventWrapper(GameObject targetGameObject, IInteractableView interactableView, Action onSelect, Action onHover, Action onUnhover)
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

        InitializeUnityEvents(eventWrapper);

        eventWrapper.InjectAllInteractableUnityEventWrapper(interactableView);
        Debug.Log("Injected InteractableView.");

        UnityEvent whenSelect = eventWrapper.WhenSelect;
        if (whenSelect == null)
        {
            Debug.LogError("whenSelect is null.");
            return;
        }
        whenSelect.AddListener(() => OnInteractionSelect(onSelect));

        UnityEvent whenHover = eventWrapper.WhenHover;
        if (whenHover == null)
        {
            Debug.LogError("whenHover is null.");
            return;
        }
        whenHover.AddListener(() => OnInteractionHover(onHover));

        UnityEvent whenUnhover = eventWrapper.WhenUnhover;
        if (whenUnhover == null)
        {
            Debug.LogError("whenUnhover is null.");
            return;
        }
        whenUnhover.AddListener(() => OnInteractionUnhover(onUnhover));
    }

    /// <summary>
    /// Initializes UnityEvents in the event wrapper.
    /// </summary>
    /// <param name="eventWrapper">The event wrapper to initialize.</param>
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

    /// <summary>
    /// Called when the interactable is selected.
    /// </summary>
    /// <param name="onSelection">The action to be called on selection.</param>
    private void OnInteractionSelect(Action onSelection)
    {
        if (onSelection != null)
        {
            onSelection();
        }
        else
        {
            Debug.Log("Interactable selected, but no action was provided.");
        }
    }

    /// <summary>
    /// Called when the interactable is hovered.
    /// </summary>
    /// <param name="onHoverAction">The action to be called on hover.</param>
    private void OnInteractionHover(Action onHoverAction)
    {
        if (onHoverAction != null)
        {
            onHoverAction();
        }
        else
        {
            Debug.Log("Interactable hovered, but no action was provided.");
        }
    }

    /// <summary>
    /// Called when the interactable is unhovered.
    /// </summary>
    /// <param name="onUnhoverAction">The action to be called on unhover.</param>
    private void OnInteractionUnhover(Action onUnhoverAction)
    {
        if (onUnhoverAction != null)
        {
            onUnhoverAction();
        }
        else
        {
            Debug.Log("Interactable unhovered, but no action was provided.");
        }
    }

    #endregion

    #region Surface

    /// <summary>
    /// Injects a surface patch into the target GameObject's interactables.
    /// </summary>
    /// <param name="targetGameObject">The target GameObject.</param>
    /// <param name="surfacePatch">The surface patch to inject.</param>
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

    /// <summary>
    /// Adjusts the size of the BoundsClipper to match the target GameObject's size.
    /// </summary>
    /// <param name="surfaceInstance">The surface instance.</param>
    /// <param name="targetGameObject">The target GameObject.</param>
    private void AdjustBoundsClipperSize(GameObject surfaceInstance, GameObject targetGameObject)
    {
        BoxCollider targetCollider = targetGameObject.GetComponent<BoxCollider>();
        if (targetCollider != null)
        {
            Vector3 targetSize = targetCollider.size;
            Vector3 targetCenter = targetCollider.center;

            BoundsClipper boundsClipper = surfaceInstance.GetComponent<BoundsClipper>();
            if (boundsClipper != null)
            {
                boundsClipper.Size = new Vector3(targetSize.x+0.1f, targetSize.y+0.1f, targetSize.z+0.1f);
                boundsClipper.transform.position = targetGameObject.transform.position + targetCenter;
            }
            else
            {
                Debug.LogWarning("Surface instance does not have a BoundsClipper component.");
            }
        }
        else
        {
            Debug.LogWarning("Target GameObject does not have a BoxCollider component.");
        }
    }

    #endregion
}
