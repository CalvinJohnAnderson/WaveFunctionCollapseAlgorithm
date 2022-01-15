using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

public class ExponentialWFC : SerializedMonoBehaviour
{
    [SerializeField] private MeshGenerator _meshGenerator;
    [SerializeField] private ErrorHandler _errorHandler;
    [SerializeField] private List<GameObject> tiles;
    [SerializeField] private List<VALUE> values;
    [SerializeField] private Dictionary<Vector3Int, List<VALUE>> finalValues;
    [SerializeField] private Transform parent;
    [SerializeField] private int AreaSize = 5;
    
    //private Node[,,] WFC_Area = new Node[5, 5, 5];//was 5, 5, 5
    private Node[,,] WFC_Area;
    
    private int maxIterations = 1000;//was 5 then 7
    public int currentIteration = 0;
    private bool isBuilding = false;
    private bool isCollapsing = false;
    private Vector3Int currentEvaluation;
    private Vector3Int startingPos;

    private bool isBuilt = false;
    private bool gracePeriod = false;
    private bool invokeRequired = true;
    private bool buildCurrentArea = false;
    private bool buildFinal = false;
    private bool calledFinalBuild = false;

    private int level = 0;

    private void OnEnable()
    {
        //Debug.LogWarning("Script has been enabled");
        WFC_Area = new Node[AreaSize, AreaSize, AreaSize];
        InitializeArea();
    }

    private void OnDisable()
    {
        //Debug.LogWarning("Script has been disabled");
    }

    private void Start()
    {
        Debug.Log("WFC: Initializing");
        //InitializeArea();
    }

    private void Update()
    {
        /*if (buildFinal)
        {
            buildFinal = false;
            foreach (Transform child in parent.transform)
            {
                Destroy(child.gameObject);
            }

            foreach (var node in WFC_Area)
            {
                if (node.PossibleTiles[0].tileValue != VALUE.AIR)
                {
                    GameObject newNode = Instantiate(node.PossibleTiles[0].tile, parent);
                    newNode.transform.position = node.nodePosition;
                }
            }
        }*/
        if (buildFinal)
        {
            //Debug.LogWarning("Building final");
            buildFinal = false;
            foreach (Transform child in parent.transform)
            {
                Destroy(child.gameObject);
            }
            for (int x = 0; x < WFC_Area.GetLength(0); x++)
            {
                for (int y = 0; y < WFC_Area.GetLength(1); y++)
                {
                    for (int z = 0; z < WFC_Area.GetLength(2); z++)
                    {
                        if (WFC_Area[x, y, z] == null)
                        {
                            //Debug.LogError("Null reference in array");
                        }
                        if (WFC_Area[x, y, z].PossibleTiles[0].tileValue != VALUE.AIR)
                        {
                            GameObject newTile = Instantiate(WFC_Area[x, y, z].PossibleTiles[0].tile, parent);
                            newTile.transform.position = new Vector3(x, y, z);
                        }
                        /*foreach (var tile in WFC_Area[x, y, z].PossibleTiles)
                        {
                            GameObject newTile = Instantiate(tile.tile);
                            newTile.transform.position = new Vector3(x, y, z);
                        }*/
                    } 
                } 
            }
            _meshGenerator.GenerateMesh(WFC_Area);
        }
        
        
        /*if (buildCurrentArea)
        {
            Debug.LogWarning("Building current");
            buildCurrentArea = false;
            foreach (Transform child in parent.transform)
            {
                Destroy(child.gameObject);
            }
            for (int x = 0; x < WFC_Area.GetLength(0); x++)
            {
                for (int y = 0; y < WFC_Area.GetLength(1); y++)
                {
                    for (int z = 0; z < WFC_Area.GetLength(2); z++)
                    {
                        if (WFC_Area[x, y, z].isCollapsed)
                        {
                            GameObject newTile = Instantiate(WFC_Area[x, y, z].PossibleTiles[0].tile, parent);
                            newTile.transform.position = new Vector3(x, y, z);
                        }
                        /*foreach (var tile in WFC_Area[x, y, z].PossibleTiles)
                        {
                            GameObject newTile = Instantiate(tile.tile);
                            newTile.transform.position = new Vector3(x, y, z);
                        }*/
                   /* } 
                } 
            }
            /*Debug.LogWarning("Building area");
            buildCurrentArea = false;
            foreach (Transform child in parent.transform)
            {
                Destroy(child.gameObject);
            }
            foreach (var node in WFC_Area)
            {
                if (node.isCollapsed)
                {
                    GameObject newNode = Instantiate(node.PossibleTiles[0].tile, parent);
                    newNode.transform.position = node.nodePosition;
                }
            }*/

            /*isBuilt = false;
            CollapseRandomNode();
        }*/
            else if (buildCurrentArea)
            {
                try
                {
                    //Debug.LogWarning("Building area");
                    buildCurrentArea = false;
                    foreach (Transform child in parent.transform)
                    {
                        Destroy(child.gameObject);
                    }
                    foreach (var node in WFC_Area)
                    {
                        if (node.isCollapsed)
                        {
                            GameObject newNode = Instantiate(node.PossibleTiles[0].tile, parent);
                            newNode.transform.position = node.nodePosition;
                        }
                    }

                    isBuilt = false;
                    CollapseRandomNode();
                }
                catch (Exception e)
                {
                    //Debug.LogError("Error in update: recalling script");
                    //_errorHandler.ResetScript();
                }
                isBuilt = false;
                //CollapseRandomNode();
            }
            
        
    }

