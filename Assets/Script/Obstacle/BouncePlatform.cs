using Unity.Netcode;
using UnityEngine;

public class BouncePlatform : NetworkBehaviour
{
    [SerializeField] private float bounceForce = 12f;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // Kinematic para que la plataforma no se mueva ni la empuje el jugador
        if (rb != null)
        {
            rb.isKinematic = true;
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Mantenemos kinematic en servidor y clientes
        if (rb != null)
        {
            rb.isKinematic = true;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsServer) return;

        PlayerController player = collision.gameObject.GetComponentInParent<PlayerController>();
        Rigidbody playerRb = collision.gameObject.GetComponentInParent<Rigidbody>();

        if (player != null && playerRb != null)
        {
            BouncePlayerServerRpc(player.NetworkObjectId);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void BouncePlayerServerRpc(ulong playerNetworkId)
    {
        if (!IsServer) return;

        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(playerNetworkId, out NetworkObject obj))
        {
            Rigidbody targetRb = obj.GetComponent<Rigidbody>();
            if (targetRb != null)
            {
                targetRb.AddForce(Vector3.up * bounceForce, ForceMode.Impulse);
                BounceClientRpc(obj.OwnerClientId);
            }
        }
    }

    [ClientRpc]
    private void BounceClientRpc(ulong clientId)
    {
        Debug.Log($"[RED] Rebote del jugador {clientId}");
    }
}