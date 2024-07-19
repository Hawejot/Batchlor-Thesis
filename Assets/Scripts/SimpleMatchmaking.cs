using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Netcode;
using TMPro; // Make sure you have the TextMeshPro namespace

/// <summary>
/// This script handles simplified matchmaking for a Unity game using the Netcode for GameObjects library.
/// All users are placed in the same lobby. If a user cannot join as a client, they start as a host.
/// </summary>
public class SimpleMatchmaking : MonoBehaviour
{
    public string lobbyName = "defaultLobby";
    public bool startLobby = false;
    public TextMeshProUGUI displayText; // Ensure this is assigned in the Inspector
    private const float RetryInterval = 2.0f;
    private const int MaxRetries = 5;
    private CancellationTokenSource cancellationTokenSource; // Add this line

    /// <summary>
    /// Unity's Update method. Checks if the startLobby flag is set and starts the lobby if true.
    /// </summary>
    void Update()
    {
        if (startLobby)
        {
            LogMessage("Starting lobby");
            StartLobbyFunction();
            startLobby = false;
        }
    }

    /// <summary>
    /// Method to be called by a button to start the lobby process.
    /// </summary>
    public void StartLobbyButtonStart()
    {
        LogMessage("Starting lobby");
        StartLobbyFunction();
    }

    /// <summary>
    /// Attempts to connect as a client. If it fails after several retries, it starts as a host.
    /// </summary>
    private async void StartLobbyFunction()
    {
        if (NetworkManager.Singleton == null)
        {
            LogMessage("NetworkManager.Singleton is not initialized.");
            return;
        }

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

        // Check if already hosting or connected as a client.
        if (NetworkManager.Singleton.IsServer)
        {
            LogMessage("Already hosting a game");
            return;
        }

        if (NetworkManager.Singleton.IsClient)
        {
            LogMessage("Already connected as a client");
            return;
        }

        cancellationTokenSource = new CancellationTokenSource(); // Initialize the cancellation token source

        // Attempt to join as a client.
        for (int i = 0; i < MaxRetries; i++)
        {
            if (await TryJoinAsClient(cancellationTokenSource.Token))
            {
                LogMessage("Successfully joined as a client");
                return;
            }
            LogMessage($"Retrying to join as client... attempt {i + 1}");
            await Task.Delay(TimeSpan.FromSeconds(RetryInterval), cancellationTokenSource.Token);
        }

        // If joining as a client fails, start as a host.
        if (NetworkManager.Singleton.StartHost())
        {
            LogMessage("Started as host");
        }
        else
        {
            LogMessage("Failed to start as host", true);
        }
    }

    /// <summary>
    /// Attempts to join a lobby as a client. If successful within the timeout period,
    /// it returns true. Otherwise, it shuts down and returns false.
    /// </summary>
    private async Task<bool> TryJoinAsClient(CancellationToken cancellationToken)
    {
        NetworkManager.Singleton.StartClient();
        const float timeout = 10f;
        float timer = 0f;

        while (!NetworkManager.Singleton.IsConnectedClient && timer < timeout)
        {
            await Task.Yield();
            timer += Time.deltaTime;

            if (cancellationToken.IsCancellationRequested)
            {
                NetworkManager.Singleton.Shutdown();
                LogMessage("Join attempt cancelled");
                return false;
            }
        }

        if (NetworkManager.Singleton.IsConnectedClient)
        {
            return true;
        }

        NetworkManager.Singleton.Shutdown();
        return false;
    }

    /// <summary>
    /// Cancels the current join attempt.
    /// </summary>
    private void CancelJoinAttempt()
    {
        if (cancellationTokenSource != null)
        {
            cancellationTokenSource.Cancel();
            LogMessage("Cancelled join attempt");
        }
    }

    /// <summary>
    /// Disconnects the user from the network.
    /// </summary>
    private void Disconnect()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
            LogMessage("Disconnected from the network");
        }
    }

    /// <summary>
    /// Handels the logic to either disconnect from a current lobby or abort the current join attempt.
    /// </summary>
    public void DisconnectButton()
    {
        if (NetworkManager.Singleton != null)
        {
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsClient)
            {
                Disconnect();
            }
            else
            {
                CancelJoinAttempt();
            }
        }
    }

    /// <summary>
    /// Callback method triggered when a client connects to the server.
    /// </summary>
    private void OnClientConnected(ulong clientId)
    {
        LogMessage($"Client connected: {clientId}");
        if (NetworkManager.Singleton.IsHost)
        {
            // Handle new client connected on the host
        }
    }

    /// <summary>
    /// Callback method triggered when a client disconnects from the server.
    /// </summary>
    private void OnClientDisconnected(ulong clientId)
    {
        LogMessage($"Client disconnected: {clientId}");
        if (NetworkManager.Singleton.IsHost)
        {
            // Handle client disconnected on the host
        }
    }

    /// <summary>
    /// Unity's OnDestroy method. Unsubscribes from network manager events when the script is destroyed.
    /// </summary>
    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }

    /// <summary>
    /// Logs a message to the console and to the TextMeshPro display.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="isError">Indicates if the message is an error.</param>
    private void LogMessage(string message, bool isError = false)
    {
        if (isError)
        {
            Debug.LogError(message);
        }
        else
        {
            Debug.Log(message);
        }

        if (displayText != null)
        {
            displayText.text = message;
        }
    }
}
