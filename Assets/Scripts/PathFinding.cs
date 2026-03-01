using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Unity.VisualScripting;
using UnityEngine;

public class PathFinding : MonoBehaviour
{ 
    public static PathFinding Instance { get; private set; }

    // A*路径查找算法常量
    private const int MOVE_STRAIGHT_COST = 10;  // 直线移动成本
    private const int MOVE_DIAGONAL_COST = 14;  // 对角线移动成本
    
    [SerializeField] private LayerMask obsticlesLayerMask;  // 障碍物层遮罩
    [SerializeField] private Transform debugPrefab;         // 调试用预制体
    [SerializeField] private LayerMask floorLayerMask;  
    [SerializeField] private Transform pathFindingLinkContainer;  

    private int width;      // 网格宽度
    private int height;     // 网格高度
    private int cellSize;   // 格子大小
    private int floorAmount;
    private List<GridSystem<PathNode>> gridSystemList;  // 路径节点网格系统

    private List<PathFindingLink> pathfindingLinkList;
    private void Awake()
    {
        // 单例初始化
        if (Instance != null)
        {
            Debug.LogError("There's more than one PathFinding! " + transform + " - " + Instance);
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // 初始化路径查找系统
    public void Setup(int Width, int Height, int CellSize, int floorAmount)
    {
        this.width = Width;
        this.height = Height;
        this.cellSize = CellSize;
        this.floorAmount = floorAmount;

        gridSystemList = new List<GridSystem<PathNode>>();

        for (int floor = 0; floor < floorAmount; floor++)
        {
            GridSystem<PathNode> gridSystem = new GridSystem<PathNode>(width, height, cellSize, floor, LevelGrid.FLOOR_HEIGHT,
                (GridSystem<PathNode> gameObject, GridPosition gridPosition) => new PathNode(gridPosition));

            gridSystemList.Add(gridSystem);
        }

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                for (int floor = 0; floor < floorAmount; floor++)
                {
                    GridPosition gridPosition = new GridPosition(x, z, floor);
                    Vector3 worldPosition = LevelGrid.Instance.GetWorldPosition(gridPosition);
                    float raycastOffsetDistance = 1f;

                    GetNode(x, z, floor).SetIsWalkable(false);  // 设置不可行走

                    if (Physics.Raycast(
                        worldPosition + Vector3.up * raycastOffsetDistance,
                        Vector3.down,
                        raycastOffsetDistance * 2,
                        floorLayerMask))
                    {
                        GetNode(x, z, floor).SetIsWalkable(true);  // 设置可行走
                    }

                    // 使用射线检测障碍物
                    if (Physics.Raycast(
                        worldPosition + Vector3.down * raycastOffsetDistance,
                        Vector3.up,
                        raycastOffsetDistance * 2,
                        obsticlesLayerMask))
                    {
                        GetNode(x, z, floor).SetIsWalkable(false);  // 设置不可行走
                    }
                }
            }
        }

        pathfindingLinkList = new List<PathFindingLink>();

        foreach (Transform pathfindingLinkTransform in pathFindingLinkContainer)
        {
            if (pathfindingLinkTransform.TryGetComponent(out PathFindingLinkMonoBehaviour pathFindingLinkMonoBehaviour)){
                pathfindingLinkList.Add(pathFindingLinkMonoBehaviour.GetPathFindingLink());
            };
        }
        
    }

