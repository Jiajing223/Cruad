// FogOfWarManager.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FogOfWarManager : MonoBehaviour
{
    public static FogOfWarManager Instance { get; private set; }

    [SerializeField] private int sightRange = 8;
    [SerializeField] private LayerMask sightBlockLayerMask;
    private HashSet<GridPosition> visiblePositions = new HashSet<GridPosition>();
    private int floorCount;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        floorCount = LevelGrid.Instance.GetFloorCount();
    }

    private void Start()
    {
        TurnSystem.Instance.OnTurnChanged += OnTurnChanged;
        Unit.OnAnyUnitSpawned += OnUnitChanged;
        Unit.OnAnyUnitDead += OnUnitChanged;
        StartCoroutine(StartDelayedFogRefresh());
    }
    // Wait one frame to ensure all units are spawned before the first fog refresh, so that the fog can be correctly initialized based on unit positions
    private IEnumerator StartDelayedFogRefresh()
    {
        yield return null;
        RefreshVisibility();
    }

    private void OnTurnChanged(object sender, System.EventArgs e) => RefreshVisibility();
    private void OnUnitChanged(object sender, System.EventArgs e) => RefreshVisibility();

    public void RefreshVisibility()
    {
        if (FogofwarVisual.Instance == null || !FogofwarVisual.Instance.IsReady) return;
        if (UnitManager.Instance == null) return;

        List<Unit> friendlyUnits = UnitManager.Instance.GetFriendlyUnitList();
        if (friendlyUnits.Count == 0) return;

        visiblePositions.Clear();

        foreach (Unit unit in friendlyUnits)
            ComputeVisibilityForUnit(unit);

        FogofwarVisual.Instance?.UpdateFogVisual(visiblePositions);
        UpdateEnemyVisibility();
    }

    private void ComputeVisibilityForUnit(Unit unit)
    {
        GridPosition unitPos = unit.GetGridPosition();
        Vector3 unitWorld = LevelGrid.Instance.GetWorldPosition(unitPos) + Vector3.up * 1.0f;

        // Fill the list of grid that should not be blocked by fog of war AKA the grids that are visible by all the friendly units
        for (int x = -sightRange; x <= sightRange; x++)
        {
            for (int z = -sightRange; z <= sightRange; z++)
            {
                // Skip tiles outside the circular radius
                if (x * x + z * z > sightRange * sightRange) continue;

                // Check the same (x,z) column across every floor independently
                for (int f = 0; f < floorCount; f++)
                {
                    GridPosition testPos = new GridPosition(
                        unitPos.x + x,
                        unitPos.z + z,
                        f
                    );
                    // Not valid position
                    if (!LevelGrid.Instance.IsValidGridPosition(testPos)) continue;

                    Vector3 testWorld = LevelGrid.Instance.GetWorldPosition(testPos)
                                      + Vector3.up * 0.05f;
                    Vector3 dir = testWorld - unitWorld;
                    float dist = dir.magnitude;

                    // Blocked by obstacle
                    if (Physics.Raycast(unitWorld, dir.normalized, dist, sightBlockLayerMask))
                        continue;

                    visiblePositions.Add(testPos);
                }
            }
        }
    }

    private void UpdateEnemyVisibility()
    {
        // Show enemy units if they are in visible positions, hide them otherwise
        foreach (Unit enemy in UnitManager.Instance.GetEnemyUnitList())
        {
            bool visible = visiblePositions.Contains(enemy.GetGridPosition());
            foreach (Renderer r in enemy.GetComponentsInChildren<Renderer>())
            {
                if (r.GetComponent<UnitSelectVisual>() != null) continue;
                r.enabled = visible;
            }
            foreach (Canvas c in enemy.GetComponentsInChildren<Canvas>())
                c.enabled = visible;
        }
    }

    public bool IsVisible(GridPosition gridPosition) => visiblePositions.Contains(gridPosition);
    public LayerMask SightBlockLayerMask => sightBlockLayerMask;
}