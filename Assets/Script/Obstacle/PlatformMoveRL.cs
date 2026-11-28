using Unity.Netcode;
using UnityEngine;

public class PlatformMoveRL : NetworkBehaviour
{
    [Header("Puntos de movimiento")]
    public Transform pointA;
    public Transform pointB;
    public float speed = 2f;
    public bool invertDirection = false;

    private Rigidbody rb;
    private float timer;
    private Rigidbody playerRb;

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

        timer = 0f;
        playerRb = null;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        playerRb = null;
    }

    private void FixedUpdate()
    {
        if (!IsServer)
        {
            return;
        }

        if (rb == null || pointA == null || pointB == null)
        {
            return;
        }

        timer += Time.fixedDeltaTime * speed;
        float t = Mathf.PingPong(timer, 1f);

        Vector3 startPos = invertDirection ? pointB.position : pointA.position;
        Vector3 endPos = invertDirection ? pointA.position : pointB.position;

        Vector3 nextPos = Vector3.Lerp(startPos, endPos, t);
        Vector3 moveDelta = nextPos - rb.position;

        rb.MovePosition(nextPos);

        if (playerRb != null && !playerRb.isKinematic)
        {
            playerRb.MovePosition(playerRb.position + moveDelta);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsServer)
        {
            return;
        }

        PlayerController player = collision.gameObject.GetComponentInParent<PlayerController>();
        if (player != null)
        {
            playerRb = player.GetComponent<Rigidbody>();
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (!IsServer)
        {
            return;
        }

        PlayerController player = collision.gameObject.GetComponentInParent<PlayerController>();
        if (player != null && player.GetComponent<Rigidbody>() == playerRb)
        {
            playerRb = null;
        }
    }
} 