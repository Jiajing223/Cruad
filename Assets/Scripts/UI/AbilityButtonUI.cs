using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AbilityButtonUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI actionNameText;
    [SerializeField] private Button button;
    private BaseAction action;

    public void Setup(BaseAction action, Action<BaseAction> onClicked)
    {
        this.action = action;
        actionNameText.text = action.GetActionName();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClicked(action));
    }

    public void UpdateSelectedVisual()
    {
        BaseAction selectedAction = UnitActionSystem.Instance.GetSelectedAction();
        button.image.color = selectedAction == action ? Color.yellow : Color.white;
    }
}