    // A*路径查找主算法
    public List<GridPosition> FindPath(GridPosition startGridPosition, GridPosition endGridPosition, out int pathLength)
    {
        List<PathNode> openList = new List<PathNode>();    // 待检查节点
        List<PathNode> closeList = new List<PathNode>();   // 已检查节点

        PathNode startNode = GetGridSystem(startGridPosition.floor).GetGridObject(startGridPosition);
        PathNode endNode = GetGridSystem(endGridPosition.floor).GetGridObject(endGridPosition);
        openList.Add(startNode);

        // 初始化所有节点
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                for (int floor = 0; floor < floorAmount; floor++)
                {
                    GridPosition gridPosition = new GridPosition(x, z, floor);
                    PathNode pathNode = GetGridSystem(floor).GetGridObject(gridPosition);

                    pathNode.SetGCost(int.MaxValue);  // 实际代价初始化为最大值
                    pathNode.SetHCost(0);             // 启发式代价
                    pathNode.CalculateFCost();        // 总代价
                    pathNode.ResetCameFromNode();     // 重置父节点
                }
            }
        }

        // 设置起始节点代价
        startNode.SetGCost(0);
        startNode.SetHCost(CalculateDistance(startGridPosition, endGridPosition));
        startNode.CalculateFCost();

        // A*算法主循环
        while (openList.Count > 0)
        {
            PathNode currentNode = GetLowestFCostPathNode(openList);  // 获取最小F代价节点

            // 如果到达目标节点
            if (currentNode == endNode)
            {
                pathLength = endNode.GetFCost();
                return CalculatePath(endNode);  // 计算并返回路径
            }

            openList.Remove(currentNode);
            closeList.Add(currentNode);

            // 检查所有相邻节点
            foreach (PathNode neighborNode in GetNeighborList(currentNode))
            {
                if (closeList.Contains(neighborNode)) continue;  // 跳过已检查节点

                if (!IsWalkableGridPosition(neighborNode.GetGridPosition()))
                {
                    closeList.Add(neighborNode);  
                    continue;
                }

                // 计算新的G代价
                int tentativeGCost = currentNode.GetGCost() + CalculateDistance(currentNode.GetGridPosition(), neighborNode.GetGridPosition());
                
                // 如果找到更好路径
                if (tentativeGCost < neighborNode.GetGCost())
                {
                    neighborNode.SetCameFromNode(currentNode);  // 设置父节点
                    neighborNode.SetGCost(tentativeGCost);      // 更新G代价
                    neighborNode.SetHCost(CalculateDistance(neighborNode.GetGridPosition(), endGridPosition));  // 更新H代价
                    neighborNode.CalculateFCost();               // 重新计算F代价

                    if (!openList.Contains(neighborNode))
                    {
                        openList.Add(neighborNode);  // 添加到待检查列表
                    }
                }
            }
        }

        // 未找到路径
        pathLength = 0;
        return null;
    }

    // 计算两个网格位置之间的启发式距离（对角线距离）
    public int CalculateDistance(GridPosition a, GridPosition b)
    {
        GridPosition gridPositionDistance = a - b;
        int xDistance = Mathf.Abs(gridPositionDistance.x);
        int zDistance = Mathf.Abs(gridPositionDistance.z);
        int remaining = Mathf.Abs(xDistance - zDistance);
        return Mathf.Min(xDistance, zDistance) * MOVE_DIAGONAL_COST + remaining * MOVE_STRAIGHT_COST;
    }

    // 从列表中获取F代价最小的节点
    private PathNode GetLowestFCostPathNode(List<PathNode> pathNodeList)
    {
        PathNode lowestFCostPathNode = pathNodeList[0];
        for (int i = 1; i < pathNodeList.Count; i++)
        {
            if (pathNodeList[i].GetFCost() < lowestFCostPathNode.GetFCost())
            {
                lowestFCostPathNode = pathNodeList[i];
            }
        }
        return lowestFCostPathNode;
    }

    private GridSystem<PathNode> GetGridSystem(int floor)
    {
        return gridSystemList[floor];
    }
    // 获取指定坐标的节点
    private PathNode GetNode(int x, int z, int floor)
    {
        return GetGridSystem(floor).GetGridObject(new GridPosition(x, z, floor));
    }

    // 获取当前节点的所有相邻节点（8方向）
    private List<PathNode> GetNeighborList(PathNode currentNode)
    {
        List<PathNode> neighborList = new List<PathNode>();
        GridPosition gridPosition = currentNode.GetGridPosition();

        // 检查左边相邻节点
        if (gridPosition.x - 1 >= 0)
        {
            neighborList.Add(GetNode(gridPosition.x - 1, gridPosition.z, gridPosition.floor));  // 左
            
            if (gridPosition.z - 1 >= 0)
            {
                neighborList.Add(GetNode(gridPosition.x - 1, gridPosition.z - 1, gridPosition.floor));  // 左下
            }
            if (gridPosition.z + 1 < height)
            {
                neighborList.Add(GetNode(gridPosition.x - 1, gridPosition.z + 1, gridPosition.floor));  // 左上
            }
        }

        // 检查右边相邻节点
        if (gridPosition.x + 1 < width)
        {
            neighborList.Add(GetNode(gridPosition.x + 1, gridPosition.z, gridPosition.floor));  // 右
            
            if (gridPosition.z - 1 >= 0)
            {
                neighborList.Add(GetNode(gridPosition.x + 1, gridPosition.z - 1, gridPosition.floor));  // 右下
            }
            if (gridPosition.z + 1 < height)
            {
                neighborList.Add(GetNode(gridPosition.x + 1, gridPosition.z + 1, gridPosition.floor));  // 右上
            }
        }

        // 检查上下相邻节点
        if (gridPosition.z - 1 >= 0)
        {
            neighborList.Add(GetNode(gridPosition.x, gridPosition.z - 1, gridPosition.floor));  // 下
        }
        if (gridPosition.z + 1 < height)
        {
            neighborList.Add(GetNode(gridPosition.x, gridPosition.z + 1, gridPosition.floor));  // 上
        }

        List<PathNode> totalNeighbourList = new List<PathNode>();
        totalNeighbourList.AddRange(neighborList);

        List<GridPosition> pathfindingLinkGridPositionList = GetPathFindingLinkConnectedGridPositionList(gridPosition);

        foreach (GridPosition pathfindingLinkGridPosition in pathfindingLinkGridPositionList)
        {
            totalNeighbourList.Add(
                GetNode(
                    pathfindingLinkGridPosition.x,
                    pathfindingLinkGridPosition.z,
                    pathfindingLinkGridPosition.floor
                )
            );
        }
        return totalNeighbourList;
    }
    private List<GridPosition> GetPathFindingLinkConnectedGridPositionList(GridPosition gridPosition)
    {
        List<GridPosition> gridPositionList = new List<GridPosition>();

        foreach (PathFindingLink pathFindingLink in pathfindingLinkList)
        {
            if (pathFindingLink.gridPositionA == gridPosition)
                gridPositionList.Add(pathFindingLink.gridPositionB);

            if (pathFindingLink.gridPositionB == gridPosition)
                gridPositionList.Add(pathFindingLink.gridPositionA);
        }

        return gridPositionList;
    }
    // 从终点回溯构建完整路径
    private List<GridPosition> CalculatePath(PathNode endNode)
    {
        List<PathNode> pathNodeList = new List<PathNode>();
        pathNodeList.Add(endNode);
        PathNode currentNode = endNode;
        
        // 回溯父节点链
        while (currentNode.GetCameFromNode() != null)
        {
            pathNodeList.Add(currentNode.GetCameFromNode());
            currentNode = currentNode.GetCameFromNode();
        }

        pathNodeList.Reverse();  // 反转列表得到从起点到终点的路径

        // 转换为网格位置列表
        List<GridPosition> gridPositionList = new List<GridPosition>();
        foreach (PathNode pathNode in pathNodeList)
        {
            gridPositionList.Add(pathNode.GetGridPosition());
        }
        return gridPositionList;
    }
    // Tool functions
    public bool IsWalkableGridPosition(GridPosition gridPosition, bool ignoreUnits = false)
    {
        // Grid walkability
        if (!GetGridSystem(gridPosition.floor).GetGridObject(gridPosition).GetIsWalkable())
            return false;

        if (!ignoreUnits)
        {
            List<Unit> enemyUnits = UnitManager.Instance.GetEnemyUnitList();

            foreach (Unit enemy in enemyUnits)
            {
                if (enemy.GetGridPosition() == gridPosition)
                {
                    // Tile occupied by enemy, cannot walk here
                    return false;
                }
            }
        }

        return true;
    }

    public void SetIsWalkableGridPosition(GridPosition gridPosition, bool Walkable)
    {
        GetGridSystem(gridPosition.floor).GetGridObject(gridPosition).SetIsWalkable(Walkable);
    }

    public bool HasPath(GridPosition startGridPosition, GridPosition endGridPosition)
    {
        return FindPath(startGridPosition, endGridPosition, out int pathLength) != null;
    }

    public int GetPathLength(GridPosition startGridPosition, GridPosition endGridPosition)
    {
        FindPath(startGridPosition, endGridPosition, out int pathLength);
        return pathLength;
    }
}