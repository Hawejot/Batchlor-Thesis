using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Oculus.Interaction;
using Oculus.Interaction.Surfaces;
using System;

/// <summary>
/// Class responsible for building and attaching a neural network model to a given GameObject.
/// </summary>
public class GameObjectModelBuilder : MonoBehaviour
{
    // Conversion factor from pixels to Unity units
    private const float PixelToUnit = 0.04f;

    // The parent GameObject that will hold the entire model
    private GameObject _annParent;

    // The GameObject to attach the model to
    public GameObject targetGameObject;

    // Prefabs for different layer types
    public GameObject conv2DPrefab;
    public GameObject maxPooling2DPrefab;
    public GameObject densePrefab;
    public GameObject flattenPrefab;
    public GameObject dropoutPrefab;
    public GameObject inputPrefab;
    public GameObject reshapePrefab;
    public GameObject upSampling2DPrefab;
    public GameObject concatenatePrefab;

    // Materials and UI elements for interaction
    public Material hoverMaterial;
    public Material hoverParticleMaterial;
    public GameObject uiWindow;
    public TextMeshProUGUI typeText;
    public TextMeshProUGUI indexText;
    public TextMeshProUGUI outputShapeText;
    public TextMeshProUGUI activationText;

    // Dictionary mapping layer class names to prefabs
    public Dictionary<string, GameObject> classToPrefab = new Dictionary<string, GameObject>();

    // Prefab for interactive surface
    public GameObject surfacePrefab;

    /// <summary>
    /// Initializes the class to prefab dictionary.
    /// </summary>
    private void Awake()
    {
        classToPrefab["Conv2D"] = conv2DPrefab;
        classToPrefab["MaxPooling2D"] = maxPooling2DPrefab;
        classToPrefab["Dense"] = densePrefab;
        classToPrefab["Flatten"] = flattenPrefab;
        classToPrefab["Dropout"] = dropoutPrefab;
        classToPrefab["InputLayer"] = inputPrefab;
        classToPrefab["Reshape"] = reshapePrefab;
        classToPrefab["UpSampling2D"] = upSampling2DPrefab;
        classToPrefab["Concatenate"] = concatenatePrefab;
    }

    /// <summary>
    /// Instantiates the layers of the model and attaches them to the target GameObject.
    /// </summary>
    /// <param name="layers">Array of LayerInfo representing the model's layers.</param>
    public void InstantiateLayers(ApiDataFetcher.LayerInfo[] layers)
    {
        if (targetGameObject == null)
        {
            Debug.LogError("Target GameObject is not assigned.");
            return;
        }

        float zPosition = 0f;
        const float SpaceBetweenLayers = 1f;
        float annDepth = 0f;

        // Destroy previous model if it exists
        if (_annParent != null)
        {
            Destroy(_annParent);
        }

        // Create a new parent for the model and attach it to the target GameObject
        _annParent = new GameObject("ANNModel");
        _annParent.transform.SetParent(targetGameObject.transform);
        _annParent.transform.localPosition = Vector3.zero;
        List<GameObject> instantiatedLayers = new List<GameObject>();

        for (int i = 0; i < layers.Length; i++)
        {
            ApiDataFetcher.LayerInfo layer = layers[i];
            GameObject layerParent = CreateLayerParent(layer, zPosition);
            instantiatedLayers.Add(layerParent);
            bool isLastLayer = (i == layers.Length - 1);
            InstantiateLayerComponents(layer, layerParent, isLastLayer);
            AddColliderToLayer(layerParent);
            UpdateLayerDepth(ref annDepth, layerParent, SpaceBetweenLayers, ref zPosition);

            // Add interaction components using InteractableAdder
            InteractableAdder interactableAdder = gameObject.AddComponent<InteractableAdder>();
            interactableAdder.surfacePrefab = surfacePrefab;
            interactableAdder.AddInteractables(layerParent, () => OnLayerSelected(layerParent));

            // Configure LayerInteraction
            LayerInteraction interactionScript = layerParent.GetComponent<LayerInteraction>() ?? layerParent.AddComponent<LayerInteraction>();
            interactionScript.hoverMaterial = hoverMaterial;
            interactionScript.hoverParticleMaterial = hoverParticleMaterial;
            interactionScript.typeText = typeText;
            interactionScript.indexText = indexText;
            interactionScript.outputShapeText = outputShapeText;
            interactionScript.activationText = activationText;
            interactionScript.uiWindow = uiWindow;
            interactionScript.layerInfo = layer;
            interactionScript.isNeuronLayer = IsDenseDropoutFlattenLayer(layer);
            interactionScript.Initialize();
        }

        ApplySelectiveScaling(layers, instantiatedLayers);
        PositionLayers(instantiatedLayers, _annParent, ref zPosition, SpaceBetweenLayers, ref annDepth);
        AdjustBoxColliderToBounds(targetGameObject);
    }

