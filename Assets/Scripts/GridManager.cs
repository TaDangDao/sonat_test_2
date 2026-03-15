using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Collections;
using Unity.Mathematics;

public class GridManager : MonoBehaviour
{
    [Header("Board Settings")]
    [SerializeField] private LayerMask clickableLayer;
    [SerializeField] private Block blockPrefab;
    [SerializeField] private Transform blockParent;
    [SerializeField] private TextMeshProUGUI moveTurnText;
    [SerializeField] private int gridRows = 10;
    [SerializeField] private int gridColumns = 10;
    [Header("Difficulty")]
    [SerializeField] private int defaultMoveTurn = 10;
    [SerializeField] private int minMoveTurnForScaling = 4;
    [SerializeField] private int maxMoveTurnForScaling = 24;
    [SerializeField] private int minBlocks = 6;
    [SerializeField] private int maxBlocks = 18;
    [Header("Rotator Settings")]
    [SerializeField, Range(0f, 1f)] private float rotatorSpawnChance = 0.2f;
    [SerializeField] private int maxRotatorsPerLevel = 1;
    [SerializeField] private Rotator rotatorPrefab;
    [SerializeField] private GameObject bombPrefab;
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private BombBooster bombBooster;
    private List<Rotator> rotators = new List<Rotator>();
    private Rotator[,] rotatorGrid;
    private Block[,] grid;
    private List<Block> blocks;
    private Block currentBlock;
    private int currentRow;
    private int currentCol;
    private Block fartestBlock;
    private Vector2 direction;
    private int moveTurn;
    private Vector2 gridPos;
    private static readonly Type[] directionPool = { Type.LEFT, Type.RIGHT, Type.UP, Type.DOWN };


    private void Awake()
    {
        blocks = new List<Block>();
    }

