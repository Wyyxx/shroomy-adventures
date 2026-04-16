using UnityEngine;

public class Player : Character
{
    public static Player Instance;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    void Start()
    {
        // 1. CARGAR DATOS GLOBALES AL INICIAR EL COMBATE
        if (PlayerRunData.Instance != null)
        {
            maxHealth = PlayerRunData.Instance.maxHealth;
            currentHealth = PlayerRunData.Instance.currentHealth;
        }
        else
        {
            Debug.LogWarning("No se encontró PlayerRunData. Usando valores por defecto.");
            maxHealth = 100;
            currentHealth = maxHealth;
        }

        currentBlock = 0;
        
        OnHealthChanged();
    }

    protected override void OnHealthChanged()
    {
        // Tu código actual para la consola o UI
        Debug.Log($"Jugador - HP: {currentHealth}/{maxHealth} | Block: {currentBlock}");

        // 2. GUARDAR DATOS GLOBALES CADA VEZ QUE LA VIDA CAMBIA
        if (PlayerRunData.Instance != null)
        {
            PlayerRunData.Instance.currentHealth = this.currentHealth;
            PlayerRunData.Instance.maxHealth = this.maxHealth; 
        }
    }

    protected override void Die()
    {
        Debug.Log("¡Muriste! Te consumió el Cobalto.");

        // Delegamos el control al CombatManager para que cambie de Fase
        if (CombatManager.Instance != null)
        {
            CombatManager.Instance.LoseCombat();
        }
    }
}