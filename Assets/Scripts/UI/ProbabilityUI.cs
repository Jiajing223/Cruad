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
        BaseAction action = UnitActionSystem.Instance.GetSelectedAction();
        Unit shooter = UnitActionSystem.Instance.GetSelectedUnit();
        Unit target = LevelGrid.Instance.GetUnitOnGridPosition(args.targetGridPosition);

        if (target == null) return;

        float? hitChance = action.GetHitChancePreview(shooter.GetGridPosition(), target);

        if (hitChance == null)
        {
            gameObject.SetActive(false);
            return;
        }

        float value = hitChance.Value;

        probabilityText.text = $"Hit Chance: {Mathf.RoundToInt(value * 100f)}%";
        probabilityText.color = value >= 0.8f ? highChanceColor 
                            : value >= 0.7f ? mediumChanceColor 
                            : lowChanceColor;

        gameObject.SetActive(true);
    }
}
