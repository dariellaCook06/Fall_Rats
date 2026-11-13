
using Unity.Netcode;
using UnityEngine;

public class CameraThirdPerson : NetworkBehaviour
{
    // Configuración de la cámara
    [SerializeField] private float distance = 8f;
    [SerializeField] private float height = 3f;
    [SerializeField] private float speed = 5f;

    private Camera cam;

    public override void OnNetworkSpawn()
    {
        // Comprobamos si este jugador es nuestro
        if (IsOwner)
        {
            cam = Camera.main;
        }
    }

    void LateUpdate()
    {
        // Solo movemos nuestra cámara
        if (!IsOwner) return;

        // Si en el spawn aún no existía la cámara, la buscamos ahora.
        if (cam == null) cam = Camera.main;
        if (cam == null) return;

        // Calculamos la posición de la cámara
        Vector3 position = transform.position - transform.forward * distance + Vector3.up * height;

        // Movemos la cámara
        cam.transform.position = Vector3.Lerp(cam.transform.position, position, speed * Time.deltaTime);

        // Miramos al jugador
        cam.transform.LookAt(transform.position + Vector3.up * 0.5f);
    }
}
