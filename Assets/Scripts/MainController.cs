using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Netcode;
using System.Collections;

public class MainController : NetworkBehaviour
{
    #region Variables
    private const string DefaultApiUrl = "http://192.168.43.93:4999";

    [SerializeField]
    private TextAsset exampleJsonFile;

    private readonly Dictionary<string, string> modelUrls = new Dictionary<string, string>
    {
        { "Sequential", "http://192.168.43.93:4999/sequential/layer_info" },
        { "Autoencoder", "http://192.168.43.93:4999/autoencoder/layer_info" },
        { "VGG", "http://192.168.43.93:4999/vgg/layer_info" }
    };

    public TMP_Dropdown modelDropdown;
    public ApiDataFetcher apiDataFetcher;
    public ModelBuilder modelBuilder;
    public ModelSpawner modelSpawner;
    public bool useModelSpawner = false;

    private NetworkVariable<ApiDataFetcher.LayerInfoList> layerInformation = new NetworkVariable<ApiDataFetcher.LayerInfoList>();

    #endregion

    #region Unity Methods

    /// <summary>
    /// Initializes the dropdown menu and sets the default API URL.
    /// Starts fetching the layer information from the API.
    /// </summary>
    private void Start()
    {
        InitializeDropdown(modelDropdown);
        apiDataFetcher.apiUrl = DefaultApiUrl;
        StartCoroutine(apiDataFetcher.GetLayerInfo(OnSuccess, OnError));
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Changes the selected model for all clients.
    /// </summary>
    /// <param name="selectedModel">The selected model name.</param>
    public void ChangeSelectedModel(string selectedModel)
    {
        if (modelUrls.TryGetValue(selectedModel, out var url))
        {
            apiDataFetcher.apiUrl = url;
            StartCoroutine(apiDataFetcher.GetLayerInfo(OnSuccess, OnError));
        }
        else
        {
            Debug.LogError("URL for selected model not found.");
        }
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Initializes the dropdown menu with model options and sets the event listener for value change.
    /// </summary>
    /// <param name="dropdown">The dropdown to initialize.</param>
    private void InitializeDropdown(TMP_Dropdown dropdown)
    {
        dropdown.ClearOptions();
        var options = new List<string>(modelUrls.Keys);
        dropdown.AddOptions(options);
        dropdown.onValueChanged.AddListener(delegate
        {
            DropdownValueChanged(dropdown);
        });
    }

    /// <summary>
    /// Handles the event when the dropdown value is changed.
    /// Fetches the layer information for the selected model.
    /// </summary>
    /// <param name="dropdown">The dropdown whose value has changed.</param>
    private void DropdownValueChanged(TMP_Dropdown dropdown)
    {
        if (!dropdown.gameObject.activeInHierarchy)
            return;

        var selectedModel = dropdown.options[dropdown.value].text;
        if (modelUrls.TryGetValue(selectedModel, out var url))
        {
            apiDataFetcher.apiUrl = url;
            StartCoroutine(apiDataFetcher.GetLayerInfo(OnSuccess, OnError));
        }
        else
        {
            Debug.LogError("URL for selected model not found.");
        }
    }

    /// <summary>
    /// Callback for successful API call.
    /// Triggers the server RPC to change the model.
    /// </summary>
    /// <param name="layerInfoList">The list of layer information fetched from the API.</param>
    private void OnSuccess(ApiDataFetcher.LayerInfoList layerInfoList)
    {
        if (IsNetworkActive())
        {
            ChangeModelServerRpc(layerInfoList);
        }
        else
        {
            ChangeModel(layerInfoList);
        }
        
    }

    /// <summary>
    /// Callback for failed API call.
    /// Loads example data from file and triggers the server RPC to change the model.
    /// </summary>
    /// <param name="errorMessage">The error message from the failed API call.</param>
    private void OnError(string errorMessage)
    {
        if (IsNetworkActive())
        {
            ChangeModelServerRpc(LoadExampleDataFromFile());
        }
        else
        {
            ChangeModel(LoadExampleDataFromFile());
        }
        
    }

    /// <summary>
    /// Loads example layer information data from the assigned JSON file.
    /// </summary>
    /// <returns>The list of layer information loaded from the example file.</returns>
    private ApiDataFetcher.LayerInfoList LoadExampleDataFromFile()
    {
        if (exampleJsonFile != null)
        {
            var exampleJson = exampleJsonFile.text;
            return JsonUtility.FromJson<ApiDataFetcher.LayerInfoList>($"{{\"layers\":{exampleJson}}}");
        }
        else
        {
            Debug.LogError("Example JSON file not assigned in the editor.");
            return null;
        }
    }

    #endregion

    #region Network Methods

    /// <summary>
    /// Server RPC to change the model on the server.
    /// </summary>
    /// <param name="layerInfoList">The list of layer information for the new model.</param>
    [ServerRpc(RequireOwnership = false)]
    private void ChangeModelServerRpc(ApiDataFetcher.LayerInfoList layerInfoList)
    {
        ChangeModelClientRpc(layerInfoList);
    }

    /// <summary>
    /// Client RPC to change the model on all clients.
    /// </summary>
    /// <param name="layerInfoList">The list of layer information for the new model.</param>
    [ClientRpc]
    private void ChangeModelClientRpc(ApiDataFetcher.LayerInfoList layerInfoList)
    {
        if (useModelSpawner)
        {
            modelSpawner.SpawnModel(layerInfoList.layers);
        }
        else
        {
            modelBuilder.InstantiateLayers(layerInfoList.layers);
        }
    }

    /// <summary>
    /// Changes the model for single user
    /// </summary>
    /// <param name="layerInfoList"></param>
    private void ChangeModel(ApiDataFetcher.LayerInfoList layerInfoList)
    {
        if (useModelSpawner)
        {
            modelSpawner.SpawnModel(layerInfoList.layers);
        }
        else
        {
            modelBuilder.InstantiateLayers(layerInfoList.layers);
        }
    }

    /// <summary>
    /// Determines whether the network is active by checking if the application
    /// </summary>
    /// <returns></returns>
    private bool IsNetworkActive()
    {
        // Find the NetworkManager in the scene
        NetworkManager networkManager = FindObjectOfType<NetworkManager>();

        // Check if NetworkManager exists and if the network is active
        return networkManager != null &&
               (networkManager.IsServer || networkManager.IsClient || networkManager.IsHost);
    }

    #endregion
}
