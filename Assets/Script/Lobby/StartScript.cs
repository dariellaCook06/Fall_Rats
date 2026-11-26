using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class StartScript : NetworkBehaviour
{
    public Canvas startCanvas;
    public GameObject PanelStart;
    public Button beginButton;
    public TextMeshProUGUI readyButtonText;
    public TextMeshProUGUI playersCounterText;
    public NextMinigameLoader nextMinigameLoader;

    private int readyCount = 0;
    private bool isReady = false;
    private bool isStarting = false;

    private void Awake()
    {
        if (startCanvas == null)
            startCanvas = GetComponent<Canvas>();
    }

    public override void OnNetworkSpawn()
    {
        isStarting = false;
        readyCount = 0;
        isReady = false;

        HideStartCanvas();

        if (readyButtonText != null)
            readyButtonText.text = "Ready";

        if (beginButton != null)
        {
            beginButton.gameObject.SetActive(IsServer);
            beginButton.interactable = false;
        }

        RefreshText();
    }

    public void ShowStartCanvas()
    {
        if (startCanvas != null)
            startCanvas.enabled = true;

        if (PanelStart != null)
            PanelStart.SetActive(true);
    }

    public void HideStartCanvas()
    {
        if (PanelStart != null)
            PanelStart.SetActive(false);

        if (startCanvas != null)
            startCanvas.enabled = false;
    }

    public void Ready()
    {
        if (!IsSpawned || isStarting) return;

        isReady = !isReady;
        if (readyButtonText != null)
            readyButtonText.text = isReady ? "Quitar Ready" : "Ready";

        AddReadyServerRpc(isReady ? 1 : -1);
    }

    public void Begin()
    {
        if (!IsServer || !IsSpawned || isStarting) return;

        int total = NetworkManager.Singleton != null ? NetworkManager.Singleton.ConnectedClientsIds.Count : 0;
        if (total < 2 || readyCount < total) return;

        isStarting = true;
        if (beginButton != null)
            beginButton.interactable = false;

        if (nextMinigameLoader != null)
        {
            nextMinigameLoader.LoadRandomMinigame();
        }
    }

    public void Exit()
    {
        HideStartCanvas();
    }

    [ServerRpc(RequireOwnership = false)]
    private void AddReadyServerRpc(int delta)
    {
        if (NetworkManager.Singleton == null || isStarting) return;

        int total = NetworkManager.Singleton.ConnectedClientsIds.Count;
        readyCount = Mathf.Clamp(readyCount + delta, 0, total);

        bool canStart = total >= 2 && readyCount >= total;
        SyncUIClientRpc(readyCount, total, canStart);
    }

    [ClientRpc]
    private void SyncUIClientRpc(int ready, int total, bool canStart)
    {
        if (playersCounterText != null)
            playersCounterText.text = $"{ready}/{total}";

        if (beginButton != null)
            beginButton.interactable = IsServer && canStart && !isStarting;
    }

    private void RefreshText()
    {
        int total = NetworkManager.Singleton != null ? NetworkManager.Singleton.ConnectedClientsIds.Count : 0;
        if (playersCounterText != null)
            playersCounterText.text = $"{readyCount}/{total}";
    }
}