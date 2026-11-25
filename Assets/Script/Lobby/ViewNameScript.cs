using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class ViewPlayerScript : NetworkBehaviour
{
    [Header("Referencias del Scroll View")]
    [SerializeField] private Transform content; // Arrastra aquí Scroll View > Viewport > Content
    [SerializeField] private TMP_Text textoNombrePrefab; // Arrastra tu prefab de TextMeshPro

    // Lista sincronizada por red (FixedString32Bytes es necesario para Netcode)
    private NetworkList<FixedString32Bytes> nombresConectados;

    private void Awake()
    {
        nombresConectados = new NetworkList<FixedString32Bytes>();
    }

    public override void OnNetworkSpawn()
    {
        // Cada vez que la lista cambie (alguien entra o sale), actualizamos el Scroll View
        nombresConectados.OnListChanged += ActualizarScrollView;

        // Enviamos nuestro nombre local al servidor para que nos añada a la lista
        EnviarNombreAlServidorRpc(NetworkUI.NombreJugador);

        // Dibujamos los nombres que ya estuvieran conectados
        RefrescarListaUI();
    }

    public override void OnNetworkDespawn()
    {
        nombresConectados.OnListChanged -= ActualizarScrollView;
    }

    // El cliente le pide al servidor que añada su nombre a la lista oficial
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void EnviarNombreAlServidorRpc(string nombre)
    {
        if (!nombresConectados.Contains(nombre))
        {
            nombresConectados.Add(nombre);
        }
    }

    private void ActualizarScrollView(NetworkListEvent<FixedString32Bytes> changeEvent)
    {
        RefrescarListaUI();
    }

    private void RefrescarListaUI()
    {
        // Limpiar nombres anteriores del Content
        foreach (Transform hijo in content)
        {
            Destroy(hijo.gameObject);
        }

        // Instanciar un texto por cada jugador en la lista sincronizada
        foreach (var nombre in nombresConectados)
        {
            TMP_Text nuevoTexto = Instantiate(textoNombrePrefab, content);
            nuevoTexto.text = nombre.ToString();
        }
    }
}
