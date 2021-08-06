using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Node : MonoBehaviour
{
    public Vector2Int myPosInGrid;
    public bool isBlocked;
    public NodeGrid myGrid;
    public int cost;

    public void Spawn(Vector3 posInWorld, Vector2Int posGrid, NodeGrid grid)
    {
        transform.position = posInWorld;
        myPosInGrid = posGrid;
        myGrid = grid;
        ChangeCost(1);
        ChangeWallProperty(false);
    }

    public List<Node> GetNeighbors()
    {
        List<Node> neighbors = new List<Node>();
        
        Node up = myGrid.GetNodeFromGrid(myPosInGrid.x, myPosInGrid.y - 1);
        Node down = myGrid.GetNodeFromGrid(myPosInGrid.x, myPosInGrid.y + 1);
        Node left = myGrid.GetNodeFromGrid(myPosInGrid.x - 1, myPosInGrid.y);
        Node right = myGrid.GetNodeFromGrid(myPosInGrid.x + 1, myPosInGrid.y);

        if (up != null) 
            neighbors.Add(up);
        if (left != null) 
            neighbors.Add(left);
        if (down != null) 
            neighbors.Add(down);
        if (right != null) 
            neighbors.Add(right);
        
        if (myGrid.diagonals)
        {
            Node upLeft = myGrid.GetNodeFromGrid(myPosInGrid.x - 1, myPosInGrid.y - 1); 
            Node upRight = myGrid.GetNodeFromGrid(myPosInGrid.x + 1, myPosInGrid.y - 1); 
            Node downRight = myGrid.GetNodeFromGrid(myPosInGrid.x + 1, myPosInGrid.y + 1); 
            Node downLeft = myGrid.GetNodeFromGrid(myPosInGrid.x - 1, myPosInGrid.y + 1); 
            
            if (upLeft != null) 
                neighbors.Add(upLeft);
            if (upRight != null) 
                neighbors.Add(upRight);
            if (downLeft != null) 
                neighbors.Add(downLeft);
            if (downRight != null) 
                neighbors.Add(downRight);
        }

        return neighbors;
    }

    void ChangeWallProperty(bool c)
    {
        isBlocked = c;
        gameObject.layer = c ? 8 : 0;

    }

    public void ChangeCost(int v)
    {
        cost = v;
        if (cost < 1) 
            cost = 1;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 9)
            isBlocked = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == 9)
            isBlocked = false;
    }
}
