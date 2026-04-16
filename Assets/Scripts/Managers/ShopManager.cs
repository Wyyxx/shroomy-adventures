using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class ShopManager : MonoBehaviour
{
    [Header("UI General")]
    public TextMeshProUGUI playerGoldText;
    public Transform shopContainer; // El objeto con Layout Group
    public GameObject shopSlotPrefab; // El prefab que contiene ShopSlot.cs

    [Header("Inventario")]
    // Lista temporal. En producción, el inventario se genera por RNG desde una base de datos global.
    public List<CardData> possibleCardsForSale; 
    public int itemsToGenerate = 3;
    public int basePrice = 50;

    void Start()
    {
        UpdateGoldUI();
        GenerateShop();
    }

    void UpdateGoldUI()
    {
        if (PlayerRunData.Instance != null)
            playerGoldText.text = $"Oro: {PlayerRunData.Instance.currentGold}";
    }

    void GenerateShop()
    {
        // EL CAMBIO CLAVE (El escudo protector):
        // Verificamos si la lista 'possibleCardsForSale' existe ANTES de intentar usarla.
        if (possibleCardsForSale == null || possibleCardsForSale.Count == 0 || shopSlotPrefab == null)
        {
            Debug.LogWarning("ShopManager: Faltan asignar referencias importantes en el Inspector (possibleCardsForSale o shopSlotPrefab). No se generará la tienda.");
            return; 
        }

        // Creamos una copia temporal para evitar duplicados
        List<CardData> availableCards = new List<CardData>(possibleCardsForSale);
        
        // Evitamos un error si pedimos generar más cartas de las que existen en la base de datos
        int actualItemsToGenerate = Mathf.Min(itemsToGenerate, availableCards.Count);

        for (int i = 0; i < actualItemsToGenerate; i++)
        {
            int randomIndex = Random.Range(0, availableCards.Count);
            CardData selectedCard = availableCards[randomIndex];
            
            // La retiramos de la lista temporal para que no vuelva a salir
            availableCards.RemoveAt(randomIndex); 
            
            int finalPrice = basePrice + Random.Range(-10, 11);

            GameObject newSlotObj = Instantiate(shopSlotPrefab, shopContainer);
            ShopSlot slotScript = newSlotObj.GetComponent<ShopSlot>();
            
            slotScript.Setup(selectedCard, finalPrice, this);
        }
    }

    // Punto central de transacciones. Validado por el esclavo (ShopSlot).
    public void TryBuyCard(CardData card, int price, ShopSlot slot)
    {
        if (PlayerRunData.Instance == null) return;

        if (PlayerRunData.Instance.currentGold >= price)
        {
            PlayerRunData.Instance.currentGold -= price;
            PlayerRunData.Instance.masterDeck.Add(card);
            
            Debug.Log($"[Tienda] Transacción aprobada: {card.cardName}. Oro: {PlayerRunData.Instance.currentGold}");
            
            UpdateGoldUI();
            slot.MarkAsSold();
        }
        else
        {
            Debug.LogWarning("[Tienda] Fondos insuficientes.");
            // Opcional: Ejecutar animación de UI roja en el texto de oro.
        }
    }

    public void LeaveShop()
    {
        if (MapManager.Instance != null)
            MapManager.Instance.ReturnToMap("ShopScene");
    }
}