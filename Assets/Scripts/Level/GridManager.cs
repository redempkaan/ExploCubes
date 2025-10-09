using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;
using System.Collections;

public class GridManager : MonoBehaviour
{
    [Header("Grid Settings")]
    public RectTransform gridBackground;
    public float cellSize = 100f;        // Cell size

    [Header("Prefabs")]
    public GameObject redCubePrefab;
    public GameObject greenCubePrefab;
    public GameObject blueCubePrefab;
    public GameObject yellowCubePrefab;
    public GameObject rocketHPrefab;
    public GameObject rocketVPrefab;
    public GameObject boxPrefab;
    public GameObject stonePrefab;
    public GameObject vasePrefab;
    public GameObject brokenVasePrefab;

    private GridItem[,] itemMatrix; // 2D array to store grid items

    public void InitializeItemMatrix(int width, int height) // Initializing item matrix
    {
        itemMatrix = new GridItem[width, height];
    }

    public void SetGridItem(int x, int y, GridItem item)
    {
        if (x < 0 || y < 0 || x >= itemMatrix.GetLength(0) || y >= itemMatrix.GetLength(1))
        {
            Debug.LogWarning($"GridManager: Invalid coordinates ({x},{y})");
            return;
        }

        itemMatrix[x, y] = item;
    }

    public void SetGridBackground(int width, int height)
    {
        if (gridBackground == null)
        {
            Debug.LogError("No GridBackground assigned!");
            return;
        }

        float newWidth = width * cellSize;
        float newHeight = height * cellSize;

        gridBackground.sizeDelta = new Vector2(newWidth, newHeight);

        float verticalOffset = -150f; // Putting it a little bit lower
        gridBackground.anchoredPosition = new Vector2(0, verticalOffset);

        Image bgImage = gridBackground.GetComponent<Image>();
        if (bgImage != null)
        {
            bgImage.color = Color.black; // Setting background color to black
        }

        Outline outline = gridBackground.GetComponent<Outline>();
        if (outline != null)
        {
            outline.effectColor = new Color(1f, 0.5f, 0f); // Setting outline color to orange
        }
    }

