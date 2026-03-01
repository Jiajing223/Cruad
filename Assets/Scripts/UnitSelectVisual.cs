using System; 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitSelectVisual : MonoBehaviour
{
    // Know which unit this visual is representing
    [SerializeField] private Unit unit;
    // Reference to the mesh renderer component
    private MeshRenderer meshRenderer;
    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    private void Start()
    {
        // Subscribe to the event when selected unit changes
        UnitActionSystem.Instance.OnSelectedUnitChanged += UnitActionSystem_OnSelectedUnitChanged;
        UpdateVisual();
    }
    private void UnitActionSystem_OnSelectedUnitChanged(object sender, System.EventArgs empty)
    {
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        // Show the visual only if this unit is the selected unit
        if (UnitActionSystem.Instance.GetSelectedUnit() == unit)
        {
            meshRenderer.enabled = true;
        }
        else
        {
            meshRenderer.enabled = false;
        }
    }

    private void OnDestroy()
    { 
        UnitActionSystem.Instance.OnSelectedUnitChanged -= UnitActionSystem_OnSelectedUnitChanged;
    }
}
