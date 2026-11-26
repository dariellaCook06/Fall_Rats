using Unity.Netcode;
using UnityEngine;

public class PlatformDown : NetworkBehaviour
{
    [SerializeField] private float countdownSeconds = 4f;

    [Header("Materiales")]
    [SerializeField] private Material initialMaterial;
    [SerializeField] private Material greenMaterial;
    [SerializeField] private Material yellowMaterial;
    [SerializeField] private Material redMaterial;

    private readonly NetworkVariable<bool> isActive = new NetworkVariable<bool>(true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private readonly NetworkVariable<bool> isCountingDown = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private readonly NetworkVariable<int> colorStage = new NetworkVariable<int>(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

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
        colorStage.OnValueChanged += OnColorStageChanged;

        ApplyActive(isActive.Value);
        ApplyColor(colorStage.Value);
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        isActive.OnValueChanged -= OnActiveChanged;
        colorStage.OnValueChanged -= OnColorStageChanged;
    }

    private void OnCollisionEnter(Collision collision)
    {
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
        colorStage.Value = 0;
    }

    private void Update()
    {
        if (!IsServer || !isCountingDown.Value || !isActive.Value) return;

        timer -= Time.deltaTime;

        int newStage = 0;
        if (timer <= countdownSeconds * 0.33f) newStage = 2;
        else if (timer <= countdownSeconds * 0.66f) newStage = 1;

        if (newStage != colorStage.Value)
            colorStage.Value = newStage;

        if (timer <= 0f)
        {
            isCountingDown.Value = false;
            isActive.Value = false;
        }
    }

    private void OnActiveChanged(bool previousValue, bool newValue)
    {
        ApplyActive(newValue);
    }

    private void OnColorStageChanged(int previousValue, int newValue)
    {
        ApplyColor(newValue);
    }

    private void ApplyActive(bool active)
    {
        if (rend != null) rend.enabled = active;
        if (col != null) col.enabled = active;
    }

    private void ApplyColor(int stage)
    {
        if (rend == null) return;

        if (stage < 0)
        {
            if (initialMaterial != null)
                rend.sharedMaterial = initialMaterial;
        }
        else if (stage == 0)
        {
            if (greenMaterial != null)
                rend.sharedMaterial = greenMaterial;
        }
        else if (stage == 1)
        {
            if (yellowMaterial != null)
                rend.sharedMaterial = yellowMaterial;
        }
        else
        {
            if (redMaterial != null)
                rend.sharedMaterial = redMaterial;
        }
    }
}