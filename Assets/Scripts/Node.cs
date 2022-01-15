using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using UnityEngine;
using Random = System.Random;

public class Node
{
    public Vector3Int nodePosition;
    public bool isCollapsed = false;
    public List<Tile> PossibleTiles;
    public bool IterationComplete = false;
    public bool ValueChange = false;

    public Node(List<GameObject> tiles, List<VALUE> values, Vector3Int position)
    {
        nodePosition = position;
        PossibleTiles = new List<Tile>();
        for (int i = 0; i < tiles.Count; i++)
        {
            //Debug.Log("adding tile: " + values[i]);
            Tile tile = new Tile(tiles[i], values[i]);
            PossibleTiles.Add(tile);
        }
    }
    
    public void FullyCollapseNode()
    {
        Debug.Log("Node: Fully Collapsing node");
        Random rnd = new Random();
        int temp = rnd.Next(0, PossibleTiles.Count);
        foreach (var newTile in PossibleTiles)
        {
            if (newTile.tileValue == VALUE.GROUND)
            {
                temp = rnd.Next(0, 100);
                if (temp < 60)
                {
                    Debug.Log("Fully collapsed to : " + newTile.tileValue);
                    PossibleTiles.Clear();
                    PossibleTiles.Add(newTile);
                    isCollapsed = true;
                    IterationComplete = true;
                    return;
                }
            }
        }
        temp = rnd.Next(0, PossibleTiles.Count);
        Tile tile = PossibleTiles[temp];
        Debug.Log("Fully collapsed to : " + tile.tileValue);
        PossibleTiles.Clear();
        PossibleTiles.Add(tile);
        isCollapsed = true;
        IterationComplete = true;
        ValueChange = true;
    }

    public void CollapseNode(Dictionary<DIRECTIONKEY, Node> surroundingNodes)
    {
        IterationComplete = true;
        if (!isCollapsed)
        {
            foreach (var node in surroundingNodes)
            {
                Debug.Log($"Current node position = x: {nodePosition.x}, y: {nodePosition.y}, z: {nodePosition.z}");
                Debug.Log($"Getting correct tiles from node at x: {node.Value.nodePosition.x}, y: {node.Value.nodePosition.y}, z: {node.Value.nodePosition.z}");
                List<VALUE> correctTiles = new List<VALUE>();
                foreach (var tile in node.Value.PossibleTiles.ToList())
                {
                    Debug.Log($"key: {node.Key} from x: {node.Value.nodePosition.x}, y: {node.Value.nodePosition.y}, z: {node.Value.nodePosition.z} - new Key = {GetKeyPair(node.Key)}");
                    foreach (var value in tile.tileInfo[GetKeyPair(node.Key)])
                    {
                        Debug.Log($"from x: {node.Value.nodePosition.x}, y: {node.Value.nodePosition.y}, z: {node.Value.nodePosition.z} - key {node.Key} - Checking against an : {tile.tileValue}");
                        Debug.Log($"from x: {node.Value.nodePosition.x}, y: {node.Value.nodePosition.y}, z: {node.Value.nodePosition.z} - key {node.Key} - Correct tile + : {value}");
                        correctTiles.Add(value);
                    }
                }

                foreach (var tile in PossibleTiles.ToList())
                {
                    if (!correctTiles.Contains(tile.tileValue))
                    {
                        Debug.Log($"Removing: {tile.tileValue} at key: {node.Key} from x: {node.Value.nodePosition.x}, y: {node.Value.nodePosition.y}, z: {node.Value.nodePosition.z}");
                        RemoveTile(tile);
                    }
                }
            }
            //Debug.Log("Node: Collapsing node");
            /*Parallel.ForEach(surroundingNodes, node =>
            {
                Debug.Log($"Current node position = x: {nodePosition.x}, y: {nodePosition.y}, z: {nodePosition.z}");
                //Debug.Log($"Getting correct tiles from node at x: {node.Value.nodePosition.x}, y: {node.Value.nodePosition.y}, z: {node.Value.nodePosition.z}");
                List<VALUE> correctTiles = new List<VALUE>();
                Parallel.ForEach(node.Value.PossibleTiles.ToList(), tile =>
                {
                    foreach (var value in tile.tileInfo[GetKeyPair(node.Key)])
                    {
                        Debug.Log($"from : {node.Value.nodePosition} : Checking against an : " + tile.tileValue);
                        Debug.Log($"from : {node.Value.nodePosition} : Checking key - {node.Key}, new key - {GetKeyPair(node.Key)}");
                        Debug.Log($"Correct tile from : {node.Value.nodePosition} + : " + value);
                        try
                        {
                            correctTiles.Add(value);
                        }
                        catch (Exception e)
                        {
                            Debug.LogError(": Error adding to correct tile list");
                            throw;
                        }
                    }
                    
                });
                Parallel.ForEach(PossibleTiles.ToList(), tile =>
                {
                    if (correctTiles.Count == 0)
                    {
                        Debug.LogError("No correct tiles");
                    }

                    if (tile == null)
                    {
                        Debug.LogError("Tile value is null");
                    }
                    if (!correctTiles.Contains(tile.tileValue))
                    {
                        //Debug.Log("Removing: " + tile.tileValue);
                        RemoveTile(tile);
                    }
                });
            });*/
            
            
        }
    }

