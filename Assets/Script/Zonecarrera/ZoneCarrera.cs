using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ZoneCarrera : NetworkBehaviour
{
    private List<ulong> clientIds = new List<ulong>();
    private List<Vector3> checkpointPositions = new List<Vector3>();

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        clientIds.Clear();
        checkpointPositions.Clear();
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        clientIds.Clear();
        checkpointPositions.Clear();
    }

    public void SaveCheckpoint(ulong clientId, Vector3 checkpointPos)
    {
        if (!IsServer)
        {
            return;
        }

        int index = clientIds.IndexOf(clientId);
        if (index >= 0)
        {
            checkpointPositions[index] = checkpointPos;
        }
        else
        {
            clientIds.Add(clientId);
            checkpointPositions.Add(checkpointPos);
        }
    }

    public bool TryGetCheckpoint(ulong clientId, out Vector3 checkpointPos)
    {
        int index = clientIds.IndexOf(clientId);
        if (index >= 0)
        {
            checkpointPos = checkpointPositions[index];
            return true;
        }

        checkpointPos = Vector3.zero;
        return false;
    }

    public void TeleportToCheckpoint(PlayerController player, Vector3 fallbackPos)
    {
        if (!IsServer || player == null)
        {
            return;
        }

        Vector3 targetPos = fallbackPos;
        if (TryGetCheckpoint(player.OwnerClientId, out Vector3 savedPos))
        {
            targetPos = savedPos;
        }

        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            if (!rb.isKinematic)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            rb.position = targetPos;
        }

        player.transform.position = targetPos;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer)
        {
            return;
        }

        if (!other.CompareTag("Player"))
        {
            return;
        }

        PlayerController player = other.GetComponentInParent<PlayerController>();
        if (player == null)
        {
            return;
        }

        if (TryGetCheckpoint(player.OwnerClientId, out Vector3 targetPos))
        {
            TeleportToCheckpoint(player, targetPos);
        }
    }
}