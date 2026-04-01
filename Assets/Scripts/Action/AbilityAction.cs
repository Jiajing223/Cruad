using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AbilityAction : BaseAction
{
    public static event EventHandler OnAbilityMenuOpened;

    [SerializeField] private GameObject abilityMenuUI;

    public override void TakeAction(GridPosition gridPosition, Action onActionComplete)
    {
        ActionStart(onActionComplete);
        OnAbilityMenuOpened?.Invoke(this, EventArgs.Empty);
        ActionComplete();
    }

    public override string GetActionName() => "Ability";

    public override List<GridPosition> GetValidActionGridPositionList()
    {
        return new List<GridPosition>();
    }

    public override EnemyAIAction GetBestEnemyAIAction(GridPosition gridPosition)
    {
        return new EnemyAIAction
        {
            gridPosition = gridPosition,
            actionValue = 0
        };
    }
}