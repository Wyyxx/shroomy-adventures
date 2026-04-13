using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopSlot : MonoBehaviour
{
    [Header("Referencias UI")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI priceText;
    public Image cardImage; // Opcional, para el arte
    public Button buyButton;

    private CardData cardData;
    private int price;
    private ShopManager shopManager;

    // Inicializador llamado por el Manager al instanciar
    public void Setup(CardData data, int itemPrice, ShopManager manager)
    {
        cardData = data;
        price = itemPrice;
        shopManager = manager;

        nameText.text = cardData.cardName;
        priceText.text = $"${price}";
        
        if (cardImage != null && cardData.artwork != null)
            cardImage.sprite = cardData.artwork;

        // Limpiar y asignar el evento del botón dinámicamente
        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(OnBuyClicked);
    }

    void OnBuyClicked()
    {
        // Delegar la validación de la transacción al Manager
        shopManager.TryBuyCard(cardData, price, this);
    }

    public void MarkAsSold()
    {
        buyButton.interactable = false;
        priceText.text = "AGOTADO";
        priceText.color = Color.red;
    }
}