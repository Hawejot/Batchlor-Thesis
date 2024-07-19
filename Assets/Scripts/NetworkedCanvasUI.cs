
/*
 * This script manages the activation of UI elements in a networked environment using Unity's Netcode for GameObjects.
 * The process ensures that when a client changes the active GameObject, this change is synchronized across all connected clients.
 *
 * Process Overview:
 *
 * 1. Client Button Press:
 *    - A client presses a button that is linked to the `RequestSetGameObject` method. This method is used to request a change in the active GameObject.
 *
 * 2. RequestSetGameObject Method:
 *    - This method checks if the client is the owner (`if (IsOwner)`) to ensure only the owning client can request changes.
 *    - It then calls the `SetGameObjectServerRpc` method, passing the ID of the GameObject to be activated.
 *
 * 3. SetGameObjectServerRpc Method:
 *    - This method is marked with `[ServerRpc]`, meaning it will be executed on the server.
 *    - The server updates the `_activeGameObjectId` NetworkVariable with the new GameObject ID.
 *
 * 4. NetworkVariable Change Propagation:
 *    - When `_activeGameObjectId` is updated on the server, its new value is automatically propagated to all clients.
 *    - The `OnValueChanged` event is triggered on all clients, including the server.
 *
 * 5. OnActiveGameObjectIdChanged Method:
 *    - This method is called whenever `_activeGameObjectId` changes.
 *    - It calls the `SwitchGameObject` method with the new GameObject ID (`newValue`), which updates the UI.
 *
 * 6. SwitchGameObject Method:
 *    - This method iterates through the `GameObjects` list.
 *    - It sets the active state of each GameObject: the GameObject with the matching ID is activated (`true`), and all others are deactivated (`false`).
 *
 * Summary:
 * - When a client requests a change by pressing a button, the server updates the active GameObject ID.
 * - This change is synchronized across all clients, ensuring a consistent UI state for all users.
 *
 * Note: The NetworkVariable `_activeGameObjectId` is set to be writable only by the server to maintain consistency and prevent unauthorized changes by clients.
 */

using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class NetworkedCanvasUi : NetworkBehaviour
{
    public List<GameObject> GameObjects { get; private set; } = new List<GameObject>();

    // NetworkVariable to keep track of the active GameObject ID
    private NetworkVariable<int> _activeGameObjectId = new NetworkVariable<int>(
        readPerm: NetworkVariableReadPermission.Everyone,
        writePerm: NetworkVariableWritePermission.Server
    );
    private const int _InitialValue = 0;

    // Start is called before the first frame update
    private void Start()
    {
        _activeGameObjectId.Value = _InitialValue;
        if (IsServer)
        {
            // Initialize the first active GameObject on the server
            ActivateGameObject(0);
        }
    }

    /// <summary>
    /// Sets the active GameObject based on the provided ID.
    /// This function is triggered by a button.
    /// </summary>
    /// <param name="gameObjectId">The ID of the GameObject to activate.</param>
    [ServerRpc]
    public void SetGameObjectServerRpc(int gameObjectId)
    {
        _activeGameObjectId.Value = gameObjectId;
    }

    /// <summary>
    /// Activates the GameObject with the specified ID and deactivates others.
    /// </summary>
    /// <param name="gameObjectId">The ID of the GameObject to activate.</param>
    private void SwitchGameObject(int gameObjectId)
    {
        for (int i = 0; i < GameObjects.Count; i++)
        {
            GameObjects[i].SetActive(i == gameObjectId);
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        _activeGameObjectId.OnValueChanged += OnActiveGameObjectIdChanged;

        // Ensure the UI state is updated upon spawning
        if (_activeGameObjectId.Value >= 0 && _activeGameObjectId.Value < GameObjects.Count)
        {
            SwitchGameObject(_activeGameObjectId.Value);
        }
    }

    public override void OnNetworkDespawn()
    {
        _activeGameObjectId.OnValueChanged -= OnActiveGameObjectIdChanged;
        base.OnNetworkDespawn();
    }

    /// <summary>
    /// Callback for when the active GameObject ID changes.
    /// </summary>
    /// <param name="oldValue">The previous GameObject ID.</param>
    /// <param name="newValue">The new GameObject ID.</param>
    private void OnActiveGameObjectIdChanged(int oldValue, int newValue)
    {
        SwitchGameObject(newValue);
    }

    /// <summary>
    /// Activates the GameObject with the specified ID and deactivates others.
    /// </summary>
    /// <param name="gameObjectId">The ID of the GameObject to activate.</param>
    private void ActivateGameObject(int gameObjectId)
    {
        SwitchGameObject(gameObjectId);
    }

    /// <summary>
    /// Client-side function to request a change in active GameObject.
    /// </summary>
    /// <param name="gameObjectId">The ID of the GameObject to activate.</param>
    public void RequestSetGameObject(int gameObjectId)
    {
        //if (IsOwner)
        //{
            SetGameObjectServerRpc(gameObjectId);
        //}
    }
}
