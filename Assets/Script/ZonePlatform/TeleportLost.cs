using Unity.Netcode;
using UnityEngine;

public class TeleportLost : NetworkBehaviour
{
    public Transform lostPoint;

    public void SendPlayerToLost(PlayerController player)
    {
        if (!IsServer)
        {
            return;
        }

        if (player == null || lostPoint == null)
        {
            return;
        }

        Vector3 targetPos = lostPoint.position;

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
}