    private void Start()
    {
        //moveTurn = defaultMoveTurn;
        SetMoveTurnText(moveTurn);
        LoadMap();
    }
    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && GameManager_.Instance.State == State.PLAYING)
        {
            Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero, 0f, clickableLayer);
            if (hit.collider != null)
            {
                if(hit.collider.GetComponent<Block>()!=null){
                    currentBlock = hit.collider.GetComponent<Block>();
                    if (currentBlock != null && currentBlock.CanTouch)
                    {
                        if (bombBooster.IsBoosterActive)
                        {
                            ActivateBombBoosterAtBlock();
                            bombBooster.UseBooster();
                        }
                        else
                        {
                            currentRow = currentBlock.GetRowAndColumn().x;
                            currentCol = currentBlock.GetRowAndColumn().y;
                            moveTurn = Mathf.Max(0, moveTurn - 1);
                            SetMoveTurnText(moveTurn);
                            CheckValidMove(currentBlock);
                        }
                       
                        SoundManager.Instance.PlaySound(SoundType.BlockClick);
                    }
                }
                else
                {
                    if (hit.collider.GetComponent<Rotator>() != null)
                    {
                        Rotator rotator = hit.collider.GetComponent<Rotator>();
                        rotator.OnClicked();
                    }
                }
            }
        }
    }
    public void LoadMap()
    {
        gridPos.x=-(float)(gridColumns - 1) / 2;
        gridPos.y=-(float)(gridRows - 1) / 2;
        transform.position=gridPos;
    }
        public void CheckValidMove(Block block)
    {
        if (grid == null || block == null)
        {
            return;
        }

        switch (block.BlockType)
        {
            case Type.LEFT:
                fartestBlock = null;
                for (int i = currentCol - 1; i >= 0; i--)
                {
                    if (grid[currentRow, i] == null && !IsRotatorCell(currentRow, i))
                    {
                        continue;
                    }
                    fartestBlock = grid[currentRow, i];
                    direction.y = currentRow;
                    direction.x = i + 1;
                    grid[currentRow, currentCol] = null;
                    grid[currentRow, i + 1] = block;
                    block.MovetoPos(direction, false, fartestBlock);
                    block.SetBlockRowAnCol(currentRow, i + 1);
                    CheckEndGameCondition();
                    return;
                }

                direction.y = currentRow;
                direction.x = -grid.GetLength(1) - 10;
                block.MovetoPos(direction, true, fartestBlock);
                grid[currentRow, currentCol] = null;
                blocks.Remove(block);
                RefreshAllRotatorLines();
                break;

            case Type.RIGHT:
                fartestBlock = null;
                for (int i = currentCol + 1; i < grid.GetLength(1); i++)
                {
                    if (grid[currentRow, i] == null && !IsRotatorCell(currentRow, i))
                    {
                        continue;
                    }

                    fartestBlock = grid[currentRow, i];
                    direction.y = currentRow;
                    direction.x = i - 1;
                    grid[currentRow, currentCol] = null;
                    grid[currentRow, i - 1] = block;
                    block.MovetoPos(direction, false, fartestBlock);
                    block.SetBlockRowAnCol(currentRow, i - 1);
                    CheckEndGameCondition();
                    return;
                }

                direction.y = currentRow;
                direction.x = grid.GetLength(1) + 10;
                block.MovetoPos(direction, true, fartestBlock);
                grid[currentRow, currentCol] = null;
                blocks.Remove(block);
                RefreshAllRotatorLines();
                break;

            case Type.DOWN:
                fartestBlock = null;
                for (int i = currentRow - 1; i >= 0; i--)
                {
                    if (grid[i, currentCol] == null && !IsRotatorCell(i, currentCol))
                    {
                        continue;
                    }

                    fartestBlock = grid[i, currentCol];
                    direction.y = i + 1;
                    direction.x = currentCol;
                    grid[currentRow, currentCol] = null;
                    grid[i + 1, currentCol] = block;
                    block.MovetoPos(direction, false, fartestBlock);
                    block.SetBlockRowAnCol(i + 1, currentCol);
                    CheckEndGameCondition();
                    return;
                }

                direction.y = -grid.GetLength(0) - 10;
                direction.x = currentCol;
                block.MovetoPos(direction, true, fartestBlock);
                grid[currentRow, currentCol] = null;
                blocks.Remove(block);
                RefreshAllRotatorLines();
                break;

            case Type.UP:
                fartestBlock = null;
                for (int i = currentRow + 1; i < grid.GetLength(0); i++)
                {
                    if (grid[i, currentCol] == null && !IsRotatorCell(i, currentCol))
                    {
                        continue;
                    }

                    fartestBlock = grid[i, currentCol];
                    direction.y = i - 1;
                    direction.x = currentCol;
                    grid[currentRow, currentCol] = null;
                    grid[i - 1, currentCol] = block;
                    block.MovetoPos(direction, false, fartestBlock);
                    block.SetBlockRowAnCol(i - 1, currentCol);
                    CheckEndGameCondition();
                    return;
                }

                direction.y = grid.GetLength(0) + 10;
                direction.x = currentCol;
                grid[currentRow, currentCol] = null;
                block.MovetoPos(direction, true, fartestBlock);
                blocks.Remove(block);
                RefreshAllRotatorLines();
                break;
        }
        CheckEndGameCondition();
    }
