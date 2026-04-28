using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class MoveAction : BaseAction
{
    public event EventHandler OnStartMoving;
    public event EventHandler OnStopMoving;
    public event EventHandler<OnChangedFloorsStartedEventArgs> OnChangedFloorsStarted;
    [SerializeField] private AudioSource moveAudioSource;
    public class OnChangedFloorsStartedEventArgs : EventArgs
    {
        public GridPosition unitGridPosition;
        public GridPosition targetGridPosition;
    }

    private List<Vector3> positionList;
    private int currentPositionIndex = 0;
    [SerializeField] private int maxMoveDistance;

    private bool isChangingFloors;
    private float differentFloorsTeleportTimer;
    private float differentFloorsTeleportTimerMax = 1f;


    protected override void Awake()
    {
        base.Awake();
    }
    private void Update()
    {
        if (!isActive)
        {
            return;
        }
        Vector3 targetPosition = positionList[currentPositionIndex];

        if (isChangingFloors)
        {
            differentFloorsTeleportTimer -= Time.deltaTime;

            Vector3 targetSameFloorPosition = targetPosition;
            targetSameFloorPosition.y = transform.position.y;
            Vector3 rotateDirection = (targetSameFloorPosition - transform.position).normalized;
            transform.forward = Vector3.Slerp(transform.forward, rotateDirection, Time.deltaTime * 10f);
            if (differentFloorsTeleportTimer < 0f)
            {
                isChangingFloors = false;
                transform.position = targetPosition;
            }
        }
        else
        {
            Vector3 moveDirection = (targetPosition - transform.position).normalized;
            float rotateSpeed = 10f;
            // Rotate semi-linearly the unit to face the movement direction
            transform.forward = Vector3.Slerp(transform.forward, moveDirection, Time.deltaTime * rotateSpeed);
        
            float moveSpeed = 5f;
            transform.position += moveDirection * moveSpeed * Time.deltaTime;
        }

        float stoppingDistance = 0.1f;
        // Move the unit towards the target position
        if (targetPosition != null && Vector3.Distance(transform.position, targetPosition) < stoppingDistance)
        {
            currentPositionIndex++;
            if (currentPositionIndex >= positionList.Count)
            {
                if (positionList.Count >= 2)
                {
                    GridPosition finalGridPosition =
                        LevelGrid.Instance.GetGridPosition(positionList[positionList.Count - 1]);

                    GridPosition previousGridPosition =
                        LevelGrid.Instance.GetGridPosition(positionList[positionList.Count - 2]);

                    FaceGridPosition(finalGridPosition, previousGridPosition);
                }
                // Reached final position
                OnStopMoving?.Invoke(this, EventArgs.Empty);
                if (moveAudioSource != null)
                {
                    moveAudioSource.Stop();
                }
                unit.UpdateCoverState();
                FogOfWarManager.Instance?.RefreshVisibility();
                ActionComplete();
                return;
            }
            else
            {
                targetPosition = positionList[currentPositionIndex];
                GridPosition targetGridPosition = LevelGrid.Instance.GetGridPosition(targetPosition);
                GridPosition unitGridPosition = LevelGrid.Instance.GetGridPosition(transform.position);

                if (targetGridPosition.floor != unitGridPosition.floor)
                {
                    isChangingFloors = true;
                        differentFloorsTeleportTimer = differentFloorsTeleportTimerMax;
                    
                    OnChangedFloorsStarted?.Invoke(this, new OnChangedFloorsStartedEventArgs
                    {
                        unitGridPosition = unitGridPosition,
                        targetGridPosition = targetGridPosition
                    });
                }
                }
            }
        }
        
    public override void TakeAction(GridPosition gridPosition, Action onActionComplete)
    {
        List<GridPosition> pathGridPositionList = PathFinding.Instance.FindPath(
            unit.GetGridPosition(),
            gridPosition,
            out int pathLength
        );
        currentPositionIndex = 0;
        positionList = new List<Vector3>();

        foreach (GridPosition pathGridPosition in pathGridPositionList)
        {
            positionList.Add(LevelGrid.Instance.GetWorldPosition(pathGridPosition));
        }
        // positionList.RemoveAt(0); // Remove the starting position
        // Move the unit to the target position
        OnStartMoving?.Invoke(this, EventArgs.Empty);

        if (moveAudioSource != null)
        {
            moveAudioSource.Play();
        }
        ActionStart(onActionComplete);
    }

    public override List<GridPosition> GetValidActionGridPositionList()
    {
        GridPosition unitGridPosition = unit.GetGridPosition();

        List<GridPosition> validGridPositionList = new List<GridPosition>();

        // 只允许曼哈顿距离不超过maxMoveDistance的格子
        for (int x = -maxMoveDistance; x <= maxMoveDistance; x++)
        {
            for (int z = -maxMoveDistance; z <= maxMoveDistance; z++)
            {
                for (int floor = -maxMoveDistance; floor <= maxMoveDistance; floor++)
                {
                    // 只允许曼哈顿距离（|x|+|z|）不超过maxMoveDistance的格子
                    if (Mathf.Abs(x) + Mathf.Abs(z) > maxMoveDistance)
                    {
                        continue;
                    }
                    GridPosition offsetGridPosition = new GridPosition(x, z, floor);
                    GridPosition testGridPosition = unitGridPosition + offsetGridPosition;
                    
                    if (!LevelGrid.Instance.IsValidGridPosition(testGridPosition))
                    {
                        // Not valid position
                        continue;
                    }
                    if (unitGridPosition == testGridPosition)
                    {
                        // Same position
                        continue;
                    }
                    if (LevelGrid.Instance.HasAnyUnitOnGridPosition(testGridPosition))
                    {
                        // Grid position already occupied with another unit
                        continue;
                    }

                    if (!PathFinding.Instance.IsWalkableGridPosition(testGridPosition))
                    {
                        // Not walkable
                        continue;
                    }

                    if (!PathFinding.Instance.HasPath(unitGridPosition, testGridPosition))
                    {
                        // Path doesnt exist
                        continue;
                    }
                    int pathFindingDistanceMultiplier = 10;
                    if (PathFinding.Instance.GetPathLength(unitGridPosition, testGridPosition) > maxMoveDistance * pathFindingDistanceMultiplier)
                    {
                        // path leng is too long 
                        continue;
                    }
                    // Adds the path
                    validGridPositionList.Add(testGridPosition);
                }
            }
        }

        return validGridPositionList;
    }

    public override string GetActionName()
    {
        return "Move";
    }

    public override EnemyAIAction GetBestEnemyAIAction(GridPosition gridPosition)
    {
        int targetCountAtGridPosition = 0;
        if (unit.GetShootAction() != null)
        {
            targetCountAtGridPosition = unit.GetShootAction().GetTargetCountAtPosition(gridPosition);
        }
        
        // 射击目标优先
        if (targetCountAtGridPosition > 0)
        {
            return new EnemyAIAction
            {
                gridPosition = gridPosition,
                actionValue = targetCountAtGridPosition * 10,
            };
        }
        else
        {
            List<Unit> playerUnits = UnitManager.Instance.GetFriendlyUnitList();
            int closestDistance = int.MaxValue;

            foreach (Unit playerUnit in playerUnits)
            {
                GridPosition playerGridPosition = playerUnit.GetGridPosition();
                
                int distance = Mathf.Abs(gridPosition.x - playerGridPosition.x) + 
                            Mathf.Abs(gridPosition.z - playerGridPosition.z);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                }
                
            }
            
            
            int moveValue = 100 - closestDistance;
            
            return new EnemyAIAction
            {
                gridPosition = gridPosition,
                actionValue = Mathf.Max(1, moveValue),
            };
        }
    }
}

