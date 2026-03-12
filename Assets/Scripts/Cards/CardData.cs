using UnityEngine;

[CreateAssetMenu(fileName = "NewCard", menuName = "Card System/Card")]
public class CardData : ScriptableObject
{
    [Header("Información Básica")]
    public string cardName;
    [TextArea(3, 5)]
    public string description;
    public Sprite artwork;

    [Header("Costos y Tipo")]
    public int energyCost;
    public CardType cardType;
    public CardRarity rarity;

    [Header("Efectos")]
    public int damageAmount;
    public int blockAmount;
    public int drawAmount;
}