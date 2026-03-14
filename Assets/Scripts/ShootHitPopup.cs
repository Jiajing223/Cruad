using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class ShootHitPopup : MonoBehaviour
{
    [SerializeField] private TextMeshPro hitText;
    private float floatSpeed = 2f;
    // private float fadeSpeed  = 2f;
    private float lifetime   = 1f;
    private float timer;
    // private int damage;
    public void Setup(int damage)
    {
        hitText.text  = damage.ToString();
    }
    private void Update()
    {
        // Float upward
        transform.position += Vector3.up * floatSpeed * Time.deltaTime;
        // Fade out over lifetime
        timer += Time.deltaTime;
        if (timer >= lifetime) Destroy(gameObject);
    }

    // Always face the camera
    private void LateUpdate()
    {
        transform.forward = Camera.main.transform.forward;
    }
}
