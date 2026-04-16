using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public GameObject optionsPanel;

    public void OnPlayPressed()
    {
        SceneManager.LoadScene("Mapa");
    }

    public void OnOptionsPressed()
    {
        optionsPanel.SetActive(true);
    }
}