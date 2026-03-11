using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems; // Necesario para el Drag & Drop

public class Card : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public CardData cardData;

    [Header("UI Referencias")]
    public Image cardBackground;
    public Image artworkImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI costText;

    // Variables para el arrastre
    private Vector3 originalPosition;
    private Transform originalParent;
    private CanvasGroup canvasGroup;

    void Awake()
    {
        // Nos aseguramos de tener el CanvasGroup para que el mouse pueda atravesar la carta al soltarla
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void InitializeCard(CardData data)
    {
        cardData = data;
        UpdateCardVisuals();
    }

    // --- LÓGICA VISUAL (Tu código original intacto) ---
    void UpdateCardVisuals()
    {
        if (cardData == null) return;

        nameText.text = cardData.cardName;
        descriptionText.text = cardData.description;
        costText.text = cardData.energyCost.ToString();

        if (cardData.artwork != null)
            artworkImage.sprite = cardData.artwork;

        // Pintar según el tipo
        switch (cardData.cardType)
        {
            case CardType.Attack:
                cardBackground.color = new Color(0.8f, 0.2f, 0.2f); // Rojo
                break;
            case CardType.Skill:
                cardBackground.color = new Color(0.2f, 0.6f, 0.8f); // Azul
                break;
            case CardType.Power:
                cardBackground.color = new Color(0.3f, 0.8f, 0.3f); // Verde
                break;
        }
    }

    // --- NUEVA LÓGICA DE ARRASTRE (DRAG & DROP) ---

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!CombatManager.Instance.isPlayerTurn) return;

        // 1. Guardamos de dónde salió para regresarla si fallamos
        originalPosition = transform.position;
        originalParent = transform.parent;

        // 2. La sacamos del contenedor para que flote libre sobre toda la pantalla
        transform.SetParent(transform.root); 
        
        // 3. Apagamos los bloqueos de rayos. 
        // Así el mouse "atraviesa" la carta y puede detectar al enemigo que está detrás.
        canvasGroup.blocksRaycasts = false; 
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!CombatManager.Instance.isPlayerTurn) return;

        // La carta sigue al mouse
        transform.position = Input.mousePosition; 
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!CombatManager.Instance.isPlayerTurn) return;

        // 1. Volvemos a hacer la carta sólida para futuros clics
        canvasGroup.blocksRaycasts = true; 

        // 2. Lanzamos un rayo invisible desde la cámara hacia el mundo 2D
        Vector2 mousePosWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePosWorld, Vector2.zero);

        Enemy targetEnemy = null;

        // 3. Verificamos si tocamos a un enemigo
        if (hit.collider != null)
        {
            targetEnemy = hit.collider.GetComponent<Enemy>();
        }

        // 4. Le pedimos al Manager que intente jugar la carta
        bool playSuccess = CombatManager.Instance.TryPlayCard(this, targetEnemy);

        // 5. Si no se pudo jugar (no hay energía, o requería un enemigo y soltaste en la pared)
        if (!playSuccess)
        {
            // Regresamos la carta a su posición original
            transform.SetParent(originalParent);
            transform.position = originalPosition;
        }
    }
}