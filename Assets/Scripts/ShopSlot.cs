using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopSlot : MonoBehaviour
{
    [Header("Referencias UI")]
    public TextMeshProUGUI priceText;
    public Image cardImage; 
    public Button buyButton;

    private CardData cardData;
    private int price;
    private ShopManager shopManager;

    public void Setup(CardData data, int itemPrice, ShopManager manager)
    {
        cardData = data;
        price = itemPrice;
        shopManager = manager;

        priceText.text = $"${price} Oro";
        
        if (cardImage != null && cardData.artwork != null)
        {
            cardImage.sprite = cardData.artwork;
            cardImage.color = Color.white; // Evita que salga oscura
        }

        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(OnBuyClicked);
    }

    void OnBuyClicked()
    {
        shopManager.TryBuyCard(cardData, price, this);
    }

    public void MarkAsSold()
    {
        buyButton.interactable = false;
        priceText.text = "AGOTADO";
        priceText.color = Color.red;
        
        // Opcional: Oscurecer la carta visualmente al comprarla
        if (cardImage != null) cardImage.color = new Color(0.5f, 0.5f, 0.5f); 
    }
}