// Rotator Function
    public bool IsRotatorCell(int row, int col)
    {
        return rotatorGrid != null
            && row >= 0 && col >= 0
            && row < rotatorGrid.GetLength(0)
            && col < rotatorGrid.GetLength(1)
            && rotatorGrid[row, col] != null;
    }

    private bool IsInRotatorRowOrCol(int row, int col)
    {
        if (rotators == null || rotators.Count == 0) return false;
        for (int i = 0; i < rotators.Count; i++)
        {
            Rotator rotator = rotators[i];
            if (rotator == null) continue;
            Vector2Int rc = rotator.OccupiedCell;
            if (row == rc.x || col == rc.y)
            {
                return true;
            }
        }
        return false;
    }
    public List<Block> GetBlocksIn2x2(Vector2Int anchorCell)
    {
        List<Block> result = new List<Block>(4);
        if (grid == null) return result;

        int rows = grid.GetLength(0);
        int cols = grid.GetLength(1);
        int r = anchorCell.x;
        int c = anchorCell.y;
        if (r < 0 || c < 0 || r + 1 >= rows || c + 1 >= cols) return result;

        if (grid[r, c] != null) result.Add(grid[r, c]);
        if (grid[r, c + 1] != null) result.Add(grid[r, c + 1]);
        if (grid[r + 1, c + 1] != null) result.Add(grid[r + 1, c + 1]);
        if (grid[r + 1, c] != null) result.Add(grid[r + 1, c]);

        return result;
    }

    public Block GetBlockAt(Vector2Int cell)
    {
        if (grid == null) return null;
        if (cell.x < 0 || cell.y < 0) return null;
        if (cell.x >= grid.GetLength(0) || cell.y >= grid.GetLength(1)) return null;
        return grid[cell.x, cell.y];
    }

    public void RotateBlocksInCells(List<Vector2Int> cells)
    {
        if (grid == null || cells == null || cells.Count < 2) return;

        // Read current blocks (including nulls).
        List<Block> current = new List<Block>(cells.Count);
        for (int i = 0; i < cells.Count; i++)
        {
            if (IsRotatorCell(cells[i].x, cells[i].y)) return;
            current.Add(GetBlockAt(cells[i]));
        }

        // Rotate clockwise: last -> first.
        for (int i = 0; i < cells.Count; i++)
        {
            int fromIndex = (i - 1 + cells.Count) % cells.Count;
            Block block = current[fromIndex];
            Vector2Int targetCell = cells[i];

            grid[targetCell.x, targetCell.y] = block;
            if (block != null)
            {
                block.SetBlockRowAnCol(targetCell.x, targetCell.y);
                block.transform.localPosition = new Vector3(targetCell.y, targetCell.x, 0f);
            }
        }
    }

    public Vector2Int[] GetCellsIn2x2(Vector2Int anchorCell)
    {
        return new[]
        {
            new Vector2Int(anchorCell.x, anchorCell.y),
            new Vector2Int(anchorCell.x, anchorCell.y + 1),
            new Vector2Int(anchorCell.x + 1, anchorCell.y + 1),
            new Vector2Int(anchorCell.x + 1, anchorCell.y),
        };
    }

// Spawn Block 
    private Block SpawnBlockAt(Vector2Int cell)
    {
        if (blockPrefab == null) return null;
        if (grid == null) return null;
        if (cell.x < 0 || cell.y < 0) return null;
        if (cell.x >= grid.GetLength(0) || cell.y >= grid.GetLength(1)) return null;
        if (grid[cell.x, cell.y] != null) return null;
        if (IsRotatorCell(cell.x, cell.y)) return null;

        Type safeDirection = GetSafeDirection(cell.x, cell.y);
        Block block = Instantiate(blockPrefab, blockParent);
        block.transform.localPosition = new Vector3(cell.y, cell.x, 0f);
        block.SetBlockRowAnCol(cell.x, cell.y);
        block.SetDirection(safeDirection);
        block.name = $"Block_{cell.x}_{cell.y}";

        grid[cell.x, cell.y] = block;
        blocks.Add(block);
        return block;
    }

    public void SetMoveTurnText(int moveTurnValue)
    {
        if (moveTurnText != null)
        {
            moveTurnText.SetText(moveTurnValue.ToString());
        }
    }


    public void RegenerateLevel(int newMoveTurn)
    {
        moveTurn = newMoveTurn;
        SetMoveTurnText(moveTurn);
        GenerateLevel(moveTurn);
        GameManager_.Instance.ChangeState(State.PLAYING);
    }

    private void GenerateLevel(int moveTurnValue)
    {
        ClearLevel();
        int safeRows = Mathf.Max(1, gridRows);
        int safeColumns = Mathf.Max(1, gridColumns);
        grid = new Block[safeRows, safeColumns];
        rotatorGrid = new Rotator[safeRows, safeColumns];

        if (blockPrefab == null) {
            return;
        }


        int calculatedCount = CalculateBlockCount(moveTurnValue, safeRows, safeColumns);
        int blockCount = Mathf.Min(calculatedCount, moveTurnValue);

        // 1) Spawn rotators trước
        // 1) Spawn rotators trước (giới hạn theo blockCount để đảm bảo mỗi rotator có ít nhất 2 block)
        int remainingBlocks = blockCount;
        remainingBlocks = GenerateRotators(remainingBlocks);

        // 2) Spawn các block liên quan đến rotator trước
        remainingBlocks = SpawnBlocksForRotators(remainingBlocks);

        // 3) Spawn các block còn lại
        for (int i = 0; i < remainingBlocks; i++)
        {
            Vector2Int cell = FindEmptyCell(true);
            if (cell.x < 0) break;
            SpawnBlockAt(cell);
        }

        // Refresh lines after all blocks are spawned (so initial display is correct).
        if (rotators != null)
        {
            for (int i = 0; i < rotators.Count; i++)
            {
                if (rotators[i] != null) rotators[i].RefreshLines();
            }
        }
    }
    private void ClearLevel()
    {
        if (blocks == null)
        {
            blocks = new List<Block>();
        }

        for (int i = blocks.Count - 1; i >= 0; i--)
        {
            if (blocks[i] != null)
            {
                Destroy(blocks[i].gameObject);
            }
        }

        blocks.Clear();

        if (rotators != null)
        {
            for (int i = rotators.Count - 1; i >= 0; i--)
            {
                if (rotators[i] != null) Destroy(rotators[i].gameObject);
            }
            rotators.Clear();
        }

        grid = null;
        rotatorGrid = null;
    }
