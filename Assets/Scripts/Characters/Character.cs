using UnityEngine;

public abstract class Character : MonoBehaviour
{
    [Header("Stats")]
    public int currentHealth;
    public int maxHealth;
    public int currentBlock;

    public virtual void TakeDamage(int damage)
    {
        int actualDamage = damage;

        if (currentBlock > 0)
        {
            if (currentBlock >= actualDamage)
            {
                currentBlock -= actualDamage;
                actualDamage = 0;
            }
            else
            {
                actualDamage -= currentBlock;
                currentBlock = 0;
            }
        }

        currentHealth -= actualDamage;
        currentHealth = Mathf.Max(currentHealth, 0);

        OnHealthChanged();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public virtual void GainBlock(int amount)
    {
        currentBlock += amount;
        OnHealthChanged();
    }

    public virtual void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        OnHealthChanged();
    }

    protected virtual void OnHealthChanged() { }
    protected abstract void Die();
}