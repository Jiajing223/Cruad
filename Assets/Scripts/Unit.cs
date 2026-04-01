using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public enum FacingDirection
    {
        North,
        South,
        East,
        West,
        NorthEast,
        NorthWest,
        SouthEast,
        SouthWest
    }

    private FacingDirection facingDirection;

    // In case the update action point runs before the end turn
    public static event EventHandler OnAnyActionPointsChanged;
    public static event EventHandler OnAnyUnitSpawned;
    public static event EventHandler OnAnyUnitDead;
    public static event EventHandler OnAnyCoverStateChanged;
    [SerializeField] private bool isEnemy;

    private GridPosition gridPosition;
    private HealthSystem healthSystem;
    private UnitStat unitStat;
    private MoveAction moveAction;
    private SpinAction spinAction;
    private ShootAction shootAction;
    private BaseAction[] baseActionArray;
    private IAbility[] abilities;
    private const int totalActionPoints = 3;
    private int actionPoints = totalActionPoints;
    private bool isCovered;
    private void Awake()
    {
        unitStat = GetComponent<UnitStat>();
        healthSystem = GetComponent<HealthSystem>();
        moveAction = GetComponent<MoveAction>();
        spinAction = GetComponent<SpinAction>();
        shootAction = GetComponent<ShootAction>();
        baseActionArray = GetComponents<BaseAction>();
        abilities = GetComponents<IAbility>();
    }
    private void Start()
    {
        // Turn ending event
        TurnSystem.Instance.OnTurnChanged += TurnSystem_OnTurnChanged;
        OnAnyUnitSpawned?.Invoke(this, EventArgs.Empty);

        gridPosition = LevelGrid.Instance.GetGridPosition(transform.position);
        LevelGrid.Instance.AddUnitGridPosition(gridPosition, this);
        LevelGrid.Instance.OnAnyUnitMovedGridPosition += (sender, e) => UpdateCoverState();
        healthSystem.OnDead += HealthSystem_OnDead;
    }
    private void Update()
    {

        GridPosition newGridPosition = LevelGrid.Instance.GetGridPosition(transform.position);
        if (newGridPosition != gridPosition)
        {
            // Update Grid position
            GridPosition oldGridPosition = gridPosition;
            gridPosition = newGridPosition;
            LevelGrid.Instance.UnitMovedGridPosition(this, oldGridPosition, newGridPosition);
        }
    }

    public MoveAction GetMoveAction()
    {
        return moveAction;
    }
    public GridPosition GetGridPosition()
    {
        return gridPosition;
    }
    public SpinAction GetSpinAction()
    {
        return spinAction;
    }
    public ShootAction GetShootAction()
    {
        return shootAction;
    }
    public BaseAction[] GetBaseActionArray()
    {
        return baseActionArray;
    }
    public int GetActionPoints()
    {
        return actionPoints;
    }
    private void SetActionPoints(int actionPoints)
    {
        this.actionPoints = actionPoints;
    }
    public bool IsEnemy()
    {
        return isEnemy;
    }
    public List<BaseAction> GetAbilities()
    {
        List<BaseAction> abilities = new List<BaseAction>();
        foreach (BaseAction action in baseActionArray)
        {
            if (action is IAbility)
                abilities.Add(action);
        }
        return abilities;
    }
    public Vector3 GetWorldPosition()
    {
        return transform.position;
    }
    public FacingDirection GetFacingDirection()
    {
        return facingDirection;
    }
    public void SetFacingDirection(FacingDirection facingDirection)
    {
        this.facingDirection = facingDirection;
    }
    public UnitStat GetUnitStat()
    {
        return unitStat;
    }

    ///////////////////////// Action ////////////////////////////////////
    public bool TrySpendActionPointsToTakeAction(BaseAction baseAction)
    {
        if (CanSpendActionPointsToTakeAction(baseAction))
        {
            SpendActionPoints(baseAction);
            return true;
        }
        else
        {
            return false;
        }
    }
    public bool CanSpendActionPointsToTakeAction(BaseAction baseAction)
    {
        if (actionPoints >= baseAction.GetActionPointCost())
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public void SpendActionPoints(BaseAction baseAction)
    {
        actionPoints -= baseAction.GetActionPointCost();

        OnAnyActionPointsChanged?.Invoke(this, EventArgs.Empty);
    }
    
    ///////////////////////// Turn ////////////////////////////////////
    private void TurnSystem_OnTurnChanged(object shader, EventArgs e)
    {
        if ((isEnemy && !TurnSystem.Instance.IsPlayerTurn()) || (!isEnemy && TurnSystem.Instance.IsPlayerTurn()))
        {
            RegenerateActionPoints();
            UpdateCoverState();
            OnAnyActionPointsChanged?.Invoke(this, EventArgs.Empty);
        }

    }

    private void RegenerateActionPoints()
    {
        SetActionPoints(totalActionPoints);
    }

    
    ///////////////////////// Health ////////////////////////////////////
    public void Damage(Unit userUnit, Unit targetUnit)
    {
        healthSystem.Damage(userUnit, targetUnit);
    }
    public void Damage(Unit targetUnit, int damage)
    {
        // Some habilities does flat damage that doesnt take users attack
        healthSystem.Damage(null,targetUnit , damage);
    }
    private void HealthSystem_OnDead(object sender, EventArgs e)
    {
        LevelGrid.Instance.RemoveUnitGridPosition(gridPosition);
        Destroy(gameObject);
        OnAnyUnitDead?.Invoke(this, EventArgs.Empty);
    }

    public float GetHealthNormalized()
    {
        return healthSystem.GetHealthNormalized();
    }
    public bool GetIsCovered()
    {
        Vector3 unitWorld = GetWorldPosition() + Vector3.up * 0.1f;
        GridPosition[] directions = new GridPosition[]
        {
            new GridPosition( 1, 0, 0),
            new GridPosition(-1, 0, 0),
            new GridPosition( 0, 1, 0),
            new GridPosition( 0,-1, 0),
        };

        foreach (GridPosition dir in directions)
        {
            Vector3 worldDir = new Vector3(dir.x, 0, dir.z);
            if (Physics.Raycast(unitWorld, worldDir, 1f, LayerMask.GetMask("CoverObject")))
            {
                return true;
            }
        }
        return false;
    }
    public Vector3 GetCoverDirection()
    {
        Vector3 unitWorld = GetWorldPosition() + Vector3.up * 0.1f;
        GridPosition[] directions = new GridPosition[]
        {
            new GridPosition( 1, 0, 0),
            new GridPosition(-1, 0, 0),
            new GridPosition( 0, 1, 0),
            new GridPosition( 0,-1, 0),
        };

        foreach (GridPosition dir in directions)
        {
            Vector3 worldDir = new Vector3(dir.x, 0, dir.z);
            if (Physics.Raycast(unitWorld, worldDir, 1f, LayerMask.GetMask("CoverObject")))
            {
                return worldDir; // direction toward the cover
            }
        }
        return Vector3.zero;
    }
    public void UpdateCoverState()
    {
        bool newCoveredState = GetIsCovered();
        if (newCoveredState != isCovered)
        {
            isCovered = newCoveredState;
            OnAnyCoverStateChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public bool IsCovered()
    {
        return isCovered;
    }

        public void ForceUpdateCoverState()
    {
        isCovered = !GetIsCovered(); // flip it so the change check always triggers
        UpdateCoverState();
    }
}
    
