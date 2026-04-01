using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AbilityMenuUI : MonoBehaviour
{
    public static AbilityMenuUI Instance { get; private set; }

    [SerializeField] private Transform actionButtonPrefab;
    [SerializeField] private Transform actionButtonContainerTransform;
    [SerializeField] private GameObject menuContainer;

    private List<AbilityButtonUI> AbilityButtonUIList;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        AbilityButtonUIList = new List<AbilityButtonUI>();
    }

    private void Start()
    {
        UnitActionSystem.Instance.OnSelectedUnitChanged += UnitActionSystem_OnSelectedUnitChanged;
        UnitActionSystem.Instance.OnSelectedActionChanged += UnitActionSystem_OnSelectedActionChanged;
        UnitActionSystem.Instance.OnAblityActionSelected += UnitActionSystem_OnAblityActionSelected;
        CreateAbilityButtons();
        
        Hide();
    }

    private void UnitActionSystem_OnAblityActionSelected(object sender, BaseAction e)
    {
        Show();
    }

    private void UnitActionSystem_OnSelectedUnitChanged(object sender, EventArgs e)
    {
        CreateAbilityButtons();
        Hide();
    }

    private void UnitActionSystem_OnSelectedActionChanged(object sender, EventArgs e)
    {
        BaseAction selectedAction = UnitActionSystem.Instance.GetSelectedAction();
        if (selectedAction is not AbilityAction && selectedAction is not IAbility)
            Hide();
            
        UpdateSelectedVisual();
    }

    private void CreateAbilityButtons()
    {
        foreach (Transform buttonTransform in actionButtonContainerTransform)
            Destroy(buttonTransform.gameObject);

        AbilityButtonUIList.Clear();

        Unit selectedUnit = UnitActionSystem.Instance.GetSelectedUnit();

        foreach (BaseAction action in selectedUnit.GetAbilities())
        {
            Transform actionButtonTransform = Instantiate(actionButtonPrefab, actionButtonContainerTransform);
            AbilityButtonUI abilityButtonUI = actionButtonTransform.GetComponent<AbilityButtonUI>();
            abilityButtonUI.Setup(action, OnAbilityButtonClicked);
            AbilityButtonUIList.Add(abilityButtonUI);
        }
    }

    private void OnAbilityButtonClicked(BaseAction action)
    {
        UnitActionSystem.Instance.SetSelectedAction(action);
        Hide();
    }

    private void UpdateSelectedVisual()
    {
        foreach (AbilityButtonUI buttonUI in AbilityButtonUIList)
            buttonUI.UpdateSelectedVisual();
    }
    
    private void Show() => menuContainer.SetActive(true);
    private void Hide() => menuContainer.SetActive(false);
}