    private void InitializeArea()
    {
        for (int x = 0; x < WFC_Area.GetLength(0); x++)
        {
            for (int y = 0; y < WFC_Area.GetLength(1); y++)
            {
                for (int z = 0; z < WFC_Area.GetLength(2); z++)
                {
                    //Debug.Log("adding tile");
                    WFC_Area[x, y, z] = new Node(tiles, values, new Vector3Int(x, y, z));
                }
            }
        }
        //TestArray();
        //currentEvaluation = new Vector3Int(0, 0, 1);
        isBuilt = false;
        CollapseRandomNode();
    }

    private void TestArray()
    {
        Debug.Log("Testing array at 1, 3, 2");
        foreach (var tile in WFC_Area[1, 3, 2].PossibleTiles)
        {
            Debug.Log(tile.tileValue);
        }
    }

    private Vector3Int CulculateEntropy()
    {
        int maxEntropy = 0;
        Vector3Int bestNode = new Vector3Int();
        foreach (var node in WFC_Area)
        {
            if (node.GetEntropy() > maxEntropy)
            {
                maxEntropy = node.GetEntropy();
                bestNode = node.nodePosition;
            }
        }
        //Debug.LogWarning("Max entropy = " + maxEntropy);
        return bestNode;
    }
    
    public void Reiterate()
    {
        ResetIterations();
    }

    private void CollapseRandomNode()
    {
        Debug.Log("WFC: Collapsing random node");
        /*Random rnd = new Random();
        int tempx = rnd.Next(0, WFC_Area.GetLength(0));//was 10
        int tempy = rnd.Next(0, WFC_Area.GetLength(1));
        int tempz = rnd.Next(0, WFC_Area.GetLength(2));*/
        startingPos = CulculateEntropy();
        //startingPos = new Vector3Int(tempx, tempy, tempz);
        Debug.Log($"x:{startingPos.x}, y:{startingPos.y}, z:{startingPos.z}");
        ///
        //tempx = 0;
        //tempy = 0;
        //tempz = 0;
        /// 

        if (!WFC_Area[startingPos.x, startingPos.y, startingPos.z].isCollapsed)
        {
            //Debug.LogWarning("WFC: Fully collapsing node");
            WFC_Area[startingPos.x, startingPos.y, startingPos.z].ValueChange = true;
            WFC_Area[startingPos.x, startingPos.y, startingPos.z].FullyCollapseNode();
        }
        currentEvaluation = new Vector3Int(startingPos.x, startingPos.y, startingPos.z);
        CollapseNodes(WFC_Area[currentEvaluation.x, currentEvaluation.y, currentEvaluation.z]);
        //CollapseNodes(WFC_Area[tempx, tempy, tempz], tempx, tempy, tempz);
    }
    