    /// <summary>
    /// Creates a parent GameObject for a layer.
    /// </summary>
    /// <param name="layer">The layer information.</param>
    /// <param name="zPosition">The Z position for the layer.</param>
    /// <returns>The created parent GameObject for the layer.</returns>
    private GameObject CreateLayerParent(ApiDataFetcher.LayerInfo layer, float zPosition)
    {
        GameObject layerParent = new GameObject(layer.class_name + "Layer");
        layerParent.transform.localPosition = new Vector3(-5, 1f, zPosition);
        return layerParent;
    }

    /// <summary>
    /// Instantiates the components of a layer.
    /// </summary>
    /// <param name="layer">The layer information.</param>
    /// <param name="layerParent">The parent GameObject for the layer.</param>
    /// <param name="isLastLayer">Whether this is the last layer in the model.</param>
    private void InstantiateLayerComponents(ApiDataFetcher.LayerInfo layer, GameObject layerParent, bool isLastLayer)
    {
        if (classToPrefab.TryGetValue(layer.class_name, out GameObject prefab))
        {
            if (IsDenseDropoutFlattenLayer(layer))
            {
                InstantiateNeurons(layer, prefab, layerParent, isLastLayer);
            }
            else if (IsConvOrPoolingLayer(layer))
            {
                InstantiateFeatureMaps(layer, prefab, layerParent);
            }
        }
        else
        {
            Debug.LogError($"Prefab for class {layer.class_name} not found.");
        }
    }

    /// <summary>
    /// Checks if a layer is a Dense, Dropout, or Flatten layer.
    /// </summary>
    /// <param name="layer">The layer information.</param>
    /// <returns>True if the layer is a Dense, Dropout, or Flatten layer, otherwise false.</returns>
    private bool IsDenseDropoutFlattenLayer(ApiDataFetcher.LayerInfo layer)
    {
        return layer.class_name == "Dense" || layer.class_name == "Dropout" || layer.class_name == "Flatten";
    }

    /// <summary>
    /// Checks if a layer is a Convolutional or Pooling layer.
    /// </summary>
    /// <param name="layer">The layer information.</param>
    /// <returns>True if the layer is a Convolutional or Pooling layer, otherwise false.</returns>
    private bool IsConvOrPoolingLayer(ApiDataFetcher.LayerInfo layer)
    {
        return layer.class_name == "Conv2D" || layer.class_name == "MaxPooling2D" || layer.class_name == "Concatenate" || layer.class_name == "UpSampling2D" || layer.class_name == "Reshape" || layer.class_name == "InputLayer";
    }

    /// <summary>
    /// Instantiates neurons for a Dense, Dropout, or Flatten layer.
    /// </summary>
    /// <param name="layer">The layer information.</param>
    /// <param name="prefab">The prefab for the layer.</param>
    /// <param name="layerParent">The parent GameObject for the layer.</param>
    /// <param name="isLastLayer">Whether this is the last layer in the model.</param>
    private void InstantiateNeurons(ApiDataFetcher.LayerInfo layer, GameObject prefab, GameObject layerParent, bool isLastLayer)
    {
        int numberOfNeurons = layer.output_shape[1];
        GameObject neuronSystem = Instantiate(prefab, parent: layerParent.transform);
        neuronSystem.transform.localPosition = new Vector3(0, 3.5f, 0);

        ParticleSystem[] particleSystemArray = neuronSystem.GetComponentsInChildren<ParticleSystem>();

        if (particleSystemArray != null && particleSystemArray.Length > 0)
        {
            ParticleSystem particleSystem = particleSystemArray[0];

            var mainModule = particleSystem.main;
            var emissionModule = particleSystem.emission;
            var shapeModule = particleSystem.shape;
            var particleRenderer = particleSystem.GetComponent<ParticleSystemRenderer>();

            mainModule.maxParticles = numberOfNeurons;
            mainModule.startLifetime = Mathf.Infinity;
            mainModule.startSpeed = 0f;
            emissionModule.rateOverTime = 0f;

            if (isLastLayer)
            {
                neuronSystem.transform.localPosition = new Vector3(0, 0, 0);
                neuronSystem.transform.rotation = Quaternion.Euler(new Vector3(90, 90, 0));
                particleRenderer.maxParticleSize = 0.07f;
            }

            ParticleSystem.Burst burst = new ParticleSystem.Burst(0.0f, numberOfNeurons);
            emissionModule.SetBursts(new ParticleSystem.Burst[] { burst });
        }
    }

