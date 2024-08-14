using UnityEngine;
using Unity.Netcode;

public class NetworkObjectManager : NetworkBehaviour
{
    public NetworkManager networkManager; // Reference to the NetworkManager
    public GameObject[] spawnablePrefabs; // Array to hold different prefabs to spawn
    public GazeBasedPrefabPlacer gBPP; // Reference to the PrefabPositionFinder


    /// <summary>
    /// Function to spawn a network object.
    /// </summary>
    /// <param name="prefabIndex">Index of the prefab in the spawnablePrefabs array.</param>
    /// <param name="position">Position to spawn the object.</param>
    /// <param name="rotation">Rotation to spawn the object.</param>
    /// <param name="rpcParams">Optional RPC parameters.</param>
    [ServerRpc(RequireOwnership = false)]
    private void RequestSpawnNetworkObjectServerRpc(int prefabIndex, Vector3 position, Quaternion rotation, ServerRpcParams rpcParams = default)
    {
        if (prefabIndex < 0 || prefabIndex >= spawnablePrefabs.Length)
        {
            Debug.LogError("Invalid prefab index!");
            return;
        }

        GameObject prefabToSpawn = spawnablePrefabs[prefabIndex];
        GameObject spawnedObject = Instantiate(prefabToSpawn, position, rotation);
        spawnedObject.GetComponent<NetworkObject>().Spawn(true);
    }

    /// <summary>
    /// Client function to request spawning a network object.
    /// </summary>
    /// <param name="prefabIndex">Index of the prefab in the spawnablePrefabs array.</param>
    public void SpawnNetworkObject(int prefabIndex)
    {
        if(prefabIndex > 0 && prefabIndex <= spawnablePrefabs.Length)
        {
            (Vector3 position, Quaternion rotation) = gBPP.FindNearestValidPosition(spawnablePrefabs[prefabIndex]);

            if (!IsNetworkActive())
            {
                SpawnObject(prefabIndex);
            }
            else
            {
                RequestSpawnNetworkObjectServerRpc(prefabIndex, position, rotation);
            }

        }
    }

    public void SpawnObject(int prefabIndex)
    {
        if (prefabIndex > 0 && prefabIndex <= spawnablePrefabs.Length)
        {
            (Vector3 position, Quaternion rotation) = gBPP.FindNearestValidPosition(spawnablePrefabs[prefabIndex]);


            GameObject prefabToSpawn = spawnablePrefabs[prefabIndex];
            GameObject spawnedObject = Instantiate(prefabToSpawn, position, rotation);

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
}
