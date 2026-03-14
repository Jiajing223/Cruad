using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public abstract class BaseAction : MonoBehaviour
{
    // Action will be overwritten by each action that inherits from BaseAction
    public static event EventHandler OnAnyActionStarted;
    public static event EventHandler OnAnyActionCompleted;
    private const float DistancePenaltyPerTile = 0.02f;
    private const float HighGroundBonus = 0.10f;

    protected Unit unit;
    protected bool isActive;
    protected Action onActionComplete;

    private LayerMask coverLayerMask;
    
    protected virtual void Awake()
    {
        unit = GetComponent<Unit>();
        coverLayerMask = LayerMask.GetMask("CoverObject");
    }

    public abstract string GetActionName();

    public abstract void TakeAction(GridPosition gridPosition, Action onActioncComplete);

    public virtual bool isValidActionGridPosition(GridPosition gridPosition)
    {
        List<GridPosition> validGridPositionList = GetValidActionGridPositionList();
        return validGridPositionList.Contains(gridPosition);
    }
    public abstract List<GridPosition> GetValidActionGridPositionList();
    public virtual int GetActionPointCost()
    {
        return 1;
    }

    protected void ActionStart(Action onActionComplete)
    {
        isActive = true;
        this.onActionComplete = onActionComplete;

        OnAnyActionStarted?.Invoke(this, EventArgs.Empty);
    }
    protected void ActionComplete()
    {
        isActive = false;
        onActionComplete();

        OnAnyActionCompleted?.Invoke(this, EventArgs.Empty);
    }

    public Unit GetUnit()
    {
        return unit;
    }

    public EnemyAIAction GetBestEnemyAIAction()
    {
        List<EnemyAIAction> enemyAIActionList = new List<EnemyAIAction>();
        List<GridPosition> validGridPositionList = GetValidActionGridPositionList();

        foreach (GridPosition gridPosition in validGridPositionList)
        {
            EnemyAIAction enemyAIAction = GetBestEnemyAIAction(gridPosition);
            enemyAIActionList.Add(enemyAIAction);
        }

        if (enemyAIActionList.Count > 0)
        {
            enemyAIActionList.Sort((EnemyAIAction a, EnemyAIAction b) => b.actionValue - a.actionValue);
            return enemyAIActionList[0];
        }
        else
        {
            // Not possible Enemy actions
            return null;
        }
        
    }

    protected void FaceGridPosition(GridPosition targetGridPosition, GridPosition fromGridPosition)
    {
        int dx = targetGridPosition.x - fromGridPosition.x;
        int dz = targetGridPosition.z - fromGridPosition.z;

        if (dx > 0 && dz > 0) unit.SetFacingDirection(Unit.FacingDirection.NorthEast);
        else if (dx > 0 && dz < 0) unit.SetFacingDirection(Unit.FacingDirection.SouthEast);
        else if (dx < 0 && dz > 0) unit.SetFacingDirection(Unit.FacingDirection.NorthWest);
        else if (dx < 0 && dz < 0) unit.SetFacingDirection(Unit.FacingDirection.SouthWest);
        else if (dx > 0) unit.SetFacingDirection(Unit.FacingDirection.East);
        else if (dx < 0) unit.SetFacingDirection(Unit.FacingDirection.West);
        else if (dz > 0) unit.SetFacingDirection(Unit.FacingDirection.North);
        else unit.SetFacingDirection(Unit.FacingDirection.South);
    }
    public float GetHitChance(GridPosition shooterGridPosition, Unit target, float baseHitChance, bool useDistancePenalty = true, bool useHighGround = true)
    {
        float hitChance = baseHitChance;
        Vector3 attackerWorld = LevelGrid.Instance.GetWorldPosition(shooterGridPosition) + Vector3.up * 0.1f;
        Vector3 targetWorld = LevelGrid.Instance.GetWorldPosition(target.GetGridPosition()) + Vector3.up * 0.1f;
        Vector3 shootDir = (targetWorld - attackerWorld).normalized;
        float distance = Vector3.Distance(attackerWorld, targetWorld);
        GridPosition targetGrid = target.GetGridPosition();

        // Distance penalty
        if (useDistancePenalty)
        {
            int dist = Mathf.Abs(targetGrid.x - shooterGridPosition.x)
                    + Mathf.Abs(targetGrid.z - shooterGridPosition.z);
            int penaltyTiles = Mathf.Max(0, dist - 2);
            hitChance -= penaltyTiles * DistancePenaltyPerTile;
        }

        // High-ground bonus
        if (useHighGround && shooterGridPosition.floor > targetGrid.floor)
        {
            hitChance += HighGroundBonus;
        }
        
        // Flanking bonus
        if (Physics.Raycast(attackerWorld, shootDir, out RaycastHit hit, distance, coverLayerMask))
        {
            if (hit.collider.TryGetComponent<CoverObject>(out CoverObject cover))
            {
                hitChance -= cover.GetDodgeBonus() / 100f;
            }
        }
        return Mathf.Clamp01(hitChance);
    }
    public abstract EnemyAIAction GetBestEnemyAIAction(GridPosition gridPosition);
}
