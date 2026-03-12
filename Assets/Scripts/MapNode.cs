using System.Collections.Generic;
using UnityEngine;
using System;

public enum NodeType
{
    Battle,     // Enemigo normal (Gris/Blanco)
    Healing,    // Curación (Azul)
    Shop,       // Tienda (Rosa)
    MiniBoss,   // Minijefe (Morado)
    Boss        // Jefe Final (Rojo)
}

public enum NodeState
{
    Locked,     // No interactuable (Gris)
    Attainable, // Disponible para ir (Blanco)
    Visited,    // Ya pasado (Gris Oscuro)
    Active      // Donde está el jugador (Verde)
}

public class MapNode : MonoBehaviour
{
    [Header("Configuración Visual")]
    public GameObject linePrefab; 

    [Header("Datos")]
    public string nodeID;
    public NodeType nodeType; 
    public NodeState currentState = NodeState.Locked;
    
    // Coordenadas en la grilla (Piso, Índice)
    public int gridX; 
    public int gridY;

    [Header("Conexiones")]
    public List<MapNode> outgoingNodes = new List<MapNode>(); // A quién puedo ir
    public List<MapNode> incomingNodes = new List<MapNode>(); // Quién viene a mí

    // EVENTO: El nodo avisa al Manager cuando es clicado
    public static event Action<MapNode> OnNodeClicked; 

    public void Init(int x, int y, string id)
    {
        gridX = x;
        gridY = y;
        nodeID = id;
        name = $"Node_{y}_{x}"; // Ejemplo: Node_0_1 (Piso 0, Nodo 1)
    }

    public void ChangeState(NodeState newState)
    {
        currentState = newState;
    }

    public void SetType(NodeType type)
    {
        nodeType = type;
        name += $"_{type}"; // Ayuda a verlos en la jerarquía
    }

    // --- DIBUJO DE LÍNEAS ---
    public void ShowConnections()
    {
        if (linePrefab == null) return;

        foreach (var neighbor in outgoingNodes)
        {
            // Instanciamos una línea por cada vecino
            GameObject lineObj = Instantiate(linePrefab, transform.position, Quaternion.identity, transform);
            LineRenderer lr = lineObj.GetComponent<LineRenderer>();
            
            // Dibujamos desde MÍ hasta el VECINO
            lr.SetPosition(0, transform.position);
            lr.SetPosition(1, neighbor.transform.position);
        }
    }

    // --- INTERACCIÓN ---
    public void ClickNode()
    {
        if (currentState == NodeState.Attainable)
        {
            OnNodeClicked?.Invoke(this);
        }
        else
        {
            Debug.Log($"Nodo {name} clicado, pero está {currentState}");
        }
    }

    public void ShowLineTo(MapNode target)
    {
        if (linePrefab == null) return;

        GameObject lineObj = Instantiate(linePrefab, transform.position, Quaternion.identity, transform);
        LineRenderer lr = lineObj.GetComponent<LineRenderer>();
        
        lr.SetPosition(0, transform.position);
        lr.SetPosition(1, target.transform.position);
    }
}