using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class DeleteNetworkObject : MonoBehaviour
{
    private NetworkManager _networkManager;

    // Start is called before the first frame update
    private void Start()
    {
        GameObject networkManagerObject = GameObject.FindWithTag("NetworkManager");
        if (networkManagerObject != null)
        {
            _networkManager = networkManagerObject.GetComponent<NetworkManager>();
            if (_networkManager == null)
            {
                Debug.LogError("NetworkManager component not found on NetworkManager object!");
            }
        }
        else
        {
            Debug.LogError("NetworkManager object not found!");
        }
    }

    public void DespawnObject(GameObject objectReference)
    {
        if (IsNetworkActive())
        {
            // Obtain the NetworkObject and pass its ID to the ServerRpc
            NetworkObject networkObject = objectReference.GetComponent<NetworkObject>();
            if (networkObject != null)
            {
                RequestDespawnNetworkObjectServerRpc(networkObject.NetworkObjectId);
            }
            else
            {
                Debug.LogError("The object does not have a NetworkObject component!");
            }
        }
        else
        {
            Destroy(objectReference);
        }
    }

    /// <summary>
    /// Function to despawn a given network object on the server.
    /// Marked as private as it should only be called internally by other methods.
    /// </summary>
    /// <param name="networkObjectId">The NetworkObjectId of the object to despawn</param>
    [ServerRpc(RequireOwnership = false)]
    private void RequestDespawnNetworkObjectServerRpc(ulong networkObjectId)
    {
        NetworkObject networkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[networkObjectId];
        if (networkObject != null)
        {
            networkObject.Despawn();
        }
        else
        {
            Debug.LogError("Invalid network object ID to despawn!");
        }
    }

    /// <summary>
    /// Determines whether the network is active by checking if the application
    /// </summary>
    /// <returns></returns>
    private bool IsNetworkActive()
    {
        return _networkManager != null &&
               (_networkManager.IsServer || _networkManager.IsClient || _networkManager.IsHost);
    }
}
