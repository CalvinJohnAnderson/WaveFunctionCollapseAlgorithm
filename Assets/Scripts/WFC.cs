using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using Random = System.Random;

public class WFC : SerializedMonoBehaviour
{
    [SerializeField] private List<GameObject> tiles;
    [SerializeField] private List<VALUE> values;
    [SerializeField] private Dictionary<Vector3Int, List<VALUE>> finalValues;
    [SerializeField] private Transform parent;
    
    private Node[,,] WFC_Area = new Node[5, 5, 5];

    private int maxIterations = 10;//was 5 then 7
    public int currentIteration = 0;
    private bool isBuilding = false;
    private bool isCollapsing = false;
    private Vector3Int currentEvaluation;
    private Vector3Int startingPos;

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
            //surroundingNodes.Add(DIRECTIONKEY.KEY0, newNode);
            if (BoundryCheck(newNode.nodePosition.x - 1, newNode.nodePosition.y, newNode.nodePosition.z))
            {
                if (WFC_Area[(int) (newNode.nodePosition.x - 1), (int) newNode.nodePosition.y, (int) newNode.nodePosition.z].IterationComplete)
                {
                    Debug.Log($"Adding node at position: x{newNode.nodePosition.x -1}, y{newNode.nodePosition.y}, z{newNode.nodePosition.z}");
                    surroundingNodes.Add(DIRECTIONKEY.KEY1, WFC_Area[(int) (newNode.nodePosition.x - 1), (int) newNode.nodePosition.y, (int) newNode.nodePosition.z]);
                }
            }
            if (BoundryCheck(newNode.nodePosition.x, newNode.nodePosition.y, newNode.nodePosition.z+1))
            {
                if (WFC_Area[(int) newNode.nodePosition.x, (int) newNode.nodePosition.y, (int) (newNode.nodePosition.z+1)].IterationComplete)
                {
                    Debug.Log($"Adding node at position: x{newNode.nodePosition.x}, y{newNode.nodePosition.y}, z{newNode.nodePosition.z + 1}");
                    surroundingNodes.Add(DIRECTIONKEY.KEY2, WFC_Area[(int) newNode.nodePosition.x, (int) newNode.nodePosition.y, (int) (newNode.nodePosition.z+1)]);
                }
                        
            }
            if (BoundryCheck(newNode.nodePosition.x, newNode.nodePosition.y, newNode.nodePosition.z-1))
            {
                if (WFC_Area[(int) newNode.nodePosition.x, (int) newNode.nodePosition.y, (int) (newNode.nodePosition.z-1)].IterationComplete)
                {
                    Debug.Log($"Adding node at position: x{newNode.nodePosition.x}, y{newNode.nodePosition.y}, z{newNode.nodePosition.z - 1}");
                    surroundingNodes.Add(DIRECTIONKEY.KEY0, WFC_Area[newNode.nodePosition.x, newNode.nodePosition.y, (newNode.nodePosition.z -1)]);
                }
                        
            }
            if (BoundryCheck(newNode.nodePosition.x +1, newNode.nodePosition.y, newNode.nodePosition.z))
            {
                if (WFC_Area[(int) (newNode.nodePosition.x + 1), (int) newNode.nodePosition.y, (int) newNode.nodePosition.z].IterationComplete)
                {
                    Debug.Log($"Adding node at position: x{newNode.nodePosition.x + 1}, y{newNode.nodePosition.y}, z{newNode.nodePosition.z}");
                    surroundingNodes.Add(DIRECTIONKEY.KEY3, WFC_Area[(int) (newNode.nodePosition.x+1), (int) newNode.nodePosition.y, (int) newNode.nodePosition.z]);
                }
                        
            }
            if (BoundryCheck(newNode.nodePosition.x, newNode.nodePosition.y + 1, newNode.nodePosition.z))
            {
                if (WFC_Area[(int) newNode.nodePosition.x, (int) (newNode.nodePosition.y+1), (int) newNode.nodePosition.z].IterationComplete)
                {
                    Debug.Log($"Adding node at position: x{newNode.nodePosition.x}, y{newNode.nodePosition.y + 1}, z{newNode.nodePosition.z}");
                    surroundingNodes.Add(DIRECTIONKEY.KEY4, WFC_Area[(int) newNode.nodePosition.x, (int) (newNode.nodePosition.y+1), (int) newNode.nodePosition.z]);
                }
                        
            }
            if (BoundryCheck(newNode.nodePosition.x, newNode.nodePosition.y - 1, newNode.nodePosition.z))
            {
                if (WFC_Area[(int) newNode.nodePosition.x, (int) (newNode.nodePosition.y-1), (int) newNode.nodePosition.z].IterationComplete)
                {
                    Debug.Log($"Adding node at position: x{newNode.nodePosition.x}, y{newNode.nodePosition.y - 1}, z{newNode.nodePosition.z}");
                    surroundingNodes.Add(DIRECTIONKEY.KEY5, WFC_Area[(int) newNode.nodePosition.x, (int) (newNode.nodePosition.y-1), (int) newNode.nodePosition.z]);
                }
            }
            WFC_Area[(int) newNode.nodePosition.x, (int) newNode.nodePosition.y, (int) newNode.nodePosition.z].CollapseNode(surroundingNodes);
            
