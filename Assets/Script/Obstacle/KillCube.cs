using Unity.Netcode;
using UnityEngine;

public class KillCube : NetworkBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (!IsServer) return;

        PlayerController player = collision.gameObject.GetComponentInParent<PlayerController>();
        if (player != null)
        {
            KillServerRpc(player.NetworkObjectId);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void KillServerRpc(ulong playerNetworkId)
    {
        if (!IsServer) return;

        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(playerNetworkId, out NetworkObject obj))
        {
            obj.gameObject.SetActive(false);
            DiedClientRpc(obj.OwnerClientId);
        }
    }

    [ClientRpc]
    private void DiedClientRpc(ulong clientId)
    {
        Debug.Log($"Jugador {clientId} eliminado al tocar el cubo");
    }
}