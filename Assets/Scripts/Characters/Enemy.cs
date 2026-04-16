using UnityEngine;
using UnityEngine.UI; // Importante para el Slider
using TMPro; // Importante para el texto
public class Enemy : Character
{
    [Header("Enemy Info")]
    public string enemyName;
    public EnemyIntention currentIntention;

    [Header("UI Local del Enemigo")]
    public Slider localHealthBar;
    public TextMeshProUGUI localHealthText;
    public TextMeshProUGUI localNameText;

    [Header("Estados Alterados")]
    public int currentPoisonDamage;
    public int remainingPoisonTurns;

    void Start()
    {
        currentHealth = maxHealth;
        currentBlock = 0;
        PrepareNextIntention();

        if (localHealthBar != null)
        {
            localHealthBar.maxValue = maxHealth;
            localHealthBar.value = currentHealth;
        }

        if (localNameText != null)
            localNameText.text = enemyName;

        UpdateHealthUI();
    }

    public void PerformAction()
    {
        switch (currentIntention.type)
        {
            case IntentionType.Attack:
                Debug.Log($"{enemyName} ataca por {currentIntention.value}");
                Player.Instance.TakeDamage(currentIntention.value);
                break;

            case IntentionType.Defend:
                Debug.Log($"{enemyName} se defiende por {currentIntention.value}");
                GainBlock(currentIntention.value);
                break;
        }

        PrepareNextIntention();
    }

    protected virtual void PrepareNextIntention()
    {
        // Valor por defecto si olvidas programar un hijo
        currentIntention = new EnemyIntention { type = IntentionType.Attack, value = 5 };
    }

    protected override void OnHealthChanged()
    {
        // Esta función se dispara automáticamente cuando recibes daño o te curas (gracias a Character.cs)
        UpdateHealthUI();
        Debug.Log($"{enemyName} - HP: {currentHealth}/{maxHealth} | Block: {currentBlock}");
    }

    public override void TakeDamage(int damage)
    {
        base.TakeDamage(damage);

        // Trigger hit animation
        EnemySkin skin = GetComponent<EnemySkin>();
        if (skin != null)
        {
            skin.PlayHitEffect();
        }
    }

    void UpdateHealthUI()
    {
        if (localHealthBar != null)
            localHealthBar.value = currentHealth;

        if (localHealthText != null)
            localHealthText.text = $"{currentHealth}/{maxHealth}";
    }

    protected override void Die()
    {
        Debug.Log($"{enemyName} murio!");
        Destroy(gameObject);
    }

    public void ApplyPoison(int damage, int turns)
    {
        // El veneno se acumula (Stacks) como en Slay the Spire
        currentPoisonDamage += damage; 
        remainingPoisonTurns = Mathf.Max(remainingPoisonTurns, turns);
    }

    // Llamado por CombatManager AL INICIAR el turno enemigo, ANTES de PerformAction
    public void ProcessStartOfTurnEffects()
    {
        if (remainingPoisonTurns > 0)
        {
            Debug.Log($"{enemyName} sufre {currentPoisonDamage} por Veneno.");
            TakeDamage(currentPoisonDamage);
            remainingPoisonTurns--;

            if (remainingPoisonTurns <= 0) currentPoisonDamage = 0; // Se cura del veneno
        }
    }
}