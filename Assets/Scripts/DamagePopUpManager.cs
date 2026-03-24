using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamagePopUpManager : MonoBehaviour
{
    public static DamagePopUpManager Instance { get; private set; }

    [SerializeField] private ShootHitPopup defaultHitPopupPrefab;
    [SerializeField] private ShootMissedPopup defaultMissPopupPrefab;
    [SerializeField] private ShootTextPopup defaultTextPopupPrefab;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("There's more than one DamagePopupManager! " + transform + " - " + Instance);
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void ShowDamage(Vector3 worldPosition, int damage)
    {
        Vector3 spawnPos = worldPosition + Vector3.up * 2f;
        ShootHitPopup popup = Instantiate(defaultHitPopupPrefab, spawnPos, Quaternion.identity);
        popup.Setup(damage);
    }

    public void ShowMiss(Vector3 worldPosition)
    {
        Vector3 spawnPos = worldPosition + Vector3.up * 2f;
        Instantiate(defaultMissPopupPrefab, spawnPos, Quaternion.identity);
    }
    public void ShowText(Vector3 worldPosition, string text)
    {
        ShootTextPopup popup = Instantiate(defaultTextPopupPrefab, worldPosition + Vector3.up * 2f, Quaternion.identity);
        popup.Setup(text);
    }
}
