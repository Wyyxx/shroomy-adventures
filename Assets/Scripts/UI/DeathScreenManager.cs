using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DeathScreenManager : MonoBehaviour
{
    public static DeathScreenManager Instance;

    [Header("Referencias UI")]
    public GameObject deathScreenPanel;
    public Button mainMenuButton;
    public Button newGameButton;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // Aseguramos que la pantalla de muerte empiece oculta
        if (deathScreenPanel != null)
            deathScreenPanel.SetActive(false);

        // Conectar botones
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(OnMainMenuPressed);

        if (newGameButton != null)
            newGameButton.onClick.AddListener(OnNewGamePressed);
    }

    public void ShowDeathScreen()
    {
        if (deathScreenPanel != null)
        {
            deathScreenPanel.SetActive(true);
            Debug.Log("<color=red>¡Te consumió el Cobalto!</color>");
        }
    }

    void OnMainMenuPressed()
    {
        if (PlayerRunData.Instance != null)
            Destroy(PlayerRunData.Instance.gameObject);

        // LoadSceneMode.Single destruye TODAS las escenas aditivas activas
        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }

    void OnNewGamePressed()
    {
        if (PlayerRunData.Instance != null)
            Destroy(PlayerRunData.Instance.gameObject);

        SceneManager.LoadScene("Mapa", LoadSceneMode.Single);
    }
}
