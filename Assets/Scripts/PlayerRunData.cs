using UnityEngine;
using System.Collections.Generic;

public class PlayerRunData : MonoBehaviour
{
    public static PlayerRunData Instance;

    [Header("Estadísticas del Jugador")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("Mazo Global (Master Deck)")]
    // Este es el mazo con el que viajas. En combate sacarás una copia de aquí.

    [Header("Estado Actual de la Run")]
    public NodeType currentEncounterType;

    public List<CardData> masterDeck = new List<CardData>(); 

    void Awake()
    {
        // PATRÓN SINGLETON INMORTAL
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // ¡Esta línea lo hace indestructible entre escenas!
            
            // Inicializar la salud solo al arrancar el juego por primera vez
            currentHealth = maxHealth; 
        }
        else
        {
            Destroy(gameObject); // Evita duplicados si recargas la escena del mapa
        }
    }

    // Funciones útiles para el futuro (recompensas, curación en fogatas, etc)
    public void Heal(int amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;
        Debug.Log($"Te curaste. Vida actual: {currentHealth}/{maxHealth}");
    }

    public void AddCard(CardData newCard)
    {
        masterDeck.Add(newCard);
        Debug.Log($"Carta {newCard.name} añadida al mazo global.");
    }
}