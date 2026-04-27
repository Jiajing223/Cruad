using System;
using System.Collections;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private GameFinishUI gameFinishUI;

    private void Start()
    {
        ObjectiveUI objectiveUI = FindObjectOfType<ObjectiveUI>();
        objectiveUI.ShowObjective("Kill all enemys");
        ObjectiveUI.OnObjectiveCompleted += ObjectiveUI_OnObjectiveCompleted;
        StartCoroutine(InitAfterStart());

    }
    private IEnumerator InitAfterStart()
    {
        yield return null;
        yield return null;
        ObjectiveUI objectiveUI = FindObjectOfType<ObjectiveUI>();
        objectiveUI.SetEnemyCount(UnitManager.Instance.GetEnemyUnitList().Count);
        objectiveUI.ShowObjective("Kill all enemys");
    }
    private void OnDestroy()
    {
        ObjectiveUI.OnObjectiveCompleted -= ObjectiveUI_OnObjectiveCompleted;
    }

    private void ObjectiveUI_OnObjectiveCompleted(object sender, EventArgs e)
    {
        gameFinishUI.Show();
    }
}