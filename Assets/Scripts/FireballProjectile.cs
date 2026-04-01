using System;
using UnityEngine;

public class FireballProjectile : MonoBehaviour
{
    private Vector3 targetPosition;
    private float speed;
    private Action onHit;

    private float growTime = 0.3f;
    private float growTimer;

    private bool hasGrown = false;

    public void Setup(Vector3 targetPosition, float speed, Action onHit)
    {
        this.targetPosition = targetPosition;
        this.speed = speed;
        this.onHit = onHit;

        growTimer = growTime;

        transform.localScale = Vector3.zero;
        transform.forward = (targetPosition - transform.position).normalized;
    }

    private void Update()
    {
        if (!hasGrown)
        {
            HandleGrow();
        }
        else
        {
            HandleMovement();
        }
    }

    private void HandleGrow()
    {
        growTimer -= Time.deltaTime;

        float t = 1f - (growTimer / growTime);
        transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, t);

        if (growTimer <= 0f)
        {
            transform.localScale = Vector3.one;
            hasGrown = true;
        }
    }

    private void HandleMovement()
    {
        Vector3 moveDir = (targetPosition - transform.position).normalized;

        float distanceBefore = Vector3.Distance(transform.position, targetPosition);

        transform.position += moveDir * speed * Time.deltaTime;

        float distanceAfter = Vector3.Distance(transform.position, targetPosition);

        if (distanceAfter > distanceBefore)
        {
            transform.position = targetPosition;

            onHit?.Invoke();
            Destroy(gameObject);
        }
    }
}