using System;
using System.Collections;
using System.Collections.Generic;

using System.Runtime;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }
    [SerializeField] private GameObject actionCameraGameObject;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); 
            return;
        }
        Instance = this;
    }
    private void Start()
    {
        BaseAction.OnAnyActionStarted += BaseAction_OnAnyActionStarted;
        BaseAction.OnAnyActionCompleted += BaseAction_OnAnyActionCompleted;
        SelectionNoShootUI.Instance.OnTargetSelectionCancel += UnitActionSystem_OnTargetSelectionCancel;
    }

    private void SetupActionCamera(Unit attackerUnit, Unit targetUnit, float sideOffset, float backwardOffset)
    {
        Vector3 cameraCharacterHeight = Vector3.up * 0.8f;
        Vector3 direction = (targetUnit.GetWorldPosition() - attackerUnit.GetWorldPosition()).normalized;
        Vector3 shoulder = Quaternion.Euler(0, 90, 0) * direction * sideOffset;

        actionCameraGameObject.transform.position =
            attackerUnit.GetWorldPosition() +
            cameraCharacterHeight +
            shoulder -
            (direction * backwardOffset);

        actionCameraGameObject.transform.LookAt(targetUnit.GetWorldPosition() + cameraCharacterHeight);
        ShowActionCamera();
    }

    private void BaseAction_OnAnyActionStarted(object sender, System.EventArgs e)
    {
        switch (sender)
        {
            case ShootAction shootAction:
                SetupActionCamera(shootAction.GetUnit(), shootAction.GetTargetUnit(), sideOffset: 0.5f, backwardOffset: 1f);
                break;
            case SwordAction swordAction:
                SetupActionCamera(swordAction.GetUnit(), swordAction.GetTargetUnit(), sideOffset: 0.7f, backwardOffset: 1.2f);
                break;
        }
    }

    public void ShowShootCameraPreview(Unit shooterUnit, GridPosition targetPosition)
    {
        Unit targetUnit = LevelGrid.Instance.GetUnitOnGridPosition(targetPosition);
        SetupActionCamera(shooterUnit, targetUnit, sideOffset: 0.5f, backwardOffset: 1f);
    }

    private void BaseAction_OnAnyActionCompleted(object sender, System.EventArgs e)
    {
        switch(sender)
        {
            case ShootAction shootAction:
                HideActionCamera();
                break;
            case SwordAction swordAction:
                HideActionCamera();
                break;
        }
    }

    private void UnitActionSystem_OnTargetSelectionCancel(object sender, EventArgs e)
    {
        HideActionCamera();
    }
    private void ShowActionCamera()
    {
        actionCameraGameObject.SetActive(true);
    }
    private void HideActionCamera()
    {
        actionCameraGameObject.SetActive(false);
    }
}
