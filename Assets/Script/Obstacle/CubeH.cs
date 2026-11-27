using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CubeH : NetworkBehaviour
{
    [Header("Giro")]
    public float speed = 180f;
    public Vector3 upAxis = Vector3.up;
    public bool invertDirection = false;

    private Rigidbody rb;
    private readonly HashSet<Rigidbody> playersOnPlatform = new HashSet<Rigidbody>();

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
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

    private void FixedUpdate()
    {
        if (!IsServer || rb == null) return;

        float dir = invertDirection ? -1f : 1f;
        Vector3 rotAx = upAxis.sqrMagnitude > 0.0001f ? upAxis.normalized : Vector3.up;
        Quaternion rotStep = Quaternion.AngleAxis(speed * dir * Time.fixedDeltaTime, rotAx);
        Vector3 pivot = rb.position;

        rb.MoveRotation(rb.rotation * rotStep);

        playersOnPlatform.RemoveWhere(playerRb => playerRb == null);

        foreach (Rigidbody playerRb in playersOnPlatform)
        {
            if (playerRb.isKinematic) continue;

            Vector3 offset = playerRb.position - pivot;
            Vector3 targetPos = pivot + (rotStep * offset);

            targetPos.y = playerRb.position.y;

            playerRb.MovePosition(targetPos);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsServer) return;
        if (!collision.gameObject.CompareTag("Player")) return;

        Rigidbody playerRb = collision.rigidbody;
        if (playerRb == null) return;

        for (int i = 0; i < collision.contactCount; i++)
        {
            if (collision.GetContact(i).normal.y < -0.5f)
            {
                playersOnPlatform.Add(playerRb);
                break;
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (!IsServer) return;

        if (collision.rigidbody != null)
        {
            playersOnPlatform.Remove(collision.rigidbody);
        }
    }
}