// Spawn rotator and its block
    private int SpawnBlocksForRotators(int remainingBlocks)
    {
        if (rotators == null || rotators.Count == 0) return remainingBlocks;
        if (remainingBlocks <= 0) return 0;

        for (int i = 0; i < rotators.Count; i++)
        {
            if (remainingBlocks <= 0) break;
            Rotator rotator = rotators[i];
            if (rotator == null) continue;

            List<Vector2Int> available = new List<Vector2Int>(4);
            List<Vector2Int> controlCells = rotator.ControlledCells;
            for (int k = 0; k < controlCells.Count; k++)
            {
                Vector2Int cell = controlCells[k];
                if (cell.x < 0 || cell.y < 0) continue;
                if (cell.x >= grid.GetLength(0) || cell.y >= grid.GetLength(1)) continue;
                if (grid[cell.x, cell.y] != null) continue;
                if (IsRotatorCell(cell.x, cell.y)) continue;
                available.Add(cell);
            }

            if (available.Count == 0) continue;

            int target = Mathf.Min(2, remainingBlocks, available.Count);

            for (int t = 0; t < target; t++)
            {
                int pick = UnityEngine.Random.Range(0, available.Count);
                Vector2Int cell = available[pick];
                available.RemoveAt(pick);
                SpawnBlockAt(cell);
                remainingBlocks--;
                if (remainingBlocks <= 0) break;
            }

            while (remainingBlocks > 0 && available.Count > 0)
            {
                int pick = UnityEngine.Random.Range(0, available.Count);
                Vector2Int cell = available[pick];
                available.RemoveAt(pick);
                SpawnBlockAt(cell);
                remainingBlocks--;
            }

            rotator.RefreshLines();
        }

        return remainingBlocks;
    }
    private int GenerateRotators(int remainingBlocks)
    {
        if (grid == null || rotatorPrefab == null) return remainingBlocks;

        int rows = grid.GetLength(0);
        int cols = grid.GetLength(1);
        int rotatorLimit = Mathf.Clamp(maxRotatorsPerLevel, 0, 1);

        if (rotators != null && rotators.Count > 0)
        {
            for (int i = rotators.Count - 1; i >= 0; i--)
            {
                if (rotators[i] != null) Destroy(rotators[i].gameObject);
            }
            rotators.Clear();
        }

        rotatorGrid = new Rotator[rows, cols];
        HashSet<Vector2Int> controlledCells = new HashSet<Vector2Int>();

        for (int r = 0; r < rows - 1; r++)
        {
            for (int c = 0; c < cols - 1; c++)
            {
                List<Vector2Int> emptyCells = new List<Vector2Int>(4);

                Vector2Int[] cells = GetCellsIn2x2(new Vector2Int(r, c));

                for (int k = 0; k < cells.Length; k++)
                {
                    Vector2Int cell = cells[k];
                    if (rotatorGrid[cell.x, cell.y] == null)
                    {
                        emptyCells.Add(cell);
                    }
                }

                if (rotators.Count >= rotatorLimit) return remainingBlocks;
                if (emptyCells.Count > 0 && UnityEngine.Random.value <= rotatorSpawnChance)
                {
                    Vector2Int rotatorCell = emptyCells[UnityEngine.Random.Range(0, emptyCells.Count)];
                    List<Vector2Int> control = new List<Vector2Int>(3);
                    for (int k = 0; k < cells.Length; k++)
                    {
                        Vector2Int cell = cells[k];
                        if (cell == rotatorCell) continue;
                        if (controlledCells.Contains(cell)) { control.Clear(); break; }
                        control.Add(cell);
                    }

                    if (control.Count < 2) continue;
                    if (remainingBlocks < 2) continue;

                    Rotator rotator = Instantiate(rotatorPrefab, blockParent);
                    rotator.transform.localPosition = new Vector3(rotatorCell.y, rotatorCell.x, -0.1f);
                    rotator.name = $"Rotator_{rotatorCell.x}_{rotatorCell.y}";
                    rotator.Init(this, new Vector2Int(r, c), rotatorCell, control);

                    rotators.Add(rotator);
                    rotatorGrid[rotatorCell.x, rotatorCell.y] = rotator;
                    for (int k = 0; k < control.Count; k++)
                    {
                        controlledCells.Add(control[k]);
                    }
                    remainingBlocks -= 2;
                }
            }
        }
        return remainingBlocks;
    }

    public void SwapBlocks(Block a, Block b)
    {
        if (grid == null || a == null || b == null) return;

        Vector2Int posA = a.GetRowAndColumn();
        Vector2Int posB = b.GetRowAndColumn();

        if (posA.x < 0 || posA.y < 0 || posB.x < 0 || posB.y < 0) return;
        if (posA.x >= grid.GetLength(0) || posB.x >= grid.GetLength(0)) return;
        if (posA.y >= grid.GetLength(1) || posB.y >= grid.GetLength(1)) return;
        if (IsRotatorCell(posA.x, posA.y) || IsRotatorCell(posB.x, posB.y)) return;

        // Swap trong máº£ng grid
        grid[posA.x, posA.y] = b;
        grid[posB.x, posB.y] = a;


        a.SetBlockRowAnCol(posB.x, posB.y);
        b.SetBlockRowAnCol(posA.x, posA.y);


        a.transform.localPosition = new Vector3(posB.y, posB.x, 0);
        b.transform.localPosition = new Vector3(posA.y, posA.x, 0);
    }

    private Type GetSafeDirection(int row, int col)
    {
        List<Type> possibleDirections = new List<Type>(directionPool);


        for (int c = 0; c < gridColumns; c++)
        {
            if (grid[row, c] != null)
            {
                Type otherType = grid[row, c].BlockType;
                if (otherType == Type.LEFT) possibleDirections.Remove(Type.RIGHT);
                if (otherType == Type.RIGHT) possibleDirections.Remove(Type.LEFT);
            }
        }


        for (int r = 0; r < gridRows; r++)
        {
            if (grid[r, col] != null)
            {
                Type otherType = grid[r, col].BlockType;
                if (otherType == Type.UP) possibleDirections.Remove(Type.DOWN);
                if (otherType == Type.DOWN) possibleDirections.Remove(Type.UP);
            }
        }

        if (possibleDirections.Count == 0) 
            return directionPool[UnityEngine.Random.Range(0, directionPool.Length)];

        return possibleDirections[UnityEngine.Random.Range(0, possibleDirections.Count)];
    }
    private Vector2Int FindEmptyCell(bool avoidRotatorLines = false)
    {
        if (grid == null)
        {
            return new Vector2Int(-1, -1);
        }

        int rows = grid.GetLength(0);
        int columns = grid.GetLength(1);
        int maxAttempts = rows * columns;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            int randomRow = UnityEngine.Random.Range(0, rows);
            int randomCol = UnityEngine.Random.Range(0, columns);
            if (grid[randomRow, randomCol] == null
                && !IsRotatorCell(randomRow, randomCol)
                && (!avoidRotatorLines || !IsInRotatorRowOrCol(randomRow, randomCol)))
            {
                return new Vector2Int(randomRow, randomCol);
            }
        }

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                if (grid[row, col] == null
                    && !IsRotatorCell(row, col)
                    && (!avoidRotatorLines || !IsInRotatorRowOrCol(row, col)))
                {
                    return new Vector2Int(row, col);
                }
            }
        }

        return new Vector2Int(-1, -1);
    }
    private int CalculateBlockCount(int moveTurnValue, int rows, int columns)
    {
        int clampedMove = Mathf.Clamp(moveTurnValue, minMoveTurnForScaling, maxMoveTurnForScaling);
        float ratio = Mathf.InverseLerp(minMoveTurnForScaling, maxMoveTurnForScaling, clampedMove);

        int totalCells = Mathf.Clamp(rows * columns, 1, int.MaxValue);
        int adjustedMinBlocks = Mathf.Clamp(minBlocks, 1, totalCells);
        int adjustedMaxBlocks = Mathf.Clamp(maxBlocks, adjustedMinBlocks, totalCells);

        int blockCount = Mathf.RoundToInt(Mathf.Lerp(adjustedMaxBlocks, adjustedMinBlocks, ratio));
        return Mathf.Clamp(blockCount, adjustedMinBlocks, adjustedMaxBlocks);
    }
    private void RefreshAllRotatorLines()
    {
        if (rotators == null) return;
        for (int i = 0; i < rotators.Count; i++)
        {
            if (rotators[i] != null) rotators[i].RefreshLines();
        }
    }
