using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FogOfWarManager : MonoBehaviour
{
    public static FogOfWarManager Instance { get; private set; }

    [SerializeField] private int sightRange = 8;
    [SerializeField] private LayerMask sightBlockLayerMask; // walls, obstacles

    // Tracks which grid positions are currently visible this turn
    private HashSet<GridPosition> visiblePositions = new HashSet<GridPosition>();
    // Tracks which positions have EVER been seen (this turn or any previous turn)
    private HashSet<GridPosition> exploredPositions = new HashSet<GridPosition>();

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        TurnSystem.Instance.OnTurnChanged += OnTurnChanged;
        Unit.OnAnyUnitSpawned += OnUnitChanged;
        Unit.OnAnyUnitDead   += OnUnitChanged;
        StartCoroutine(StartDelayedFogRefresh());
    }
    private IEnumerator StartDelayedFogRefresh()
    {
        yield return null; // Wait one frame to ensure all units are initialized
        RefreshVisibility();
    }
    private void OnTurnChanged(object sender, System.EventArgs e)
    {
        RefreshVisibility();
    }

    private void OnUnitChanged(object sender, System.EventArgs e)
    {
        RefreshVisibility();
    }

    public void RefreshVisibility()
    {
        if (FogofwarVisual.Instance == null || !FogofwarVisual.Instance.IsReady)
            return;

        if (UnitManager.Instance == null)
            return;

        List<Unit> friendlyUnits = UnitManager.Instance.GetFriendlyUnitList();
        if (friendlyUnits.Count == 0)
            return;

        Debug.Log("Units: " + UnitManager.Instance.GetFriendlyUnitList().Count);
        visiblePositions.Clear();

        foreach (Unit unit in friendlyUnits)
        {
            ComputeVisibilityForUnit(unit);
        }

        foreach (GridPosition gp in visiblePositions)
            exploredPositions.Add(gp);

        FogofwarVisual.Instance?.UpdateFogVisual(visiblePositions, exploredPositions);
        
        UpdateEnemyVisibility();
    }

    private void ComputeVisibilityForUnit(Unit unit)
    {
        GridPosition unitPos = unit.GetGridPosition();
        Vector3 unitWorld   = LevelGrid.Instance.GetWorldPosition(unitPos) + Vector3.up * 1.0f;

        for (int x = -sightRange; x <= sightRange; x++)
        {
            for (int z = -sightRange; z <= sightRange; z++)
            {
                // Circular range check
                if (x * x + z * z > sightRange * sightRange) continue;

                GridPosition testPos = new GridPosition(
                    unitPos.x + x,
                    unitPos.z + z,
                    unitPos.floor
                );

                if (!LevelGrid.Instance.IsValidGridPosition(testPos)) continue;

                Vector3 testWorld = LevelGrid.Instance.GetWorldPosition(testPos) + Vector3.up * 1.0f;
                Vector3 dir       = (testWorld - unitWorld);
                float   dist      = dir.magnitude;

                if (Physics.Raycast(unitWorld, dir.normalized, dist, sightBlockLayerMask))
                    continue; 

                visiblePositions.Add(testPos);
            }
        }
    }

    private void UpdateEnemyVisibility()
    {
        foreach (Unit enemy in UnitManager.Instance.GetEnemyUnitList())
        {
            GridPosition enemyPos = enemy.GetGridPosition();
            bool visible = visiblePositions.Contains(enemyPos);
            // Toggle all renderers on the enemy
            foreach (Renderer r in enemy.GetComponentsInChildren<Renderer>())
                r.enabled = visible;
        }
    }

    public bool IsVisible(GridPosition gridPosition)  => visiblePositions.Contains(gridPosition);
    public bool IsExplored(GridPosition gridPosition) => exploredPositions.Contains(gridPosition);
}