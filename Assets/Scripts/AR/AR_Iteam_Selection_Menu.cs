using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode; // For MLAPI or Netcode for GameObjects (Unity's networking solution)


/// <summary>
/// Manages the selection and spawning of items in an AR environment, 
/// handling both networked and non-networked scenarios.
/// </summary>
public class ARItemSelectionMenu : MonoBehaviour
{
    /// <summary>
    /// Reference to the NetworkObjectManager, which handles networked object spawning.
    /// </summary>
    public NetworkObjectManager networkObjectManager;

    /// <summary>
    /// Spawns an item based on the provided index. Determines whether to spawn
    /// a networked or non-networked object based on the current network state.
    /// </summary>
    /// <param name="index">The index of the item to spawn.</param>
    public void ItemSelectionMenu(int index)
    {
        if (IsNetworkActive())
        {
            // Spawn networked object based on the provided index
            SpawnNetworkedObject(index);
        }
        else
        {
            // Spawn non-networked object based on the provided index
            SpawnNonNetworkedObject(index);
        }
    }

    /// <summary>
    /// Determines whether the network is active by checking if the application
    /// is acting as a server, client, or host.
    /// </summary>
    /// <returns>True if the network is active; otherwise, false.</returns>
    private bool IsNetworkActive()
    {
        // Find the NetworkManager in the scene
        NetworkManager networkManager = FindObjectOfType<NetworkManager>();

        // Check if NetworkManager exists and if the network is active
        return networkManager != null &&
               (networkManager.IsServer || networkManager.IsClient || networkManager.IsHost);
    }

    /// <summary>
    /// Spawns a networked object based on the provided index.
    /// </summary>
    /// <param name="index">The index of the item to spawn.</param>
    private void SpawnNetworkedObject(int index)
    {
        switch (index)
        {
            case 0:
                networkObjectManager.SpawnNetworkObject(0);
                break;
            case 1:
                networkObjectManager.SpawnNetworkObject(1);
                break;
            case 2:
                networkObjectManager.SpawnNetworkObject(2);
                break;
            case 3:
                networkObjectManager.SpawnNetworkObject(3);
                break;
            case 4:
                networkObjectManager.SpawnNetworkObject(4);
                break;
            case 5:
                networkObjectManager.SpawnNetworkObject(5);
                break;
            default:
                Debug.LogWarning("Invalid index for networked object spawn.");
                break;
        }
    }

    /// <summary>
    /// Spawns a non-networked object based on the provided index.
    /// </summary>
    /// <param name="index">The index of the item to spawn.</param>
    private void SpawnNonNetworkedObject(int index)
    {
        switch (index)
        {
            case 0:
                networkObjectManager.SpawnObject(0);
                break;
            case 1:
                networkObjectManager.SpawnObject(1);
                break;
            case 2:
                networkObjectManager.SpawnObject(2);
                break;
            case 3:
                networkObjectManager.SpawnObject(3);
                break;
            case 4:
                networkObjectManager.SpawnObject(4);
                break;
            case 5:
                networkObjectManager.SpawnObject(5);
                break;
            default:
                Debug.LogWarning("Invalid index for non-networked object spawn.");
                break;
        }
    }
}
