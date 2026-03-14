using UnityEngine;

public class CoverSystem : MonoBehaviour
{
    private static readonly GridPosition[] neighborOffsets = new GridPosition[]
    {
        new GridPosition( 1, 0, 0),
        new GridPosition(-1, 0, 0),
        new GridPosition( 0, 1, 0),
        new GridPosition( 0,-1, 0),
    };

    private void Start()
    {
        foreach (CoverObject cover in FindObjectsOfType<CoverObject>())
        {
            GridPosition coverPos = LevelGrid.Instance.GetGridPosition(cover.transform.position);
            foreach (GridPosition offset in neighborOffsets)
            {
                GridPosition neighborPos = coverPos + offset;
                if (!LevelGrid.Instance.IsValidGridPosition(neighborPos)) continue;
                GridObject gridObject = LevelGrid.Instance.GetGridObject(neighborPos);
                gridObject.SetCoverDodgeBonus(cover.GetDodgeBonus());
            }
        }
    }
}