    /// <summary>
    /// Instantiates feature maps for a Convolutional or Pooling layer.
    /// </summary>
    /// <param name="layer">The layer information.</param>
    /// <param name="prefab">The prefab for the layer.</param>
    /// <param name="layerParent">The parent GameObject for the layer.</param>
    private void InstantiateFeatureMaps(ApiDataFetcher.LayerInfo layer, GameObject prefab, GameObject layerParent)
    {
        if (layer.output_shape != null)
        {
            int pixel = layer.output_shape[1];
            int featureMaps = layer.output_shape[3];
            int dimension = Mathf.CeilToInt(Mathf.Sqrt(featureMaps));
            float spacing = pixel * 0.01f;
            float boxWidth = pixel * PixelToUnit;
            float totalRowWidth = dimension * boxWidth + (dimension - 1) * spacing;
            float startX = -totalRowWidth / 2 + boxWidth / 2;

            for (int i = 0; i < featureMaps; i++)
            {
                int row = i / dimension;
                int col = i % dimension;
                GameObject featureMapBox = Instantiate(prefab, parent: layerParent.transform);
                featureMapBox.transform.localScale = new Vector3(boxWidth, boxWidth, 0.3f);
                featureMapBox.transform.localPosition = new Vector3(startX + col * (boxWidth + spacing), row * (boxWidth + spacing), 0);
            }
        }
        else
        {
            Debug.Log("Invalid output shape.");
        }
    }

    /// <summary>
    /// Adds a BoxCollider to the layer object.
    /// </summary>
    /// <param name="layerObject">The layer object to add the collider to.</param>
    private void AddColliderToLayer(GameObject layerObject)
    {
        if (layerObject.GetComponent<Renderer>() == null && layerObject.GetComponentsInChildren<Renderer>().Length == 0)
        {
            return;
        }

        BoxCollider collider = layerObject.AddComponent<BoxCollider>();
        collider.size = CalculateBoundsSize(layerObject);
        collider.center = CalculateBoundsCenter(layerObject);
        collider.isTrigger = true;
    }

    /// <summary>
    /// Calculates the bounds size of the layer object.
    /// </summary>
    /// <param name="layerObject">The layer object to calculate the bounds for.</param>
    /// <returns>The size of the bounds.</returns>
    private Vector3 CalculateBoundsSize(GameObject layerObject)
    {
        Bounds bounds = new Bounds(layerObject.transform.position, Vector3.zero);
        foreach (Renderer renderer in layerObject.GetComponentsInChildren<Renderer>())
        {
            bounds.Encapsulate(renderer.bounds);
        }
        return bounds.size;
    }

    /// <summary>
    /// Calculates the bounds center of the layer object.
    /// </summary>
    /// <param name="layerObject">The layer object to calculate the bounds for.</param>
    /// <returns>The center of the bounds.</returns>
    private Vector3 CalculateBoundsCenter(GameObject layerObject)
    {
        Bounds bounds = new Bounds(layerObject.transform.position, Vector3.zero);
        foreach (Renderer renderer in layerObject.GetComponentsInChildren<Renderer>())
        {
            bounds.Encapsulate(renderer.bounds);
        }
        return bounds.center - layerObject.transform.position;
    }

    /// <summary>
    /// Applies selective scaling to Convolutional or Pooling layers.
    /// </summary>
    /// <param name="layers">Array of LayerInfo representing the model's layers.</param>
    /// <param name="instantiatedLayers">List of instantiated layer GameObjects.</param>
    private void ApplySelectiveScaling(ApiDataFetcher.LayerInfo[] layers, List<GameObject> instantiatedLayers)
    {
        for (int i = 0; i < instantiatedLayers.Count; i++)
        {
            if (IsConvOrPoolingLayer(layers[i]))
            {
                float layerSize = CalculateLayerSize(instantiatedLayers[i]);
                float layerScaleFactor = SigmoidScale(layerSize);

                Vector3 currentScale = instantiatedLayers[i].transform.localScale;
                instantiatedLayers[i].transform.localScale = new Vector3(
                    currentScale.x * layerScaleFactor,
                    currentScale.y * layerScaleFactor,
                    currentScale.z * layerScaleFactor);
            }
        }
    }

