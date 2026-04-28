using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EnemyAI : MonoBehaviour
{
    // 敌人AI状态枚举
    private enum State
    {
        WaitingForEnemyTurn,  // 等待敌人回合开始
        TakingTurn,           // 正在执行回合行动
        Busy                  // 正在处理行动中（等待行动完成）
    }

    private State state;      // 当前AI状态
    private float timer;      // 用于控制行动间隔的计时器

    private void Awake()
    {
        // 初始化状态为等待敌人回合
        state = State.WaitingForEnemyTurn;
    }

    private void Start()
    {
        // 订阅回合变更事件
        TurnSystem.Instance.OnTurnChanged += TurnSystem_OnTurnChanged;
    }

    private void Update()
    {
        // 如果是玩家回合，不执行任何AI逻辑
        if (TurnSystem.Instance.IsPlayerTurn())
        {
            return;
        }

        // 根据当前状态执行相应逻辑
        switch (state)
        {
            case State.WaitingForEnemyTurn:
                // 等待状态，不执行任何操作
                break;

            case State.TakingTurn:
                // 执行回合状态：倒计时，然后尝试执行AI行动
                timer -= Time.deltaTime;
                if (timer <= 0f)
                {
                    // 计时结束，进入忙碌状态并尝试执行AI行动
                    state = State.Busy;

                    // 尝试执行敌人AI行动，如果成功则保持忙碌状态，否则切换到下一回合
                    if (TryTakeEnemyAIAction(SetStateTakingTurn))
                    {
                        state = State.Busy; // 这行可能冗余，因为上面已经设置了
                    }
                    else
                    {
                        // 没有可执行的行动，结束当前回合
                        TurnSystem.Instance.NextTurn();
                    }
                }
                break;

            case State.Busy:
                // 忙碌状态：等待当前行动完成，不执行新操作
                break;
        }
    }

    private void SetStateTakingTurn()
    {
        // 设置状态为正在执行回合，并重置计时器
        state = State.TakingTurn;
        timer = 1f; // 设置行动间隔时间
    }

    private void TurnSystem_OnTurnChanged(object sender, EventArgs e)
    {
        // 如果切换到敌人回合，开始执行AI逻辑
        if (!TurnSystem.Instance.IsPlayerTurn())
        {
            state = State.TakingTurn;
            timer = 2f; // 敌人回合开始时的初始延迟
        }
    }

    private bool TryTakeEnemyAIAction(Action OnEnemyAIActionComplete)
    {
        // 遍历所有敌人单位
        foreach (Unit enemyUnit in UnitManager.Instance.GetEnemyUnitList())
        {
            // 尝试为当前单位执行行动
            if (TryTakeEnemyAIAction(enemyUnit, OnEnemyAIActionComplete))
            {
                return true; // 成功执行行动
            }
        }
        return false; // 所有单位都没有可执行的行动
    }

    private bool TryTakeEnemyAIAction(Unit enemyUnit, Action OnEnemyAIActionComplete)
        {
            EnemyAIAction bestEnemyAIAction = null;
            BaseAction bestBaseAction = null;

            foreach (BaseAction baseAction in enemyUnit.GetBaseActionArray())
            {
                if (!baseAction.GetIsEnemyUsable()) continue;
                if (!enemyUnit.CanSpendActionPointsToTakeAction(baseAction))
                {
                    Debug.Log($"{enemyUnit.name} can't afford {baseAction.GetActionName()}");
                    continue;
                }

                EnemyAIAction testEnemyAIAction = baseAction.GetBestEnemyAIAction();
                Debug.Log($"{enemyUnit.name} evaluating {baseAction.GetActionName()}: {(testEnemyAIAction == null ? "null" : testEnemyAIAction.actionValue.ToString())}");

                if (bestBaseAction == null)
                {
                    bestEnemyAIAction = testEnemyAIAction;
                    bestBaseAction = baseAction;
                }
                else if (testEnemyAIAction != null && testEnemyAIAction.actionValue > bestEnemyAIAction.actionValue)
                {
                    bestEnemyAIAction = testEnemyAIAction;
                    bestBaseAction = baseAction;
                }
            }

            Debug.Log($"{enemyUnit.name} best action: {(bestBaseAction == null ? "none" : bestBaseAction.GetActionName())}");
            
            if (bestEnemyAIAction != null && enemyUnit.TrySpendActionPointsToTakeAction(bestBaseAction))
            {
                bestBaseAction.TakeAction(bestEnemyAIAction.gridPosition, OnEnemyAIActionComplete);
                return true;
            }

            return false;
        }
}
