using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class NetworkUI : MonoBehaviour
{
    [Header("Arrastra aquí el panel, la IP y el nombre")]
    public GameObject panelConexion;
    public TMP_InputField campoIP;
    public TMP_InputField campoNombre;

    // Aquí se queda el nombre para usarlo luego en la lista
    public static string NombreJugador = "";

    private string ip = "127.0.0.1";

    private void Start()
    {
        // Recupera el nombre guardado la última vez
        NombreJugador = PlayerPrefs.GetString("NombreJugador", "");
        if (campoNombre != null) campoNombre.text = NombreJugador;
    }

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

    // Guarda el nombre del campo en PlayerPrefs (queda guardado en el PC)
    private void GuardarNombre()
    {
        if (campoNombre == null) return;

        NombreJugador = campoNombre.text.Trim();
        if (NombreJugador.Length > 16) NombreJugador = NombreJugador.Substring(0, 16);
        if (NombreJugador == "") NombreJugador = "Jugador";

        PlayerPrefs.SetString("NombreJugador", NombreJugador);
        PlayerPrefs.Save();
    }

    // Esta la llamas desde el OnClick del botón Host
    public void Host()
    {
        GuardarNombre();

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
        GuardarNombre();

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