using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject panelConexion;
    public TMP_InputField campoIP;
    public string menuSceneName = "Menu";

    private string ip = "127.0.0.1";
    private bool isSubscribed;

    private void OnEnable()
    {
        TrySubscribe();
    }

    private void Start()
    {
        TrySubscribe();
    }

    private void OnDisable()
    {
        if (isSubscribed && NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnect;
            isSubscribed = false;
        }
    }

    private void Update()
    {
        if (NetworkManager.Singleton == null) return;

        TrySubscribe();

        if (NetworkManager.Singleton.IsConnectedClient)
        {
            if (panelConexion != null)
                panelConexion.SetActive(false);
        }
    }

    public void Host()
    {
        if (NetworkManager.Singleton == null) return;
        if (NetworkManager.Singleton.IsListening || NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer) return;

        TrySubscribe();

        UnityTransport transport =
            NetworkManager.Singleton.GetComponent<UnityTransport>();

        if (transport != null)
        {
            transport.SetConnectionData("127.0.0.1", 7777, "0.0.0.0");
            Debug.Log("Host started on port 7777");
        }

        NetworkManager.Singleton.StartHost();
    }

    public void Join()
    {
        if (NetworkManager.Singleton == null) return;
        if (NetworkManager.Singleton.IsListening) return;

        TrySubscribe();

        if (campoIP != null && campoIP.text.Trim() != "")
            ip = campoIP.text.Trim();

        UnityTransport transport =
            NetworkManager.Singleton.GetComponent<UnityTransport>();

        if (transport != null)
        {
            transport.SetConnectionData(ip, 7777);
            transport.ConnectTimeoutMS = 1000;
            transport.MaxConnectAttempts = 8;

            Debug.Log($"Connecting to {ip}:7777");
        }

        NetworkManager.Singleton.StartClient();
    }

    private void TrySubscribe()
    {
        if (isSubscribed || NetworkManager.Singleton == null) return;

        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
        isSubscribed = true;
    }

    private void OnClientDisconnect(ulong clientId)
    {
        if (NetworkManager.Singleton == null) return;
        if (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsHost) return;

        if (clientId == NetworkManager.ServerClientId || clientId == NetworkManager.Singleton.LocalClientId)
        {
            if (panelConexion != null)
                panelConexion.SetActive(true);

            string targetScene = string.IsNullOrWhiteSpace(menuSceneName) || menuSceneName == "MainMenu"
                ? "Menu"
                : menuSceneName.Trim();

            if (SceneManager.GetActiveScene().name != targetScene)
            {
                if (Application.CanStreamedLevelBeLoaded(targetScene))
                {
                    SceneManager.LoadScene(targetScene, LoadSceneMode.Single);
                }
                else
                {
                    Debug.LogWarning($"La escena '{targetScene}' no está añadida a File > Build Profiles.");
                }
            }
        }
    }
}