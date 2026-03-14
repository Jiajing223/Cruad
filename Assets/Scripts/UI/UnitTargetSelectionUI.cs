using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitTargetSelectionUI : MonoBehaviour
{
    private void Start()
    {
        UnitActionSystem.Instance.OnUnitSelectingTarget += UnitActionSystem_OnUnitSelectingTarget;
        Hide();
    }


    private void UnitActionSystem_OnUnitSelectingTarget(object sender, UnitActionSystem.ShootSelectionEventArgs args)
    {
        if (args.isSelectingTarget)
        {
            Show();
        }
        else
        {
            Hide();
        }
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }


}
