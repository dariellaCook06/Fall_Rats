using Unity.Netcode;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Seguimiento")]
    public Vector3 offset = new Vector3(0f, 4.5f, -7f);
    public float smoothSpeed = 8f;
    public Vector3 lookOffset = new Vector3(0f, 1.2f, 0f);

    private Transform target;
    private bool snapped;

    private void LateUpdate()
    {
        if (target == null)
        {
            target = FindLocalPlayer();
            if (target == null) return;
            snapped = false;
        }

        Vector3 desiredPosition = target.position + offset;

        if (!snapped)
        {
            // Evita que la cámara "vuele" desde su posición inicial hasta el jugador.
            transform.position = desiredPosition;
            snapped = true;
        }
        else
        {
            transform.position = Vector3.Lerp(
                transform.position,
                desiredPosition,
                smoothSpeed * Time.deltaTime);
        }

        transform.LookAt(target.position + lookOffset);
    }

    private Transform FindLocalPlayer()
    {
        PlayerController[] players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);

        foreach (PlayerController player in players)
        {
            if (player.IsSpawned && player.IsOwner)
            {
                return player.transform;
            }
        }

        return null;
    }
}
