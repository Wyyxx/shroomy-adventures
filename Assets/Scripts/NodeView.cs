using UnityEngine;

[RequireComponent(typeof(MapNode))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(CircleCollider2D))] // Aseguramos el collider
public class NodeView : MonoBehaviour
{
    private MapNode mapNode;
    private SpriteRenderer sr;
    private Vector3 baseScale;

    [Header("Sprites por Tipo")]
    public Sprite spriteBattle;
    public Sprite spriteHealing;
    public Sprite spriteShop;
    public Sprite spriteMiniBoss;
    public Sprite spriteBoss;
    public Sprite spriteDefault; // Fallback si no hay sprite asignado

    [Header("Colores por Tipo")]
    public Color colorBattle = Color.white;
    public Color colorHealing = Color.blue;   // Curación
    public Color colorShop = new Color(1f, 0.4f, 0.7f); // Rosa
    public Color colorMiniBoss = new Color(0.6f, 0f, 1f); // Morado
    public Color colorBoss = Color.red;

    [Header("Paleta de Colores")]
    public Color colorLocked = new Color(0.5f, 0.5f, 0.5f);
    public Color colorAttainable = Color.white;
    public Color colorVisited = new Color(0.2f, 0.2f, 0.2f);
    public Color colorActive = Color.green;

    [Header("Animación")]
    public Color colorHover = Color.yellow;
    public float scaleHover = 1.2f;

    void Awake()
    {
        mapNode = GetComponent<MapNode>();
        sr = GetComponent<SpriteRenderer>();
        baseScale = transform.localScale; // Memoriza la escala del Prefab
    }

    public void RefreshVisuals()
    {
        transform.localScale = baseScale;

        // Asignar sprite según el tipo de nodo
        Sprite typeSprite = GetSpriteByType(mapNode.nodeType);
        if (typeSprite != null)
        {
            sr.sprite = typeSprite;
        }

        // 1. Si el jugador está aquí, es VERDE
        if (mapNode.currentState == NodeState.Active)
        {
            sr.color = colorActive;
            return;
        }
        // 2. Si ya lo visitamos, es GRIS (apagado)
        if (mapNode.currentState == NodeState.Visited)
        {
            sr.color = colorVisited;
            return;
        }
        // 3. Si está Lejos (Locked) o Disponible (Attainable), 
        // mostramos su color de TIPO para que el jugador sepa qué es.
        Color typeColor = GetColorByType(mapNode.nodeType);
        if (mapNode.currentState == NodeState.Locked)
        {
            // Mezclamos el color del nodo con Negro puro (0.6f = 60% oscuro)
            typeColor = Color.Lerp(typeColor, Color.black, 0.6f); 
            
            // Garantizamos que sea 100% sólido (nada de transparencia)
            typeColor.a = 1.0f; 
        }
        else // Attainable
        {
            typeColor.a = 1.0f; // Color brillante y sólido
        }
        sr.color = typeColor;
    }

    Sprite GetSpriteByType(NodeType type)
    {
        switch (type)
        {
            case NodeType.Battle: return spriteBattle;
            case NodeType.Healing: return spriteHealing;
            case NodeType.Shop: return spriteShop;
            case NodeType.MiniBoss: return spriteMiniBoss;
            case NodeType.Boss: return spriteBoss;
            default: return spriteDefault;
        }
    }

    Color GetColorByType(NodeType type)
    {
        switch (type)
        {
            case NodeType.Healing: return colorHealing;
            case NodeType.Shop: return colorShop;
            case NodeType.MiniBoss: return colorMiniBoss;
            case NodeType.Boss: return colorBoss;
            default: return colorBattle;
        }
    }

    // --- MOUSE ---
    void OnMouseEnter()
    {
        if (mapNode.currentState == NodeState.Attainable)
        {
            sr.color = colorHover;
            transform.localScale = baseScale * scaleHover;

            // Cambiar cursor a hover
            if (CursorManager.Instance != null)
                CursorManager.Instance.SetHover();
        }
    }

    void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(1)) 
        {
            MapManager.Instance.HandleRightClick(mapNode);
        }
    }

    void OnMouseExit()
    {
        RefreshVisuals();

        // Restaurar cursor normal
        if (CursorManager.Instance != null)
            CursorManager.Instance.SetNormal();
    }
    void OnMouseDown() => mapNode.ClickNode();
}
