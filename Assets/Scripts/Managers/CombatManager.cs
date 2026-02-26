using UnityEngine;
using System.Collections.Generic;

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance;

    [Header("Configuración")]
    public int maxEnergy = 3;
    public int handSize = 5;
    public int maxPlayerActions = 3;

    [Header("Estado Actual")]
    public int currentEnergy;
    public int playerActionsLeft;
    public bool isPlayerTurn = false;

    [Header("Referencias")]
    public List<CardData> deck = new List<CardData>();
    public List<CardData> hand = new List<CardData>();
    public List<CardData> discardPile = new List<CardData>();
    
    // CAMBIO ARQUITECTÓNICO: Reemplazamos la lista de selectedCards por una única referencia al script físico.
    [Header("Estado de Selección")]
    public Card activeCard; 

    private List<CardData> drawPile = new List<CardData>();

    void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    void Start()
    {
        InitializeDeck();
        StartPlayerTurn();
    }

    void InitializeDeck()
    {
        drawPile = new List<CardData>(deck);
        ShuffleDeck(drawPile);
        Debug.Log($"Mazo inicializado con {drawPile.Count} cartas");
    }

    void ShuffleDeck(List<CardData> deckToShuffle)
    {
        for (int i = 0; i < deckToShuffle.Count; i++)
        {
            int randomIndex = Random.Range(i, deckToShuffle.Count);
            CardData temp = deckToShuffle[i];
            deckToShuffle[i] = deckToShuffle[randomIndex];
            deckToShuffle[randomIndex] = temp;
        }
    }

    public void StartPlayerTurn()
    {
        isPlayerTurn = true;
        currentEnergy = maxEnergy;
        playerActionsLeft = maxPlayerActions;
        Player.Instance.currentBlock = 0;
        
        // Limpiamos la carta activa en lugar de la lista
        activeCard = null; 
        
        DrawCards(handSize);
        Debug.Log($"--- Turno del Jugador | Acciones: {playerActionsLeft} ---");
    }

    public void DrawCards(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            if (drawPile.Count == 0)
            {
                if (discardPile.Count == 0) return;
                drawPile = new List<CardData>(discardPile);
                discardPile.Clear();
                ShuffleDeck(drawPile);
                Debug.Log("Mazo rebarajado!");
            }

            CardData card = drawPile[0];
            drawPile.RemoveAt(0);
            hand.Add(card);
            
            // NOTA DEL LEAD DEV: Aquí en el futuro deberás Instanciar el prefab de la carta
            // en el Canvas para que el jugador la vea físicamente en su mano.
        }
    }

    // NUEVA FUNCIÓN: Centraliza la lógica de clics. Reemplaza a SelectCard y DeselectCard.
    public void HandleCardClick(Card clickedCard)
    {
        // CASO 1: Clic en la carta que ya estaba activa. 
        // ¡AHORA ESTO SIGNIFICA JUGAR LA CARTA!
        if (activeCard == clickedCard)
        {
            ConfirmSelectedCard(); // Jugamos la carta, aplicamos efectos y la destruimos
            return;
        }

        // CASO 2: Clic en una carta diferente. Soltamos la anterior automáticamente.
        if (activeCard != null)
        {
            DeselectActiveCard();
        }

        // CASO 3: Intentar seleccionar la nueva carta
        if (currentEnergy >= clickedCard.cardData.energyCost)
        {
            activeCard = clickedCard;
            currentEnergy -= activeCard.cardData.energyCost;
            
            activeCard.SetVisualSelection(true); 
            Debug.Log($"Carta seleccionada: {activeCard.cardData.cardName}");
        }
        else
        {
            Debug.Log("No tienes suficiente energía!");
        }
    }

    // NUEVA FUNCIÓN: Limpia el estado de selección de manera segura y devuelve la energía.
    public void DeselectActiveCard()
    {
        if (activeCard != null)
        {
            currentEnergy += activeCard.cardData.energyCost; 
            activeCard.SetVisualSelection(false); 
            Debug.Log($"Carta deseleccionada: {activeCard.cardData.cardName}");
            
            activeCard = null; 
        }
    }

    public void ConfirmSelectedCard()
    {
        if (activeCard == null)
        {
            Debug.Log("No hay carta seleccionada!");
            return;
        }

        CardData dataToPlay = activeCard.cardData;

        // 1. Aplicamos los efectos de la carta
        ApplyCardEffect(dataToPlay);

        // 2. Sacamos la carta de la mano y va al descarte
        hand.Remove(dataToPlay);
        discardPile.Add(dataToPlay);
        
        // 3. Destruimos el objeto visual en la escena
        Destroy(activeCard.gameObject);
        activeCard = null;

        // 4. Restamos la acción
        playerActionsLeft--;
        Debug.Log($"Acción usada! Acciones restantes: {playerActionsLeft}");
    }

    void ApplyCardEffect(CardData card)
    {
        Enemy enemy = FindFirstObjectByType<Enemy>();

        if (card.damageAmount > 0 && enemy != null)
        {
            enemy.TakeDamage(card.damageAmount);
            Debug.Log($"{card.cardName}: {card.damageAmount} daño!");
        }

        if (card.blockAmount > 0)
        {
            Player.Instance.GainBlock(card.blockAmount);
            Debug.Log($"{card.cardName}: {card.blockAmount} bloqueo!");
        }

        if (card.drawAmount > 0)
        {
            DrawCards(card.drawAmount);
        }
    }

    public void EndPlayerTurn()
    {
        if (!isPlayerTurn) return;

        // Si el jugador le dio a "Terminar Turno" con una carta levantada, la soltamos
        DeselectActiveCard();

        isPlayerTurn = false;

        // Descartar mano
        discardPile.AddRange(hand);
        hand.Clear();

        // NOTA DEL LEAD DEV: Aquí en el futuro deberás destruir todos los GameObjects
        // de las cartas que sobraron en la mano física del jugador.

        Debug.Log("--- Turno del Enemigo ---");
        StartEnemyTurn();
    }

    void StartEnemyTurn()
    {
        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        foreach (Enemy enemy in enemies)
        {
            enemy.PerformAction();
        }

        StartPlayerTurn();
    }
}