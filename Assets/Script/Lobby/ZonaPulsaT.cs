using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class ZonaPulsaT : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject promptCanvas;
    [SerializeField] private GameObject startCanvas;

    private bool canInteract;
    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
        if (promptCanvas != null) promptCanvas.SetActive(false);
        if (startCanvas != null) startCanvas.SetActive(false);
    }

    private void LateUpdate()
    {
        if (promptCanvas == null || !promptCanvas.activeSelf) return;
        if (mainCamera == null) mainCamera = Camera.main;
        if (mainCamera == null) return;

        promptCanvas.transform.rotation = mainCamera.transform.rotation;
    }

    private void Update()
    {
        if (!canInteract) return;
        if (Keyboard.current == null || !Keyboard.current.tKey.wasPressedThisFrame) return;

        if (promptCanvas != null) promptCanvas.SetActive(false);
        if (startCanvas != null) startCanvas.SetActive(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsLocalPlayer(other)) return;

        canInteract = true;
        if (promptCanvas != null) promptCanvas.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsLocalPlayer(other)) return;

        canInteract = false;
        if (promptCanvas != null) promptCanvas.SetActive(false);
    }

    private bool IsLocalPlayer(Collider other)
    {
        NetworkObject player = other.GetComponent<NetworkObject>();
        return player != null && player.IsPlayerObject && player.IsOwner;
    }
}