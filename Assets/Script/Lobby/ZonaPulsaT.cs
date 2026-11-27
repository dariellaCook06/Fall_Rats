using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class ZonaPulsaT : MonoBehaviour
{
    public GameObject promptCanvas;
    public Canvas startCanvas;
    public StartScript startScript;

    private bool canInteract;
    private bool isOpen;

    private void Start()
    {
        if (startScript == null && startCanvas != null)
            startScript = startCanvas.GetComponent<StartScript>();

        if (startScript == null)
            startScript = FindFirstObjectByType<StartScript>();

        if (promptCanvas != null)
            promptCanvas.SetActive(false);

        CloseStartUI();
    }

    private void LateUpdate()
    {
        if (promptCanvas != null && promptCanvas.activeSelf && Camera.main != null)
            promptCanvas.transform.rotation = Camera.main.transform.rotation;
    }

    private void Update()
    {
        if (!canInteract || Keyboard.current == null) return;

        if (Keyboard.current.tKey.wasPressedThisFrame)
        {
            if (isOpen)
                CloseStartUI();
            else
                OpenStartUI();
        }
    }

    private void OpenStartUI()
    {
        isOpen = true;

        if (promptCanvas != null)
            promptCanvas.SetActive(false);

        if (startScript != null)
            startScript.ShowStartCanvas();
        else if (startCanvas != null)
            startCanvas.enabled = true;
    }

    private void CloseStartUI()
    {
        isOpen = false;

        if (startScript != null)
            startScript.HideStartCanvas();
        else if (startCanvas != null)
            startCanvas.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        var netObj = other.GetComponentInParent<NetworkObject>();
        if (netObj == null || !netObj.IsPlayerObject || !netObj.IsOwner) return;

        canInteract = true;

        if (!isOpen && promptCanvas != null)
            promptCanvas.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        var netObj = other.GetComponentInParent<NetworkObject>();
        if (netObj == null || !netObj.IsPlayerObject || !netObj.IsOwner) return;

        canInteract = false;

        if (promptCanvas != null)
            promptCanvas.SetActive(false);

        CloseStartUI();
    }
}