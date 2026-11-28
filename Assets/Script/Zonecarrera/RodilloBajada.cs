using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class RodilloBajada : NetworkBehaviour
{
    [Header("Referencias del rodillo")]
    public GameObject cylinderObject;
    public Transform startPoint;
    public Transform endPoint;

    [Header("Movimiento y tiempos")]
    public float moveSpeed = 6f;
    public float rollSpeed = 360f;
    public float respawnDelay = 2f;

    private Rigidbody rb;
    private bool isMoving = false;
    private bool isWaiting = false;

    private void Awake()
    {
        if (cylinderObject == null) return;

        rb = cylinderObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsServer)
        {
            return;
        }

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        StartRoller();
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        StopAllCoroutines();
        isMoving = false;
        isWaiting = false;
    }

    private void StartRoller()
    {
        if (!IsServer)
        {
            return;
        }

        if (cylinderObject == null || startPoint == null || endPoint == null)
        {
            return;
        }

        if (rb != null)
        {
            rb.position = startPoint.position;
        }
        else
        {
            cylinderObject.transform.position = startPoint.position;
        }

        isWaiting = false;
        isMoving = true;
    }

    private void FixedUpdate()
    {
        if (!IsServer)
        {
            return;
        }

        if (!isMoving || isWaiting)
        {
            return;
        }

        if (cylinderObject == null || endPoint == null)
        {
            return;
        }

        Vector3 currentPos = rb != null ? rb.position : cylinderObject.transform.position;
        Vector3 newPos = Vector3.MoveTowards(currentPos, endPoint.position, moveSpeed * Time.fixedDeltaTime);

        if (rb != null)
        {
            rb.MovePosition(newPos);
            rb.MoveRotation(rb.rotation * Quaternion.Euler(0f, rollSpeed * Time.fixedDeltaTime, 0f));
        }
        else
        {
            cylinderObject.transform.position = newPos;
            cylinderObject.transform.Rotate(Vector3.up * rollSpeed * Time.fixedDeltaTime, Space.Self);
        }

        float distanceToEnd = Vector3.Distance(newPos, endPoint.position);
        if (distanceToEnd <= 0.05f)
        {
            isMoving = false;
            StartCoroutine(RespawnAfterDelay());
        }
    }

    private IEnumerator RespawnAfterDelay()
    {
        isWaiting = true;
        yield return new WaitForSeconds(respawnDelay);
        StartRoller();
    }
}
