using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
using Unity.Jobs;
using UnityEngine;

public class WFCJobSystem : SerializedMonoBehaviour
{
    [SerializeField] private List<GameObject> tiles;
    [SerializeField] private List<VALUE> values;
    [SerializeField] private Dictionary<Vector3Int, List<VALUE>> finalValues;
    [SerializeField] private Transform parent;
    
    private Node[,,] WFC_Area = new Node[5, 5, 5];

    private int maxIterations = 1000;//was 5 then 7
    public int currentIteration = 0;
    private bool isBuilding = false;
    private bool isCollapsing = false;
    private Vector3Int currentEvaluation;
    private Vector3Int startingPos;

    private bool isBuilt = false;

    private int level = 0;

    private void Start()
    {
        Debug.Log("WFC: Initializing");
        InitializeArea();
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
        Debug.LogWarning("Max entropy = " + maxEntropy);
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
            Debug.Log("WFC: Fully collapsing node");
            WFC_Area[startingPos.x, startingPos.y, startingPos.z].FullyCollapseNode();
        }
        currentEvaluation = new Vector3Int(startingPos.x, startingPos.y, startingPos.z);
        //CollapseNodes(WFC_Area[currentEvaluation.x, currentEvaluation.y, currentEvaluation.z]);
        JobHandle jobHandle =
            CallCollapseNodeJob(WFC_Area[currentEvaluation.x, currentEvaluation.y, currentEvaluation.z]);
        jobHandle.Complete();
        Debug.LogWarning("Job has been completed");
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

