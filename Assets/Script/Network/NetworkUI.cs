using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class NetworkUI : MonoBehaviour
{
    private string ip = "127.0.0.1";

    private void OnGUI()
    {
        // Sin NetworkManager en la escena no hay nada que iniciar.
        // Mostramos un aviso para que sea evidente (antes lanzaba NullReferenceException aquí).
        if (NetworkManager.Singleton == null)
        {
            GUI.Label(new Rect(10, 10, 400, 30), "No hay NetworkManager activo en esta escena");
            return;
        }

        // Si ya somos host o cliente, ocultamos los botones.
        if (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsServer) return;

        ip = GUI.TextField(new Rect(10, 10, 150, 25), ip);

        if (GUI.Button(new Rect(10, 45, 100, 30), "Host"))
        {
            var transport = NetworkManager.Singleton.GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>();
            if (transport != null)
            {
                transport.SetConnectionData("127.0.0.1", 7777, "0.0.0.0");
                Debug.Log("Configurando Host en puerto 7777 y escuchando en 0.0.0.0 (todas las interfaces)");
            }
            NetworkManager.Singleton.StartHost();
        }

        if (GUI.Button(new Rect(10, 85, 100, 30), "Client"))
        {
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
}