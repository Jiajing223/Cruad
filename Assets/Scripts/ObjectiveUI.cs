using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectiveUI : MonoBehaviour
{
    public static event EventHandler OnObjectiveCompleted;

    public GameObject objectivePanel;
    public GameObject[] objectiveText;
    private int enemyCount;
    private int aliveEnemyCount;
    private string currentObjective;

    private void Start()
    {
        objectivePanel.SetActive(true);
        Unit.OnEnemyUnitDead += Unit_OnEnemyUnitDead;
    }


    private void OnDestroy()
    {
        Unit.OnEnemyUnitDead -= Unit_OnEnemyUnitDead;
    }

    private void Unit_OnEnemyUnitDead(object sender, EventArgs e)
    {
        aliveEnemyCount--;

        if (currentObjective == "Kill all enemys")
        {
            ShowObjective(currentObjective);

            if (aliveEnemyCount <= 0)
            {
                objectiveText[0].GetComponent<TMPro.TextMeshProUGUI>().color = Color.green;
                OnObjectiveCompleted?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    public void ShowObjective(string objective)
    {
        currentObjective = objective;

        switch (objective)
        {
            case "Kill all enemys":
                objectiveText[0].GetComponent<TMPro.TextMeshProUGUI>().text =
                    "Kill all enemies: " + (enemyCount - aliveEnemyCount) + "/" + enemyCount;
                break;
            case "Open the door":
                objectiveText[0].GetComponent<TMPro.TextMeshProUGUI>().text = "Open the door";
                break;
            case "Defeat the boss":
                objectiveText[0].GetComponent<TMPro.TextMeshProUGUI>().text = "Defeat the boss";
                break;
            default:
                objectiveText[0].GetComponent<TMPro.TextMeshProUGUI>().text = objective;
                break;
        }

        objectivePanel.SetActive(true);
    }

    public void HideObjective()
    {
        objectivePanel.SetActive(false);
    }
    public void SetEnemyCount(int count)
    {
        enemyCount = count;
        aliveEnemyCount = count;
    }
}