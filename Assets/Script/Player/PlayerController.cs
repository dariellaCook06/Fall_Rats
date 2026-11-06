using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : NetworkBehaviour
{
    [Header("Moviment")]
    public float speed = 5f;
    public float runSpeed = 8f;
    public float jumpImpulse = 12f;

    [Header("Sòl")]
    public float groundCheckDistance = 1.1f;
    public LayerMask groundMask = ~0;

    [Header("Objectes")]
    public float grabDistance = 3f;
    public float dropImpulse = 12f;
    public Vector3 holdOffset = new Vector3(0f, 1f, 1.2f);

    [Header("Empènyer")]
    public float pushForce = 12f;
    public float pushDuration = 0.3f;

    private Rigidbody rb;
    private Rigidbody heldObject;
    private Rigidbody pushTarget;
    private float pushTimer;
    private Vector2 inputVector;
    private bool runPressed;
    private bool jumpRequested;
    private bool grabRequested;
    private bool pushRequested;

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
        bool running = false;
        bool jumpPressed = false;
        bool grabPressed = false;
        bool pushPressed = false;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) h -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) h += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) v -= 1f;
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) v += 1f;

            running = Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed;
            jumpPressed = Keyboard.current.spaceKey.wasPressedThisFrame;
            grabPressed = Keyboard.current.fKey.wasPressedThisFrame;
            pushPressed = Keyboard.current.eKey.wasPressedThisFrame;
        }

        Vector2 input = new Vector2(h, v).normalized;

        // Enviem l'input i les accions al servidor mitjançant ServerRpc.
        // Important: NO toquem jumpRequested aquí, perquè en el host aquest mateix
        // objecte és alhora client i servidor i esborraríem la petició de salt.
        SubmitInputServerRpc(input, running, jumpPressed, grabPressed, pushPressed);
    }

    [ServerRpc]
    private void SubmitInputServerRpc(Vector2 input, bool running, bool jumpPressed, bool grabPressed, bool pushPressed)
    {
        inputVector = input;
        runPressed = running;

        if (jumpPressed)
        {
            jumpRequested = true;
        }

        if (grabPressed)
        {
            grabRequested = true;
        }

        if (pushPressed)
        {
            pushRequested = true;
        }
    }

    private void FixedUpdate()
    {
        // El servidor calcula totes les físiques de manera autoritària
        if (!IsServer) return;

        // 1. Moviment XZ (caminar a 5, córrer a 8)
        float currentSpeed = runPressed ? runSpeed : speed;
        Vector3 moveDirection = new Vector3(inputVector.x, 0f, inputVector.y);
        Vector3 targetVelocity = moveDirection * currentSpeed;
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

        // 4. Agafar / deixar anar objecte amb F
        if (grabRequested)
        {
            if (heldObject != null)
            {
                DropObject();
            }
            else
            {
                TryGrabObject();
            }

            grabRequested = false;
        }

        // 5. Empènyer objecte amb E durant 0.3 s
        if (pushRequested)
        {
            pushTarget = GetFocusedRigidbody();
            pushTimer = pushTarget != null ? pushDuration : 0f;
            pushRequested = false;
        }

        if (pushTarget != null && pushTimer > 0f)
        {
            pushTarget.AddForce(transform.forward * pushForce, ForceMode.Force);
            pushTimer -= Time.fixedDeltaTime;
        }
    }

    private Rigidbody GetFocusedRigidbody()
    {
        Ray ray = new Ray(transform.position + Vector3.up * 0.5f, transform.forward);

        if (!Physics.Raycast(ray, out RaycastHit hit, grabDistance)) return null;
        if (hit.rigidbody == null || hit.rigidbody == rb) return null;
        if (hit.collider.GetComponent<PlayerController>() != null) return null;

        return hit.rigidbody;
    }

    private void TryGrabObject()
    {
        Rigidbody target = GetFocusedRigidbody();
        if (target == null) return;

        heldObject = target;
        heldObject.linearVelocity = Vector3.zero;
        heldObject.angularVelocity = Vector3.zero;
        heldObject.isKinematic = true;
        heldObject.transform.SetParent(transform);
        heldObject.transform.localPosition = holdOffset;
        heldObject.transform.localRotation = Quaternion.identity;
    }

    private void DropObject()
    {
        heldObject.transform.SetParent(null);
        heldObject.isKinematic = false;
        heldObject.AddForce(transform.forward * dropImpulse, ForceMode.Impulse);
        heldObject = null;
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
