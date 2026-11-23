using Unity.Netcode;
using UnityEngine;

public class PlatformDown : NetworkBehaviour
{
    [SerializeField] private float countdownSeconds = 4f;

    // Solo el servidor escribe, todos leen[cite: 1, 2]
    private readonly NetworkVariable<bool> isActive = new NetworkVariable<bool>(
        true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private readonly NetworkVariable<bool> isCountingDown = new NetworkVariable<bool>(
        false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private float timer;
    private Renderer rend;
    private Collider col;

    private void Awake()
    {
        rend = GetComponent<Renderer>();
        col = GetComponent<Collider>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        isActive.OnValueChanged += OnActiveChanged;
        ApplyActive(isActive.Value);
        if (rend != null) rend.material.color = Color.green;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        isActive.OnValueChanged -= OnActiveChanged;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Si pisa un jugador, pedimos al servidor iniciar la cuenta atrás[cite: 2, 3]
        if (!isActive.Value || isCountingDown.Value) return;
        if (collision.gameObject.GetComponentInParent<PlayerController>() != null)
        {
            StartCountdownServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void StartCountdownServerRpc()
    {
        if (!isActive.Value || isCountingDown.Value) return;
        timer = countdownSeconds;
        isCountingDown.Value = true;
    }

    private void Update()
    {
        if (isActive.Value && isCountingDown.Value && rend != null)
        {
            // Cambio simple de color: verde -> amarillo -> rojo
            if (timer > countdownSeconds * 0.66f) rend.material.color = Color.green;
            else if (timer > countdownSeconds * 0.33f) rend.material.color = Color.yellow;
            else rend.material.color = Color.red;
        }

        // Solo el servidor descuenta el tiempo y desactiva la plataforma[cite: 2, 3]
        if (!IsServer || !isCountingDown.Value || !isActive.Value) return;

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            isCountingDown.Value = false;
            isActive.Value = false;
            PlatformGoneClientRpc();
        }
    }

    private void OnActiveChanged(bool previousValue, bool newValue)
    {
        ApplyActive(newValue);
    }

    private void ApplyActive(bool active)
    {
        if (rend != null) rend.enabled = active;
        if (col != null) col.enabled = active;
    }

    [ClientRpc]
    private void PlatformGoneClientRpc()
    {
        Debug.Log("[RED] Plataforma destruida.");
    }
}