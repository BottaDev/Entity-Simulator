using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeGrid : MonoBehaviour
{
    public int width;
    public int height;
    public float nodeSize = 1;
    public bool diagonals;

    private Node[,] _myGrid;

    public GameObject nodePrefab;
    
    private void CreateMatrix()
    {
        _myGrid = new Node[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var g = Instantiate(nodePrefab);
                Node n = g.GetComponent<Node>(); ;
                _myGrid[x, y] = n;

                n.Spawn(new Vector3(x * nodeSize, 0, y * nodeSize), new Vector2Int(x, y), this);
            }
        }
    }

    public Node GetNodeFromGrid(int x, int y)
    {
        if (InBounds(x, y)) return _myGrid[x, y];
        else return null;
    }

    private bool InBounds(int x, int y)
    {
        if (x < 0 || x >= width) 
            return false;
        if (y < 0 || y >= height) 
            return false;

        return true;
    }
    
    private void Start()
    {
        CreateMatrix();
    }
}
