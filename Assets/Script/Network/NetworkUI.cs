using Unity.Netcode;
using UnityEngine;

public class NetworkUI : MonoBehaviour
{
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

        if (GUI.Button(new Rect(10, 10, 100, 30), "Host"))
        {
            NetworkManager.Singleton.StartHost();
        }

        if (GUI.Button(new Rect(10, 50, 100, 30), "Client"))
        {
            NetworkManager.Singleton.StartClient();
        }
    }
}
