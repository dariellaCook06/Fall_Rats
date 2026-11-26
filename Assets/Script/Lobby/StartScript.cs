using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartScript : NetworkBehaviour
{
    public GameObject PanelStart;
    public Button beginButton;
    public Button readyButton;

    private readonly string[] scenes = { "FallAtrapaHuevos", "FallPlatform", "FallCarrera" };
    private readonly HashSet<ulong> readyPlayers = new HashSet<ulong>();

    public override void OnNetworkSpawn()
    {
        if (readyButton != null)
            readyButton.onClick.AddListener(Ready);

        if (beginButton != null)
        {
            beginButton.onClick.AddListener(Begin);
            beginButton.interactable = false;
        }
    }

    public void Ready()
    {
        if (readyButton != null)
            readyButton.interactable = false;

        SetReadyServerRpc();
    }

    public void Begin()
    {
        if (!IsServer) return;
        if (NetworkManager.Singleton.ConnectedClientsIds.Count < 2) return;

        foreach (ulong id in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (!readyPlayers.Contains(id)) return;
        }

        string randomScene = scenes[Random.Range(0, scenes.Length)];
        HidePanelClientRpc();
        NetworkManager.Singleton.SceneManager.LoadScene(randomScene, LoadSceneMode.Single);
    }

    public void Exit()
    {
        if (PanelStart != null)
            PanelStart.SetActive(false);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetReadyServerRpc(ServerRpcParams rpcParams = default)
    {
        readyPlayers.Add(rpcParams.Receive.SenderClientId);

        bool allReady = NetworkManager.Singleton.ConnectedClientsIds.Count >= 2;
        foreach (ulong id in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (!readyPlayers.Contains(id))
            {
                allReady = false;
                break;
            }
        }

        SetBeginButtonClientRpc(allReady);
    }

    [ClientRpc]
    private void SetBeginButtonClientRpc(bool active)
    {
        if (beginButton != null)
            beginButton.interactable = IsServer && active;
    }

    [ClientRpc]
    private void HidePanelClientRpc()
    {
        if (PanelStart != null)
            PanelStart.SetActive(false);
    }
}