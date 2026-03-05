using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public void OnPlayPressed()
    {
        SceneManager.LoadScene("Mapa");
    }

    public void OnOptionsPressed()
    {
        Debug.Log("Opciones proximamente!");
    }
}