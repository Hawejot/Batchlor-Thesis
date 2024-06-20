using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MainController : MonoBehaviour
{
    private string apiUrl = "http://192.168.43.93:4999";

    // SerializeField attribute makes this field visible in the Unity Editor
    [SerializeField]
    private TextAsset exampleJsonFile;

    private Dictionary<string, string> modelUrls = new Dictionary<string, string>{
        {"None",""},
        {"Sequential", "http://192.168.43.93:4999/sequential/layer_info"},
        {"Autoencoder", "http://192.168.43.93:4999/autoencoder/layer_info"},
        {"VGG", "http://192.168.43.93:4999/vgg/layer_info"}
    };

    public TMP_Dropdown modelDropdown;
    public ApiDataFetcher apiDataFetcher;
    public ModelBuilder modelBuilder;

    public ModelSpawner modelspawner;
    public bool useModelSpawner = false;

    void Start()
    {
        Debug.Log("Start method called.");
        InitializeDropdown(modelDropdown);
        apiDataFetcher.apiUrl = apiUrl;
        Debug.Log($"API URL set to: {apiUrl}");
        StartCoroutine(apiDataFetcher.GetLayerInfo(OnSuccess, OnError));
        Debug.Log("GetLayerInfo method is running.");
    }

    private void InitializeDropdown(TMP_Dropdown dropdown)
    {
        Debug.Log("Initializing dropdown with model options.");
        dropdown.ClearOptions();
        List<string> options = new List<string>(modelUrls.Keys);
        dropdown.AddOptions(options);
        Debug.Log("Dropdown options added.");
        dropdown.onValueChanged.AddListener(delegate {
            DropdownValueChanged(dropdown);
        });
        Debug.Log("Dropdown value changed listener added.");
    }

    void DropdownValueChanged(TMP_Dropdown dropdown)
    {
        if (!dropdown.gameObject.activeInHierarchy) return;

        string selectedModel = dropdown.options[dropdown.value].text;
        Debug.Log($"Dropdown value changed: {selectedModel}");
        if (modelUrls.TryGetValue(selectedModel, out string url))
        {
            apiUrl = url;
            apiDataFetcher.apiUrl = apiUrl;
            Debug.Log($"API URL updated to: {apiUrl}");
            StartCoroutine(apiDataFetcher.GetLayerInfo(OnSuccess, OnError));
            Debug.Log("GetLayerInfo method is running for selected model.");
        }
        else
        {
            Debug.LogError("URL for selected model not found.");
        }
    }

    void OnSuccess(ApiDataFetcher.LayerInfoList layerInfoList)
    {
        Debug.Log("OnSuccess callback called with layer info.");

        if (useModelSpawner)
        {
            modelspawner.SpawnModel (layerInfoList.layers);
        }
        else
        {
            modelBuilder.InstantiateLayers(layerInfoList.layers);
        }
        
        Debug.Log("Layers instantiated successfully.");
    }

    void OnError(string errorMessage)
    {
        Debug.LogError($"OnError callback called with message: {errorMessage}");
        LoadExampleDataFromFile();
    }

    void LoadExampleDataFromFile()
    {
        Debug.Log("Attempting to load example data from file.");
        if (exampleJsonFile != null)
        {
            Debug.Log("Example JSON file assigned.");
            string exampleJson = exampleJsonFile.text;
            Debug.Log("Example JSON file read successfully.");
            ApiDataFetcher.LayerInfoList layerInfoList = JsonUtility.FromJson<ApiDataFetcher.LayerInfoList>("{\"layers\":" + exampleJson + "}");
            OnSuccess(layerInfoList);
        }
        else
        {
            Debug.LogError("Example JSON file not assigned in the editor.");
        }
    }
}
