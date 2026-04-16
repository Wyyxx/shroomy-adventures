using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class RewardScreenManager : MonoBehaviour
{
    public static RewardScreenManager Instance;

    [Header("Referencias UI")]
    public GameObject rewardPanel;
    public TextMeshProUGUI goldRewardText;
    public Button continueButton;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (rewardPanel != null) rewardPanel.SetActive(false);
        if (continueButton != null) continueButton.onClick.AddListener(ReturnToMap);
    }

    // Se invoca al ganar la pelea
    public void ShowRewards(int goldEarned)
    {
        if (rewardPanel != null) rewardPanel.SetActive(true);
        
        if (goldRewardText != null) 
            goldRewardText.text = $"+{goldEarned} Oro"; // Aquí se presenta visualmente como un "+"

        // Inyectar el oro a la run global
        if (PlayerRunData.Instance != null)
        {
            PlayerRunData.Instance.currentGold += goldEarned;
        }
    }

    void ReturnToMap()
    {
        // Al darle "Continuar", salimos de la escena de combate
        if (MapManager.Instance != null)
        {
            MapManager.Instance.ReturnToMap("CombatScene");
        }
    }
}
