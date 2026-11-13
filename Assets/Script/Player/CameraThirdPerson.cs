using Unity.Netcode;
using UnityEngine;

public class CameraThirdPerson : NetworkBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float distance = 5f;
    [SerializeField] private float height = 2f;

    void LateUpdate()
    {
        // Comprobamos si somos el jugador propietario
        if (!IsOwner || target == null) return;

        // Calculamos la posición detrás del jugador
        Vector3 offset = -target.forward * distance;

        // Colocamos la cámara detrás y arriba
        transform.position = target.position + offset + Vector3.up * height;

        // Hacemos que mire al jugador
        transform.LookAt(target.position);
    }
}