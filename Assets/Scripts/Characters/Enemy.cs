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

    void PrepareNextIntention()
    {
        // Por ahora siempre ataca por 10
        currentIntention = new EnemyIntention
        {
            type = IntentionType.Attack,
            value = 10
        };
    }

    protected override void OnHealthChanged()
    {
        // Esta función se dispara automáticamente cuando recibes daño o te curas (gracias a Character.cs)
        UpdateHealthUI();
        Debug.Log($"{enemyName} - HP: {currentHealth}/{maxHealth} | Block: {currentBlock}");
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
}