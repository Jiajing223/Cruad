using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class UnitActionSystem : MonoBehaviour
{
    public static UnitActionSystem Instance { get; private set; }
    public event EventHandler OnSelectedUnitChanged;
    public event EventHandler<ShootSelectionEventArgs> OnUnitSelectingTarget;
    public class ShootSelectionEventArgs : EventArgs
    {
        public bool isSelectingTarget;
        public GridPosition targetGridPosition;
    }
    public event EventHandler OnSelectedActionChanged;
    public event EventHandler<BaseAction> OnAblityActionSelected;
    public event EventHandler<bool> OnBusyChanged;

    public event EventHandler OnActionStarted;
    // The currently selected unit
    [SerializeField] private Unit selectedUnit;
    // The currently selected action
    private BaseAction selectedAction;
    // LayerMask for raycasting units
    [SerializeField] private LayerMask unitLayerMask;

    // Whether the system is busy (during an action animation)
    private bool isBusy;
    private bool isSelectingTarget;

    private GridPosition tempTargetPosition;
    private Vector3 originalDirection;
    private Vector3 targetRotationDirection;
    private bool isRotating;
    private float rotateSpeed = 8f;

    // Singleton initialization
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
    // Initialize and set the initial selected unit
    private void Start()
    {
        SetSelectedUnit(selectedUnit);
        SelectionShootUI.Instance.OnTargetSelectionConfirm += UnitActionSystem_OnTargetSelectionConfirm;
        SelectionNoShootUI.Instance.OnTargetSelectionCancel += UnitActionSystem_OnTargetSelectionCancel;
    }

    // Main update loop: handles unit selection and actions
    private void Update()
    {
        // Smoothly rotate the unit toward the target direction
        if (isRotating)
        {
            selectedUnit.transform.forward = Vector3.Slerp(selectedUnit.transform.forward, targetRotationDirection, Time.deltaTime * rotateSpeed);
            // Stop rotating once close enough to avoid infinite micro adjustment
            if (Vector3.Angle(selectedUnit.transform.forward, targetRotationDirection) < 0.5f)
            {
                selectedUnit.transform.forward = targetRotationDirection;
                isRotating = false;
            }
        }
        
        if (isBusy)
        {
            return;
        }

        if(!TurnSystem.Instance.IsPlayerTurn())
        {
            return;
        }

        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }
        if (TryHandleUnitSelection())
        {
            return;
        }

        HandleSelectedAction();
        UpdateThrustDirection();
    }
    // Handles execution logic for the currently selected action
    private void HandleSelectedAction()
    {
        if (InputManager.Instance.IsMouseButtonDownThisFrame())
        {
            // Handles clicking into a model
            Ray ray = Camera.main.ScreenPointToRay(InputManager.Instance.GetMouseScreenPosition());
            if (Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, unitLayerMask))
            {
                if (hit.transform.TryGetComponent<Unit>(out Unit clickedUnit))
                {
                    // Only performs action when its shoot or sword
                    if (clickedUnit.IsEnemy())
                    {
                        GridPosition enemyGridPosition = LevelGrid.Instance.GetGridPosition(clickedUnit.GetWorldPosition());
                        if (selectedAction.isValidActionGridPosition(enemyGridPosition))
                        {
                            if (selectedAction.GetActionName() == "Shoot")
                            {
                                HandleShootSelection(enemyGridPosition);
                            }
                            else if (selectedAction.GetActionName() == "Sword")
                            {
                                if (selectedUnit.TrySpendActionPointsToTakeAction(selectedAction))
                                {
                                    SetBusy();
                                    selectedAction.TakeAction(enemyGridPosition, ClearBusy);
                                }
                            }
                            OnActionStarted?.Invoke(this, EventArgs.Empty);
                        }
                    }
                    return;
                }
            }

            // Handles clicking into a grid
            GridPosition mouseGridPosition = LevelGrid.Instance.GetGridPosition(MouseWorld.GetPositionOnlyHitVisible());

            if (selectedAction.isValidActionGridPosition(mouseGridPosition))
            {
                if (selectedAction.GetActionName() == "Shoot" || selectedAction.GetActionName() == "Fireball")
                {
                    HandleShootSelection(mouseGridPosition);
                } 
                else
                {
                    if (selectedUnit.TrySpendActionPointsToTakeAction(selectedAction))
                    {
                        SetBusy();
                        selectedAction.TakeAction(mouseGridPosition, ClearBusy);
                    }
                }
            }
            OnActionStarted?.Invoke(this, EventArgs.Empty);
        }
    }
    // Checks if the mouse clicked a unit and switches the selected unit
    private bool TryHandleUnitSelection()
    {
        if (InputManager.Instance.IsMouseButtonDownThisFrame())
        {
            // Get the selected object using Raycast
            Ray ray = Camera.main.ScreenPointToRay(InputManager.Instance.GetMouseScreenPosition());
            if (Physics.Raycast(ray, out RaycastHit raycastHit, float.MaxValue, unitLayerMask))
            {
                // Getting the Unit component of the object
                if (raycastHit.transform.TryGetComponent<Unit>(out Unit unit))
                {
                    if (unit == selectedUnit)
                    {
                        // Unit already selected
                        return false;
                    }
                    if (unit.IsEnemy())
                    {
                        // Cannot select enemy units
                        return false;
                    }
                    SetSelectedUnit(unit);
                    selectedUnit = unit;
                    return true;
                }
            }
        }
        return false;
    }
    private void HandleShootSelection(GridPosition mouseGridPosition)
    {
        // Store the current direction so we can restore it if the player cancels, and starts rotating
        originalDirection = selectedUnit.transform.forward;

        Unit targetUnit = LevelGrid.Instance.GetUnitOnGridPosition(mouseGridPosition);
        Vector3 aimDirection = (targetUnit.GetWorldPosition() - selectedUnit.GetWorldPosition()).normalized;
        aimDirection.y = 0f;

        targetRotationDirection = aimDirection;
        isRotating = true;

        // Starts the target selection sequence
        CameraManager.Instance.ShowShootCameraPreview(selectedUnit, mouseGridPosition);
        isSelectingTarget = true;
        tempTargetPosition = mouseGridPosition;
        OnUnitSelectingTarget?.Invoke(this, new ShootSelectionEventArgs 
        { 
            isSelectingTarget = true, 
            targetGridPosition = mouseGridPosition 
        });
    }

    private void UpdateThrustDirection()
    {
        if (selectedAction is not ThrustAction thrustAction) return;

        GridPosition unitGrid = selectedUnit.GetGridPosition();
        Vector3 unitWorld = LevelGrid.Instance.GetWorldPosition(unitGrid);
        Vector3 mouseWorld = MouseWorld.GetPositionOnlyHitVisible();

        Vector3 delta = mouseWorld - unitWorld;
        delta.y = 0f;

        GridPosition dir;
        if (Mathf.Abs(delta.x) >= Mathf.Abs(delta.z))
            dir = new GridPosition(delta.x > 0 ? 1 : -1, 0, 0);
        else
            dir = new GridPosition(0, delta.z > 0 ? 1 : -1, 0);

        thrustAction.SetMouseDirection(dir);
    }

    private void UnitActionSystem_OnTargetSelectionConfirm(object sender, EventArgs e)
    {
        // Performs the action
        if (selectedUnit.TrySpendActionPointsToTakeAction(selectedAction))
        {
            SetBusy();
            selectedAction.TakeAction(tempTargetPosition, ClearBusy);
        }
        else
        {
            UnitActionSystem_OnTargetSelectionCancel(sender, e);
            return;
        }

        // Disables the UI
        isSelectingTarget = false;
        OnUnitSelectingTarget?.Invoke(this, new ShootSelectionEventArgs 
        { 
            isSelectingTarget = false 
        });
    }
    private void UnitActionSystem_OnTargetSelectionCancel(object sender, EventArgs e)
    {
        // Rotate back to where the unit was looking before the preview
        targetRotationDirection = originalDirection;
        isRotating = true;
        // Disables the UI
        isSelectingTarget = false;
        OnUnitSelectingTarget?.Invoke(this, new ShootSelectionEventArgs 
        { 
            isSelectingTarget = false 
        });
    }
    // Set the system to busy (disables new actions)
    private void SetBusy()
    {
        isBusy = true;
        OnBusyChanged?.Invoke(this, isBusy);
    }
    // Clear the busy state (enables new actions)
    private void ClearBusy()
    {
        isBusy = false;
        OnBusyChanged?.Invoke(this, isBusy);
    }
    // Switch the selected unit and default to its MoveAction
    private void SetSelectedUnit(Unit unit)
    {
        selectedUnit = unit;
        SetSelectedAction(unit.GetMoveAction());
        OnSelectedUnitChanged?.Invoke(this, EventArgs.Empty);
    }

    // Switch the currently selected action
    public void SetSelectedAction(BaseAction baseAction)
    {
        selectedAction = baseAction;
        OnSelectedActionChanged?.Invoke(this, EventArgs.Empty);
        if (baseAction.GetActionName() == "Ability")
        {
            OnAblityActionSelected?.Invoke(this, baseAction);
        }
    }
    // Get the currently selected action
    public BaseAction GetSelectedAction()
    {
        return selectedAction;
    }

    // Get the currently selected unit
    public Unit GetSelectedUnit()
    {
        return selectedUnit;
    }

    public GridPosition GetTempTargetPosition() => tempTargetPosition;

}
