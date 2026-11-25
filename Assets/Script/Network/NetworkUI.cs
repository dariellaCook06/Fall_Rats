
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class NetworkUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject panelConexion;
    public TMP_InputField campoIP;

    private string ip = "127.0.0.1";

    private void Update()
    {
        if (NetworkManager.Singleton == null) return;

        if (NetworkManager.Singleton.IsConnectedClient)
        {
            if (panelConexion != null)
                panelConexion.SetActive(false);
        }
    }

    public void Host()
    {
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
}
