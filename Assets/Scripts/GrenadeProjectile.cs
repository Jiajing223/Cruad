using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public class GrenadeProjectile : MonoBehaviour
{
    [SerializeField] private Transform grenadeExplosionVfxPrefab; 
    [SerializeField] private TrailRenderer trailRenderer;          
    [SerializeField] private AnimationCurve arcYAnimationCurve;    

    public static event EventHandler OnAnyGrenadeExploded;

    private Vector3 targetPosition;          
    private Action OnGrenadeBehaviourComplete; 
    private float totalDistance;  
    private Vector3 positionXZ;   // 手榴弹在XZ平面上的位置（忽略Y轴）
    private int grenadeDamage = 30;

    private void Update()
    {
        // 计算朝向目标的方向向量（在XZ平面上）
        Vector3 moveDir = (targetPosition - positionXZ).normalized;


        float moveSpeed = 15f;
        positionXZ += moveDir * moveSpeed * Time.deltaTime;

        // 判断是否到达目标的距离阈值
        float reachedTargetDistance = .2f;
        float distance = Vector3.Distance(positionXZ, targetPosition);

        // 计算手榴弹的抛物线高度
        float maxHeight = totalDistance / 3f;  
        float distanceNormalized = 1 - distance / totalDistance;
        // 通过动画曲线和最大高度计算当前Y轴位置
        float positionY = arcYAnimationCurve.Evaluate(distanceNormalized) * maxHeight;

        transform.position = new Vector3(positionXZ.x, positionY, positionXZ.z);

        // if reaches target pos
        if (Vector3.Distance(positionXZ, targetPosition) < reachedTargetDistance)
        {
            float damageRadius = 3f;
            Collider[] colliderArray = Physics.OverlapSphere(targetPosition, damageRadius);

            foreach (Collider collider in colliderArray)
            {
                // Deal dmg
                if (collider.TryGetComponent<Unit>(out Unit targetUnit))
                {
                    targetUnit.Damage(targetUnit, grenadeDamage);
                    DamagePopUpManager.Instance.ShowDamage(targetUnit.GetWorldPosition(), grenadeDamage);
                }

                // Destroy destructable
                if (collider.TryGetComponent<DestructableCrate>(out DestructableCrate destructableCrate))
                {
                    DamagePopUpManager.Instance.ShowText(destructableCrate.transform.position, "Broken!");
                    destructableCrate.Damage();
                }
            }

            // Trigger explosion event, spawn vfx, destroy grenade
            OnAnyGrenadeExploded?.Invoke(this, EventArgs.Empty);
            trailRenderer.transform.parent = null;
            Instantiate(grenadeExplosionVfxPrefab, targetPosition + Vector3.up * 1f, Quaternion.identity);
            Destroy(gameObject);


            OnGrenadeBehaviourComplete();
        }
    }

    public void Setup(GridPosition targetGridPosition, Action OnGrenadeBehaviourComplete)
    {
        this.OnGrenadeBehaviourComplete = OnGrenadeBehaviourComplete;
        // Convert grid position to world position
        targetPosition = LevelGrid.Instance.GetWorldPosition(targetGridPosition);

        positionXZ = transform.position;
        positionXZ.y = 0;
        totalDistance = Vector3.Distance(positionXZ, targetPosition);
    }
}