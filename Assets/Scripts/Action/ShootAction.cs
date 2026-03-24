using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class ShootAction : BaseAction
{
    private enum State
    {
        Aiming,
        Shooting,
        Cooloff,
    }
    public event EventHandler<OnShootEventArgs> OnShoot;
    public static event EventHandler<OnShootEventArgs> OnAnyShoot;


    // To pass the target unit to the event listener, mainly for the shooting visual
    public class OnShootEventArgs : EventArgs
    {
        public Unit targetUnit;
        public Unit shootingUnit;
        public bool isHit;
    }

    [SerializeField] private LayerMask obstaclesLayerMask;
    private State state;
    private int maxShootDistance = 12;
    private float stateTimer;
    private float baseHitChance;
    private Unit targetUnit;
    private Vector3 targetUnitPosition;
    private bool canShootBullet;
    private float rotateSpeed = 10f;
    protected override void Awake()
    {
        base.Awake();
        baseHitChance = UnitStat.GetAccuracy() / 100f;
    }

    public override string GetActionName()
    {
        return "Shoot";
    }
    public float GetHitChance(GridPosition shooterGridPosition, Unit target)
    {
        return GetHitChance(shooterGridPosition, target, baseHitChance);
    }
    public override void TakeAction(GridPosition gridPosition, Action onActionComplete)
    {
        targetUnit = LevelGrid.Instance.GetUnitOnGridPosition(gridPosition);

        // To know the facing direction
        if (targetUnit != null)
        {
            GridPosition attackerGrid = unit.GetGridPosition();
            GridPosition targetGrid = targetUnit.GetGridPosition();
            FaceGridPosition(targetGrid, attackerGrid); 
        }

        state = State.Aiming;
        float aimingStateTime = 1f;
        stateTimer = aimingStateTime;
        canShootBullet = true;

        ActionStart(onActionComplete);
    }
    public override List<GridPosition> GetValidActionGridPositionList()
    {
        GridPosition unitGridPosition = unit.GetGridPosition();
        return GetValidActionGridPositionList(unitGridPosition);
    }

    public List<GridPosition> GetValidActionGridPositionList(GridPosition gridPosition)
    {
        GridPosition unitGridPosition = unit.GetGridPosition();

        List<GridPosition> validGridPositionList = new List<GridPosition>();

        // 只允许曼哈顿距离不超过maxMoveDistance的格子
        for (int x = -maxShootDistance; x <= maxShootDistance; x++)
        {
            for (int z = -maxShootDistance; z <= maxShootDistance; z++)
            {
                for (int floor = -maxShootDistance; floor <= maxShootDistance; floor++)
                {
                // 只允许曼哈顿距离（|x|+|z|）不超过maxMoveDistance的格子
                if (Mathf.Abs(x) + Mathf.Abs(z) > maxShootDistance)
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
                
                if (!LevelGrid.Instance.HasAnyUnitOnGridPosition(testGridPosition))
                {
                    // Grid position empty
                    continue;
                }

                Unit targetUnit = LevelGrid.Instance.GetUnitOnGridPosition(testGridPosition);

                if (targetUnit.IsEnemy() == unit.IsEnemy())
                {
                    // Can't shoot own team
                    continue;
                }

                Vector3 unitWorldPosition = LevelGrid.Instance.GetWorldPosition(unitGridPosition);
                Vector3 shootDir = (targetUnit.GetWorldPosition() - unitWorldPosition).normalized;
                float unitShoulderHeight = 1.0f;
                
                if (Physics.Raycast(
                    unitWorldPosition + Vector3.up * unitShoulderHeight,
                    shootDir,
                    Vector3.Distance(unitWorldPosition, targetUnit.GetWorldPosition()),
                    obstaclesLayerMask))
                {
                    continue;
                }


                validGridPositionList.Add(testGridPosition);
            }
            } 
        }

        return validGridPositionList;
    }

    private void Update()
    {

        if (!isActive)
        {
            return;
        }

        stateTimer -= Time.deltaTime;
        switch (state)
        {
            case State.Aiming:
                Vector3 aimDirection = (targetUnit.GetWorldPosition() - unit.GetWorldPosition()).normalized;

                aimDirection.y = 0f;
                transform.forward = Vector3.Slerp(transform.forward, aimDirection, Time.deltaTime * rotateSpeed);
                break;
            case State.Shooting:

                if (canShootBullet)
                {
                    Shoot();
                    canShootBullet = false;
                }
                break;
            case State.Cooloff:
                break;
        }
        if (stateTimer <= 0f)
        {
            NextState();
        }
        
    }
    private void NextState()
    {
        switch (state)
        {
            case State.Aiming:
                state = State.Shooting;
                float shootingStateTime = 0.1f;
                stateTimer = shootingStateTime;
                break;
            case State.Shooting:
                state = State.Cooloff;
                float coolOffStateTime = 0.5f;
                stateTimer = coolOffStateTime;
                break;
            case State.Cooloff:
                ActionComplete();
                break;
        }
    }/*
    public float GetHitChance(GridPosition shooterGridPosition, Unit target)
    {
        float hitChance = baseHitChance;

        // Distance penalty 
        GridPosition targetGrid = target.GetGridPosition();
        int dist = Mathf.Abs(targetGrid.x - shooterGridPosition.x)
                          + Mathf.Abs(targetGrid.z - shooterGridPosition.z);

        // Penalty starts from distance 3 onward
        int penaltyTiles = Mathf.Max(0, dist - 2);
        hitChance -= penaltyTiles * DistancePenaltyPerTile;

        // High-ground bonus
        if (shooterGridPosition.floor > targetGrid.floor)
        {
            hitChance += HighGroundBonus;
        }

        // Cover dodge penalty
        GridObject targetGridObject = LevelGrid.Instance.GetGridObject(targetGrid);
        float coverPenalty = targetGridObject.GetCoverDodgeBonus() / 100f;
        hitChance -= coverPenalty;
        return Mathf.Clamp01(hitChance);
    }*/
    private void Shoot()
    {
        GridPosition shooterGrid = unit.GetGridPosition();
        float hitChance = GetHitChance(shooterGrid, targetUnit);
        bool isHit = UnityEngine.Random.value <= hitChance;
        OnAnyShoot?.Invoke(this, new OnShootEventArgs
        {
            targetUnit = targetUnit,
            shootingUnit = unit,
            isHit = isHit
        });
        OnShoot?.Invoke(this, new OnShootEventArgs
        {
            targetUnit = targetUnit,
            shootingUnit = unit,
            isHit = isHit
        });
        if (isHit)
        {
            int damage = targetUnit.GetComponent<HealthSystem>().GetCalculatedDamage(unit, targetUnit);
            targetUnit.Damage(unit, targetUnit);
            DamagePopUpManager.Instance.ShowDamage(targetUnit.GetWorldPosition(), damage);
        }
        else
        {
            DamagePopUpManager.Instance.ShowMiss(targetUnit.GetWorldPosition());
        }
    }

    public Unit GetTargetUnit()
    {
        return targetUnit;
    }

    public int GetMaxShootDistance()
    {
        return maxShootDistance;
    }

    public override EnemyAIAction GetBestEnemyAIAction(GridPosition gridPosition)
    {
        Unit targetUnit = LevelGrid.Instance.GetUnitOnGridPosition(gridPosition);
        float hitChance = GetHitChance(unit.GetGridPosition(), targetUnit);
        return new EnemyAIAction
        {
            gridPosition = gridPosition,
            actionValue = 100 + Mathf.RoundToInt(hitChance * (1 - targetUnit.GetHealthNormalized()) * 100f),
        };
    }

    public int GetTargetCountAtPosition(GridPosition gridPosition)
    {
        return GetValidActionGridPositionList(gridPosition).Count;
    }

    public LayerMask GetObstacleLayerMask()
    {
        return obstaclesLayerMask;
    }
}
