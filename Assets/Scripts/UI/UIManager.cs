using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Panel del Jugador")]
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI blockText;
    public TextMeshProUGUI energyText;
    public TextMeshProUGUI actionsText;
    public Slider playerHealthBar;

    [Header("Panel del Enemigo")]
    public Slider enemyHealthBar;
    public TextMeshProUGUI enemyHealthText;

    [Header("Cartas")]
    public GameObject cardPrefab;
    public Transform handContainer;

    [Header("Botones")]
    public Button endTurnButton;
    public Button confirmCardButton;

    private List<GameObject> cardObjects = new List<GameObject>();
    private int lastHandCount = -1;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    void Start()
    {
        endTurnButton.onClick.AddListener(OnEndTurnPressed);
        confirmCardButton.onClick.AddListener(OnConfirmCardPressed);
        confirmCardButton.gameObject.SetActive(false);
    }

    void Update()
    {
        UpdateStatsUI();

        // Simplemente verificamos si el Manager tiene una carta activa en memoria.
        if (confirmCardButton != null)
        {
            confirmCardButton.gameObject.SetActive(
                CombatManager.Instance.activeCard != null
            );
        }

        int currentHandCount = CombatManager.Instance.hand.Count;
        if (currentHandCount != lastHandCount)
        {
            lastHandCount = currentHandCount;
            RefreshHandUI();
        }
    }

    void UpdateStatsUI()
    {
        if (Player.Instance != null)
        {
            healthText.text = $"HP: {Player.Instance.currentHealth}/{Player.Instance.maxHealth}";
            blockText.text = $"Block: {Player.Instance.currentBlock}";

            if (playerHealthBar != null)
            {
                playerHealthBar.maxValue = Player.Instance.maxHealth;
                playerHealthBar.value = Player.Instance.currentHealth;
            }
        }

        if (CombatManager.Instance != null)
        {
            energyText.text = $"Energía: {CombatManager.Instance.currentEnergy}/{CombatManager.Instance.maxEnergy}";

            if (actionsText != null)
                actionsText.text = $"Acciones: {CombatManager.Instance.playerActionsLeft}/{CombatManager.Instance.maxPlayerActions}";
        }

        Enemy enemy = FindFirstObjectByType<Enemy>();
        if (enemy != null && enemyHealthBar != null)
        {
            enemyHealthBar.maxValue = enemy.maxHealth;
            enemyHealthBar.value = enemy.currentHealth;

            if (enemyHealthText != null)
                enemyHealthText.text = $"{enemy.currentHealth}/{enemy.maxHealth}";
        }
    }

    public void RefreshHandUI()
    {
        foreach (GameObject obj in cardObjects)
            Destroy(obj);
        cardObjects.Clear();

        foreach (CardData cardData in CombatManager.Instance.hand)
        {
            GameObject cardObj = Instantiate(cardPrefab, handContainer);
            Card card = cardObj.GetComponent<Card>();
            card.InitializeCard(cardData);
            cardObjects.Add(cardObj);
        }
    }

    void OnConfirmCardPressed()
    {
        CombatManager.Instance.ConfirmSelectedCard();
    }

    void OnEndTurnPressed()
    {
        CombatManager.Instance.EndPlayerTurn();
    }

    public void ClearHandVisuals()
    {
        foreach (GameObject obj in cardObjects)
        {
            if (obj != null) Destroy(obj);
        }
        
        cardObjects.Clear();
        
        // Sincronizamos el contador para que el Update no intente redibujar fantasmas
        lastHandCount = 0; 
    }
}