            if (!BoundryCheck(currentEvaluation.x, currentEvaluation.y, currentEvaluation.z + 1))
            {
                if (!BoundryCheck(currentEvaluation.x, currentEvaluation.y + 1, currentEvaluation.z))
                {
                    if (!BoundryCheck(currentEvaluation.x + 1, currentEvaluation.y, currentEvaluation.z))
                    {
                        currentEvaluation.x = 0;
                        currentEvaluation.y = 0;
                        currentEvaluation.z = 0;
                        if (startingPos == currentEvaluation)
                        {
                            Debug.LogWarning("Complete full iteration");    
                            Debug.Log($"x:{currentEvaluation.x}, y:{currentEvaluation.y}, z:{currentEvaluation.z}");
                            Debug.Log($"x:{startingPos.x}, y:{startingPos.y}, z:{startingPos.z}");
                            if (IsFullyCollapsed())
                            {
                                Debug.LogWarning("Area is now fully collapsed");
                                BuildWaveFuction();
                            }
                            else
                            {
                                Debug.LogError("Could not collapse area");
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
                                /*if (currentIteration < maxIterations)
                                {
                                    currentIteration++;
                                    ResetIterations();
                                }
                                else
                                {
                                    Debug.LogError("Could not collapse area");
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
                                }*/
                            }
                        }
                        else
                        {
                            CollapseNodes(WFC_Area[currentEvaluation.x, currentEvaluation.y, currentEvaluation.z]);
                        }
                        /*Debug.LogWarning("Complete full iteration");
                        currentIteration++;
                        if (IsFullyCollapsed())
                        {
                            Debug.LogWarning("Fully collapsed");
                        }
                        else
                        {
                            if (currentIteration < maxIterations)
                            {
                                Debug.LogWarning("ResettingGrid");
                                ResetIterations();
                            }
                        }*/
                    }
                    else
                    {
                        
                        //send next one
                        currentEvaluation.x = currentEvaluation.x + 1;
                        currentEvaluation.y = 0;
                        currentEvaluation.z = 0;
                        if (startingPos == currentEvaluation)
                        {
                            Debug.LogWarning("Complete full iteration");  
                            Debug.Log($"x:{currentEvaluation.x}, y:{currentEvaluation.y}, z:{currentEvaluation.z}");
                            Debug.Log($"x:{startingPos.x}, y:{startingPos.y}, z:{startingPos.z}");
                            if (IsFullyCollapsed())
                            {
                                Debug.LogWarning("Area is now fully collapsed");
                                BuildWaveFuction();
                            }
                            else
                            {
                                Debug.LogError("Could not collapse area");
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
                                /*if (currentIteration < maxIterations)
                                {
                                    currentIteration++;
                                    ResetIterations();
                                }
                                else
                                {
                                    Debug.LogError("Could not collapse area");
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
                                }*/
                            }
                        }
                        else
                        {
                            CollapseNodes(WFC_Area[currentEvaluation.x, currentEvaluation.y, currentEvaluation.z]);
                        }
                    }
                }
                else
                {
                    //call next one
                    
                    
                    currentEvaluation.y = currentEvaluation.y + 1;
                    currentEvaluation.z = 0;
                    if (startingPos == currentEvaluation)
                    {
                        Debug.LogWarning("Complete full iteration");   
                        Debug.Log($"x:{currentEvaluation.x}, y:{currentEvaluation.y}, z:{currentEvaluation.z}");
                        Debug.Log($"x:{startingPos.x}, y:{startingPos.y}, z:{startingPos.z}");
                        if (IsFullyCollapsed())
                        {
                            Debug.LogWarning("Area is now fully collapsed");
                            BuildWaveFuction();
                        }
                        else
                        {
                            Debug.LogError("Could not collapse area");
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
                            /*if (currentIteration < maxIterations)
                            {
                                currentIteration++;
                                ResetIterations();
                            }
                            else
                            {
                                Debug.LogError("Could not collapse area");
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
                            }*/
                        }
                    }
                    else
                    {
                        CollapseNodes(WFC_Area[currentEvaluation.x, currentEvaluation.y, currentEvaluation.z]);
                    }
                    
                }
            }
            else
            {
                //call next one
                currentEvaluation.z = currentEvaluation.z + 1;
                if (startingPos == currentEvaluation)
                {
                    Debug.LogWarning("Complete full iteration");  
                    Debug.Log($"x:{currentEvaluation.x}, y:{currentEvaluation.y}, z:{currentEvaluation.z}");
                    Debug.Log($"x:{startingPos.x}, y:{startingPos.y}, z:{startingPos.z}");
                    if (IsFullyCollapsed())
                    {
                        Debug.LogWarning("Area is now fully collapsed");
                        BuildWaveFuction();
                    }
                    else
                    {
                        Debug.LogError("Could not collapse area");
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
                        /*if (currentIteration < maxIterations)
                        {
                            currentIteration++;
                            ResetIterations();
                        }
                        else
                        {
                            Debug.LogError("Could not collapse area");
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
                        }*/
                    }
                    
                }
                else
                {
                    CollapseNodes(WFC_Area[currentEvaluation.x, currentEvaluation.y, currentEvaluation.z]);
                }
            }
        /*for (int x = 0; x < WFC_Area.GetLength(0); x++)
        {
            for (int y = 0; y < WFC_Area.GetLength(1); y++)
            {
                for (int z = 1; z < WFC_Area.GetLength(2); z++)
                {
                    Debug.Log($"{x}, {y}, {z}");
                    Dictionary<DIRECTIONKEY, Node> surroundingNodes = new Dictionary<DIRECTIONKEY, Node>();
                    surroundingNodes.Add(DIRECTIONKEY.KEY0, newNode);
                    if (BoundryCheck(x - 1, y, z))
                    {
                        if (WFC_Area[x - 1, y, z].IterationComplete)
                        {
                            surroundingNodes.Add(DIRECTIONKEY.KEY1, WFC_Area[x-1, y, z]);
                        }
                    }
                    if (BoundryCheck(x, y, z+1))
                    {
                        if (WFC_Area[x, y, z+1].IterationComplete)
                        {
                            surroundingNodes.Add(DIRECTIONKEY.KEY2, WFC_Area[x, y, z+1]);
                        }
                        
                    }
                    if (BoundryCheck(x +1, y, z))
                    {
                        if (WFC_Area[x + 1, y, z].IterationComplete)
                        {
                            surroundingNodes.Add(DIRECTIONKEY.KEY3, WFC_Area[x+1, y, z]);
                        }
                        
                    }
                    if (BoundryCheck(x, y + 1, z))
                    {
                        if (WFC_Area[x, y+1, z].IterationComplete)
                        {
                            surroundingNodes.Add(DIRECTIONKEY.KEY4, WFC_Area[x, y+1, z]);
                        }
                        
                    }
                    if (BoundryCheck(x, y - 1, z))
                    {
                        if (WFC_Area[x, y-1, z].IterationComplete)
                        {
                            surroundingNodes.Add(DIRECTIONKEY.KEY5, WFC_Area[x, y-1, z]);
                        }
                    }
                    WFC_Area[x, y, z].CollapseNode(surroundingNodes);
                }
            }
        }
        /*Dictionary<DIRECTIONKEY, Node> surroundingNodes = new Dictionary<DIRECTIONKEY, Node>();
        surroundingNodes.Add(DIRECTIONKEY.KEY0, node);
        if (BoundryCheck(x - 1, level, z))
        {
            surroundingNodes.Add(DIRECTIONKEY.KEY1, WFC_Area[x-1, y, z]);
        }
        if (BoundryCheck(x, level, z+1))
        {
            surroundingNodes.Add(DIRECTIONKEY.KEY2, WFC_Area[x, y, z+1]);
        }
        if (BoundryCheck(x +1, level, z))
        {
            surroundingNodes.Add(DIRECTIONKEY.KEY3, WFC_Area[x+1, y, z]);
        }
        if (BoundryCheck(x, level + 1, z))
        {
            surroundingNodes.Add(DIRECTIONKEY.KEY4, WFC_Area[x-1, y, z]);
        }
        if (BoundryCheck(x, level - 1, z))
        {
            surroundingNodes.Add(DIRECTIONKEY.KEY5, WFC_Area[x-1, y, z]);
        }*/
        //Debug.Log("Completed a single iteration");
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
    
