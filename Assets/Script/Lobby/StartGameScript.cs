using Unity.Netcode;
using UnityEngine;

public class StartGameScript : NetworkBehaviour
{
    // Creamos esat varaible para verificar que esten ready
    public NetworkVariable<bool> readyplayer = new NetworkVariable<bool>(false,NetworkVariableReadPermission.Everyone,NetworkVariableWritePermission.Server);

    // Funcion que sera del ready
    public void ToggleReady()
    {
        if (!IsOwner) return; // Solo el dueño de este jugador puede pulsar su botón
        SetReadyServerRpc(!readyplayer.Value);
    }

    // El cliente pide al servidor cambiar su estado
    [ServerRpc]
    private void SetReadyServerRpc(bool readyState)
    {
        readyplayer.Value = readyState;

        // Avisamos al lobby que se peude empezar
    }
}
