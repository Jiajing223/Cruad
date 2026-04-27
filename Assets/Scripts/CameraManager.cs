using System;
using System.Collections;
using System.Collections.Generic;

using System.Runtime;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private bool hasStoredTransform = false;
    private Coroutine followCoroutine;
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
            case MoveAction moveAction:
                if (!hasStoredTransform)
                {
                    originalPosition = actionCameraGameObject.transform.position;
                    originalRotation = actionCameraGameObject.transform.rotation;
                    hasStoredTransform = true;
                }
                if (followCoroutine != null)
                {
                    StopCoroutine(followCoroutine);
                }

                followCoroutine = StartCoroutine(FollowUnit(moveAction.GetUnit()));
                ShowActionCamera();
                break;
            case ShootAction shootAction:
                SetupActionCamera(shootAction.GetUnit(), shootAction.GetTargetUnit(), sideOffset: 0.5f, backwardOffset: 1f);
                break;
            case SwordAction swordAction:
                SetupActionCamera(swordAction.GetUnit(), swordAction.GetTargetUnit(), sideOffset: 0.7f, backwardOffset: 1.2f);
                break;
            case FireBallAction fireBallAction:
                SetupActionCamera(fireBallAction.GetUnit(), fireBallAction.GetTargetUnit(), sideOffset: 0.5f, backwardOffset: 1f);
                break;
        }
    }
    private IEnumerator FollowUnit(Unit unit)
    {
        float height = 0.9f;
        float shoulderOffset = 1f;
        float distance = 1f;

        while (unit != null)
        {
            Vector3 forward = unit.transform.forward;
            Vector3 right = unit.transform.right;

            Vector3 targetPosition =
                unit.GetWorldPosition() +
                Vector3.up * height -
                forward * distance +
                right * shoulderOffset;

            actionCameraGameObject.transform.position = targetPosition;

            Vector3 lookTarget =
                unit.GetWorldPosition() +
                Vector3.up * height +
                forward * 1.5f;

            actionCameraGameObject.transform.LookAt(lookTarget);

            yield return null;
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
            case MoveAction moveAction:
                if (followCoroutine != null)
                {
                    StopCoroutine(followCoroutine);
                    followCoroutine = null;
                }

                if (hasStoredTransform)
                {
                    actionCameraGameObject.transform.position = originalPosition;
                    actionCameraGameObject.transform.rotation = originalRotation;
                    hasStoredTransform = false;
                }

                HideActionCamera();
                break;
            case ShootAction shootAction:
                HideActionCamera();
                break;
            case SwordAction swordAction:
                HideActionCamera();
                break;
            case FireBallAction fireBallAction:
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
