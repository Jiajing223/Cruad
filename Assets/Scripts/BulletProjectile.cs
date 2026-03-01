using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletProjectile : MonoBehaviour
{
    [SerializeField] private Transform BulletHitVfxPrefab;
    [SerializeField] private TrailRenderer trailRenderer;
    
    private Vector3 targetPosition;

    public void Setup(Vector3 targetPos)
    {
        this.targetPosition = targetPos;
    }


    private void Update()
    {
        Vector3 moveDirection = (targetPosition - transform.position).normalized;

        float distanceBeforeM = Vector3.Distance(transform.position, targetPosition);

        float speed = 200f;
        transform.position += moveDirection * speed * Time.deltaTime;

        float distanceAfterM = Vector3.Distance(transform.position, targetPosition);

        // Destroy the bullet if it has reached or passed the target position
        if (distanceAfterM > distanceBeforeM)
        {
            transform.position = targetPosition;

            trailRenderer.transform.parent = null;

            Destroy(gameObject);

            Instantiate(BulletHitVfxPrefab, targetPosition, Quaternion.identity);
        }

    }
}
