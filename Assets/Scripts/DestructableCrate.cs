using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class DestructableCrate : MonoBehaviour
{
    public static event EventHandler OnAnyDestroyed;
    private GridPosition gridPosition;
    [SerializeField] private Transform crateDestroyPrefab;
    private void Start()
    {
        gridPosition = LevelGrid.Instance.GetGridPosition(transform.position);
    }

    public GridPosition GetGridPosition()
    {
        return gridPosition;
    }
    public void Damage()
    {
        Transform crateDestroyTransform = Instantiate(crateDestroyPrefab, transform.position, transform.rotation);
        ApplyExplosionToChildren(crateDestroyTransform, 150, transform.position, 5f);
        
        Destroy(gameObject);

        OnAnyDestroyed?.Invoke(this, EventArgs.Empty);
    }

    private void ApplyExplosionToChildren(Transform root, float force, Vector3 explosionPosition, float radius)
    {
        foreach (Transform child in root)
        {
            if (child.TryGetComponent<Rigidbody>(out Rigidbody childRb))
            {
                childRb.AddExplosionForce(force, explosionPosition, radius);
            }

            ApplyExplosionToChildren(child, force, explosionPosition, radius);
        }
    }
}
