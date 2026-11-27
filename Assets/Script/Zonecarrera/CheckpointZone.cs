using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class CheckpointZone : NetworkBehaviour
{
    public ZoneCarrera zoneCarrera;

    private void Awake()
    {
        BoxCollider box = GetComponent<BoxCollider>();
        if (box != null)
        {
            box.isTrigger = true;
        }

        if (zoneCarrera == null)
        {
            zoneCarrera = FindFirstObjectByType<ZoneCarrera>();
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

        if (zoneCarrera != null)
        {
            zoneCarrera.SaveCheckpoint(player.OwnerClientId, transform.position);
        }
    }
}