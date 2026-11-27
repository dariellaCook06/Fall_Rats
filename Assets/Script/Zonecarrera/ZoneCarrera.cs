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

        if (!IsServer)
        {
            return;
        }

        clientIds.Clear();
        checkpointPositions.Clear();
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (!IsServer)
        {
            return;
        }

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

        int index = clientIds.IndexOf(player.OwnerClientId);
        if (index >= 0)
        {
            Vector3 targetPos = checkpointPositions[index];

            Rigidbody rb = player.GetComponent<Rigidbody>();
            if (rb != null && !rb.isKinematic)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.position = targetPos;
            }

            player.transform.position = targetPos;
        }
    }
}