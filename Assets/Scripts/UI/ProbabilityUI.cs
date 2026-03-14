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

    void Awake()
    {
        UnitActionSystem.Instance.OnUnitSelectingTarget += UnitActionSystem_OnUnitSelectingTarget;
        gameObject.SetActive(false);
    }

    private void UnitActionSystem_OnUnitSelectingTarget(object sender, UnitActionSystem.ShootSelectionEventArgs args)
    {
        if (!args.isSelectingTarget)
        {
            gameObject.SetActive(false);
            return;
        }

        ShootAction shootAction = UnitActionSystem.Instance.GetSelectedAction() as ShootAction;
        if (shootAction == null) return;

        Unit targetUnit = LevelGrid.Instance.GetUnitOnGridPosition(args.targetGridPosition);
        if (targetUnit == null) return;

        GridPosition shooterGrid = UnitActionSystem.Instance.GetSelectedUnit().GetGridPosition();
        float hitChance = shootAction.GetHitChance(shooterGrid, targetUnit);

        probabilityText.text = $"Hit Chance: {Mathf.RoundToInt(hitChance * 100f)}%";
        probabilityText.color = hitChance >= 0.8f ? highChanceColor 
                            : hitChance >= 0.7f ? mediumChanceColor 
                            : lowChanceColor;

        gameObject.SetActive(true);
    }
}
