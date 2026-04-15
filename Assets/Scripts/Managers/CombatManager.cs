using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

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
    
    public GameObject[] normalEnemies;
    public GameObject[] miniBosses;
    public GameObject[] bosses;

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
            enemy.PerformAction();
        }

        StartPlayerTurn();
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
            SceneManager.LoadScene("MapScene"); 
        }
    }

    void SpawnEnemies()
    {
        NodeType encounterType = NodeType.Battle; 
        if (PlayerRunData.Instance != null)
            encounterType = PlayerRunData.Instance.currentEncounterType;

        // 2. Decidimos cuántos enemigos van a aparecer
        int enemiesAmount = 1;

        switch (encounterType)
        {
            case NodeType.Battle:
                // Batalla normal: De 1 a 3 enemigos (Depende de cuántos puntos de spawn crees)
                enemiesAmount = Random.Range(1, Mathf.Min(4, enemySpawnPoints.Length + 1));
                break;
            case NodeType.MiniBoss:
                // Minijefe: 1 jefe y quizás 1 esbirro
                enemiesAmount = Random.Range(1, Mathf.Min(3, enemySpawnPoints.Length + 1));
                break;
            case NodeType.Boss:
                // El jefe suele estar solo al inicio
                enemiesAmount = 1; 
                break;
        }

        // 3. Spawneamos la cantidad decidida en los diferentes puntos
        for (int i = 0; i < enemiesAmount; i++)
        {
            GameObject prefabToSpawn = null;

            // Lógica para elegir quién aparece
            if (encounterType == NodeType.Boss)
            {
                prefabToSpawn = bosses[Random.Range(0, bosses.Length)];
            }
            else if (encounterType == NodeType.MiniBoss && i == 0)
            {
                // El primer enemigo es el minijefe, los demás son normales
                prefabToSpawn = miniBosses[Random.Range(0, miniBosses.Length)];
            }
            else
            {
                // Relleno de batallas normales o esbirros
                prefabToSpawn = normalEnemies[Random.Range(0, normalEnemies.Length)];
            }

            // Instanciamos usando el punto de Spawn correspondiente a este ciclo [i]
            if (prefabToSpawn != null && i < enemySpawnPoints.Length)
            {
                // Al añadir "enemySpawnPoints[i]" al final, le estamos diciendo a Unity
                // que coloque al enemigo DENTRO del objeto del SpawnPoint correspondiente.
                Instantiate(prefabToSpawn, enemySpawnPoints[i].position, Quaternion.identity, enemySpawnPoints[i]);
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

        // 2. Verificamos Objetivo (¡Muy importante!)
        // Si la carta hace daño y no hay enemigo (target == null), la jugada es inválida
        if (data.damageAmount > 0 && target == null)
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

        // Movemos la carta al descarte
        hand.Remove(data);
        discardPile.Add(data);
        
        // Destruimos el objeto visual de la carta porque ya se jugó
        Destroy(cardScript.gameObject);

        // Retornamos true para que la carta sepa que NO debe regresar a la mano
        return true; 
    }

    // APLICAR LOS EFECTOS
    void ApplyCardEffect(CardData card, Enemy target)
    {
        // Daño a un enemigo específico
        if (card.damageAmount > 0 && target != null)
        {
            target.TakeDamage(card.damageAmount);
            Debug.Log($"{card.cardName} hizo {card.damageAmount} daño a {target.name}!");
            
            CheckForVictory(); // Comprobamos si lo matamos
        }

        // Efectos globales (Jugador)
        if (card.blockAmount > 0)
        {
            Player.Instance.GainBlock(card.blockAmount);
        }

        if (card.drawAmount > 0)
        {
            DrawCards(card.drawAmount);
        }
    }

    // FASE 1: VICTORIA
    public void WinCombat()
    {
        currentState = CombatState.Victory;
        Debug.Log("<color=green>¡Victoria! Todos los enemigos derrotados.</color>");

        // Limpiamos las cartas de la pantalla para que no estorben
        UIManager.Instance.ClearHandVisuals();

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