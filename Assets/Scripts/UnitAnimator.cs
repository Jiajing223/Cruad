using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class UnitAnimator : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private Transform bulletProjectilePrefab;
    [SerializeField] private Transform shootPointTransform;
    [SerializeField] private Transform rifleTransform;
    [SerializeField] private Transform swordTransform;

    private bool isShooting;
    private void Awake()
    {
        if (TryGetComponent<MoveAction>(out MoveAction moveAction))
        {
            moveAction.OnStartMoving += MoveAction_OnStartMoving;
            moveAction.OnStopMoving += MoveAction_OnStopMoving;
            moveAction.OnChangedFloorsStarted += MoveAction_OnChangedFloorsStarted;
        }
        if (TryGetComponent<ShootAction>(out ShootAction shootAction))
        {
            shootAction.OnShoot += ShootAction_OnShoot;
        }
        if (TryGetComponent<SwordAction>(out SwordAction swordAction))
        {
            swordAction.OnSwordActionStarted += SwordAction_OnSwordActionStarted;
            swordAction.OnSwordActionCompleted += SwordAction_OnSwordActionCompleted;
        }
        // if (TryGetComponent<Unit>(out Unit unit))
        // {
        //     Unit.OnAnyCoverStateChanged += Unit_OnAnyCoverStateChanged;
        // }
    }
    
    private void Start()
    {
        EquipRifle();
        // UnitActionSystem.Instance.OnUnitSelectingTarget += UnitActionSystem_OnUnitSelectingTarget;
        // SelectionNoShootUI.Instance.OnTargetSelectionCancel += SelectionNoShootUI_OnTargetSelectionCancel;
        // ShootAction.OnAnyShoot += ShootAction_OnAnyShoot;
        // BaseAction.OnAnyActionCompleted += BaseAction_OnAnyActionCompleted;
    }

    private void MoveAction_OnChangedFloorsStarted(object sender, MoveAction.OnChangedFloorsStartedEventArgs e)
    {
        if (e.targetGridPosition.floor > e.unitGridPosition.floor)
        {
            animator.SetTrigger("JumpUp");
        }
        else
        {
            animator.SetTrigger("JumpDown");
        }
    }
    
    private void SwordAction_OnSwordActionStarted(object sender, System.EventArgs e)
    {
        EquipSword();
        animator.SetTrigger("SwordSlash");
    }
    private void SwordAction_OnSwordActionCompleted(object sender, System.EventArgs e)
    {
        EquipRifle();
    }
    private void MoveAction_OnStartMoving(object sender, System.EventArgs e)
    {
        animator.SetBool("isWalking", true);
        // animator.SetBool("isCovered", false);
    }
    private void MoveAction_OnStopMoving(object sender, System.EventArgs e)
    {
        animator.SetBool("isWalking", false);
    }
    private void ShootAction_OnShoot(object sender, ShootAction.OnShootEventArgs e)
    {
        animator.SetTrigger("Shoot");
        isShooting = true;
        Transform bulletProjectileTransform =
        Instantiate(bulletProjectilePrefab, shootPointTransform.position, Quaternion.identity);

        BulletProjectile bulletProjectile = bulletProjectileTransform.GetComponent<BulletProjectile>();

        float unitShoulderHeight = 1.0f;
        Vector3 targetUnitShootAtPosition = e.targetUnit.GetWorldPosition();
        targetUnitShootAtPosition.y += unitShoulderHeight;
        bulletProjectile.Setup(targetUnitShootAtPosition);
    }

    // private void UnitActionSystem_OnUnitSelectingTarget(object sender, UnitActionSystem.ShootSelectionEventArgs args)
    // {
    //     Unit thisUnit = GetComponent<Unit>();
    //     if (UnitActionSystem.Instance.GetSelectedUnit() != thisUnit) return;
    //     if (args.isSelectingTarget)
    //     {
    //         animator.ResetTrigger("TakingCover");
    //         animator.SetBool("isCovered", false);
    //     }
    //     else
    //     {
    //         thisUnit.ForceUpdateCoverState();
    //     }
    // }

    // private void SelectionNoShootUI_OnTargetSelectionCancel(object sender, System.EventArgs e)
    // {
    //     Unit thisUnit = GetComponent<Unit>();
    //     if (UnitActionSystem.Instance.GetSelectedUnit() != thisUnit) return;
    //     thisUnit.ForceUpdateCoverState();
    // }

    // private void ShootAction_OnAnyShoot(object sender, ShootAction.OnShootEventArgs e)
    // {
    //     Unit thisUnit = GetComponent<Unit>();
    //     if (e.shootingUnit == thisUnit) return;
    //     if (UnitActionSystem.Instance.GetSelectedUnit() != thisUnit) return;
    //     thisUnit.ForceUpdateCoverState();
    // }

    // private void Unit_OnAnyCoverStateChanged(object sender, EventArgs e)
    // {
    //     if (isShooting) return;
    //     Unit unit = sender as Unit;
    //     Vector3 coverDir = unit.GetCoverDirection();
    //     if (unit != gameObject.GetComponent<Unit>()) return;
    //     if (unit.IsCovered())
    //     {
    //         if (coverDir != Vector3.zero)
    //         {
    //             unit.transform.forward = -coverDir;
    //         }
    //         animator.SetTrigger("TakingCover");
    //         animator.SetBool("isCovered", true);
    //     }
    //     else
    //     {
    //         coverDir = unit.GetCoverDirection();
    //         animator.ResetTrigger("TakingCover");
    //         animator.SetBool("isCovered", false);
    //     }
    // }

    // private void BaseAction_OnAnyActionCompleted(object sender, EventArgs e)
    // {
    //     Unit thisUnit = GetComponent<Unit>();
    //     BaseAction action = sender as BaseAction;
    //     if (action == null || action.GetUnit() != thisUnit) return;
    //     isShooting = false;
    //     thisUnit.ForceUpdateCoverState();
    // }

    private void EquipSword()
    {
        swordTransform.gameObject.SetActive(true);
        rifleTransform.gameObject.SetActive(false);
    }

    private void EquipRifle()
    {
        swordTransform.gameObject.SetActive(false);
        rifleTransform.gameObject.SetActive(true);
    }
}