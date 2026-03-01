using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class LevelGrid : MonoBehaviour
{
    public const float FLOOR_HEIGHT = 3f;
    public event System.EventHandler OnAnyUnitMovedGridPosition;

    public static LevelGrid Instance { get; private set; }

    [SerializeField] private Transform debugPrefab; // Debug

    [SerializeField] private int width;   
    [SerializeField] private int height;  
    [SerializeField] private int cellSize; 
    [SerializeField] private int floorAmount; 

    private List<GridSystem<GridObject>> gridSystemList; 

    private void Awake()
    {
        // 单例初始化
        if (Instance != null)
        {
            Debug.LogError("There's more than one UnitActionSystem! " + transform + " - " + Instance);
            Destroy(gameObject);
            return;
        }
        Instance = this;

        gridSystemList = new List<GridSystem<GridObject>>();

        for (int floor = 0; floor < floorAmount; floor++)
        {
            // 创建网格系统
            GridSystem<GridObject> gridSystem = new GridSystem<GridObject>(width, height, cellSize, floor, FLOOR_HEIGHT,
                    (GridSystem<GridObject> g, GridPosition gridPosition) => new GridObject(g, gridPosition));


            gridSystemList.Add(gridSystem);
        }
    }

    private void Start()
    {
        // 初始化路径查找系统
        PathFinding.Instance.Setup(width, height, cellSize, floorAmount);
    }
    private GridSystem<GridObject> GetGridSystem(int floor)
    {
        return gridSystemList[floor];
    }
    // 在网格位置添加单位
    public void AddUnitGridPosition(GridPosition gridPosition, Unit unit)
    {
        GridObject gridObject = GetGridSystem(gridPosition.floor).GetGridObject(gridPosition);
        gridObject.AddUnit(unit);
    }

    // 获取网格位置上的所有单位
    public List<Unit> GetUnitListGridPosition(GridPosition gridPosition)
    {
        GridObject gridObject = GetGridSystem(gridPosition.floor).GetGridObject(gridPosition);
        return gridObject.GetUnitList();
    }

    // 从网格位置移除特定单位
    public void RemoveUnitGridPosition(GridPosition gridPosition, Unit unit)
    {
        GridObject gridObject = GetGridSystem(gridPosition.floor).GetGridObject(gridPosition);
        gridObject.RemoveUnit(unit);
    }

    // 从网格位置移除所有单位
    public void RemoveUnitGridPosition(GridPosition gridPosition)
    {
        GridObject gridObject = GetGridSystem(gridPosition.floor).GetGridObject(gridPosition);
        Unit unit = gridObject.GetUnit();
        gridObject.RemoveUnit(unit);
    }

    // 处理单位网格位置移动
    public void UnitMovedGridPosition(Unit unit, GridPosition fromGridPosition, GridPosition toGridPosition)
    {
        RemoveUnitGridPosition(fromGridPosition, unit);
        AddUnitGridPosition(toGridPosition, unit);
        OnAnyUnitMovedGridPosition?.Invoke(this, System.EventArgs.Empty);
    }

    public int GetFloor(Vector3 worldPosition)
    {
        return Mathf.RoundToInt(worldPosition.y / FLOOR_HEIGHT);
    }
    // 坐标转换方法
    public GridPosition GetGridPosition(Vector3 worldPosition)
    {
        int floor = GetFloor(worldPosition);
        return GetGridSystem(floor).GetGridPosition(worldPosition);
    }
    public Vector3 GetWorldPosition(GridPosition gridPosition) => GetGridSystem(gridPosition.floor).GetWorldPosition(gridPosition);

    // 网格信息获取方法
    public int GetWidth() => GetGridSystem(0).GetWidth();
    public int GetHeight() => GetGridSystem(0).GetHeight();
    public bool IsValidGridPosition(GridPosition gridPosition) {
        if (gridPosition.floor < 0 || gridPosition.floor >= floorAmount)
        {
            return false;
        }
        else
        {
            return GetGridSystem(gridPosition.floor).IsValidGridPosition(gridPosition); 
        }
    }

    public int GetFloorAmount() => floorAmount;
    // 单位查询方法
    public bool HasAnyUnitOnGridPosition(GridPosition gridPosition)
    {
        GridObject gridObject = GetGridSystem(gridPosition.floor).GetGridObject(gridPosition);
        return gridObject.HasAnyUnit();
    }

    public Unit GetUnitOnGridPosition(GridPosition gridPosition)
    {
        GridObject gridObject = GetGridSystem(gridPosition.floor).GetGridObject(gridPosition);
        return gridObject.GetUnit();
    }

    public IInteractable GetInteractableAtGridPosition(GridPosition gridPosition)
    {
        GridObject gridObject = GetGridSystem(gridPosition.floor).GetGridObject(gridPosition);
        return gridObject.GetInteractable();
    }

    public void SetInteractableAtGridPosition(GridPosition gridPosition, IInteractable interactable)
    {
        GridObject gridObject = GetGridSystem(gridPosition.floor).GetGridObject(gridPosition);
        gridObject.SetInteractable(interactable);
    }

    public bool DoesFloorExist(GridPosition gridPosition, int floor)
    {
        return floor > gridPosition.floor;
    }

    public int GetTopValidFloor(GridPosition gridPosition)
    {
        for (int f = floorAmount - 1; f >= 0; f--)
        {
            GridPosition gp = new GridPosition(gridPosition.x, gridPosition.z, f);
            if (IsValidGridPosition(gp))
            {
                return f;
            }
        }
        return -1; // No valid floor
    }
}