    /// <summary>
    /// Calculates the size of a layer based on its bounds.
    /// </summary>
    /// <param name="layerParent">The parent GameObject of the layer.</param>
    /// <returns>The size of the layer.</returns>
    private float CalculateLayerSize(GameObject layerParent)
    {
        Renderer[] renderers = layerParent.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
        {
            return 0f;
        }

        Bounds bounds = renderers[0].bounds;
        foreach (Renderer renderer in renderers)
        {
            bounds.Encapsulate(renderer.bounds);
        }
        return bounds.size.x;
    }

    /// <summary>
    /// Calculates a sigmoid-based scale factor for a given size.
    /// </summary>
    /// <param name="x">The size to scale.</param>
    /// <returns>The calculated scale factor.</returns>
    private float SigmoidScale(float x)
    {
        const float A = 2.0f;
        const float ScaleLimit = 1f;
        float normalizedX = x / 9.0f;
        const float MinValue = 0.1f;
        float scaleValue = ScaleLimit - (ScaleLimit / (1.0f + Mathf.Exp(-A * (normalizedX - 0.5f))));
        return scaleValue > MinValue ? scaleValue : MinValue;
    }

    /// <summary>
    /// Updates the depth of the model as layers are added.
    /// </summary>
    /// <param name="annDepth">The current depth of the model.</param>
    /// <param name="layerParent">The parent GameObject of the layer.</param>
    /// <param name="spaceBetweenLayers">The space between layers.</param>
    /// <param name="zPosition">The current Z position for placing layers.</param>
    private void UpdateLayerDepth(ref float annDepth, GameObject layerParent, float spaceBetweenLayers, ref float zPosition)
    {
        annDepth += layerParent.transform.localScale.z + spaceBetweenLayers;
        zPosition -= layerParent.transform.localScale.z + spaceBetweenLayers;
    }

    /// <summary>
    /// Positions the layers within the model.
    /// </summary>
    /// <param name="instantiatedLayers">List of instantiated layer GameObjects.</param>
    /// <param name="annParent">The parent GameObject for the model.</param>
    /// <param name="zPosition">The current Z position for placing layers.</param>
    /// <param name="spaceBetweenLayers">The space between layers.</param>
    /// <param name="annDepth">The current depth of the model.</param>
    private void PositionLayers(List<GameObject> instantiatedLayers, GameObject annParent, ref float zPosition, float spaceBetweenLayers, ref float annDepth)
    {
        const float MaxDepth = 27f;
        float layerScaleFactor = CalculateScaleFactor(annDepth, MaxDepth);

        zPosition = -2f;

        foreach (GameObject layerObject in instantiatedLayers)
        {
            layerObject.transform.SetParent(annParent.transform);
            layerObject.transform.localScale *= layerScaleFactor;
            layerObject.transform.localPosition = new Vector3(0f, 1f, zPosition - (layerObject.transform.localScale.z / 2f * layerScaleFactor));
            zPosition -= (layerObject.transform.localScale.z * layerScaleFactor + spaceBetweenLayers * layerScaleFactor);
        }

        annDepth -= spaceBetweenLayers;
    }

    /// <summary>
    /// Calculates the scale factor for the model based on its depth.
    /// </summary>
    /// <param name="annDepth">The current depth of the model.</param>
    /// <param name="maxDepth">The maximum allowed depth for the model.</param>
    /// <returns>The calculated scale factor.</returns>
    private float CalculateScaleFactor(float annDepth, float maxDepth)
    {
        return annDepth > maxDepth ? maxDepth / annDepth : 1f;
    }

    /// <summary>
    /// Adjusts the BoxCollider of the target GameObject to fit its bounds.
    /// </summary>
    /// <param name="gameObject">The target GameObject.</param>
    private void AdjustBoxColliderToBounds(GameObject gameObject)
    {
        BoxCollider collider = gameObject.GetComponent<BoxCollider>() ?? gameObject.AddComponent<BoxCollider>();
        collider.size = CalculateBoundsSize(gameObject);
        collider.center = CalculateBoundsCenter(gameObject);
    }

    /// <summary>
    /// Handles the event when a layer is selected.
    /// </summary>
    /// <param name="layerParent">The parent GameObject of the selected layer.</param>
    private void OnLayerSelected(GameObject layerParent)
    {
        Debug.Log("Layer selected: " + layerParent.name);
        LayerInteraction interactionScript = layerParent.GetComponent<LayerInteraction>();
        if (interactionScript != null)
        {
            interactionScript.OpenUIWindow();
        }
    }
}
