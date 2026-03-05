using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class MapManager : MonoBehaviour
{
    [Header("Configuración de Mapa")]
    public static MapManager Instance;
    public GameObject nodePrefab; 
    public Transform mapParent;
    
    [Range(3, 10)] public int totalFloors = 8; // Cuántos pisos de profundidad
    [Range(2, 4)] public int nodesPerFloorMin = 2;
    [Range(2, 5)] public int nodesPerFloorMax = 4;

    [Header("Espaciado")]
    public float xSpacing = 2.0f; // Distancia horizontal entre nodos
    public float ySpacing = 2.5f; // Distancia vertical entre pisos

    [Header("Estado del Juego")]
    public MapNode currentNode;
    
    // Lista de Listas: Piso 0 -> [NodoA], Piso 1 -> [NodoB, NodoC], etc.
    private List<List<MapNode>> mapStructure = new List<List<MapNode>>();

    [Header("Editor de Conexiones")]
    public MapNode pendingConnectionNode; // Aquí guardamos el primer nodo seleccionado

    [Header("Transición de Escenas")]
    public Canvas mapCanvas; 
    public Camera mapCamera; 
    public GameObject mapEventSystem;

    void Awake()
    {
        Instance = this;
    }

    private void OnEnable() => MapNode.OnNodeClicked += OnNodeSelected;
    private void OnDisable() => MapNode.OnNodeClicked -= OnNodeSelected;

    void Start()
    {
        GenerateProceduralMap();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            RegenerateMap();
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            ClearMap();
        }
    }
    // --- GENERACIÓN DEL MAPA ---
    void GenerateProceduralMap()
    {
        for (int floor = 0; floor < totalFloors; floor++)
        {
            List<MapNode> currentFloorNodes = new List<MapNode>();
            
            int nodesCount = (floor == 0 || floor == totalFloors - 1) ? 1 : UnityEngine.Random.Range(nodesPerFloorMin, nodesPerFloorMax + 1);
            float floorWidth = (nodesCount - 1) * xSpacing;

            for (int i = 0; i < nodesCount; i++)
            {
                float x = (-floorWidth / 2) + (i * xSpacing);
                float y = floor * ySpacing;
                if (floor > 0 && floor < totalFloors - 1) x += UnityEngine.Random.Range(-0.3f, 0.3f);

                MapNode newNode = CreateNode(i, floor, new Vector3(x, y-5, 0));
                
                // --- ASIGNACIÓN DE TIPO ---
                AssignNodeType(newNode, floor);
                // -------------------------

                currentFloorNodes.Add(newNode);
            }
            mapStructure.Add(currentFloorNodes);
        }

        ConnectFloors();

        // Inicialización Visual
        foreach (var list in mapStructure)
        {
            foreach (var node in list)
            {
                node.ShowConnections();
                // Forzamos update visual para que se pinten los colores correctos al inicio
                node.GetComponent<NodeView>().RefreshVisuals();
            }
        }

        SetCurrentNode(mapStructure[0][0]);
    }

    void AssignNodeType(MapNode node, int floor)
    {
        // Regla 1: El primer piso siempre es Batalla fácil
        if (floor == 0)
        {
            node.SetType(NodeType.Battle);
            return;
        }

        // Regla 2: El último piso es JEFE
        if (floor == totalFloors - 1)
        {
            node.SetType(NodeType.Boss);
            return;
        }
        // Regla 3: A mitad del mapa, forzamos un Minijefe o Tienda para garantizar variedad
        // (Por ejemplo, en el piso 4 o 5)
        if (floor == totalFloors / 2)
        {
             // 50% Tienda, 50% Minijefe
             node.SetType(UnityEngine.Random.value > 0.5f ? NodeType.Shop : NodeType.MiniBoss);
             return;
        }

        // Regla 4: Aleatoriedad ponderada para el resto
        float randomVal = UnityEngine.Random.value; // Retorna entre 0.0 y 1.0

        if (randomVal < 0.05f) 
            node.SetType(NodeType.Healing);
        else if (randomVal < 0.10f) 
            node.SetType(NodeType.Shop);
        else if (randomVal < 0.15f) 
            node.SetType(NodeType.MiniBoss);
        else 
            node.SetType(NodeType.Battle);
    }

    MapNode CreateNode(int xIndex, int yIndex, Vector3 pos)
    {
        GameObject obj = Instantiate(nodePrefab, pos, Quaternion.identity, mapParent);
        MapNode node = obj.GetComponent<MapNode>();
        node.Init(xIndex, yIndex, System.Guid.NewGuid().ToString().Substring(0, 5));
        return node;
    }

    void ConnectFloors()
    {
        for (int i = 0; i < mapStructure.Count - 1; i++)
        {
            var currentFloor = mapStructure[i];
            var nextFloor = mapStructure[i + 1];

            // Paso A: Cada nodo actual debe tener AL MENOS 1 salida
            foreach (var node in currentFloor)
            {
                var target = GetRandomNode(nextFloor);
                ConnectNodes(node, target);
            }

            // Paso B: Cada nodo del siguiente piso debe tener AL MENOS 1 entrada
            // (Evitamos islas a las que no se puede llegar)
            foreach (var nextNode in nextFloor)
            {
                if (nextNode.incomingNodes.Count == 0)
                {
                    var parent = GetRandomNode(currentFloor);
                    ConnectNodes(parent, nextNode);
                }
            }
        }
    }

    MapNode GetRandomNode(List<MapNode> list) => list[UnityEngine.Random.Range(0, list.Count)];

    void ConnectNodes(MapNode from, MapNode to)
    {
        // Evitamos duplicados
        if (!from.outgoingNodes.Contains(to))
        {
            from.outgoingNodes.Add(to);
            to.incomingNodes.Add(from);
        }
    }

    // --- LÓGICA DE JUEGO (MOVIMIENTO) ---

    void OnNodeSelected(MapNode selectedNode)
    {
        // 1. Movemos al jugador en el mapa
        SetCurrentNode(selectedNode);

        // 2. Dependiendo del tipo de nodo, cargamos la escena correspondiente
        if (selectedNode.nodeType == NodeType.Battle || 
            selectedNode.nodeType == NodeType.MiniBoss || 
            selectedNode.nodeType == NodeType.Boss)
        {
            Debug.Log($"Cargando CombatScene por nodo tipo: {selectedNode.nodeType}");
            GoToCombat();
        }
        else
        {
            // Si es curación o tienda, tal vez quieras cargar otra escena o hacer un efecto aquí mismo
            Debug.Log($"Llegaste a un nodo pacífico: {selectedNode.nodeType}");
            // SceneManager.LoadScene("ShopScene"); // Ejemplo
        }
    }

    void SetCurrentNode(MapNode newNode)
    {
        // 1. Nodo anterior pasa a Visited
        if (currentNode != null)
        {
            currentNode.ChangeState(NodeState.Visited);
            currentNode.GetComponent<NodeView>().RefreshVisuals(); // Actualizar color
        }

        // 2. Nuevo nodo pasa a Active
        currentNode = newNode;
        currentNode.ChangeState(NodeState.Active);
        currentNode.GetComponent<NodeView>().RefreshVisuals();

        // 3. Bloquear todo el mapa para limpiar
        foreach(var list in mapStructure)
        {
            foreach(var node in list)
            {
                if (node.currentState != NodeState.Visited && node != currentNode)
                {
                    node.ChangeState(NodeState.Locked);
                    node.GetComponent<NodeView>().RefreshVisuals();
                }
            }
        }

        // 4. Desbloquear solo vecinos
        foreach(var neighbor in currentNode.outgoingNodes)
        {
            neighbor.ChangeState(NodeState.Attainable);
            neighbor.GetComponent<NodeView>().RefreshVisuals();
        }
    }

    public void RegenerateMap()
    {
        Console.WriteLine("Regenerando mapa...");
        ClearMap();             // 1. Borrar lo viejo
        GenerateProceduralMap(); // 2. Crear lo nuevo
    }

    // Función de limpieza
    void ClearMap()
    {
        // Recorremos nuestra lista de listas para destruir los objetos físicos
        foreach (var floor in mapStructure)
        {
            foreach (var node in floor)
            {
                if (node != null)
                {
                    // Al destruir el nodo, se destruyen también las líneas (porque son hijas)
                    Destroy(node.gameObject); 
                }
            }
        }

        // Limpiamos la lista lógica y reseteamos variables
        mapStructure.Clear();
        currentNode = null;
    }

    public void HandleRightClick(MapNode targetNode)
    {
        // 1. Seguridad: Si el juego no ha empezado o es el mismo nodo, no hacemos nada
        if (currentNode == null || targetNode == currentNode) return;

        // 2. Evitar duplicados: Si ya existe la conexión, nos salimos
        if (currentNode.outgoingNodes.Contains(targetNode))
        {
            Debug.Log("¡Ya existe una conexión hacia ese nodo!");
            return;
        }

        // 3. Conexión Lógica (Datos)
        ConnectNodes(currentNode, targetNode);

        // 4. Conexión Visual (Línea)
        currentNode.ShowLineTo(targetNode); 

        // 5. ACTUALIZACIÓN DE ESTADO (El arreglo del bug)
        if (targetNode.currentState != NodeState.Visited)
        {
            targetNode.ChangeState(NodeState.Attainable);
            
            // Forzamos al NodeView a repintarse de blanco/amarillo
            targetNode.GetComponent<NodeView>().RefreshVisuals();
        }

        Debug.Log($"<color=cyan>Puente creado: {currentNode.name} -> {targetNode.name}</color>");
    }
    public void GoToCombat()
    {
        Debug.Log("Ocultando mapa y cargando combate...");
        
        // 1. Apagamos todo
        if (mapParent != null) mapParent.gameObject.SetActive(false);
        if (mapCanvas != null) mapCanvas.gameObject.SetActive(false);
        if (mapCamera != null) mapCamera.gameObject.SetActive(false);
        if (mapEventSystem != null) mapEventSystem.SetActive(false); // Apagamos el conflicto de clics

        // 2. Cargamos combate
        SceneManager.LoadScene("CombatScene", LoadSceneMode.Additive);
    }

    public void ReturnFromCombat()
    {
        Debug.Log("Combate terminado, restaurando mapa...");

        // 1. Descargamos combate
        SceneManager.UnloadSceneAsync("CombatScene");

        // 2. Encendemos todo
        if (mapParent != null) mapParent.gameObject.SetActive(true);
        if (mapCanvas != null) mapCanvas.gameObject.SetActive(true);
        if (mapCamera != null) mapCamera.gameObject.SetActive(true);
        if (mapEventSystem != null) mapEventSystem.SetActive(true); 
    }
}