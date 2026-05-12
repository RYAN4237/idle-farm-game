using UnityEngine;

/// Creates a FarmPlot — plant layer + collider + logic only.
/// The dirt ground is rendered by the Tilemap underneath.
public static class PlotFactory
{
    public static GameObject Create(Vector3 worldPos, float cellSize = 1f)
    {
        var go = new GameObject("FarmPlot");
        go.transform.position = worldPos;

        // Invisible base renderer (no dirt sprite — tilemap handles that)
        var sr = go.AddComponent<SpriteRenderer>();
        sr.color = Color.clear;
        sr.sortingOrder = 1;

        // Plant sprite layer — shown when Growing/Ready
        var plantGO = new GameObject("Plant");
        plantGO.transform.SetParent(go.transform, false);
        var plantSR = plantGO.AddComponent<SpriteRenderer>();
        plantSR.sortingOrder = 5;
        plantSR.color = Color.clear;

        // Click collider
        go.AddComponent<BoxCollider2D>().size = new Vector2(cellSize * 0.9f, cellSize * 0.9f);

        // Logic
        var plot = go.AddComponent<FarmPlot>();
        plot.growthDuration = 20f;
        plot.plantCost      = 10f;
        plot.harvestReward  = 30f;
        plot.emptyColor     = Color.white;
        plot.growingColor   = Color.white;
        plot.readyColor     = Color.white;

        // Plant visual driver
        var visual = go.AddComponent<FarmPlotVisual>();
        visual.plantRenderer = plantSR;

        return go;
    }
}
