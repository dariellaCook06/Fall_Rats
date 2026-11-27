using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PointPlayerView : NetworkBehaviour
{
    public static PointPlayerView Instance { get; private set; }

    [Header("UI References")]
    public Canvas canvasPointPlayer;
    public TextMeshProUGUI countPointText;

    [Header("Format")]
    public string prefix = "Puntos: ";

    private PlayerController localPlayer;
    private bool isSubscribed;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (canvasPointPlayer == null)
        {
            canvasPointPlayer = GetComponent<Canvas>();
        }

        EnsureVisible();
        RefreshUI(0);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        SceneManager.sceneLoaded += OnSceneLoaded;
        BindLocalPlayer();
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        SceneManager.sceneLoaded -= OnSceneLoaded;
        UnsubscribeFromPlayer();
        RefreshUI(0);
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        UnsubscribeFromPlayer();

        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UnsubscribeFromPlayer();
        EnsureVisible();
        BindLocalPlayer();
    }

    public void RegisterLocalPlayer(PlayerController controller)
    {
        if (controller == null || !controller.IsOwner) return;

        UnsubscribeFromPlayer();
        localPlayer = controller;
        localPlayer.points.OnValueChanged += OnPointsChanged;
        isSubscribed = true;

        EnsureVisible();
        RefreshUI(localPlayer.points.Value);
    }

    public void UnregisterLocalPlayer(PlayerController controller)
    {
        if (controller != null && controller == localPlayer)
        {
            UnsubscribeFromPlayer();
            RefreshUI(0);
        }
    }

    private void BindLocalPlayer()
    {
        if (isSubscribed) return;

        if (NetworkManager.Singleton == null || NetworkManager.Singleton.LocalClient == null)
        {
            RefreshUI(0);
            return;
        }

        NetworkObject localPlayerObj = NetworkManager.Singleton.LocalClient.PlayerObject;
        if (localPlayerObj == null)
        {
            RefreshUI(0);
            return;
        }

        PlayerController controller = localPlayerObj.GetComponent<PlayerController>();
        if (controller == null)
        {
            controller = localPlayerObj.GetComponentInChildren<PlayerController>();
        }

        if (controller != null && controller.IsOwner)
        {
            RegisterLocalPlayer(controller);
        }
        else
        {
            RefreshUI(0);
        }
    }

    private void UnsubscribeFromPlayer()
    {
        if (isSubscribed && localPlayer != null)
        {
            localPlayer.points.OnValueChanged -= OnPointsChanged;
        }

        localPlayer = null;
        isSubscribed = false;
    }

    private void OnPointsChanged(int previousValue, int newValue)
    {
        RefreshUI(newValue);
    }

    private void EnsureVisible()
    {
        if (canvasPointPlayer == null) return;

        if (!canvasPointPlayer.gameObject.activeSelf)
        {
            canvasPointPlayer.gameObject.SetActive(true);
        }

        if (!canvasPointPlayer.enabled)
        {
            canvasPointPlayer.enabled = true;
        }
    }

    private void RefreshUI(int currentPoints)
    {
        if (countPointText != null)
        {
            countPointText.text = $"{prefix}{currentPoints}";
        }
    }
}