using Unity.Netcode;
using UnityEngine;

public class KillCube : NetworkBehaviour
{
    [Header("Checkpoint y reaparición")]
    public ZoneCarrera zoneCarrera;
    public Transform fallbackSpawn;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (zoneCarrera == null)
        {
            zoneCarrera = FindFirstObjectByType<ZoneCarrera>();
        }
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
        if (!IsServer)
        {
            return;
        }

        PlayerController player = collision.gameObject.GetComponentInParent<PlayerController>();
        if (player == null)
        {
            return;
        }

        NetworkObject playerNetObj = player.GetComponent<NetworkObject>();
        if (playerNetObj == null || !playerNetObj.IsSpawned)
        {
            return;
        }

        ulong clientId = playerNetObj.OwnerClientId;
        Vector3 targetPos = player.transform.position;

        if (fallbackSpawn != null)
        {
            targetPos = fallbackSpawn.position;
        }

        if (zoneCarrera != null)
        {
            zoneCarrera.TeleportToCheckpoint(player, targetPos);
        }
        else
        {
            Rigidbody playerRb = player.GetComponent<Rigidbody>();
            if (playerRb != null)
            {
                if (!playerRb.isKinematic)
                {
                    playerRb.linearVelocity = Vector3.zero;
                    playerRb.angularVelocity = Vector3.zero;
                }

                playerRb.position = targetPos;
            }

            player.transform.position = targetPos;
        }

        DiedClientRpc(clientId);
    }

    [ClientRpc]
    private void DiedClientRpc(ulong clientId)
    {
        Debug.Log($"Jugador {clientId} tocó KillCube y volvió a su checkpoint");
    }
}