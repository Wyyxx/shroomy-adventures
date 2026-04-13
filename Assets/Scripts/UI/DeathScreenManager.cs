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
        // Destruir los datos de la run para empezar limpio
        if (PlayerRunData.Instance != null)
            Destroy(PlayerRunData.Instance.gameObject);

        // Si la escena de combate fue cargada aditivamente, descargamos todo
        // y cargamos el menú principal de forma limpia
        SceneManager.LoadScene("MainMenu");
    }

    void OnNewGamePressed()
    {
        // Destruir los datos de la run para empezar limpio
        if (PlayerRunData.Instance != null)
            Destroy(PlayerRunData.Instance.gameObject);

        // Cargar directamente la escena del mapa para una nueva partida
        SceneManager.LoadScene("Mapa");
    }
}
