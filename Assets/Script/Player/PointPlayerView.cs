using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PointPlayerView : MonoBehaviour
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
            canvasPointPlayer = GetComponent<Canvas>();

        EnsureVisible();
        RefreshUI(0);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        TryBindLocalPlayer();
        RefreshUI(localPlayer != null ? localPlayer.points.Value : 0);
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Update()
    {
        EnsureVisible();

        if (!isSubscribed)
        {
            TryBindLocalPlayer();
        }
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
        TryBindLocalPlayer();
        RefreshUI(localPlayer != null ? localPlayer.points.Value : 0);
    }

    private void EnsureVisible()
    {
        if (canvasPointPlayer != null)
        {
            if (!canvasPointPlayer.gameObject.activeSelf)
                canvasPointPlayer.gameObject.SetActive(true);

            if (!canvasPointPlayer.enabled)
                canvasPointPlayer.enabled = true;
        }
    }

    private void TryBindLocalPlayer()
    {
        if (isSubscribed) return;

        if (NetworkManager.Singleton == null || NetworkManager.Singleton.LocalClient == null)
            return;

        NetworkObject localPlayerObj = NetworkManager.Singleton.LocalClient.PlayerObject;
        if (localPlayerObj == null) return;

        PlayerController controller = localPlayerObj.GetComponent<PlayerController>();
        if (controller == null)
            controller = localPlayerObj.GetComponentInChildren<PlayerController>();

        if (controller == null || !controller.IsOwner) return;

        localPlayer = controller;
        localPlayer.points.OnValueChanged += OnPointsChanged;
        isSubscribed = true;

        EnsureVisible();
        RefreshUI(localPlayer.points.Value);
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

    private void RefreshUI(int currentPoints)
    {
        if (countPointText != null)
        {
            countPointText.text = $"{prefix}{currentPoints}";
        }
    }
}