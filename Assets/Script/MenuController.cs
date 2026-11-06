using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    // Método para ir al Lobby por nombre de escena
    public void LoadLobbyScene()
    {
        SceneManager.LoadScene("Lobby");
    }

    // Método para salir del juego
    public void QuitGame()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();
    }
}