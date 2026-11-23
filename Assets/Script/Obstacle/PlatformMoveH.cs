using Unity.Netcode;
using UnityEngine;

public class PlatformMoveH : NetworkBehaviour
{
    [SerializeField] private float spinSpeed = 180f;
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // Kinematic para que el jugador no mueva/empuje la plataforma
        if (rb != null)
        {
            rb.isKinematic = true;
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Mantenemos kinematic en servidor y clientes para que no lo mueva el jugador[cite: 2, 4]
        if (rb != null)
        {
            rb.isKinematic = true;
        }
    }

    private void FixedUpdate()
    {
        // Solo el servidor rota la plataforma y sincroniza por NetworkTransform[cite: 2, 4]
        if (!IsServer || rb == null) return;

        Quaternion step = Quaternion.Euler(0f, spinSpeed * Time.fixedDeltaTime, 0f);
        rb.MoveRotation(rb.rotation * step);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetSpinServerRpc(float newSpeed)
    {
        if (!IsServer) return;
        spinSpeed = newSpeed;
        SpinChangedClientRpc(spinSpeed);
    }

    [ClientRpc]
    private void SpinChangedClientRpc(float currentSpeed)
    {
        Debug.Log($"Giro horizontal a {currentSpeed}");
    }
}