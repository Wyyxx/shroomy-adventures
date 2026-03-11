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

    [Header("Cartas")]
    public GameObject cardPrefab;
    public Transform handContainer;

    [Header("Botones")]
    public Button endTurnButton;
    // ¡Adiós confirmCardButton!

    private List<GameObject> cardObjects = new List<GameObject>();
    private int lastHandCount = -1;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    void Start()
    {
        // Añadimos un escudo protector por si olvidas arrastrar el botón en el Inspector
        if (endTurnButton != null)
        {
            endTurnButton.onClick.AddListener(OnEndTurnPressed);
        }
        else
        {
            Debug.LogWarning("UIManager: Falta asignar el botón de End Turn en el Inspector.");
        }
    }

    void Update()
    {
        UpdateStatsUI();

        if (CombatManager.Instance == null) return;

        if (CombatManager.Instance.hand != null)
        {
            int currentHandCount = CombatManager.Instance.hand.Count;
            if (currentHandCount != lastHandCount)
            {
                lastHandCount = currentHandCount;
                RefreshHandUI();
            }
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