    public void GenerateGrid(LevelData levelData) // Placing the grid items
    {
        foreach (Transform child in gridBackground)
        {
            Destroy(child.gameObject); // Cleaning old childs
        }

        LevelManager.Instance.CurrentLevel.obstacleCounts.Clear(); // Reseting obstacleCounts dictionary

        int width = levelData.grid_width;
        int height = levelData.grid_height;

        float offsetX = (width - 1) * cellSize * 0.5f;
        float offsetY = (height - 1) * cellSize * 0.5f;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = y * width + x;
                if (index >= levelData.grid.Length) continue;

                string code = levelData.grid[index];
                var (prefab, finalCode) = GetPrefabForCode(code); // Returning specified item code's prefab

                if (prefab != null)
                {
                    GameObject obj = Instantiate(prefab, gridBackground); // Instantiating the game object

                    RectTransform rt = obj.GetComponent<RectTransform>(); // Getting its RectTransform component

                    if (rt != null)
                    {
                        rt.anchoredPosition = new Vector2(x * cellSize - offsetX, y * cellSize - offsetY); // Setting object's position up

                        if (finalCode == "vro" || finalCode == "hro")
                        {
                            Destroy(obj);
                            RocketItem.InitializeRocket(x, y, rt.anchoredPosition, this); // If it is a rocket, leave its initialization to Rocket class
                        }
                        else
                        {
                            GridItem gridItem = obj.GetComponent<GridItem>();

                            gridItem.gridX = x; // X coordinate
                            gridItem.gridY = y; // Y coordinate
                            gridItem.itemCode = finalCode;

                            itemMatrix[x, y] = gridItem; // Marking its place in item matrix
                        }
                    }
                }
            }
        }
    }

    private (GameObject prefab, string code) GetPrefabForCode(string code) // Returning prefabs according to input item codes and if the code belongs to an obstacle, registering it
    {
        switch (code)
        {
            case "r": return (redCubePrefab, "r");
            case "g": return (greenCubePrefab, "g");
            case "b": return (blueCubePrefab, "b");
            case "y": return (yellowCubePrefab, "y");

            case "rand":
                int rnd = Random.Range(0, 4);
                if (rnd == 0) return (redCubePrefab, "r");
                if (rnd == 1) return (greenCubePrefab, "g");
                if (rnd == 2) return (blueCubePrefab, "b");
                return (yellowCubePrefab, "y");

            case "hro": return (rocketHPrefab, "hro");
            case "vro": return (rocketVPrefab, "vro");

            case "bo":
                LevelManager.Instance?.RegisterObstacle("bo");
                return (boxPrefab, "bo");
            case "s":
                LevelManager.Instance?.RegisterObstacle("s");
                return (stonePrefab, "s");
            case "v":
                LevelManager.Instance?.RegisterObstacle("v");
                return (vasePrefab, "v");

            default: return (null, null);
        }
    }


    public void OnItemClicked(ClickableItem item)
    {
        // Hedef kod
        string targetCode = item.itemCode;

        HashSet<int> affectedColumns = new HashSet<int>(); // Storing affected columns
        ItemsToExplode result; // Using our helper class to store near grid items

        if (targetCode != "vro" && targetCode != "hro")
        {
            result = FindConnectedItems(item.gridX, item.gridY, item.itemCode); // If the clicked item is not a rocket, find connected items

            Debug.Log($"Found group size: {result.connectedCubes.Count}");

            if (result.connectedCubes.Count >= 3) // If found group size is equals to or bigger than 3, explode them
            {
                // Storing cube's coordinates and RectTransform component in any case of creating a rocket
                int rocketX = item.gridX;
                int rocketY = item.gridY;
                RectTransform rocketRt = item.GetComponent<RectTransform>();
                Vector2 rocketPos = rocketRt.anchoredPosition;

                LevelManager.Instance.DecreaseMove(); // Decrease remaining moves as it is a valid move

                affectedColumns = result.DestroyItemsToExplode(itemMatrix); // Exploding the near grid items and returning a column list

                if (result.connectedCubes.Count >= 4) // If group is bigger than 3, create a rocket also
                {
                    RocketItem.InitializeRocket(rocketX, rocketY, rocketPos, this);
                }
                foreach (int col in affectedColumns)
                {
                    CollapseColumn(col);
                }
            }
        }
        else // Rocket explosion block
        {
            RocketItem rocket = item as RocketItem; // It is guaranteed to be a rocket, so we are getting its RocketItem script
            if (rocket == null) return;

            ItemsToExplode chainResult = rocket.PrepareFullChain(itemMatrix); // Calculating the rocket chain

            if (chainResult.nearRockets.Count > 1) // If there are more than 1 rockets to explode, trigger a chain
            {
                StartCoroutine(ExplodeAllRocketsSequentially(chainResult, itemMatrix));
            }
            else // Single rocket explosion block
            {
                rocket.OnRocketExplosionComplete += (result) => // Waiting for explosion to be completed before collapsing the columns
                {
                    LevelManager.Instance.DecreaseMove();

                    var affectedCols = result.DestroyItemsToExplode(itemMatrix);

                    foreach (int col in affectedCols)
                        CollapseColumn(col);
                };

                rocket.ExplodeRocket(itemMatrix, this);
            }
        }
            }

    public void CollapseColumn(int x)
    {
        int width = itemMatrix.GetLength(0);
        int height = itemMatrix.GetLength(1);

        if (x < 0 || x >= width) return;

        int emptyRow = 0; // Bottom of the column

        for (int y = 0; y < height; y++) // Moving the non-null items to bottom rows
        {
            GridItem item = itemMatrix[x, y];
            if (item == null) continue;

            if (item.itemCode == "bo" || item.itemCode == "s") // If the item is box or stone, skip it
            {
                if (y != emptyRow)
                {
                    emptyRow = y + 1;
                }
                else
                {
                    emptyRow++;
                }
                continue;
            }

            if (y != emptyRow)
            {
                itemMatrix[x, emptyRow] = item;
                itemMatrix[x, y] = null;

                item.gridY = emptyRow;
                // Reassigning the item's position
                Vector2 targetPos = GridToLocalPosition(x, emptyRow, width, height);
                RectTransform rt = item.GetComponent<RectTransform>();
                rt.DOAnchorPos(targetPos, 0.6f).SetEase(Ease.OutQuad);
            }

            emptyRow++;
        }

        for (int y = emptyRow; y < height; y++) // Filling empty rows with new cubes
        {
            var (prefab, code) = GetPrefabForCode("rand");
            GameObject obj = Instantiate(prefab, gridBackground);
            RectTransform rt = obj.GetComponent<RectTransform>();

            GridItem newItem = obj.GetComponent<GridItem>();
            newItem.gridX = x;
            newItem.gridY = y;
            newItem.itemCode = code;

            rt.anchoredPosition = new Vector2( // Spawning it from a little bit higher than the top of the grid
                x * cellSize - (width - 1) * cellSize * 0.5f,
                (height + 2) * cellSize
            );
            // Animation
            Vector2 targetPos = GridToLocalPosition(x, y, width, height);
            rt.DOAnchorPos(targetPos, 0.8f).SetEase(Ease.OutQuad);

            itemMatrix[x, y] = newItem;
        }
    }

    private Vector2 GridToLocalPosition(int x, int y, int width, int height) // Function to calculate an item's position on the grid
    {
        float offsetX = (width - 1) * cellSize * 0.5f;
        float offsetY = (height - 1) * cellSize * 0.5f;
        return new Vector2(x * cellSize - offsetX, y * cellSize - offsetY);
    }

    private ItemsToExplode FindConnectedItems(int startX, int startY, string targetCode) // BFS to find connected items
    {
        ItemsToExplode result = new ItemsToExplode();
        HashSet<GridItem> visited = new HashSet<GridItem>();
        Queue<(int, int)> queue = new Queue<(int, int)>();

        queue.Enqueue((startX, startY)); // Starting with clicked item

        while (queue.Count > 0)
        {
            var (x, y) = queue.Dequeue();

            if (x < 0 || y < 0 || x >= itemMatrix.GetLength(0) || y >= itemMatrix.GetLength(1)) // Matrix border control
                continue;

            GridItem current = itemMatrix[x, y];
            if (current == null) continue;

            if (visited.Contains(current)) continue;
            visited.Add(current);

            if (current.itemCode == "bo" ||   // If it is an obstacle
                current.itemCode == "v")
            {
                result.nearObstacles.Add(current); // Add into the obstacle dictionary and cut the BFS
                continue; 
            }

            if (current.itemCode != targetCode) continue; // If colors dismatch, cut the BFS

            result.connectedCubes.Add(current); // Else add into the cubes hashset

            // Enqueueing the near grid items
            queue.Enqueue((x + 1, y));
            queue.Enqueue((x - 1, y));
            queue.Enqueue((x, y + 1));
            queue.Enqueue((x, y - 1));
        }

        return result;
    }
    private IEnumerator ExplodeAllRocketsSequentially(ItemsToExplode chainResult, GridItem[,] itemMatrix)
{
    List<RocketItem> rockets = new List<RocketItem>();
    foreach (GridItem g in chainResult.nearRockets)
    {
        if (g is RocketItem rocket)
            rockets.Add(rocket);
    }

    if (rockets.Count == 0)
        yield break;

    int completedCount = 0;

    ItemsToExplode accumulated = new ItemsToExplode();

    foreach (var rocket in rockets) // Listening completion of rocket explosions to provide synchronization
    {
        rocket.OnRocketExplosionComplete += (res) =>
        {
            completedCount++;
            // Merging connectedCubes and nearObstacles
            accumulated.connectedCubes.UnionWith(res.connectedCubes);
            accumulated.nearObstacles.UnionWith(res.nearObstacles);
        };
    }

    foreach (var rocket in rockets)
    {
        if (rocket != null)
        {
            rocket.ExplodeRocket(itemMatrix, this);
            yield return new WaitForSeconds(0.10f);
        }
    }

    yield return new WaitUntil(() => completedCount >= rockets.Count); // Wait until all explosions are done

    ItemsToExplode destroyResult = new ItemsToExplode();

    // Merging all results
    destroyResult.connectedCubes.UnionWith(chainResult.connectedCubes);
    destroyResult.nearObstacles.UnionWith(chainResult.nearObstacles);
    destroyResult.connectedCubes.UnionWith(accumulated.connectedCubes);
    destroyResult.nearObstacles.UnionWith(accumulated.nearObstacles);

    var affectedCols = destroyResult.DestroyItemsToExplode(itemMatrix);

    foreach (int col in affectedCols)
    {
        CollapseColumn(col);
    }
}

}