// Check comdition
    public void CheckEndGameCondition()
    {
        // Left empty intentionally for future requirements.
        if (moveTurn < blocks.Count)
        {
            GameManager_.Instance.LoseGame();
            return;
        }

        if (blocks.Count <= 0)
        {
            GameManager_.Instance.WinGame();
        }
    }

    // Booster bomb
    public void ActivateBombBoosterAtBlock(int radius = 3)
    {
        if (Camera.main == null || blockParent == null) return;
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 local = blockParent.InverseTransformPoint(mouseWorldPos);
        int row = Mathf.RoundToInt(local.y);
        int col = Mathf.RoundToInt(local.x);
        GameObject bomb =Instantiate(bombPrefab,grid[row,col].transform.position,Quaternion.identity);
        GameObject explosion =Instantiate(explosionPrefab,grid[row,col].transform.position,Quaternion.identity);
        StartCoroutine(BomExploseCoroutine(row,col,radius,bomb,explosion));
    }
    public IEnumerator BomExploseCoroutine(int row, int col, int radius,GameObject bomb,GameObject explosion)
    {
        yield return new WaitForSeconds(0.5f);
        Destroy(bomb);
        explosion.SetActive(true);
        BombAtCell(new Vector2Int(row, col), radius);
        yield return new WaitForSeconds(0.5f);
        Destroy(explosion);
    }

    public void BombAtCell(Vector2Int center, int radius = 3)
    {
        if (grid == null) return;
        int rows = grid.GetLength(0);
        int cols = grid.GetLength(1);

        for (int r = Mathf.Max(0, center.x - radius); r <= Mathf.Min(rows - 1, center.x + radius); r++)
        {
            for (int c = Mathf.Max(0, center.y - radius); c <= Mathf.Min(cols - 1, center.y + radius); c++)
            {
                int dist = Mathf.Abs(r - center.x) + Mathf.Abs(c - center.y);
                if (dist > radius) continue;

                Block block = grid[r, c];
                if (block != null)
                {
                    grid[r, c] = null;
                    blocks.Remove(block);
                    Destroy(block.gameObject);
                }
            }
        }

        RefreshAllRotatorLines();
        CheckEndGameCondition();
    }
}
