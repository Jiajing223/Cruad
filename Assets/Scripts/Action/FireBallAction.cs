using System;
using System.Collections.Generic;
using UnityEngine;

public class FireBallAction : BaseAction, IAbility
{
    private enum State { Aiming, Shooting, Cooloff }
    [SerializeField] private AudioSource fireballAudioSource;
    public event EventHandler<OnFireballEventArgs> OnFireball;
    public static event EventHandler<OnFireballEventArgs> OnAnyFireball;
    public class OnFireballEventArgs : EventArgs
    {
        public Unit targetUnit;
        public Unit shootingUnit;
        public bool isHit;
    }

    [SerializeField] private LayerMask obstaclesLayerMask;
    [SerializeField] private Transform fireballProjectilePrefab;
    private float projectileSpeed = 10f;

    private int maxRange = 8;
    private float bonusHitChance = 0.20f; // +20% on top of base accuracy
    private float damageMultiplier = 1f;

    private State state;
    private float stateTimer;
    private Unit targetUnit;
    private bool canShoot;
    private float rotateSpeed = 10f;

    private float baseHitChance;

    protected override void Awake()
    {
        base.Awake();
        baseHitChance = (UnitStat.GetAccuracy() / 100f) + bonusHitChance;
    }

    public override string GetActionName() => "Fireball";

    public override void TakeAction(GridPosition gridPosition, Action onActionComplete)
    {
        targetUnit = LevelGrid.Instance.GetUnitOnGridPosition(gridPosition);

        state = State.Aiming;
        stateTimer = 1f;
        canShoot = true;

        ActionStart(onActionComplete);
    }

    private void Update()
    {
        if (!isActive) return;

        stateTimer -= Time.deltaTime;

        switch (state)
        {
            case State.Aiming:
                Vector3 aimDirection = (targetUnit.GetWorldPosition() - unit.GetWorldPosition()).normalized;
                aimDirection.y = 0f;
                transform.forward = Vector3.Slerp(transform.forward, aimDirection, Time.deltaTime * rotateSpeed);
                break;
            case State.Shooting:
                if (canShoot)
                {
                    Shoot();
                    canShoot = false;
                }
                break;
            case State.Cooloff:
                break;
        }

        if (stateTimer <= 0f) NextState();
    }

    private void NextState()
    {
        switch (state)
        {
            case State.Aiming:
                state = State.Shooting;
                stateTimer = 0.1f;
                break;
            case State.Shooting:
                state = State.Cooloff;
                stateTimer = 0.5f;
                break;
            case State.Cooloff:
                ActionComplete();
                break;
        }
    }

    private void Shoot()
    {
        Vector3 startPosition = unit.GetWorldPosition() + Vector3.up * 1f + transform.forward * 0.5f;
        Vector3 targetPosition = targetUnit.GetWorldPosition() + Vector3.up * 0.8f;

        OnAnyFireball?.Invoke(this, new OnFireballEventArgs {
            targetUnit = targetUnit,
            shootingUnit = unit,
            isHit = true 
        });

        OnFireball?.Invoke(this, new OnFireballEventArgs {
            targetUnit = targetUnit,
            shootingUnit = unit,
            isHit = true
        });
        if (fireballAudioSource != null)
        {
            fireballAudioSource?.Play();
        }
        Transform projectileTransform = Instantiate(fireballProjectilePrefab, startPosition, Quaternion.identity);

        FireballProjectile projectile = projectileTransform.GetComponent<FireballProjectile>();
        projectile.Setup(targetPosition, projectileSpeed, () =>
        {
            float hitChance = GetHitChance(unit.GetGridPosition(), targetUnit, baseHitChance);
            bool isHit = UnityEngine.Random.value <= hitChance;

            if (isHit)
            {
                HealthSystem health = targetUnit.GetComponent<HealthSystem>();
                int damage = Mathf.RoundToInt(health.GetCalculatedDamage(unit, targetUnit) * damageMultiplier);
                health.Damage(unit, targetUnit, damage);
                DamagePopUpManager.Instance.ShowDamage(targetUnit.GetWorldPosition(), damage);
            }
            else
            {
                DamagePopUpManager.Instance.ShowMiss(targetUnit.GetWorldPosition());
            }
        });
    }

    public override List<GridPosition> GetValidActionGridPositionList()
    {
        GridPosition unitGridPosition = unit.GetGridPosition();
        List<GridPosition> validGridPositionList = new List<GridPosition>();

        for (int x = -maxRange; x <= maxRange; x++)
        {
            for (int z = -maxRange; z <= maxRange; z++)
            {
                if (Mathf.Abs(x) + Mathf.Abs(z) > maxRange) continue;

                GridPosition testGridPosition = unitGridPosition + new GridPosition(x, z, 0);

                if (!LevelGrid.Instance.IsValidGridPosition(testGridPosition)) continue;
                if (!LevelGrid.Instance.HasAnyUnitOnGridPosition(testGridPosition)) continue;

                Unit target = LevelGrid.Instance.GetUnitOnGridPosition(testGridPosition);
                if (target.IsEnemy() == unit.IsEnemy()) continue;

                Vector3 unitWorld = LevelGrid.Instance.GetWorldPosition(unitGridPosition);
                Vector3 shootDir = (target.GetWorldPosition() - unitWorld).normalized;

                if (Physics.Raycast(
                    unitWorld + Vector3.up,
                    shootDir,
                    Vector3.Distance(unitWorld, target.GetWorldPosition()),
                    obstaclesLayerMask)) continue;

                validGridPositionList.Add(testGridPosition);
            }
        }

        return validGridPositionList;
    }

    public override EnemyAIAction GetBestEnemyAIAction(GridPosition gridPosition)
    {
        Unit target = LevelGrid.Instance.GetUnitOnGridPosition(gridPosition);
        float hitChance = GetHitChance(unit.GetGridPosition(), target, baseHitChance);
        return new EnemyAIAction
        {
            gridPosition = gridPosition,
            actionValue = 150 + Mathf.RoundToInt(hitChance * (1 - target.GetHealthNormalized()) * 100f)
        };
    }

    public int GetMaxRange()
    {
        return maxRange;
    }

    public LayerMask GetObstacleLayerMask()
    {
        return obstaclesLayerMask;
    }

    public override int GetDamagePreview(Unit attacker, Unit target)
    {
        int baseDamage = target.GetComponent<HealthSystem>()
            .GetCalculatedDamage(attacker, target);

        return Mathf.RoundToInt(baseDamage * damageMultiplier);
    }

    public override float? GetHitChancePreview(GridPosition shooterGrid, Unit target)
    {
        return GetHitChance(shooterGrid, target, baseHitChance);
    }
    public Unit GetTargetUnit()
    {
        return targetUnit;
    }
}