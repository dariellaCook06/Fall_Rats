using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NextMinigameLoader : MonoBehaviour
{
    [Header("Escenas de minijuegos")]
    public string[] minigameScenes =
    {
        "FallAtrapaHuevos",
        "FallPlatform",
        "FallCarrera"
    };

    public void LoadRandomMinigame()
    {
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer)
            return;

        string currentScene = SceneManager.GetActiveScene().name;
        List<string> options = new List<string>();

        foreach (string scene in minigameScenes)
        {
            if (!string.IsNullOrEmpty(scene) && scene != currentScene)
            {
                options.Add(scene);
            }
        }

        if (options.Count == 0) return;

        string nextScene = options[Random.Range(0, options.Count)];
        NetworkManager.Singleton.SceneManager.LoadScene(nextScene, LoadSceneMode.Single);
    }
}