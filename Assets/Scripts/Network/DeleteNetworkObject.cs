using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class DeleteNetworkObject : MonoBehaviour
{
    private NetworkManager _networkManager;

    // Start is called before the first frame update
    void Start()
    {
        GameObject networkManagerObject =  GameObject.FindWithTag("NetworkManager");
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

    /// <summary>
    /// Function to despawn a given network object
    /// </summary>
    /// <param name="networkObjectReference">Reference to the network object to despawn</param>
    /// <param name="rpcParams">Optional RPC parameters</param>
    [ServerRpc(RequireOwnership = false)]
    public void RequestDespawnNetworkObjectServerRpc(NetworkObjectReference networkObjectReference, ServerRpcParams rpcParams = default)
    {
        if (networkObjectReference.TryGet(out NetworkObject networkObject))
        {
            networkObject.Despawn();
            Destroy(networkObject.gameObject);
        }
        else
        {
            Debug.LogError("Invalid network object to despawn!");
        }
    }

    /// <summary>
    /// Client function to request despawning a network object
    /// </summary>
    /// <param name="networkObject">The network object to despawn</param>
    public void DespawnNetworkObject(NetworkObject networkObject)
    {
        if (networkObject != null)
        {
            NetworkObjectReference networkObjectReference = networkObject;
            RequestDespawnNetworkObjectServerRpc(networkObjectReference);
        }
        else
        {
            Debug.LogError("Network object is null!");
        }
    }
}
