using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCamera : NetworkBehaviour
{
    [Header("Referencias")]
    public Transform target;
    public Camera cam;
    public AudioListener audioListener;

    [Header("Distancia y altura")]
    public Vector3 lookOffset = new Vector3(0f, 1.5f, 0f);
    public float distance = 6f;
    public float minDistance = 1.2f;
    public float followSmoothness = 12f;

    [Header("Rotación")]
    public float sensitivityX = 2.5f;
    public float sensitivityY = 2f;
    public float minPitch = -20f;
    public float maxPitch = 65f;
    public bool lockCursor = true;

    [Header("Evitar atravesar paredes")]
    public LayerMask obstacleMask;
    public float sphereRadius = 0.35f;

    private float yaw = 0f;
    private float pitch = 15f;
    private Vector3 currentDesiredPos;

    private static PlayerCamera localInstance;
    private bool cursorFree;
    public static void SetCursorVisible(bool visible)
    {
        if (localInstance == null)
        {
            return;
        }

        localInstance.cursorFree = visible;

        bool showCursor = visible || !localInstance.lockCursor;
        Cursor.lockState = showCursor ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = showCursor;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsOwner)
        {
            if (cam != null)
            {
                cam.enabled = false;
            }

            if (audioListener != null)
            {
                audioListener.enabled = false;
            }

            enabled = false;
            return;
        }

        if (cam != null)
        {
            cam.enabled = true;
        }

        if (audioListener != null)
        {
            audioListener.enabled = true;
        }

        if (target == null)
        {
            target = transform.root;
        }

        yaw = target.eulerAngles.y;
        currentDesiredPos = transform.position;

        localInstance = this;
        SetCursorVisible(false);
    }

    public override void OnNetworkDespawn()
    {
        if (localInstance == this)
        {
            localInstance = null;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        base.OnNetworkDespawn();
    }

    private void LateUpdate()
    {
        if (!IsOwner)
        {
            return;
        }

        if (target == null)
        {
            return;
        }

        Vector2 lookDelta = Vector2.zero;

        if (!cursorFree)
        {
            if (Mouse.current != null)
            {
                lookDelta += Mouse.current.delta.ReadValue();
            }

            if (Gamepad.current != null)
            {
                lookDelta += Gamepad.current.rightStick.ReadValue() * 10f;
            }
        }

        float mouseX = lookDelta.x * sensitivityX * 0.02f;
        float mouseY = lookDelta.y * sensitivityY * 0.02f;

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 pivotPos = target.position + lookOffset;
        Vector3 desiredDirection = rotation * Vector3.back;

        float finalDistance = distance;

        if (Physics.SphereCast(pivotPos, sphereRadius, desiredDirection, out RaycastHit hit, distance, obstacleMask))
        {
            finalDistance = Mathf.Clamp(hit.distance - 0.1f, minDistance, distance);
        }

        Vector3 targetPosition = pivotPos + desiredDirection * finalDistance;
        currentDesiredPos = Vector3.Lerp(currentDesiredPos, targetPosition, followSmoothness * Time.deltaTime);

        transform.position = currentDesiredPos;
        transform.LookAt(pivotPos);
    }
}