    /*private void CollapseNodes(Node node, int x, int y, int z)
    {
        for (int a = x-1; a < x+1; a++)
        {
            for (int b = y-1; b < y+1; b++)
            {
                for (int c = z-1; c < z+1; c++)
                {
                    if (BoundryCheck(a, b, c))
                    {
                        if (!WFC_Area[a, b, c].IterationComplete)
                        {
                            if (!WFC_Area[a, b, c].isCollapsed)
                            {
                                if (currentIteration < maxIterations)
                                {
                                    Debug.Log("WFC: Getting direction key");
                                    GetDirectionKey(node, x, y, z, a, b, c);
                                    isCollapsing = true;
                                }
                                else
                                {
                                    Debug.Log("WFC: Reached Max Iterations");
                                }
                            }
                        }
                    }
                }
            }
        }
        
        if (isCollapsing)
        {
            isCollapsing = false;
        }
        else
        {
            ResetIterations();
        }
        
        currentIteration++;
        if (IsFullyCollapsed())
        {
            Debug.Log("WFC: Completed WaveFunction collapse");
            if (!isBuilding)
            {
                isBuilding = true;
                BuildWaveFuction();
            }
        }

        else if (currentIteration > Mathf.Pow((WFC_Area.GetLength(0)), 3))
        {
            ResetIterations();
        }
    }

    private bool BoundryCheck(int x, int y, int z)
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

    private void GetDirectionKey(Node node, int x, int y, int z, int a, int b, int c)
    {
        Debug.Log("WFC: Searching for direction key");
        DIRECTIONKEY newKey;
        if (a < x && b == y && c == z)
        {
            newKey = DIRECTIONKEY.KEY1;
            if (!WFC_Area[a, b, c].isCollapsed)
            {
                Debug.Log("WFC: Collapsing node at key1");
                Debug.Log($"WFC: x = {a}, y = {b}, z = {c}");
                WFC_Area[a, b, c].CollapseNode(node, newKey);
            }
            Debug.Log("WFC: reiterating");
            CollapseNodes(WFC_Area[a, b, c], a, b, c);
        }

        else if (c < z && a == x && b == y)
        {
            newKey = DIRECTIONKEY.KEY0;
            if (!WFC_Area[a, b, c].isCollapsed)
            {
                Debug.Log("WFC: Collapsing node at key0");
                Debug.Log($"WFC: x = {a}, y = {b}, z = {c}");
                WFC_Area[a, b, c].CollapseNode(node, newKey);
            }
            CollapseNodes(WFC_Area[a, b, c], a, b, c);
        }
        else if (c > z && a == x && b == y)
        {
            newKey = DIRECTIONKEY.KEY2;
            if (!WFC_Area[a, b, c].isCollapsed)
            {
                Debug.Log("WFC: Collapsing node at key2");
                Debug.Log($"WFC: x = {a}, y = {b}, z = {c}");
                WFC_Area[a, b, c].CollapseNode(node, newKey);
            }
            CollapseNodes(WFC_Area[a, b, c], a, b, c);
        }
        else if (a > x && b == y && c == z)
        {
            newKey = DIRECTIONKEY.KEY3;
            if (!WFC_Area[a, b, c].isCollapsed)
            {
                Debug.Log("WFC: Collapsing node at key3");
                Debug.Log($"WFC: x = {a}, y = {b}, z = {c}");
                WFC_Area[a, b, c].CollapseNode(node, newKey);
            }
            CollapseNodes(WFC_Area[a, b, c], a, b, c);
        }
        else if (b > y && a == x && c == z)
        {
            newKey = DIRECTIONKEY.KEY4;
            if (!WFC_Area[a, b, c].isCollapsed)
            {
                Debug.Log("WFC: Collapsing node at key4");
                Debug.Log($"WFC: x = {a}, y = {b}, z = {c}");
                WFC_Area[a, b, c].CollapseNode(node, newKey);
            }
            CollapseNodes(WFC_Area[a, b, c], a, b, c);
        }
        else if (b < y && a == x && c == z)
        {
            newKey = DIRECTIONKEY.KEY5;
            if (!WFC_Area[a, b, c].isCollapsed)
            {
                Debug.Log("WFC: Collapsing node at key5");
                Debug.Log($"WFC: x = {a}, y = {b}, z = {c}");
                WFC_Area[a, b, c].CollapseNode(node, newKey);
            }
            CollapseNodes(WFC_Area[a, b, c], a, b, c);
        }
        else
        {
            Debug.Log("WFC: Not within quidelines");
            Debug.Log($"WFC: x = {a}, y = {b}, z = {c}");
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

    private void BuildWaveFuction()
    {
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
                        GameObject newTile = Instantiate(WFC_Area[x, y, z].PossibleTiles[0].tile);
                        newTile.transform.position = new Vector3(x, y, z);
                    }
                    foreach (var tile in WFC_Area[x, y, z].PossibleTiles)
                    {
                        GameObject newTile = Instantiate(tile.tile);
                        newTile.transform.position = new Vector3(x, y, z);
                    }
                }
            }
        }
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
        CollapseRandomNode();
    }*/
}
