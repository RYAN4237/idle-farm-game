using UnityEngine;

/// Marks a FarmPlot as having been placed on the grid.
/// Stores its grid cell so we can free it when removed.
public class PlacedPlot : MonoBehaviour
{
    public Vector2Int cell;

    void OnDestroy()
    {
        if (FarmGrid.Instance != null)
            FarmGrid.Instance.SetOccupied(cell, false);
    }
}
