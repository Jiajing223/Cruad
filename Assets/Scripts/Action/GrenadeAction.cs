
using UnityEngine;
using System;
using System.Collections.Generic;
public class GrenadeAction : BaseAction
{
    [SerializeField] private Transform grenadeProjectilePrefab;
    [SerializeField] private AudioSource grenadeThrowAudioSource;
    private int maxThrowDistance = 7;
    private void Update()
    {
        if (!isActive)
        {
            return;
        }
    }
    public override void TakeAction(GridPosition gridPosition, Action onActionComplete)
    {
        Transform grenadeProjectileTransform = Instantiate(grenadeProjectilePrefab, unit.GetWorldPosition(), Quaternion.identity);
        GrenadeProjectile grenadeProjectile = grenadeProjectileTransform.GetComponent<GrenadeProjectile>();
        grenadeProjectile.Setup(gridPosition, OnGrenadeBehaviourComplete);

        ActionStart(onActionComplete);
        if (grenadeThrowAudioSource != null)
        {
            grenadeThrowAudioSource.Play();
        }
        // StartCoroutine(PlayGrenadeThrowSound());
    }

    private System.Collections.IEnumerator PlayGrenadeThrowSound()
    {
        yield return new WaitForSeconds(0.5f);
        if (grenadeThrowAudioSource != null)        {
            grenadeThrowAudioSource.Play();
        }
    } 

    private void OnGrenadeBehaviourComplete()
    {
        ActionComplete();
    }
    public override string GetActionName()
    {
        return "Grenade";
    }

    public override List<GridPosition> GetValidActionGridPositionList()
    {
        GridPosition unitGridPosition = unit.GetGridPosition();

        List<GridPosition> validGridPositionList = new List<GridPosition>();

        for (int x = -maxThrowDistance; x <= maxThrowDistance; x++)
        {
            for (int z = -maxThrowDistance; z <= maxThrowDistance; z++)
            {
                GridPosition offsetGridPosition = new GridPosition(x, z, 0);
                GridPosition testGridPosition = unitGridPosition + offsetGridPosition;

                if (!LevelGrid.Instance.IsValidGridPosition(testGridPosition))
                {
                    // Not valid position
                    continue;
                }

                int testDistance = Mathf.Abs(x) + Mathf.Abs(z);
                if (testDistance > maxThrowDistance)
                {
                    continue;
                }

                validGridPositionList.Add(testGridPosition);
            }
        }

        return validGridPositionList;
    }

    public override EnemyAIAction GetBestEnemyAIAction(GridPosition gridPosition)
    {
        return new EnemyAIAction
        {
            gridPosition = gridPosition,
            actionValue = 0
        };
    }
}
