using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class SonicShout : NetworkBehaviour
{
    [SerializeField] private float duration = 3f;
    [SerializeField] private float radius = 5f;
    [SerializeField] private float pushForce = 8f;

    private float timer;

    private void Update()
    {
        if (!IsOwner || Keyboard.current == null) return;

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            ShoutServerRpc();
        }
    }

    [ServerRpc]
    private void ShoutServerRpc()
    {
        if (timer > 0f) return;
        timer = duration;
        ShoutEffectClientRpc();
    }

    [ClientRpc]
    private void ShoutEffectClientRpc()
    {
        Debug.Log("[RED] ¡Alarido sónico de 3 segundos!");
    }

    private void FixedUpdate()
    {
        if (!IsServer || timer <= 0f) return;

        timer -= Time.fixedDeltaTime;
        Collider[] hits = Physics.OverlapSphere(transform.position, radius);

        foreach (Collider hit in hits)
        {
            if (hit.gameObject == gameObject) continue;
            Rigidbody targetRb = hit.GetComponent<Rigidbody>();
            if (targetRb != null)
            {
                Vector3 dir = (hit.transform.position - transform.position).normalized;
                targetRb.AddForce(dir * pushForce, ForceMode.Force);
            }
        }
    }
}