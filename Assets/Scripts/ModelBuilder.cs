using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Oculus.Interaction;
using Oculus.Interaction.Surfaces;
using System;

public class ModelBuilder : MonoBehaviour
{

    private float pixelToUnit = 0.04f;
    private GameObject annParent;

    public GameObject Conv2DPrefab;
    public GameObject MaxPooling2DPrefab;
    public GameObject DensePrefab;
    public GameObject FlattenPrefab;
    public GameObject DropoutPrefab;
    public GameObject InputPrefab;
    public GameObject ReshapePrefab;
    public GameObject UpSampling2DPrefab;
    public GameObject ConcatenatePrefab;

    public Material hoverMaterial, hoverParticleMaterial;
    public GameObject uiWindow;
    public TextMeshProUGUI typeText, indexText, outputShapeText, activationText;

    public Dictionary<string, GameObject> classToPrefab = new Dictionary<string, GameObject>();

    public GameObject surfacePrefab; // Assign the Surface prefab in the Inspector

    void Awake()
    {
        classToPrefab["Conv2D"] = Conv2DPrefab;
        classToPrefab["MaxPooling2D"] = MaxPooling2DPrefab;
        classToPrefab["Dense"] = DensePrefab;
        classToPrefab["Flatten"] = FlattenPrefab;
        classToPrefab["Dropout"] = DropoutPrefab;
        classToPrefab["InputLayer"] = InputPrefab;
        classToPrefab["Reshape"] = ReshapePrefab;
        classToPrefab["UpSampling2D"] = UpSampling2DPrefab;
        classToPrefab["Concatenate"] = ConcatenatePrefab;
    }

    public void InstantiateLayers(ApiDataFetcher.LayerInfo[] layers)
    {
        float zPosition = 0f;
        float spaceBetweenLayers = 1f;
        float annDepth = 0f;

        if (annParent != null)
        {
            Destroy(annParent);
        }

        annParent = new GameObject("ANNModel");
        List<GameObject> instantiatedLayers = new List<GameObject>();

        for (int i = 0; i < layers.Length; i++)
        {
            ApiDataFetcher.LayerInfo layer = layers[i];
            GameObject layerParent = CreateLayerParent(layer, zPosition);
            instantiatedLayers.Add(layerParent);
            bool isLastLayer = (i == layers.Length - 1);
            InstantiateLayerComponents(layer, layerParent, isLastLayer);
            AddColliderToLayer(layerParent);
            UpdateLayerDepth(ref annDepth, layerParent, spaceBetweenLayers, ref zPosition);

            // Add interaction components using InteractableAdder
            InteractableAdder interactableAdder = gameObject.AddComponent<InteractableAdder>();
            interactableAdder.surfacePrefab = surfacePrefab;
            interactableAdder.AddInteractables(layerParent, () => OnLayerSelected(layerParent));

            // Configure LayerInteraction
            LayerInteraction interactionScript = layerParent.GetComponent<LayerInteraction>();
            if (interactionScript == null)
            {
                interactionScript = layerParent.AddComponent<LayerInteraction>();
            }
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
        PositionLayers(instantiatedLayers, annParent, ref zPosition, spaceBetweenLayers, ref annDepth);
    }

    GameObject CreateLayerParent(ApiDataFetcher.LayerInfo layer, float zPosition)
    {
        GameObject layerParent = new GameObject(layer.class_name + "Layer");
        layerParent.transform.localPosition = new Vector3(-5, 1f, zPosition);
        return layerParent;
    }

    void InstantiateLayerComponents(ApiDataFetcher.LayerInfo layer, GameObject layerParent, bool isLastLayer)
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

    bool IsDenseDropoutFlattenLayer(ApiDataFetcher.LayerInfo layer)
    {
        return layer.class_name == "Dense" || layer.class_name == "Dropout" || layer.class_name == "Flatten";
    }

    bool IsConvOrPoolingLayer(ApiDataFetcher.LayerInfo layer)
    {
        return layer.class_name == "Conv2D" || layer.class_name == "MaxPooling2D" || layer.class_name == "Concatenate" || layer.class_name == "UpSampling2D" || layer.class_name == "Reshape" || layer.class_name == "InputLayer";
    }

    void InstantiateNeurons(ApiDataFetcher.LayerInfo layer, GameObject prefab, GameObject layerParent, bool isLastLayer)
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

    void InstantiateFeatureMaps(ApiDataFetcher.LayerInfo layer, GameObject prefab, GameObject layerParent)
    {
        if (layer.output_shape != null)
        {
            int pixel = layer.output_shape[1];
            int featureMaps = layer.output_shape[3];
            int dimension = Mathf.CeilToInt(Mathf.Sqrt(featureMaps));
            float spacing = pixel * 0.01f;
            float boxWidth = pixel * pixelToUnit;
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
            Debug.Log("invalid outputshape");
        }
    }

    void AddColliderToLayer(GameObject layerObject)
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

    Vector3 CalculateBoundsSize(GameObject layerObject)
    {
        Bounds bounds = new Bounds(layerObject.transform.position, Vector3.zero);
        foreach (Renderer renderer in layerObject.GetComponentsInChildren<Renderer>())
        {
            bounds.Encapsulate(renderer.bounds);
        }
        return bounds.size;
    }

    Vector3 CalculateBoundsCenter(GameObject layerObject)
    {
        Bounds bounds = new Bounds(layerObject.transform.position, Vector3.zero);
        foreach (Renderer renderer in layerObject.GetComponentsInChildren<Renderer>())
        {
            bounds.Encapsulate(renderer.bounds);
        }
        return bounds.center - layerObject.transform.position;
    }

    void ApplySelectiveScaling(ApiDataFetcher.LayerInfo[] layers, List<GameObject> instantiatedLayers)
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

    float CalculateLayerSize(GameObject layerParent)
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

    float SigmoidScale(float x)
    {
        float a = 2.0f;
        float scaleLimit = 1f;
        float normalizedX = x / 9.0f;
        float minValue = 0.1f;
        float scaleValue = scaleLimit - (scaleLimit / (1.0f + Mathf.Exp(-a * (normalizedX - 0.5f))));
        return scaleValue > minValue ? scaleValue : minValue;
    }

    void UpdateLayerDepth(ref float annDepth, GameObject layerParent, float spaceBetweenLayers, ref float zPosition)
    {
        annDepth += layerParent.transform.localScale.z + spaceBetweenLayers;
        zPosition -= layerParent.transform.localScale.z + spaceBetweenLayers;
    }

    void PositionLayers(List<GameObject> instantiatedLayers, GameObject annParent, ref float zPosition, float spaceBetweenLayers, ref float annDepth)
    {
        float maxDepth = 27f;
        float layerScaleFactor = CalculateScaleFactor(annDepth, maxDepth);

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

    float CalculateScaleFactor(float annDepth, float maxDepth)
    {
        return annDepth > maxDepth ? maxDepth / annDepth : 1f;
    }

    void OnLayerSelected(GameObject layerParent)
    {
        Debug.Log("Layer selected: " + layerParent.name);
        LayerInteraction interactionScript = layerParent.GetComponent<LayerInteraction>();
        if (interactionScript != null)
        {
            interactionScript.OpenUIWindow();
        }
    }
}
