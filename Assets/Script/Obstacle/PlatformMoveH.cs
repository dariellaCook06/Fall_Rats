using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlatformMoveH : NetworkBehaviour
{
    [SerializeField] private float spinSpeed = 180f;

    [SerializeField] private bool invertDirection = false;

    private Rigidbody rb;

    private readonly HashSet<Rigidbody> playersOnTop = new HashSet<Rigidbody>();

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
        Quaternion step = Quaternion.Euler(0f, spinSpeed * dir * Time.fixedDeltaTime, 0f);
        Vector3 pivot = rb.position;

        rb.MoveRotation(rb.rotation * step);

        playersOnTop.RemoveWhere(playerRb => playerRb == null);

        foreach (Rigidbody playerRb in playersOnTop)
        {
            if (playerRb.isKinematic) continue;

            Vector3 offset = playerRb.position - pivot;
            Vector3 targetPos = pivot + (step * offset);

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
                playersOnTop.Add(playerRb);
                break;
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (!IsServer) return;

        if (collision.rigidbody != null)
        {
            playersOnTop.Remove(collision.rigidbody);
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void SetSpinServerRpc(float newSpeed)
    {
        if (!IsServer) return;
        spinSpeed = newSpeed;
        SpinChangedClientRpc(spinSpeed);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void SpinChangedClientRpc(float currentSpeed)
    {
        Debug.Log($"Giro horizontal a {currentSpeed}");
    }
}