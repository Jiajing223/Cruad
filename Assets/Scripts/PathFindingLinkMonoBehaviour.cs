using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFindingLinkMonoBehaviour : MonoBehaviour
{
    public Vector3 linkPositionA;
    public Vector3 linkPositionB;

    public PathFindingLink GetPathFindingLink()
    {
        return new PathFindingLink
        {
            gridPositionA = LevelGrid.Instance.GetGridPosition(linkPositionA),
            gridPositionB = LevelGrid.Instance.GetGridPosition(linkPositionB)
        };
    }
}
