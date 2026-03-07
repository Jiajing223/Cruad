using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class DamagePreviewUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI damagePreviewText;

    // Start is called before the first frame update
    void Start()
    {
        UnitActionSystem.Instance.OnUnitSelectingTarget += UnitActionSystem_OnUnitSelectingTarget;
        gameObject.SetActive(false);
    }

    private void UnitActionSystem_OnUnitSelectingTarget(object sender, bool isSelectingTarget)
    {
        gameObject.SetActive(isSelectingTarget);
        if (!isSelectingTarget) return;

        Unit shooterUnit = UnitActionSystem.Instance.GetSelectedUnit();
        GridPosition targetGrid = UnitActionSystem.Instance.GetTempTargetPosition();
        Unit targetUnit = LevelGrid.Instance.GetUnitOnGridPosition(targetGrid);

        if (targetUnit == null) return;

        int damage = targetUnit.GetComponent<HealthSystem>().GetCalculatedDamage(shooterUnit, targetUnit);
        damagePreviewText.text = $"Damage: {damage}";
    }
    void Update()
    {
        
    }
}
