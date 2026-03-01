using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathNode
{
    private GridPosition gridPosition;
    private int gCost;
    private int hCost;
    private int fCost;
    private PathNode cameFromNode;
    private bool isWalkable = true;
    public PathNode(GridPosition gridPosition)
    {
        this.gridPosition = gridPosition;
    }

    public override string ToString()
    {
        return gridPosition.ToString();
    }
    public int GetGCost() => gCost;
    public int GetHCost() => hCost;
    public int GetFCost() => fCost;

    public void SetGCost(int gCost)
    {
        this.gCost = gCost;
        CalculateFCost();
    }

    public void SetHCost(int hCost)
    {
        this.hCost = hCost;
        CalculateFCost();
    }
    public void CalculateFCost() => fCost = gCost + hCost;

    public void ResetCameFromNode() => cameFromNode = null;

    public void SetCameFromNode(PathNode node) => cameFromNode = node;
    public PathNode GetCameFromNode() => cameFromNode;
    public GridPosition GetGridPosition() => gridPosition;

    public bool GetIsWalkable() => isWalkable;

    public void SetIsWalkable(bool isWalkable) => this.isWalkable = isWalkable;

}
