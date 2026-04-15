using UnityEngine;
using UnityEngine.UI;

public class HealingScreenManager : MonoBehaviour
{
    public static HealingScreenManager Instance;

    [Header("Referencias UI")]
    public GameObject healingPanel;
    public Button healButton;

    void Awake() => Instance = this;

    void Start()
    {
        if (healingPanel != null) healingPanel.SetActive(false);
        
        if (healButton != null) 
            healButton.onClick.AddListener(ProcessHealing);
    }

    public void ShowPopup()
    {
        if (healingPanel != null) healingPanel.SetActive(true);
    }

    void ProcessHealing()
    {
        if (PlayerRunData.Instance != null)
        {
            // Calcula exactamente el 30% de la vida máxima
            int healAmount = Mathf.RoundToInt(PlayerRunData.Instance.maxHealth * 0.30f);
            
            // Suma la vida sin sobrepasar el límite máximo
            PlayerRunData.Instance.currentHealth = Mathf.Min(PlayerRunData.Instance.currentHealth + healAmount, PlayerRunData.Instance.maxHealth);
            
            Debug.Log($"<color=green>Curaste {healAmount} HP. Vida actual: {PlayerRunData.Instance.currentHealth}/{PlayerRunData.Instance.maxHealth}</color>");
        }

        // Cierra el panel automáticamente tras curarse
        if (healingPanel != null) healingPanel.SetActive(false);
    }
}