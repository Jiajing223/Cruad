using System;
using System.Collections.Generic;
using UnityEngine;

public class AttackBuffAction : BaseAction, IAbility
{
    private int buffAmount = 5;
    private int buffDuration = 2;

    public override void TakeAction(GridPosition gridPosition, Action onActionComplete)
    {
        ActionStart(onActionComplete);

        unit.GetUnitStat().ApplyAttackModifier(buffAmount, buffDuration);
        DamagePopUpManager.Instance.ShowText(unit.GetWorldPosition(), "ATK UP!");

        ActionComplete();
    }

    public override string GetActionName() => "ATK Buff";

    public override List<GridPosition> GetValidActionGridPositionList()
    {
        return new List<GridPosition> { unit.GetGridPosition() };
    }

    public override EnemyAIAction GetBestEnemyAIAction(GridPosition gridPosition)
    {
        return new EnemyAIAction { gridPosition = gridPosition, actionValue = 50 };
    }
}