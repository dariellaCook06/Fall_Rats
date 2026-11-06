using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : NetworkBehaviour
{
    [Header("Moviment")]
    public float speed = 5f;
    public float jumpImpulse = 12f;

    [Header("Sòl")]
    public float groundCheckDistance = 1.1f;
    public LayerMask groundMask = ~0;

    private Rigidbody rb;
    private Vector2 inputVector;
    private bool jumpRequested;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Regla de física en xarxa: Servidor dinàmic, Clients cinemàtics
        if (IsServer)
        {
            rb.isKinematic = false;
        }
        else
        {
            rb.isKinematic = true;
        }
    }

    private void Update()
    {
        // Només el jugador propietari llegeix el teclat
        if (!IsOwner) return;

        float h = 0f;
        float v = 0f;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) h -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) h += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) v -= 1f;
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) v += 1f;

            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                jumpRequested = true;
            }
        }

        Vector2 input = new Vector2(h, v).normalized;

        // Enviem l'input i la petició de salt al servidor mitjançant ServerRpc
        SubmitInputServerRpc(input, jumpRequested);

        // Resetegem localment la petició per evitar bucles
        if (jumpRequested) jumpRequested = false;
    }

    [ServerRpc]
    private void SubmitInputServerRpc(Vector2 input, bool jumpPressed)
    {
        inputVector = input;

        if (jumpPressed)
        {
            jumpRequested = true;
        }
    }

    private void FixedUpdate()
    {
        // El servidor calcula totes les físiques de manera autoritària
        if (!IsServer) return;

        // 1. Moviment XZ
        Vector3 moveDirection = new Vector3(inputVector.x, 0f, inputVector.y);
        Vector3 targetVelocity = moveDirection * speed;
        rb.linearVelocity = new Vector3(targetVelocity.x, rb.linearVelocity.y, targetVelocity.z);

        // 2. Orientació (LookAt / Rotació)
        if (moveDirection.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, 0.15f));
        }

        // 3. Salt amb validació de terra
        if (jumpRequested && IsGrounded())
        {
            rb.AddForce(Vector3.up * jumpImpulse, ForceMode.Impulse);

            // Notificació a tots els clients per reproduir el so de salt (ClientRpc)
            PlayJumpSoundClientRpc();

            jumpRequested = false;
        }
    }

    private bool IsGrounded()
    {
        return Physics.Raycast(
            transform.position,
            Vector3.down,
            groundCheckDistance,
            groundMask,
            QueryTriggerInteraction.Ignore);
    }

    [ClientRpc]
    private void PlayJumpSoundClientRpc()
    {
        AudioSource audioSource = GetComponent<AudioSource>();
        if (audioSource != null)
        {
            audioSource.Play();
        }
    }
}