        BuildCurrentNodes();
        CollapseRandomNode();
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
    }
    private void BuildWaveFuction()
    {
        foreach (Transform child in parent.transform)
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
                }
            }
        }
    }

    private void CollapseNodes(Node newNode)
    {
        Debug.Log($"{newNode.nodePosition.x}, {newNode.nodePosition.y}, {newNode.nodePosition.z}");
        Dictionary<DIRECTIONKEY, Node> surroundingNodes = new Dictionary<DIRECTIONKEY, Node>();
        List<Node> incompleteNodes = new List<Node>();
        //surroundingNodes.Add(DIRECTIONKEY.KEY0, newNode);
        if (BoundryCheck(newNode.nodePosition.x - 1, newNode.nodePosition.y, newNode.nodePosition.z))
        {
            if (WFC_Area[(int) (newNode.nodePosition.x - 1), (int) newNode.nodePosition.y, (int) newNode.nodePosition.z].IterationComplete)
            { 
                Debug.Log($"Adding node at position: x{newNode.nodePosition.x -1}, y{newNode.nodePosition.y}, z{newNode.nodePosition.z}"); 
                surroundingNodes.Add(DIRECTIONKEY.KEY1, WFC_Area[(int) (newNode.nodePosition.x - 1), (int) newNode.nodePosition.y, (int) newNode.nodePosition.z]);
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
                Debug.Log($"Adding node at position: x{newNode.nodePosition.x}, y{newNode.nodePosition.y}, z{newNode.nodePosition.z + 1}"); 
                surroundingNodes.Add(DIRECTIONKEY.KEY2, WFC_Area[(int) newNode.nodePosition.x, (int) newNode.nodePosition.y, (int) (newNode.nodePosition.z+1)]);
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
                Debug.Log($"Adding node at position: x{newNode.nodePosition.x}, y{newNode.nodePosition.y}, z{newNode.nodePosition.z - 1}"); 
                surroundingNodes.Add(DIRECTIONKEY.KEY0, WFC_Area[newNode.nodePosition.x, newNode.nodePosition.y, (newNode.nodePosition.z -1)]);
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
                Debug.Log($"Adding node at position: x{newNode.nodePosition.x + 1}, y{newNode.nodePosition.y}, z{newNode.nodePosition.z}"); 
                surroundingNodes.Add(DIRECTIONKEY.KEY3, WFC_Area[(int) (newNode.nodePosition.x+1), (int) newNode.nodePosition.y, (int) newNode.nodePosition.z]);
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
                Debug.Log($"Adding node at position: x{newNode.nodePosition.x}, y{newNode.nodePosition.y + 1}, z{newNode.nodePosition.z}"); 
                surroundingNodes.Add(DIRECTIONKEY.KEY4, WFC_Area[(int) newNode.nodePosition.x, (int) (newNode.nodePosition.y+1), (int) newNode.nodePosition.z]);
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
                Debug.Log($"Adding node at position: x{newNode.nodePosition.x}, y{newNode.nodePosition.y - 1}, z{newNode.nodePosition.z}"); 
                surroundingNodes.Add(DIRECTIONKEY.KEY5, WFC_Area[(int) newNode.nodePosition.x, (int) (newNode.nodePosition.y-1), (int) newNode.nodePosition.z]);
                
            }
            else 
            { 
                incompleteNodes.Add(WFC_Area[newNode.nodePosition.x, newNode.nodePosition.y - 1, newNode.nodePosition.z]);
            }
        } 
        WFC_Area[newNode.nodePosition.x, newNode.nodePosition.y, newNode.nodePosition.z].CollapseNode(surroundingNodes); 
        if (incompleteNodes.Count == 0) 
        { 
            if (IsFullyCollapsed()) 
            { 
                Debug.LogWarning("Area is fully collapsed");
                BuildWaveFuction();
            }
            else 
            {
                if (!isBuilt)
                {
                    isBuilt = true;
                    Debug.LogError("Could not fully collapse this iteration");
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
                }
                
            }
        }
        else 
        { 
            foreach (var node in incompleteNodes.ToList())
            {
                Debug.Log($"incomplete node at: x: {node.nodePosition.x}, y: {node.nodePosition.y}, z: {node.nodePosition.z}");
            }
            /*foreach (var node in incompleteNodes.ToList())
            {
                Debug.Log("Calling exponential collapse");
                CollapseNodes(node);
            }*/
            Parallel.ForEach(incompleteNodes, node =>
            {
                Debug.Log("Calling exponential collapse");
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

    private JobHandle CallCollapseNodeJob(Node startingNode)
    {
        CollapseNodeJob job = new CollapseNodeJob(WFC_Area, startingNode);
        return job.Schedule();
    }
}

public struct CollapseNodeJob : IJob
{
    //private Dictionary<Vector3Int, List<VALUE>> finalValues;
    public Node[,,] WFC_Area;
    private bool isBuilt;
    private Node startingNode;

    public CollapseNodeJob(Node[,,] copyArray, Node baseNode)
    {
        WFC_Area = copyArray;
        isBuilt = false;
        //finalValues = new Dictionary<Vector3Int, List<VALUE>>();
        startingNode = baseNode;
    }
    
    public void Execute()
    {
        CollapseNodes(startingNode);
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

    private void CollapseNodes(Node newNode)
    {
        Debug.Log($"{newNode.nodePosition.x}, {newNode.nodePosition.y}, {newNode.nodePosition.z}");
        Dictionary<DIRECTIONKEY, Node> surroundingNodes = new Dictionary<DIRECTIONKEY, Node>();
        List<Node> incompleteNodes = new List<Node>();
        //surroundingNodes.Add(DIRECTIONKEY.KEY0, newNode);
        if (BoundryCheck(newNode.nodePosition.x - 1, newNode.nodePosition.y, newNode.nodePosition.z))
        {
            if (WFC_Area[(int) (newNode.nodePosition.x - 1), (int) newNode.nodePosition.y, (int) newNode.nodePosition.z].IterationComplete)
            { 
                Debug.Log($"Adding node at position: x{newNode.nodePosition.x -1}, y{newNode.nodePosition.y}, z{newNode.nodePosition.z}"); 
                surroundingNodes.Add(DIRECTIONKEY.KEY1, WFC_Area[(int) (newNode.nodePosition.x - 1), (int) newNode.nodePosition.y, (int) newNode.nodePosition.z]);
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
                Debug.Log($"Adding node at position: x{newNode.nodePosition.x}, y{newNode.nodePosition.y}, z{newNode.nodePosition.z + 1}"); 
                surroundingNodes.Add(DIRECTIONKEY.KEY2, WFC_Area[(int) newNode.nodePosition.x, (int) newNode.nodePosition.y, (int) (newNode.nodePosition.z+1)]);
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
                Debug.Log($"Adding node at position: x{newNode.nodePosition.x}, y{newNode.nodePosition.y}, z{newNode.nodePosition.z - 1}"); 
                surroundingNodes.Add(DIRECTIONKEY.KEY0, WFC_Area[newNode.nodePosition.x, newNode.nodePosition.y, (newNode.nodePosition.z -1)]);
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
                Debug.Log($"Adding node at position: x{newNode.nodePosition.x + 1}, y{newNode.nodePosition.y}, z{newNode.nodePosition.z}"); 
                surroundingNodes.Add(DIRECTIONKEY.KEY3, WFC_Area[(int) (newNode.nodePosition.x+1), (int) newNode.nodePosition.y, (int) newNode.nodePosition.z]);
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
                Debug.Log($"Adding node at position: x{newNode.nodePosition.x}, y{newNode.nodePosition.y + 1}, z{newNode.nodePosition.z}"); 
                surroundingNodes.Add(DIRECTIONKEY.KEY4, WFC_Area[(int) newNode.nodePosition.x, (int) (newNode.nodePosition.y+1), (int) newNode.nodePosition.z]);
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
                Debug.Log($"Adding node at position: x{newNode.nodePosition.x}, y{newNode.nodePosition.y - 1}, z{newNode.nodePosition.z}"); 
                surroundingNodes.Add(DIRECTIONKEY.KEY5, WFC_Area[(int) newNode.nodePosition.x, (int) (newNode.nodePosition.y-1), (int) newNode.nodePosition.z]);
                
            }
            else 
            { 
                incompleteNodes.Add(WFC_Area[newNode.nodePosition.x, newNode.nodePosition.y - 1, newNode.nodePosition.z]);
            }
        } 
        WFC_Area[newNode.nodePosition.x, newNode.nodePosition.y, newNode.nodePosition.z].CollapseNode(surroundingNodes); 
        if (incompleteNodes.Count == 0) 
        { 
            if (IsFullyCollapsed()) 
            { 
                Debug.LogWarning("Area is fully collapsed");
                //BuildWaveFuction();
                return;
            }
            else 
            {
                if (!isBuilt)
                {
                    /*isBuilt = true;
                    Debug.LogError("Could not fully collapse this iteration");
                    finalValues = new Dictionary<Vector3Int, List<VALUE>>();
                    foreach (var node in WFC_Area)
                    {
                        List<VALUE> values = new List<VALUE>();
                        foreach (var tile in node.PossibleTiles)
                        {
                            values.Add(tile.tileValue);
                        }
                        finalValues.Add(node.nodePosition, values);
                    }*/
                }
                
            }
        }
        else 
        { 
            foreach (var node in incompleteNodes.ToList())
            {
                Debug.Log($"incomplete node at: x: {node.nodePosition.x}, y: {node.nodePosition.y}, z: {node.nodePosition.z}");
            }
            /*foreach (var node in incompleteNodes.ToList())
            {
                Debug.Log("Calling exponential collapse");
                CollapseNodes(node);
            }*/
            CollapseNodeJob tmpThis = this;
            Parallel.ForEach(incompleteNodes, node =>
            {
                Debug.Log("Calling exponential collapse");
                tmpThis.CollapseNodes(node);
            });
            
        }
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
    
    
}
