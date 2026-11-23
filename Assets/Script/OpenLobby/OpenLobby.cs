using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class OpenLobby : NetworkBehaviour
{
    [SerializeField] private GameObject promptText; // Texto en el Canvas de la escena

    private bool canInteract;

    private void OnTriggerEnter(Collider other)
    {
        PlayerController player = other.GetComponentInParent<PlayerController>();
        if (player != null && player.IsOwner)
        {
            canInteract = true;
            if (promptText != null) promptText.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerController player = other.GetComponentInParent<PlayerController>();
        if (player != null && player.IsOwner)
        {
            canInteract = false;
            if (promptText != null) promptText.SetActive(false);
        }
    }

    private void Update()
    {
        if (!canInteract || Keyboard.current == null) return;

        if (Keyboard.current.tKey.wasPressedThisFrame)
        {
            if (promptText != null) promptText.SetActive(false);
            OpenCubeServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void OpenCubeServerRpc()
    {
        // El servidor recibe la petición y avisa a los clientes
        OpenCubeClientRpc();
    }

    [ClientRpc]
    private void OpenCubeClientRpc()
    {
        Debug.Log("[RED] ¡Pulso la T y se abre el cubo!");
    }
}