using UnityEngine;

// Ya no declaramos los enums aquí para evitar el error CS0101.
// Unity usará la definición de CardType que ya tienes en otro script.

[CreateAssetMenu(fileName = "NewCard", menuName = "Card System/Card")]
public class CardData : ScriptableObject
{
    [Header("Información Básica")]
    public string cardName;
    [TextArea(3, 5)] public string description;
    public Sprite artwork;

    [Header("Costos y Tipo")]
    public int energyCost;
    public CardType cardType; 
    // Variable de rareza eliminada (Cancelación de RNG)

    [Header("Efectos Básicos")]
    public int damageAmount;
    public int blockAmount;
    public int drawAmount;

    [Header("Nuevos Efectos")]
    public bool isAoE; 
    [Range(0f, 1f)] public float lifestealPercentage; 
    public int poisonAmount; 
    public int poisonDuration; 
}