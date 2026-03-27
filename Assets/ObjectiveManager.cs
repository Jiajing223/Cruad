using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build.Content;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ObjectiveManager : MonoBehaviour
{
    private List<Unit> enemyUnitList;
    void Start()
    {
        enemyUnitList = UnitManager.Instance.GetEnemyUnitList();
        Unit.OnAnyUnitDead += Unit_OnAnyUnitDead;
    }

    private void Unit_OnAnyUnitDead(object sender, EventArgs e)
    {
        enemyUnitList = UnitManager.Instance.GetEnemyUnitList();

        if (enemyUnitList.Count == 0)
        {
            SceneManager.LoadScene("GameScene");
        }
    }
}
