using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ThrustAction : BaseAction
{
    public static event EventHandler OnAnyThrustHit;
    public event EventHandler OnThrustActionStarted;
    public event EventHandler OnThrustActionCompleted;

    private int thrustDistance = 2;
    private GridPosition mouseDirection = new GridPosition(1, 0, 0);

    private enum State
    {
        SwingingSwordBeforeHit,
        SwingingSwordAfterHit,
    }

    private State state;
    private float stateTimer;
    private List<Unit> targetUnits = new List<Unit>();

    private void Update()
    {
        if (!isActive) return;

        stateTimer -= Time.deltaTime;

        switch (state)
        {
            case State.SwingingSwordBeforeHit:
                if (targetUnits.Count > 0)
                {
                    Vector3 aimDirection = (targetUnits[0].GetWorldPosition() - unit.GetWorldPosition()).normalized;
                    float rotateSpeed = 10f;
                    transform.forward = Vector3.Lerp(transform.forward, aimDirection, Time.deltaTime * rotateSpeed);
                }
                break;
            case State.SwingingSwordAfterHit:
                break;
        }

        if (stateTimer <= 0f)
            NextState();
    }

    private void NextState()
    {
        switch (state)
        {
            case State.SwingingSwordBeforeHit:
                state = State.SwingingSwordAfterHit;
                stateTimer = 0.6f;

                foreach (Unit targetUnit in targetUnits)
                {
                    int damage = targetUnit.GetComponent<HealthSystem>().Damage(unit, targetUnit);
                    DamagePopUpManager.Instance.ShowDamage(targetUnit.GetWorldPosition(), damage);
                    targetUnit.GetComponent<UnitStat>().ApplyDefenseModifier(-5, 2);
                    OnAnyThrustHit?.Invoke(this, EventArgs.Empty);
                }
                break;
            case State.SwingingSwordAfterHit:
                ActionComplete();
                OnThrustActionCompleted?.Invoke(this, EventArgs.Empty);
                break;
        }
    }

    public void SetMouseDirection(GridPosition direction)
    {
        mouseDirection = direction;
    }

    public override void TakeAction(GridPosition gridPosition, Action onActionComplete)
    {
        targetUnits.Clear();
        GridPosition unitGrid = unit.GetGridPosition();

        for (int i = 1; i <= thrustDistance; i++)
        {
            GridPosition checkPos = new GridPosition(
                unitGrid.x + mouseDirection.x * i,
                unitGrid.z + mouseDirection.z * i,
                unitGrid.floor
            );

            if (!LevelGrid.Instance.IsValidGridPosition(checkPos)) break;

            if (LevelGrid.Instance.HasAnyUnitOnGridPosition(checkPos))
            {
                Unit targetUnit = LevelGrid.Instance.GetUnitOnGridPosition(checkPos);
                if (targetUnit.IsEnemy() != unit.IsEnemy())
                    targetUnits.Add(targetUnit);
            }
        }

        state = State.SwingingSwordBeforeHit;
        stateTimer = 0.7f;
        OnThrustActionStarted?.Invoke(this, EventArgs.Empty);
        ActionStart(onActionComplete);
    }

    public override List<GridPosition> GetValidActionGridPositionList()
    {
        GridPosition unitGrid = unit.GetGridPosition();
        List<GridPosition> validGridPositionList = new List<GridPosition>();

        for (int i = 1; i <= thrustDistance; i++)
        {
            GridPosition checkPos = new GridPosition(
                unitGrid.x + mouseDirection.x * i,
                unitGrid.z + mouseDirection.z * i,
                unitGrid.floor
            );

            if (!LevelGrid.Instance.IsValidGridPosition(checkPos)) break;

            Unit targetUnit = LevelGrid.Instance.GetUnitOnGridPosition(checkPos);
            if (targetUnit != null && targetUnit.IsEnemy() == unit.IsEnemy()) break;

            validGridPositionList.Add(checkPos);
        }

        return validGridPositionList;
    }

    public override string GetActionName() => "Thrust";

    public override EnemyAIAction GetBestEnemyAIAction(GridPosition gridPosition)
    {
        return new EnemyAIAction
        {
            gridPosition = gridPosition,
            actionValue = 900,
        };
    }
}