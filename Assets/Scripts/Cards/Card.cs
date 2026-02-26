using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems; // NUEVO: Necesario para detectar clics avanzados (izquierdo y derecho)

// CAMBIO ARQUITECTÓNICO: Agregamos IPointerClickHandler a la definición de la clase
public class Card : MonoBehaviour, IPointerClickHandler 
{
    public CardData cardData;

    [Header("UI Referencias")]
    public Image cardBackground;
    public Image artworkImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI costText;

    private Vector3 originalPosition;

    // ELIMINADO: Ya no necesitamos la variable 'Button button' ni el Awake() que lo configuraba.

    public void InitializeCard(CardData data)
    {
        cardData = data;
        UpdateCardVisuals();
    }

    // NUEVA FUNCIÓN: Reemplaza a 'OnCardClicked'. Intercepta cualquier clic del mouse sobre la carta.
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!CombatManager.Instance.isPlayerTurn) return;

        // Si hacemos Clic Izquierdo (Seleccionar / Jugar)
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            CombatManager.Instance.HandleCardClick(this);
        }
        // Si hacemos Clic Derecho (Cancelar / Deseleccionar)
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            // Solo deseleccionamos si esta es la carta que está actualmente activa
            if (CombatManager.Instance.activeCard == this)
            {
                CombatManager.Instance.DeselectActiveCard();
            }
        }
    }

    public void SetVisualSelection(bool isSelectedByManager)
    {
        if (isSelectedByManager)
        {
            // Guardamos la posición original justo antes de levantarla
            originalPosition = transform.localPosition;

            transform.localPosition = new Vector3(
                originalPosition.x,
                originalPosition.y + 40f,
                originalPosition.z
            );

            cardBackground.color = new Color(
                Mathf.Min(cardBackground.color.r + 0.3f, 1f),
                Mathf.Min(cardBackground.color.g + 0.3f, 1f),
                cardBackground.color.b
            );
        }
        else
        {
            // La devolvemos a su lugar
            transform.localPosition = originalPosition;
            UpdateCardVisuals(); // Esto restaura los colores originales
        }
    }

    void UpdateCardVisuals()
    {
        if (cardData == null) return;

        nameText.text = cardData.cardName;
        descriptionText.text = cardData.description;
        costText.text = cardData.energyCost.ToString();

        if (cardData.artwork != null)
            artworkImage.sprite = cardData.artwork;

        switch (cardData.cardType)
        {
            case CardType.Attack:
                cardBackground.color = new Color(0.8f, 0.2f, 0.2f);
                break;
            case CardType.Skill:
                cardBackground.color = new Color(0.2f, 0.6f, 0.8f);
                break;
            case CardType.Power:
                cardBackground.color = new Color(0.3f, 0.8f, 0.3f);
                break;
        }
    }
}