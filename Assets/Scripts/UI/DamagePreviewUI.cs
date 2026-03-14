using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class DamagePreviewUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI damagePreviewText;

    private void Awake()
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

        Unit shooterUnit = UnitActionSystem.Instance.GetSelectedUnit();
        Unit targetUnit = LevelGrid.Instance.GetUnitOnGridPosition(args.targetGridPosition);
        if (targetUnit == null) return;

        int damage = targetUnit.GetComponent<HealthSystem>().GetCalculatedDamage(shooterUnit, targetUnit);
        damagePreviewText.text = $"Damage: {damage}";

        gameObject.SetActive(true);
    }
    void Update()
    {
        
    }
}
