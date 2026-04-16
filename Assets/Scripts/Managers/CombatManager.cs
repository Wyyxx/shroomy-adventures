using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

[System.Serializable]
public class EncounterDef
{
    public string name; // Ej: "Pandilla del bosque"
    public GameObject[] enemiesToSpawn; // Arrastras [Araña, Araña, Ladrón]
}

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

    [Header("Referencias")]
    public List<CardData> deck = new List<CardData>();
    public List<CardData> hand = new List<CardData>();
    public List<CardData> discardPile = new List<CardData>();

    [Header("Generación de Enemigos")]

    public Transform[] enemySpawnPoints; 
    
    [Header("Formaciones Predefinidas")]
    public EncounterDef[] easyBattles;   // Nodos iniciales
    public EncounterDef[] mediumBattles; // Nodos intermedios
    public EncounterDef[] bossBattles;   // Nodos de Jefe

    private List<CardData> drawPile = new List<CardData>();

    public enum CombatState { Setup, PlayerTurn, EnemyTurn, Victory, Defeat }
    
    [Header("Estado Actual")]
    public CombatState currentState; // Reemplaza la lógica suelta de 'isPlayerTurn'

    void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    void Start()
    {
        InitializeDeck();
        SpawnEnemies();
        StartPlayerTurn();
    }

    void InitializeDeck()
    {
        // 1. Verificamos si existe nuestra Mochila Global
        if (PlayerRunData.Instance != null && PlayerRunData.Instance.masterDeck.Count > 0)
        {
            // Creamos una COPIA del mazo global para la pila de robar
            drawPile = new List<CardData>(PlayerRunData.Instance.masterDeck);
            Debug.Log($"Mazo cargado desde PlayerRunData con {drawPile.Count} cartas");
        }
        else
        {
            // Respaldo de emergencia (por si estás probando la escena de combate sola sin abrir el mapa)
            drawPile = new List<CardData>(deck); 
            Debug.LogWarning("Usando mazo de respaldo del CombatManager.");
        }

        ShuffleDeck(drawPile);
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
        if (currentState == CombatState.Defeat || currentState == CombatState.Victory) return;
        currentState = CombatState.PlayerTurn;
        currentEnergy = maxEnergy;
        playerActionsLeft = maxPlayerActions;
        Player.Instance.currentBlock = 0;
        
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

    public void EndPlayerTurn()
    {
        if (currentState != CombatState.PlayerTurn) return;

        currentState = CombatState.EnemyTurn;

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
        if (enemy == null) continue;

        // 1. Procesar veneno
        enemy.ProcessStartOfTurnEffects();

        // 2. IMPORTANTE: Verificar victoria inmediatamente después del veneno
        // Si el veneno mató al último enemigo, no queremos que el turno siga.
        CheckForVictory(); 
        if (currentState == CombatState.Victory) return;

        // 3. Si sigue vivo y no hemos ganado, el enemigo actúa
        if (enemy.currentHealth > 0)
        {
            enemy.PerformAction();
        }
    }

    // Solo si no hubo victoria tras las acciones de los enemigos, regresamos al jugador
    if (currentState != CombatState.Victory)
    {
        StartPlayerTurn();
    }
}

    public void CheckForVictory()
    {
        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        bool allDead = true;
        
        foreach (Enemy e in enemies)
        {
            if (e != null && e.currentHealth > 0) 
            {
                allDead = false;
                break; 
            }
        }

        if (allDead && currentState != CombatState.Victory)
        {
            WinCombat();
        }
    }

    void EndCombatAndReturnToMap()
    {
        // En lugar de LoadScene, le decimos al mapa que nos regrese
        if (MapManager.Instance != null)
        {
            MapManager.Instance.ReturnToMap("CombatScene");
        }
        else
        {
            Debug.LogError("No se encontró el MapManager. ¿Iniciaste el juego desde la escena del mapa?");
            // Respaldo de emergencia en caso de que pruebes la escena de combate directamente:
            SceneManager.LoadScene("Mapa"); 
        }
    }

    void SpawnEnemies()
    {
        NodeType encounterType = NodeType.Battle; 
        if (PlayerRunData.Instance != null) encounterType = PlayerRunData.Instance.currentEncounterType;

        EncounterDef selectedEncounter = null;

        // Elegimos el grupo basado en el tipo de nodo
        if (encounterType == NodeType.Boss)
        {
            selectedEncounter = bossBattles[Random.Range(0, bossBattles.Length)];
        }
        else if (encounterType == NodeType.MiniBoss)
        {
            selectedEncounter = mediumBattles[Random.Range(0, mediumBattles.Length)];
        }
        else 
        {
            // Opcional: Aquí en el futuro puedes leer en qué piso vas para dar 'easyBattles' o 'mediumBattles'
            selectedEncounter = easyBattles[Random.Range(0, easyBattles.Length)];
        }

        // Spawneamos físicamente al grupo seleccionado
        if (selectedEncounter != null)
        {
            Debug.Log($"Iniciando encuentro: {selectedEncounter.name}");
            
            for (int i = 0; i < selectedEncounter.enemiesToSpawn.Length; i++)
            {
                if (i < enemySpawnPoints.Length)
                {
                    Instantiate(selectedEncounter.enemiesToSpawn[i], enemySpawnPoints[i].position, Quaternion.identity, enemySpawnPoints[i]);
                }
            }
        }
    }

    public bool TryPlayCard(Card cardScript, Enemy target)
    {
        if (currentState != CombatState.PlayerTurn) return false; // Bloqueo estricto

        CardData data = cardScript.cardData;

        // 1. Verificamos Energía y Acciones
        if (currentEnergy < data.energyCost || playerActionsLeft <= 0)
        {
            Debug.Log("No tienes energía o acciones suficientes.");
            return false; 
        }

        // 2. Verificamos Objetivo
        // Si hace daño, no es de área (isAoE), y no tocaste a un enemigo, la jugada es inválida
        if (data.damageAmount > 0 && !data.isAoE && target == null)
        {
            Debug.Log("Esta carta requiere un objetivo. Arrástrala sobre un enemigo.");
            return false;
        }

        // --- SI LLEGAMOS AQUÍ, LA JUGADA ES VÁLIDA ---

        // Cobramos el costo
        currentEnergy -= data.energyCost;
        playerActionsLeft--;

        // Aplicamos el efecto
        ApplyCardEffect(data, target);

        // --- LÓGICA DE DESCARTE CORREGIDA ---
        // Buscamos de derecha a izquierda para borrar la instancia exacta que soltaste
        int exactIndex = hand.LastIndexOf(data);
        if (exactIndex != -1)
        {
            hand.RemoveAt(exactIndex);
        }
        
        discardPile.Add(data);
        
        // Destruimos el objeto visual de la carta
        Destroy(cardScript.gameObject);

        // Retornamos true para que la carta sepa que NO debe regresar a la mano
        return true; 
    }

    // APLICAR LOS EFECTOS
    void ApplyCardEffect(CardData card, Enemy target)
    {
        // 1. Daño (Individual o AoE)
        if (card.damageAmount > 0)
        {
            if (card.isAoE)
            {
                // Busca a todos los enemigos vivos y dales daño
                Enemy[] allEnemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
                foreach (Enemy e in allEnemies)
                {
                    if (e != null && e.currentHealth > 0)
                    {
                        ProcessDamageAndLifesteal(card, e);
                    }
                }
                Debug.Log($"{card.cardName} hizo Daño de Área.");
            }
            else if (target != null)
            {
                // Daño individual estándar
                ProcessDamageAndLifesteal(card, target);
            }
        }

        // 2. Efectos Globales
        if (card.blockAmount > 0) Player.Instance.GainBlock(card.blockAmount);
        if (card.drawAmount > 0) DrawCards(card.drawAmount);

        // 3. Veneno (Requiere nuevo componente en Enemy)
        if (card.poisonAmount > 0 && card.poisonDuration > 0 && target != null)
        {
            target.ApplyPoison(card.poisonAmount, card.poisonDuration);
            Debug.Log($"Envenenado por {card.poisonAmount} daño durante {card.poisonDuration} turnos.");
        }

        CheckForVictory(); 
    }

    // Función auxiliar para calcular Robo de Vida
    void ProcessDamageAndLifesteal(CardData card, Enemy e)
    {
        int actualDamageDealt = Mathf.Min(card.damageAmount, e.currentHealth + e.currentBlock); // No robar vida del overkill
        e.TakeDamage(card.damageAmount);

        if (card.lifestealPercentage > 0)
        {
            int healAmount = Mathf.FloorToInt(actualDamageDealt * card.lifestealPercentage);
            if (healAmount > 0)
            {
                Player.Instance.currentHealth = Mathf.Min(Player.Instance.currentHealth + healAmount, Player.Instance.maxHealth);
                Debug.Log($"Robaste {healAmount} de HP.");
            }
        }
    }

    // FASE 1: VICTORIA
    public void WinCombat()
    {
        currentState = CombatState.Victory;
        Debug.Log("<color=green>¡Victoria! Todos los enemigos derrotados.</color>");

        // Limpiamos las cartas de la pantalla para que no estorben
        //UIManager.Instance.ClearHandVisuals();

        // Calculamos la recompensa aleatoria
        int goldEarned = Random.Range(15, 31);

        // Mostramos la pantalla de recompensas
        if (RewardScreenManager.Instance != null)
        {
            RewardScreenManager.Instance.ShowRewards(goldEarned);
        }
        else
        {
            Debug.LogWarning("No hay RewardScreenManager. Regresando al mapa directamente.");
            EndCombatAndReturnToMap();
        }
    }

    // FASE 2: DERROTA
    public void LoseCombat()
    {
        if (currentState == CombatState.Defeat) return;
        
        currentState = CombatState.Defeat;
        Debug.Log("<color=red>El jugador ha muerto. Transición a pantalla de muerte.</color>");
        
        if (DeathScreenManager.Instance != null)
        {
            DeathScreenManager.Instance.ShowDeathScreen();
        }
    }
}