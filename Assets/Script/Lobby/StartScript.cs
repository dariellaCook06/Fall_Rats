using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class StartScript : NetworkBehaviour
{
    public GameObject PanelStart;
    public Button beginButton;
    public Button readyButton;

    private readonly HashSet<ulong> readyPlayers = new HashSet<ulong>();
    private bool isLocalReady;

    public override void OnNetworkSpawn()
    {
        if (readyButton != null)
        {
            readyButton.onClick.AddListener(OnReadyClicked);
            readyButton.interactable = true;
        }

        if (beginButton != null)
        {
            beginButton.onClick.AddListener(OnBeginClicked);
            beginButton.interactable = false;
        }

        if (IsServer)
        {
            NetworkManager.OnClientDisconnectCallback += OnClientDisconnected;
            RefreshBeginButtonState();
        }
    }

    public override void OnNetworkDespawn()
    {
        if (readyButton != null)
            readyButton.onClick.RemoveListener(OnReadyClicked);

        if (beginButton != null)
            beginButton.onClick.RemoveListener(OnBeginClicked);

        if (IsServer && NetworkManager != null)
            NetworkManager.OnClientDisconnectCallback -= OnClientDisconnected;
    }

    public void OnReadyClicked()
    {
        if (isLocalReady) return;

        isLocalReady = true;
        if (readyButton != null)
            readyButton.interactable = false;

        SubmitReadyServerRpc();
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void SubmitReadyServerRpc(RpcParams rpcParams = default)
    {
        ulong senderId = rpcParams.Receive.SenderClientId;
        readyPlayers.Add(senderId);
        RefreshBeginButtonState();
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (!IsServer) return;

        readyPlayers.Remove(clientId);
        RefreshBeginButtonState();
    }

    private void RefreshBeginButtonState()
    {
        if (!IsServer) return;

        bool allReady = AreAllConnectedPlayersReady();
        SetBeginInteractableClientRpc(allReady);
    }

    private bool AreAllConnectedPlayersReady()
    {
        if (NetworkManager == null || NetworkManager.ConnectedClientsIds.Count == 0)
            return false;

        foreach (ulong clientId in NetworkManager.ConnectedClientsIds)
        {
            if (!readyPlayers.Contains(clientId))
                return false;
        }

        return true;
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void SetBeginInteractableClientRpc(bool canBegin)
    {
        if (beginButton == null) return;

        beginButton.interactable = IsServer && canBegin;
    }

    public void OnBeginClicked()
    {
        if (!IsServer || !AreAllConnectedPlayersReady()) return;

        StartGameClientRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void StartGameClientRpc()
    {
        if (PanelStart != null)
            PanelStart.SetActive(false);
    }

    public void Exit()
    {
        PanelStart.SetActive(false);
    }
}