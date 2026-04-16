using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Card : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public CardData cardData;

    [Header("UI Referencias")]
    public Image cardImage; // Ahora solo necesitamos la imagen principal

    // Variables para el arrastre
    private Vector3 originalPosition;
    private Transform originalParent;
    private CanvasGroup canvasGroup;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

        // Autoconexión por si olvidas asignarlo en el Inspector
        if (cardImage == null) cardImage = GetComponent<Image>();
    }

    public void InitializeCard(CardData data)
    {
        cardData = data;
        UpdateCardVisuals();
    }

    void UpdateCardVisuals()
    {
        if (cardData == null || cardData.artwork == null) return;

        // Inyectamos la carta completa (arte + textos ya horneados en el PNG)
        cardImage.sprite = cardData.artwork;

        // Forzamos el color a blanco puro para evitar que Unity oscurezca la imagen
        cardImage.color = Color.white; 
    }

    // --- LÓGICA DE ARRASTRE INTACTA ---

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (CombatManager.Instance.currentState != CombatManager.CombatState.PlayerTurn) return;
        
        originalPosition = transform.position;
        originalParent = transform.parent;

        transform.SetParent(transform.root); 
        canvasGroup.blocksRaycasts = false; 
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (CombatManager.Instance.currentState != CombatManager.CombatState.PlayerTurn) return;
        transform.position = Input.mousePosition; 
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (CombatManager.Instance.currentState != CombatManager.CombatState.PlayerTurn) return;

        canvasGroup.blocksRaycasts = true; 

        Vector2 mousePosWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePosWorld, Vector2.zero);

        Enemy targetEnemy = null;

        if (hit.collider != null)
        {
            targetEnemy = hit.collider.GetComponent<Enemy>();
        }

        bool playSuccess = CombatManager.Instance.TryPlayCard(this, targetEnemy);

        if (!playSuccess)
        {
            transform.SetParent(originalParent);
            transform.position = originalPosition;
        }
    }
}