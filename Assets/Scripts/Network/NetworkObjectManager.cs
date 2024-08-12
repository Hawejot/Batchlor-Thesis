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
    public void RequestSpawnNetworkObjectServerRpc(int prefabIndex, Vector3 position, Quaternion rotation, ServerRpcParams rpcParams = default)
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
            RequestSpawnNetworkObjectServerRpc(prefabIndex, position, rotation);
        }
    }

    public void SpawnObject(int prefabIndex)
    {
        if (prefabIndex > 0 && prefabIndex <= spawnablePrefabs.Length)
        {
            (Vector3 position, Quaternion rotation) = gBPP.FindNearestValidPosition(spawnablePrefabs[prefabIndex]);


            //Test

            position = new Vector3(0, 0, 0);
            rotation = new Quaternion(0, 0, 0, 0);

            GameObject prefabToSpawn = spawnablePrefabs[prefabIndex];
            GameObject spawnedObject = Instantiate(prefabToSpawn, position, rotation);

        }
    }
}
