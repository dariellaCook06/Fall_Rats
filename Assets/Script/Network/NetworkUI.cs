using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class NetworkUI : MonoBehaviour
{
    [Header("Arrastra aquí SOLO el panel y el campo de IP")]
    public GameObject panelConexion;   // el panel que quieres que desaparezca
    public TMP_InputField campoIP;     // opcional

    private string ip = "127.0.0.1";

    private void Update()
    {
        // Sin NetworkManager en la escena no hay nada que iniciar.
        if (NetworkManager.Singleton == null) return;

        // Si ya estamos conectados, ocultamos el panel.
        if (NetworkManager.Singleton.IsConnectedClient)
        {
            if (panelConexion != null) panelConexion.SetActive(false);
        }
    }

    // Esta la llamas desde el OnClick del botón Host
    public void Host()
    {
        var transport = NetworkManager.Singleton.GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>();
        if (transport != null)
        {
            transport.SetConnectionData("127.0.0.1", 7777, "0.0.0.0");
            Debug.Log("Configurando Host en puerto 7777 y escuchando en 0.0.0.0 (todas las interfaces)");
        }
        NetworkManager.Singleton.StartHost();
    }

    // Esta la llamas desde el OnClick del botón Join
    public void Join()
    {
        if (campoIP != null && campoIP.text.Trim() != "")
        {
            ip = campoIP.text.Trim();
        }

        var transport = NetworkManager.Singleton.GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>();
        if (transport != null)
        {
            transport.SetConnectionData(ip, 7777);
            transport.ConnectTimeoutMS = 1000;
            transport.MaxConnectAttempts = 8;
            Debug.Log($"Conectando a {ip}:7777 (Timeout: 8s)");
        }
        NetworkManager.Singleton.StartClient();
    }
}