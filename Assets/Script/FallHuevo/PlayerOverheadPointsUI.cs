using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerOverheadPointsUI : NetworkBehaviour
{
    [Header("Referencias")]
    public PlayerController playerController;
    public GameObject overheadCanvas;
    public TextMeshProUGUI pointsText;

    [Header("Solo mostrar en esta escena")]
    public string allowedSceneName = "RecogerHuevos";

    private Camera mainCam;

    private void Awake()
    {
        if (playerController == null)
        {
            playerController = GetComponentInParent<PlayerController>();
        }

        if (overheadCanvas != null)
        {
            overheadCanvas.SetActive(false);
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        SceneManager.activeSceneChanged += OnActiveSceneChanged;

        if (playerController != null)
        {
            playerController.points.OnValueChanged += OnPointsChanged;
            UpdatePointsLabel(playerController.points.Value);
        }

        RefreshSceneVisibility(SceneManager.GetActiveScene().name);
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        SceneManager.activeSceneChanged -= OnActiveSceneChanged;

        if (playerController != null)
        {
            playerController.points.OnValueChanged -= OnPointsChanged;
        }

        if (overheadCanvas != null)
        {
            overheadCanvas.SetActive(false);
        }
    }

    private void OnActiveSceneChanged(Scene oldScene, Scene newScene)
    {
        RefreshSceneVisibility(newScene.name);
    }

    private void RefreshSceneVisibility(string currentSceneName)
    {
        if (overheadCanvas == null)
        {
            return;
        }

        bool showInThisScene = (currentSceneName == allowedSceneName);
        overheadCanvas.SetActive(showInThisScene);
    }

    private void OnPointsChanged(int previousValue, int newValue)
    {
        UpdatePointsLabel(newValue);
    }

    private void UpdatePointsLabel(int currentPoints)
    {
        if (pointsText != null)
        {
            pointsText.text = currentPoints.ToString();
        }
    }

    private void LateUpdate()
    {
        if (overheadCanvas == null || !overheadCanvas.activeSelf)
        {
            return;
        }

        if (mainCam == null)
        {
            mainCam = Camera.main;
        }

        if (mainCam != null)
        {
            // Hacemos que el texto mire hacia la cámara para que se lea de frente
            overheadCanvas.transform.LookAt(
                overheadCanvas.transform.position + mainCam.transform.rotation * Vector3.forward,
                mainCam.transform.rotation * Vector3.up
            );
        }
    }
}