    private bool IsFullyCollapsed()
    {
        foreach (var node in WFC_Area)
        {
            if (!node.isCollapsed)
            {
                return false;
            }
        }

        return true;
    }
    private void ResetIterations()
    {
        Debug.Log("WFC: Resetting iterations");
        for (int x = 0; x < WFC_Area.GetLength(0); x++)
        {
            for (int y = 0; y < WFC_Area.GetLength(1); y++)
            {
                for (int z = 0; z < WFC_Area.GetLength(2); z++)
                {
                    if (!WFC_Area[x, y, z].isCollapsed)
                    {
                        WFC_Area[x, y, z].IterationComplete = false;
                    }
                }
            }
        }

        buildCurrentArea = true;
        //BuildCurrentNodes();
        //CollapseRandomNode();
    }

    private void BuildCurrentNodes()
    {
        foreach (Transform child in parent.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (var node in WFC_Area)
        {
            if (node.isCollapsed)
            {
                GameObject newNode = Instantiate(node.PossibleTiles[0].tile, parent);
                newNode.transform.position = node.nodePosition;
            }
        }

        isBuilt = false;
    }
    private void BuildWaveFuction()
    {
        Debug.LogWarning("Calling final build");
        buildFinal = true;
        /*foreach (Transform child in parent.transform)
        {
            Destroy(child.gameObject);
        }
        Debug.Log("Building Wave Function");
        for (int x = 0; x < WFC_Area.GetLength(0); x++)
        {
            for (int y = 0; y < WFC_Area.GetLength(1); y++)
            {
                for (int z = 0; z < WFC_Area.GetLength(2); z++)
                {
                    if (WFC_Area[x, y, z].PossibleTiles.Count > 1)
                    {
                        Debug.LogError("WFC: Not fully collapsed");
                    }
                    else
                    {
                        if (WFC_Area[x, y, z].PossibleTiles[0].tileValue != VALUE.AIR)
                        {
                            GameObject newTile = Instantiate(WFC_Area[x, y, z].PossibleTiles[0].tile, parent);
                            newTile.transform.position = new Vector3(x, y, z);
                        }
                    }
                    /*foreach (var tile in WFC_Area[x, y, z].PossibleTiles)
                    {
                        GameObject newTile = Instantiate(tile.tile);
                        newTile.transform.position = new Vector3(x, y, z);
                    }*/
        //}
        //}
        //}
    }

    private void CollapseNodes(Node newNode)
    {
        //Debug.LogWarning($"{newNode.nodePosition.x}, {newNode.nodePosition.y}, {newNode.nodePosition.z}");
        Dictionary<DIRECTIONKEY, Node> surroundingNodes = new Dictionary<DIRECTIONKEY, Node>();
        List<Node> incompleteNodes = new List<Node>();
        //surroundingNodes.Add(DIRECTIONKEY.KEY0, newNode);
        if (BoundryCheck(newNode.nodePosition.x - 1, newNode.nodePosition.y, newNode.nodePosition.z))
        {
            if (WFC_Area[(int) (newNode.nodePosition.x - 1), (int) newNode.nodePosition.y, (int) newNode.nodePosition.z].IterationComplete)
            {
                //Debug.Log($"Checking node at position: x{newNode.nodePosition.x -1}, y{newNode.nodePosition.y}, z{newNode.nodePosition.z}"); 
                if (WFC_Area[(int) (newNode.nodePosition.x - 1), (int) newNode.nodePosition.y,
                    (int) newNode.nodePosition.z].ValueChange)
                {
                    //Debug.Log($"Adding node at position: x{newNode.nodePosition.x -1}, y{newNode.nodePosition.y}, z{newNode.nodePosition.z}"); 
                    surroundingNodes.Add(DIRECTIONKEY.KEY1, WFC_Area[(int) (newNode.nodePosition.x - 1), (int) newNode.nodePosition.y, (int) newNode.nodePosition.z]);
                }
            }
            else 
            { 
                incompleteNodes.Add(WFC_Area[newNode.nodePosition.x - 1, newNode.nodePosition.y, newNode.nodePosition.z]);
            }
        } 
        if (BoundryCheck(newNode.nodePosition.x, newNode.nodePosition.y, newNode.nodePosition.z+1)) 
        { 
            if (WFC_Area[(int) newNode.nodePosition.x, (int) newNode.nodePosition.y, (int) (newNode.nodePosition.z+1)].IterationComplete) 
            {
                //Debug.Log($"Checking node at position: x{newNode.nodePosition.x}, y{newNode.nodePosition.y}, z{newNode.nodePosition.z + 1}"); 
                if (WFC_Area[(int) newNode.nodePosition.x, (int) newNode.nodePosition.y,
                    (int) (newNode.nodePosition.z + 1)].ValueChange)
                {
                    //Debug.Log($"Adding node at position: x{newNode.nodePosition.x}, y{newNode.nodePosition.y}, z{newNode.nodePosition.z + 1}"); 
                    surroundingNodes.Add(DIRECTIONKEY.KEY2, WFC_Area[(int) newNode.nodePosition.x, (int) newNode.nodePosition.y, (int) (newNode.nodePosition.z+1)]);
                }
            }
            else 
            { 
                incompleteNodes.Add(WFC_Area[newNode.nodePosition.x, newNode.nodePosition.y, newNode.nodePosition.z + 1]);
            }        
        } 
        if (BoundryCheck(newNode.nodePosition.x, newNode.nodePosition.y, newNode.nodePosition.z-1)) 
        { 
            if (WFC_Area[(int) newNode.nodePosition.x, (int) newNode.nodePosition.y, (int) (newNode.nodePosition.z-1)].IterationComplete) 
            {
                //Debug.Log($"Checking node at position: x{newNode.nodePosition.x}, y{newNode.nodePosition.y}, z{newNode.nodePosition.z - 1}"); 
                if (WFC_Area[(int) newNode.nodePosition.x, (int) newNode.nodePosition.y,
                    (int) (newNode.nodePosition.z - 1)].ValueChange)
                {
                    //Debug.Log($"Adding node at position: x{newNode.nodePosition.x}, y{newNode.nodePosition.y}, z{newNode.nodePosition.z - 1}"); 
                    surroundingNodes.Add(DIRECTIONKEY.KEY0, WFC_Area[newNode.nodePosition.x, newNode.nodePosition.y, (newNode.nodePosition.z -1)]);
                }
            }
            else 
            { 
                incompleteNodes.Add(WFC_Area[newNode.nodePosition.x, newNode.nodePosition.y, newNode.nodePosition.z -1]);
            }        
        } 
        if (BoundryCheck(newNode.nodePosition.x +1, newNode.nodePosition.y, newNode.nodePosition.z)) 
        { 
            if (WFC_Area[(int) (newNode.nodePosition.x + 1), (int) newNode.nodePosition.y, (int) newNode.nodePosition.z].IterationComplete) 
            {
                //Debug.Log($"Checking node at position: x{newNode.nodePosition.x + 1}, y{newNode.nodePosition.y}, z{newNode.nodePosition.z}"); 
                if (WFC_Area[(int) (newNode.nodePosition.x + 1), (int) newNode.nodePosition.y,
                    (int) newNode.nodePosition.z].ValueChange)
                {
                    //Debug.Log($"Adding node at position: x{newNode.nodePosition.x + 1}, y{newNode.nodePosition.y}, z{newNode.nodePosition.z}"); 
                    surroundingNodes.Add(DIRECTIONKEY.KEY3, WFC_Area[(int) (newNode.nodePosition.x+1), (int) newNode.nodePosition.y, (int) newNode.nodePosition.z]);
                }
            }
            else 
            { 
                incompleteNodes.Add(WFC_Area[newNode.nodePosition.x + 1, newNode.nodePosition.y, newNode.nodePosition.z]);
            }
                        
        } 
        if (BoundryCheck(newNode.nodePosition.x, newNode.nodePosition.y + 1, newNode.nodePosition.z)) 
        { 
            if (WFC_Area[(int) newNode.nodePosition.x, (int) (newNode.nodePosition.y+1), (int) newNode.nodePosition.z].IterationComplete) 
            {
                //Debug.Log($"Checking node at position: x{newNode.nodePosition.x}, y{newNode.nodePosition.y + 1}, z{newNode.nodePosition.z}"); 
                if (WFC_Area[(int) newNode.nodePosition.x, (int) (newNode.nodePosition.y+1), (int) newNode.nodePosition.z].ValueChange)
                {
                    //Debug.Log($"Adding node at position: x{newNode.nodePosition.x}, y{newNode.nodePosition.y + 1}, z{newNode.nodePosition.z}"); 
                    surroundingNodes.Add(DIRECTIONKEY.KEY4, WFC_Area[(int) newNode.nodePosition.x, (int) (newNode.nodePosition.y+1), (int) newNode.nodePosition.z]);
                }
            }
            else 
            { 
                incompleteNodes.Add(WFC_Area[newNode.nodePosition.x, newNode.nodePosition.y + 1, newNode.nodePosition.z]);
            }        
        } 
        if (BoundryCheck(newNode.nodePosition.x, newNode.nodePosition.y - 1, newNode.nodePosition.z)) 
        { 
            if (WFC_Area[(int) newNode.nodePosition.x, (int) (newNode.nodePosition.y-1), (int) newNode.nodePosition.z].IterationComplete) 
            {
                //Debug.Log($"Checking node at position: x{newNode.nodePosition.x}, y{newNode.nodePosition.y - 1}, z{newNode.nodePosition.z}"); 
                if (WFC_Area[(int) newNode.nodePosition.x, (int) (newNode.nodePosition.y-1), (int) newNode.nodePosition.z].ValueChange)
                {
                    //Debug.Log($"Adding node at position: x{newNode.nodePosition.x}, y{newNode.nodePosition.y - 1}, z{newNode.nodePosition.z}"); 
                    surroundingNodes.Add(DIRECTIONKEY.KEY5, WFC_Area[(int) newNode.nodePosition.x, (int) (newNode.nodePosition.y-1), (int) newNode.nodePosition.z]);
                }
            }
            else 
            { 
                incompleteNodes.Add(WFC_Area[newNode.nodePosition.x, newNode.nodePosition.y - 1, newNode.nodePosition.z]);
            }
        }
        WFC_Area[newNode.nodePosition.x, newNode.nodePosition.y, newNode.nodePosition.z].CollapseNode(surroundingNodes);
        /*if (new Vector3Int(newNode.nodePosition.x, newNode.nodePosition.y, newNode.nodePosition.z) == startingPos)
        {
            Debug.LogWarning("Starting Node");
            WFC_Area[newNode.nodePosition.x, newNode.nodePosition.y, newNode.nodePosition.z].CollapseNode(surroundingNodes);
        }
        else if (surroundingNodes.Count != 0)
        {
            Debug.Log("Surrounding nodes not null");
            WFC_Area[newNode.nodePosition.x, newNode.nodePosition.y, newNode.nodePosition.z].CollapseNode(surroundingNodes);
        }
        else
        {
            Debug.Log("no surrounding nodes");
        }*/
        

        if (incompleteNodes.Count == 0) 
        { 
            if (IsFullyCollapsed() && calledFinalBuild == false)
            {
                calledFinalBuild = true;
                //Debug.LogWarning("Area is fully collapsed");
                BuildWaveFuction();
            }
            else 
            {
                //Debug.LogWarning("Area not fully collapsed");
                if (!isBuilt && calledFinalBuild == false)
                {
                    isBuilt = true;
                    //Debug.LogError("Could not fully collapse this iteration");
                    finalValues = new Dictionary<Vector3Int, List<VALUE>>();
                    foreach (var node in WFC_Area)
                    {
                        List<VALUE> values = new List<VALUE>();
                        foreach (var tile in node.PossibleTiles)
                        {
                            values.Add(tile.tileValue);
                        }
                        finalValues.Add(node.nodePosition, values);
                    }
                    Reiterate();
                }
                
            }
        }
        else 
        { 
            foreach (var node in incompleteNodes.ToList())
            {
                //Debug.Log($"incomplete node at: x: {node.nodePosition.x}, y: {node.nodePosition.y}, z: {node.nodePosition.z}");
            }
            /*foreach (var node in incompleteNodes.ToList())
            {
                Debug.Log("Calling exponential collapse");
                CollapseNodes(node);
            }*/
            Parallel.ForEach(incompleteNodes, node =>
            {
                //Debug.Log("Calling exponential collapse");
                CollapseNodes(node);
            });
            
        }
    }
    
    private bool BoundryCheck(float x, float y, float z)
    {
        if (x >= 0 && y >= 0 && z >= 0)
        {
            if (x < WFC_Area.GetLength(0) && y < WFC_Area.GetLength(1) && z < WFC_Area.GetLength(2))
            {
                return true;
            }
        }
        Debug.Log($"WFC: Out of bounds - x = {x}, y = {y}, z = {z}");
        return false;
    }
}
