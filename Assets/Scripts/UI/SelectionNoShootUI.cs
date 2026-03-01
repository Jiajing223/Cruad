using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectionNoShootUI : MonoBehaviour
{
    public static SelectionNoShootUI Instance { get; private set; }
    [SerializeField] private TextMeshProUGUI textMeshPro;
    [SerializeField] private Button button;

    public event EventHandler OnTargetSelectionCancel;
    
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
        button.onClick.AddListener(OnCancelButtonClicked);
    }

    private void OnCancelButtonClicked()
    {
        OnTargetSelectionCancel?.Invoke(this, EventArgs.Empty);
    }
    public void Show(string buttonText = "Cancel")
    {
        gameObject.SetActive(true);
        if (textMeshPro != null)
        {
            textMeshPro.text = buttonText;
        }
    }
}
