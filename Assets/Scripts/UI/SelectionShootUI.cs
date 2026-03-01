using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectionShootUI : MonoBehaviour
{
    public static SelectionShootUI Instance { get; private set; }
    [SerializeField] private TextMeshProUGUI textMeshPro;
    [SerializeField] private Button button;

    public event EventHandler OnTargetSelectionConfirm;
    
    private BaseAction baseAction;   

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("There's more than one UnitActionSystem! " + transform + " - " + Instance);
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    private void Start()
    {
        button.onClick.AddListener(TargetSelectionConfirm);
    }

    private void TargetSelectionConfirm()
    {
        OnTargetSelectionConfirm?.Invoke(this, EventArgs.Empty);
    }
    public void Show(string buttonText = "Confirm")
    {
        gameObject.SetActive(true);
        if (textMeshPro != null)
        {
            textMeshPro.text = buttonText;
        }
    }
}
