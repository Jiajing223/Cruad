using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class ShootTextPopup : MonoBehaviour
{
    [SerializeField] private TextMeshPro missText;

    private float floatSpeed = 2f;
    // private float fadeSpeed  = 2f;
    private float lifetime   = 1f;
    private float timer;
    public void Setup(string text)
    {
        missText.text = text;
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