    private void RemoveTile(Tile tile)
    {
        ValueChange = true;
        Debug.Log($"Changing value at x: {nodePosition.x}, y: {nodePosition.y}, z: {nodePosition.z}");
        if (PossibleTiles.Count > 1)
        {
            Debug.Log("Removing tile");
            PossibleTiles.Remove(tile);
            if (PossibleTiles.Count == 1)
            {
                isCollapsed = true;
            }
        }
    }

    public int GetEntropy()
    {
        return PossibleTiles.Count;
    }

    /*public void RemoveTile(Tile tile)
    {
        Debug.Log("Removing Tile");
        foreach (var currentTile in PossibleTiles.ToList())
        {
            if (currentTile.tileValue == tile.tileValue)
            {
                PossibleTiles.Remove(currentTile);
            }
        }

        if (PossibleTiles.Count == 1)
        {
            Debug.LogWarning("Tile is now collapsed");
            isCollapsed = true;
        }
    }

    public void FullyCollapseNode()
    {
        Debug.Log("Node: Fully Collapsing node");
        Random rnd = new Random();
        int temp = rnd.Next(0, PossibleTiles.Count);
        Tile tile = PossibleTiles[temp];
        PossibleTiles.Clear();
        PossibleTiles.Add(tile);
        isCollapsed = true;
        IterationComplete = true;
    }

    public void CollapseNode(Node previousNode, DIRECTIONKEY key)
    {
        IterationComplete = true;
        if (!isCollapsed)
        {
            Debug.Log("Node: Collapsing node");
            List<VALUE> correctTiles = new List<VALUE>();
            foreach (var tile in previousNode.PossibleTiles)
            {
                foreach (var value in tile.tileInfo[key])
                {
                    correctTiles.Add(value);
                }
            }

            foreach (var tile in PossibleTiles.ToList())
            {
                if (!correctTiles.Contains(tile.tileValue))
                {
                    RemoveTile(tile);
                }
            }
        }
        else
        {
            Debug.LogWarning("Node: Node called is already collapsed");
        }
        
    }*/

    private DIRECTIONKEY GetKeyPair(DIRECTIONKEY x)
    {
        switch (x)
        {
            case DIRECTIONKEY.KEY0:
                return DIRECTIONKEY.KEY2;
            break;
            case DIRECTIONKEY.KEY1:
                return DIRECTIONKEY.KEY3;
                break;
            case DIRECTIONKEY.KEY2:
                return DIRECTIONKEY.KEY0;
                break;
            case DIRECTIONKEY.KEY3:
                return DIRECTIONKEY.KEY1;
                break;
            case DIRECTIONKEY.KEY4:
                return DIRECTIONKEY.KEY5;
                break;
            case DIRECTIONKEY.KEY5:
                return DIRECTIONKEY.KEY4;
                break;
        }

        return DIRECTIONKEY.KEY0;
    }
}
