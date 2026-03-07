using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class ProbabilityUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI probabilityText;
    private Color highChanceColor = Color.green;   // >= 80%
    private Color mediumChanceColor = Color.yellow;  // >= 40%
    private Color lowChanceColor = Color.red;     // <  40%

    void Start()
    {
        UnitActionSystem.Instance.OnUnitSelectingTarget += UnitActionSystem_OnUnitSelectingTarget;
        gameObject.SetActive(false);
    }

    private void UnitActionSystem_OnUnitSelectingTarget(object sender, bool isSelectingTarget)
    {
        gameObject.SetActive(isSelectingTarget);
        ShootAction shootAction = UnitActionSystem.Instance.GetSelectedAction() as ShootAction;
        if (shootAction == null) return;

        GridPosition shooterGrid = UnitActionSystem.Instance.GetSelectedUnit().GetGridPosition();
        GridPosition targetGrid = UnitActionSystem.Instance.GetTempTargetPosition();
        Unit targetUnit = LevelGrid.Instance.GetUnitOnGridPosition(targetGrid);

        if (targetUnit == null) return;

        float hitChance = shootAction.GetHitChance(shooterGrid, targetUnit);
        
        probabilityText.text  = $"Hit Chance: {Mathf.RoundToInt(hitChance * 100f)}%";

        if(hitChance >= 0.8f)
        {
            probabilityText.color = highChanceColor;
        } else if (hitChance >= 0.7f)
        {
            probabilityText.color = mediumChanceColor;
        } else
        {
            probabilityText.color = lowChanceColor;
        }

    }
}
