using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LayerInteraction : MonoBehaviour
{

    public Material hoverMaterial, hoverParticleMaterial; // Shared hover material for all layers
    public GameObject uiWindow; // UI Window to show on trigger
    public TextMeshProUGUI typeText, indexText, outputShapeText, activationText;
    public ApiDataFetcher.LayerInfo layerInfo;
    public GameObject[] childObjects;
    public GameObject testObject;
    public bool isNeuronLayer;

    private Material[] originalChildMaterials; // To store the original materials of the children

    public void Initialize()
    {
        // Get the original material of all children of the layer
        if (childObjects != null)
        {
            originalChildMaterials = new Material[childObjects.Length];
            for (int i = 0; i < childObjects.Length; i++)
            {
                Renderer childRenderer = childObjects[i].GetComponent<Renderer>();
                if (childRenderer != null)
                {
                    originalChildMaterials[i] = childRenderer.material;
                }
            }
        }
    }

    // Set Texts according to Layer Information
    public void SetLayerInfo()
    {
        if (layerInfo != null)
        {
            typeText.text = "Type: " + layerInfo.class_name;
            indexText.text = "Index: " + layerInfo.index;
            outputShapeText.text = "Output Shape: " + ArrayToString(layerInfo.output_shape);
            if (layerInfo.activation != null)
            {
                activationText.text = "Activation Function: " + layerInfo.activation;
            }
        }
        else
        {
            Debug.LogError("Layer info is null");
        }
    }

    // Show UI Window
    public void OpenUIWindow()
    {
        uiWindow.SetActive(true); // Show the UI window
        SetLayerInfo();
    }

    public void CloseUIWindow()
    {
        uiWindow.SetActive(false);
    }

    private string ArrayToString(int[] array)
    {
        return "[" + string.Join(", ", array) + "]";
    }

    // Handle hover enter
    public void OnHoverEnter()
    {
        if (childObjects != null)
        {
            for (int i = 0; i < childObjects.Length; i++)
            {
                Renderer childRenderer = childObjects[i].GetComponent<Renderer>();
                if (childRenderer != null)
                {
                    childRenderer.material = hoverParticleMaterial;
                }
            }
        }
    }

    // Handle hover exit
    public void OnHoverExit()
    {
        if (childObjects != null)
        {
            for (int i = 0; i < childObjects.Length; i++)
            {
                Renderer childRenderer = childObjects[i].GetComponent<Renderer>();
                if (childRenderer != null)
                {
                    childRenderer.material = originalChildMaterials[i];
                }
            }
        }
    }
}
