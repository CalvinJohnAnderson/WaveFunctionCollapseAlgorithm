using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor.ProBuilder;
using UnityEngine;
using UnityEngine.ProBuilder;

public class MeshGenerator : MonoBehaviour
{
    [SerializeField] private Material quadMaterial;
    private Node[,,] AreaNodes;
    [SerializeField] private GameObject peak;
    private List<Vector3Int> peakPositions;
    [SerializeField] private Transform oldParent;
    [SerializeField] private Transform parent;
    [SerializeField] private GameObject newRock;

    public void GenerateMesh(Node[,,] nodes)
    {
        AreaNodes = nodes;
        peakPositions = new List<Vector3Int>();
        GetMidPoints();
    }

    private void GetMidPoints()
    {
        for (int x = 0; x < AreaNodes.GetLength(0); x++)
        {
            for (int z = 0; z < AreaNodes.GetLength(2); z++)
            {
                Vector3Int peakPosition = new Vector3Int(x, 0, z);
                for (int y = 0; y < AreaNodes.GetLength(1); y++)
                {
                    //Vector3Int peakPosition = new Vector3Int(x, y, z);
                    if (AreaNodes[x, 0, z].PossibleTiles[0].tileValue == VALUE.AIR)
                    {
                        peakPositions.Add(peakPosition);
                        break;
                    }

                    if (AreaNodes[x, y, z].PossibleTiles[0].tileValue == VALUE.GROUND)
                    {
                        peakPosition.y = y + 1;
                    }
                    if (AreaNodes[x, y, z].PossibleTiles[0].tileValue == VALUE.ROCK)
                    {
                        peakPosition.y = y + 1;
                        break;
                    }
                    //peakPositions.Add(peakPosition);
                }
                peakPositions.Add(peakPosition);
            }
        }
        PlacePeaks();
    }

    private void PlacePeaks()
    {
        /*foreach (Transform child in oldParent.transform)
        {
            Destroy(child.gameObject);
        }*/
        oldParent.gameObject.SetActive(false);
        foreach (var newPeak in peakPositions)
        {
            GameObject go = Instantiate(peak, parent);
            go.transform.position = newPeak;
            //ProBuilderMesh builder = go.GetComponent<ProBuilderMesh>();
            

            //GameObject go = Instantiate(peak, parent);
            //go.transform.position = newPeak;
        }
        PlaceMeshPeaks();
    }

    private void PlaceMeshPeaks()
    {
        Vector3[] verts = new Vector3[(int) Mathf.Pow(AreaNodes.GetLength(0), 2)];
        int counter = 0;
        for (int i = 0; i < AreaNodes.GetLength(0); ++i)//was 5
        {
            for (int j = 0; j < AreaNodes.GetLength(0); ++j)
            {
                int index = i * AreaNodes.GetLength(0) + j;
                /*verts[index].x = i;
                verts[index].y = peakPositions[counter].y;
                verts[index].z = j;
                counter++;*/
                //float randomHeight = Random.Range(-0.3f, 0.3f);
                if (i == 0 || i == AreaNodes.GetLength(0)-1 || j == 0 || j == AreaNodes.GetLength(0)-1)
                {
                    
                    verts[index].x = i;
                    verts[index].y = 0;
                    verts[index].z = j;
                    counter++;
                }
                else
                {
                    verts[index].x = i;
                    verts[index].y = peakPositions[counter].y;
                    verts[index].z = j;
                    counter++;
                }
                
            }
        }

        List<Face> faces = new List<Face>();

        for (int i = 0; i < AreaNodes.GetLength(0)-1; ++i)
        {
            for (int j = 0; j < AreaNodes.GetLength(0)-1; ++j)
            {
                int index1 = i * AreaNodes.GetLength(0) + j;
                int index2 = (i + 1) * AreaNodes.GetLength(0) + j;
                int index3 = i * AreaNodes.GetLength(0) + j + 1;
                Face face1 = new Face(new int[] { index3, index2, index1 });

                // Create the adjacent one
                index1 = (i + 1) * AreaNodes.GetLength(0) + j;
                index2 = index1 + 1;
                index3 = i * AreaNodes.GetLength(0) + j + 1;
                Face face2 = new Face(new int[] { index3, index2, index1 });

                faces.Add(face1);
                faces.Add(face2);
            }
        }

        ProBuilderMesh ocean = ProBuilderMesh.Create(verts, faces);
        ocean.SetMaterial(ocean.faces, quadMaterial);
        ocean.Refresh();
        ocean.ToMesh();
    }
}
