using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlatformMove : NetworkBehaviour
{
    [Header("Movimiento circular")]
    public float speed = 2f;
    public float radius = 3f;

    private Rigidbody rb;

    private Vector3 centerPos;
    private float angle;

    private readonly HashSet<Rigidbody> playersOnTop = new HashSet<Rigidbody>();

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        centerPos = rb != null ? rb.position : transform.position;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            centerPos = rb.position;
        }
        else
        {
            centerPos = transform.position;
        }
    }

    private void FixedUpdate()
    { 
        if (!IsServer || rb == null) return;

        angle += speed * Time.fixedDeltaTime;

        float x = centerPos.x + Mathf.Cos(angle) * radius;
        float z = centerPos.z + Mathf.Sin(angle) * radius;
        Vector3 targetPos = new Vector3(x, centerPos.y, z);

        Vector3 deltaMove = targetPos - rb.position;
        rb.MovePosition(targetPos);

        playersOnTop.RemoveWhere(playerRb => playerRb == null);

        foreach (Rigidbody playerRb in playersOnTop)
        {
            if (playerRb.isKinematic) continue;
            playerRb.MovePosition(playerRb.position + deltaMove);
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
}