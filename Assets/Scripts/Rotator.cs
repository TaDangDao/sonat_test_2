using System.Collections.Generic;
using UnityEngine;

public class Rotator : MonoBehaviour
{
    // Rotator chiếm 1 ô trong grid (ô này không chứa Block).
    // Rotator tác động lên cụm 2x2, với anchorCell là góc dưới-trái của cụm đó.
    private GridManager gridManager;
    private Vector2Int anchorCell;
    private Vector2Int occupiedCell;
    private List<Vector2Int> controlledCells = new List<Vector2Int>(3);
    [Header("Line Settings")]
    [SerializeField] private LineRenderer lineRendererPrefab;
    [SerializeField] private float lineWidth = 0.06f;
    [SerializeField] private Color lineColor = Color.white;
    private readonly List<LineRenderer> lineRenderers = new List<LineRenderer>(3);
    private bool refreshNextFrame;

    public Vector2Int AnchorCell => anchorCell;
    public Vector2Int OccupiedCell => occupiedCell;
    public List<Vector2Int> ControlledCells => controlledCells;

    public void Init(GridManager manager, Vector2Int anchorCell, Vector2Int occupiedCell, List<Vector2Int> controlledCells)
    {
        gridManager = manager;
        this.anchorCell = anchorCell;
        this.occupiedCell = occupiedCell;
        this.controlledCells = controlledCells ?? new List<Vector2Int>(3);
        EnsureLineRenderers();
        RefreshLines();
        refreshNextFrame = true;
    }

    private void Awake()
    {
        EnsureLineRenderers();
        RefreshLines();
    }

    private void LateUpdate()
    {
        if (refreshNextFrame)
        {
            RefreshLines();
            refreshNextFrame = false;
        }
    }

    private void EnsureLineRenderers()
    {
        // Ensure we have one LineRenderer per controlled cell
        int targetCount = Mathf.Max(0, controlledCells.Count);

        if (lineRenderers.Count == 0)
        {
            LineRenderer[] existing = GetComponentsInChildren<LineRenderer>(true);
            for (int i = 0; i < existing.Length; i++)
            {
                if (!lineRenderers.Contains(existing[i]))
                {
                    lineRenderers.Add(existing[i]);
                }
            }
        }

        while (lineRenderers.Count < targetCount)
        {
            LineRenderer lr = null;
            if (lineRendererPrefab != null)
            {
                lr = Instantiate(lineRendererPrefab, transform);
            }
            else
            {
                GameObject lineObj = new GameObject("RotatorLine");
                lineObj.transform.SetParent(transform, false);
                lr = lineObj.AddComponent<LineRenderer>();
            }

            lr.useWorldSpace = true;
            lr.widthMultiplier = lineWidth;
            lr.startColor = lineColor;
            lr.endColor = lineColor;
            lr.positionCount = 0;
            lineRenderers.Add(lr);
        }

        // Disable extras if any
        for (int i = targetCount; i < lineRenderers.Count; i++)
        {
            if (lineRenderers[i] != null)
            {
                lineRenderers[i].positionCount = 0;
                lineRenderers[i].enabled = false;
            }
        }
    }

    public void RefreshLines()
    {
        if (gridManager == null)
        {
            return;
        }

        EnsureLineRenderers();

        int lrIndex = 0;
        for (int i = 0; i < controlledCells.Count; i++)
        {
            Block block = gridManager.GetBlockAt(controlledCells[i]);
            if (block == null) continue;
            if (lrIndex >= lineRenderers.Count) break;

            LineRenderer lr = lineRenderers[lrIndex];
            lr.enabled = true;
            lr.positionCount = 2;
            lr.SetPosition(0, transform.position);
            lr.SetPosition(1, block.transform.position);
            lrIndex++;
        }

        // Clear remaining unused renderers
        for (int i = lrIndex; i < lineRenderers.Count; i++)
        {
            if (lineRenderers[i] != null)
            {
                lineRenderers[i].positionCount = 0;
                lineRenderers[i].enabled = false;
            }
        }
    }

    // Được gọi từ GridManager (không dùng OnMouseDown để tránh bị gọi 2 lần).
    public void OnClicked()
    {
        if (gridManager == null) return;
        Debug.Log("Rotate");
        List<Vector2Int> rotateCells = new List<Vector2Int>(controlledCells.Count);
        for (int i = 0; i < controlledCells.Count; i++)
        {
            Vector2Int cell = controlledCells[i];
            if (gridManager.IsRotatorCell(cell.x, cell.y))
            {
                continue;
            }

            Block block = gridManager.GetBlockAt(cell);
            if (block != null && !block.CanTouch)
            {
                return;
            }

            rotateCells.Add(cell);
        }

        if (rotateCells.Count < 2)
        {
            return;
        }

        // Rotate trong phạm vi ô thuộc rotator, cho phép xoay vào ô trống.
        gridManager.RotateBlocksInCells(rotateCells);
        RefreshLines();
    }
}
