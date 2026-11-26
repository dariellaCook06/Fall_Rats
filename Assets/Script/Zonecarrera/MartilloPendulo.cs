using Unity.Netcode;
using UnityEngine;

public class MartilloPendulo : NetworkBehaviour
{
    [Header("Movimiento de péndulo")]
    public float anguloMaximo = 55f;     // Cuánto oscila a izquierda y derecha
    public float velocidad = 2.2f;       // Qué tan rápido va y viene

    [Header("Empuje al jugador")]
    public float fuerzaEmpuje = 14f;     // Fuerza lateral/hacia adelante al golpear
    public float impulsoArriba = 4f;     // Pequeño salto al recibir el golpe

    private Quaternion rotacionInicial;
    private float tiempo;

    public override void OnNetworkSpawn()
    {
        rotacionInicial = transform.localRotation;
    }

    private void Awake()
    {
        rotacionInicial = transform.localRotation;
    }

    private void FixedUpdate()
    {
        // Si hay red activa, solo el servidor mueve el martillo
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening && !IsServer)
            return;

        tiempo += Time.fixedDeltaTime * velocidad;

        // Oscila suavemente entre -1 y 1 (izquierda y derecha)
        float pendulo = Mathf.Sin(tiempo) * anguloMaximo;

        // Si oscila en otro eje (por ejemplo X o Y en vez de Z), cambia el eje aquí
        Quaternion rotacionObjetivo = rotacionInicial * Quaternion.Euler(0f, 0f, pendulo);
        transform.localRotation = rotacionObjetivo;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening && !IsServer)
            return;

        Rigidbody playerRb = collision.rigidbody;
        if (playerRb == null)
            playerRb = collision.collider.GetComponentInParent<Rigidbody>();

        if (playerRb == null) return;

        // Comprueba que sea un jugador
        PlayerController player = playerRb.GetComponent<PlayerController>();
        if (player == null)
            player = playerRb.GetComponentInParent<PlayerController>();

        if (player == null) return;

        // Dirección desde el martillo hacia el jugador + algo de empuje lateral/arriba
        Vector3 dir = (playerRb.transform.position - transform.position);
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.001f)
            dir = transform.right;
        else
            dir.Normalize();

        Vector3 impulso = dir * fuerzaEmpuje + Vector3.up * impulsoArriba;
        playerRb.AddForce(impulso, ForceMode.Impulse);
    }
}