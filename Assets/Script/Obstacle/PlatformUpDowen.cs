using Unity.Netcode;
using UnityEngine;

public class PlatformUpDowen : NetworkBehaviour
{
    [Header("Arriba y abajo")]
    public float speed = 0.5f;
    public float topY = 656.8f;
    public float bottomY = 654.1f;

    private Rigidbody rb;

    private float timer;

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
    }

    private void FixedUpdate()
    {
        if (!IsServer || rb == null) return;

        timer += Time.fixedDeltaTime * speed;

        float t = Mathf.PingPong(timer, 1f);
        float targetY = Mathf.Lerp(bottomY, topY, t);

        rb.MovePosition(new Vector3(rb.position.x, targetY, rb.position.z));
    }
}