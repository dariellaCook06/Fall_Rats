using Unity.Netcode;
using UnityEngine;

public class KillCube : NetworkBehaviour
{
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsServer) return;

        PlayerController player = collision.gameObject.GetComponentInParent<PlayerController>();
        if (player == null) return;

        NetworkObject playerNetObj = player.GetComponent<NetworkObject>();
        if (playerNetObj == null || !playerNetObj.IsSpawned) return;

        ulong playerNetworkId = playerNetObj.NetworkObjectId;

        if (NetworkManager.Singleton != null && NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(playerNetworkId, out NetworkObject obj))
        {
            ulong clientId = obj.OwnerClientId;
            obj.gameObject.SetActive(false);

            DiedClientRpc(clientId);
        }
    }

    [ClientRpc]
    private void DiedClientRpc(ulong clientId)
    {
        Debug.Log($"Jugador {clientId} eliminado al tocar el cubo");
    }
}