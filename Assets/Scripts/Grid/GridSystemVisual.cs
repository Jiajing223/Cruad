using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GridSystemVisual : MonoBehaviour
{
    public static GridSystemVisual Instance { get; private set; }

    [SerializeField] private Transform gridSystemVisualSinglePrefab;

    private GridSystemVisualSingle[,,] gridSystemVisualSingleArray;

    [Serializable]
    public struct GridVisualTypeMaterial
    {
        public GridVisualType gridVisualType;
        public Material material;
    }

    private GridPosition lastHoverGridPosition;
    private bool wasBusy;
    public enum GridVisualType
    {
        White,
        Blue,
        Red,
        RedSoft,
        Yellow,
        Hover
    }

    [SerializeField] private List<GridVisualTypeMaterial> gridVisualTypeMaterialList;
    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("There's more than one UnitActionSystem! " + transform + " - " + Instance);
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    private void Start()
    {
        gridSystemVisualSingleArray = new GridSystemVisualSingle[
            LevelGrid.Instance.GetWidth(),
            LevelGrid.Instance.GetHeight(),
            LevelGrid.Instance.GetFloorAmount()];
        for (int x = 0; x < LevelGrid.Instance.GetWidth(); x++)
        {
            for (int z = 0; z < LevelGrid.Instance.GetHeight(); z++)
            {
                for (int floor = 0; floor < LevelGrid.Instance.GetFloorAmount(); floor++)
                {
                    GridPosition gridPosition = new GridPosition(x, z, floor);
                    Transform gridSystemVisualSingleTransform =
                    Instantiate(gridSystemVisualSinglePrefab, LevelGrid.Instance.GetWorldPosition(gridPosition), Quaternion.identity);
                    gridSystemVisualSingleArray[x, z, floor] = gridSystemVisualSingleTransform.GetComponent<GridSystemVisualSingle>();
                }
            }
                
        }

        UnitActionSystem.Instance.OnSelectedActionChanged += UnitActionSystem_OnSelectedActionChanged;
        UnitActionSystem.Instance.OnBusyChanged += UnitActionSystem_OnBusyChanged;
        // LevelGrid.Instance.OnAnyUnitMovedGridPosition += LevelGrid_OnAnyUnitMovedGridPosition;
        
        StartCoroutine(DelayedUpdate());
    }
    private void Update()
    {
        if (!TurnSystem.Instance.IsPlayerTurn()) return;

        Vector3 mouseWorldPosition = MouseWorld.GetPositionOnlyHitVisible();
        if (mouseWorldPosition == Vector3.zero) return;
        // Get current mouse grid position
        GridPosition mouseGridPosition = LevelGrid.Instance.GetGridPosition(mouseWorldPosition);
        

        // Only redraw if mouse moved to a different tile
        if (mouseGridPosition == lastHoverGridPosition) return;
        lastHoverGridPosition = mouseGridPosition;

        UpdateGridVisual();

        BaseAction selectedAction = UnitActionSystem.Instance.GetSelectedAction();
        if (selectedAction.isValidActionGridPosition(mouseGridPosition))
        {
            gridSystemVisualSingleArray[
                mouseGridPosition.x,
                mouseGridPosition.z,
                mouseGridPosition.floor
            ].Show(GetGridVisualTypeMaterial(GridVisualType.Hover));
        }
    }
    private void UnitActionSystem_OnBusyChanged(object sender, bool e)
    {
        UpdateGridVisual();
    }

    private IEnumerator DelayedUpdate()
    {
        yield return null; // Wait one frame
        UpdateGridVisual();
    }
    
    private void LevelGrid_OnAnyUnitMovedGridPosition(object sender, EventArgs e)
    {

        UpdateGridVisual();
    }
    private void UnitActionSystem_OnSelectedActionChanged(object sender, System.EventArgs e)
    {
        UpdateGridVisual();
    }

    public void HideAllGridPosition()
    {
        for (int x = 0; x < LevelGrid.Instance.GetWidth(); x++)
        {
            for (int z = 0; z < LevelGrid.Instance.GetHeight(); z++)
            {
                for (int floor = 0; floor < LevelGrid.Instance.GetFloorAmount(); floor++)
                {
                    gridSystemVisualSingleArray[x, z, floor].Hide();
                }
                
            }
        }
    }

    private void ShowGridPositionRange(GridPosition gridPosition, int range, GridVisualType gridVisualType, ShootAction shootAction = null)
    {
        List<GridPosition> gridPositionList = new List<GridPosition>();
        Vector3 unitWorldPosition = LevelGrid.Instance.GetWorldPosition(gridPosition);
        float unitShoulderHeight = 1.0f;
        for (int x = -range; x <= range; x++)
        {
            for (int z = -range; z <= range; z++)
            {   
                
                for (int floor = -range; floor <= range; floor++)
                {
                    GridPosition testGridPosition = gridPosition + new GridPosition(x, z, floor);

                    if (!LevelGrid.Instance.IsValidGridPosition(testGridPosition)) continue;
                    if (Mathf.Abs(x) + Mathf.Abs(z) > range) continue;
                    if (!PathFinding.Instance.IsWalkableGridPosition(testGridPosition, ignoreUnits: true)) continue;

                    // Prevents unshootable grids to be shootable
                    Vector3 testWorldPosition = LevelGrid.Instance.GetWorldPosition(testGridPosition);
                    Vector3 shootDir = (testWorldPosition - unitWorldPosition).normalized;
                    float distance = Vector3.Distance(unitWorldPosition, testWorldPosition);

                    if (Physics.Raycast(
                        unitWorldPosition + Vector3.up * unitShoulderHeight,
                        shootDir,
                        distance,
                        shootAction.GetObstacleLayerMask())) continue;

                    gridPositionList.Add(testGridPosition);
                }
    
            }
        }

        ShowGridPositionList(gridPositionList, gridVisualType);
    }

    private void ShowGridPositionRangeSquare(GridPosition gridPosition, int range, GridVisualType gridVisualType)
    {
        List<GridPosition> gridPositionList = new List<GridPosition>();
        for (int x = -range; x <= range; x++)
        {
            for (int z = -range; z <= range; z++)
            {
                for (int y = -range; z <= range; z++)
                {
                    GridPosition offsetGridPosition = new GridPosition(x, z, y);
                    GridPosition testGridPosition = gridPosition + offsetGridPosition;

                    if (!LevelGrid.Instance.IsValidGridPosition(testGridPosition))
                    {
                        // Not valid position
                        continue;
                    }
                
                    gridPositionList.Add(testGridPosition);
                }
            }
        }

        ShowGridPositionList(gridPositionList, gridVisualType);
    }
    public void ShowGridPositionList(List<GridPosition> gridPositionList, GridVisualType gridVisualType)
    {
        foreach (GridPosition gridPosition in gridPositionList)
        {
            gridSystemVisualSingleArray[gridPosition.x, gridPosition.z, gridPosition.floor].Show(
                GetGridVisualTypeMaterial(gridVisualType));
        }
    }

    private void UpdateGridVisual()
    {
        HideAllGridPosition();

        BaseAction selectedAction = UnitActionSystem.Instance.GetSelectedAction();
        Unit selectedUnit = UnitActionSystem.Instance.GetSelectedUnit();

        GridVisualType gridVisualType;
        switch (selectedAction)
        {
            default:
            case MoveAction moveAction:
                gridVisualType = GridVisualType.White;
                break;
            case SpinAction spinAction:
                gridVisualType = GridVisualType.Blue;
                break;
            case ShootAction shootAction:
                gridVisualType = GridVisualType.Red;

                ShowGridPositionRange(selectedUnit.GetGridPosition(), shootAction.GetMaxShootDistance(), GridVisualType.RedSoft, shootAction);
                break; 
            case GrenadeAction grenadeAction:
                gridVisualType = GridVisualType.Yellow;
                break;
            case InteractAction interactAction:
                gridVisualType = GridVisualType.Blue;
                break;
            case SwordAction swordAction:
                gridVisualType = GridVisualType.Red;
            
                ShowGridPositionRangeSquare(selectedUnit.GetGridPosition(), swordAction.GetMaxSwordDistance(), GridVisualType.RedSoft);
                break;  
            case ThrustAction thrustAction:
                gridVisualType = GridVisualType.Red;
                break;
        }
        
        ShowGridPositionList(
            selectedAction.GetValidActionGridPositionList(), gridVisualType);
    }

    private Material GetGridVisualTypeMaterial(GridVisualType gridVisualType)
    {
        foreach (GridVisualTypeMaterial gridVisualTypeMaterial in gridVisualTypeMaterialList)
        {
            if (gridVisualTypeMaterial.gridVisualType == gridVisualType)
            {
                return gridVisualTypeMaterial.material;
            }
        }
        return null;
    }
}
