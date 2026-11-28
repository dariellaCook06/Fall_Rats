using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : NetworkBehaviour
{
    // Referencia al Rigidbody para físicas y movimiento
    private Rigidbody rb;

    // Velocidad de desplazamiento del jugador
    public float speed = 5f;

    // Fuerza del salto
    public float jumpForce = 6f;

    public Camera playerCamera;

    private Vector3 moveDirection;

    // Sonido del salto
    private AudioSource jumpSound;

    // Puntos acumulados del jugador sincronizados en red (empiezan en 0)
    public NetworkVariable<int> points = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    // Suma puntos de forma segura desde el servidor
    public void AddPoints(int amount)
    {
        if (!IsServer)
        {
            return;
        }

        points.Value += amount;
    }

    private void Awake()
    {
        // Obtenemos los componentes del jugador
        rb = GetComponent<Rigidbody>();
        jumpSound = GetComponent<AudioSource>();

        if (playerCamera == null)
        {
            playerCamera = GetComponentInChildren<Camera>();
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // En el servidor el Rigidbody simula físicas; en clientes es cinemático
        if (rb != null)
        {
            rb.isKinematic = !IsServer;
        }
    }
    private void Update()
    {
        // Solo el jugador dueño lee sus propias teclas
        if (!IsOwner)
        {
            return;
        }

        float h = 0f;
        float v = 0f;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            {
                h -= 1f;
            }

            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            {
                h += 1f;
            }

            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
            {
                v -= 1f;
            }

            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
            {
                v += 1f;
            }
        }

        Vector3 camForward = Vector3.forward;
        Vector3 camRight = Vector3.right;

        if (playerCamera != null)
        {
            camForward = playerCamera.transform.forward;
            camRight = playerCamera.transform.right;
            camForward.y = 0f;
            camRight.y = 0f;

            if (camForward.sqrMagnitude > 0.0001f)
            {
                camForward.Normalize();
            }

            if (camRight.sqrMagnitude > 0.0001f)
            {
                camRight.Normalize();
            }
        }

        Vector3 dir = camForward * v + camRight * h;
        if (dir.magnitude > 1f)
        {
            dir.Normalize();
        }

        // Enviamos la dirección relativa a la cámara al servidor
        SubmitInputServerRpc(dir);

        // Si pulsa Espacio, pedimos al servidor validar el salto
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            JumpServerRpc();
        }
    }

    [ServerRpc]
    private void SubmitInputServerRpc(Vector3 dir)
    {
        // El servidor guarda la dirección recibida del cliente
        if (dir.magnitude > 1f)
        {
            dir.Normalize();
        }

        moveDirection = dir;
    }

    [ServerRpc]
    private void JumpServerRpc()
    {
        // Comprobamos si toca el suelo antes de aplicar la fuerza del salto
        if (!Physics.Raycast(transform.position, Vector3.down, 1.1f))
        {
            return;
        }

        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

        // Avisamos a los clientes para que suene el AudioSource
        PlayJumpSoundClientRpc();
    }

    [ClientRpc]
    private void PlayJumpSoundClientRpc()
    {
        if (jumpSound != null)
        {
            jumpSound.Play();
        }
    }

    private void FixedUpdate()
    {
        // Solo el servidor aplica la física autoritativa
        if (!IsServer)
        {
            return;
        }

        Vector3 targetVelocity = moveDirection * speed;

        // Mantenemos la velocidad vertical y aplicamos X y Z
        rb.linearVelocity = new Vector3(targetVelocity.x, rb.linearVelocity.y, targetVelocity.z);

        // Orientamos el jugador hacia la dirección en la que se mueve
        if (moveDirection != Vector3.zero)
        {
            Vector3 lookTarget = transform.position + moveDirection;
            transform.LookAt(lookTarget);
        }

        // Evitamos rotaciones residuales no deseadas
        rb.angularVelocity = Vector3.zero;
    }
}