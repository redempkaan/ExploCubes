
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;
using System.Collections;

public class RocketItem : ClickableItem // GridItem -> ClickableItem -> RocketItem
{
    [Header("Part Prefabs")]
    public GameObject rocketPart1Prefab;
    public GameObject rocketPart2Prefab;

    [Header("FX Prefabs")]
    public GameObject particleSmokePrefab;



    [Header("Rocket Parts")]
    public GameObject rocketPart1;
    public GameObject rocketPart2;
    [Header("Rocket Type")]
    public bool isHorizontal;


    private void Awake()
    {

        DOTween.SetTweensCapacity(2000, 100); // Limiting capacity to prevent game breaking freeze

        // Finding its part objects
        if (rocketPart1 == null)
            rocketPart1 = transform.Find("RocketPart1")?.gameObject;

        if (rocketPart2 == null)
            rocketPart2 = transform.Find("RocketPart2")?.gameObject;
    }
    public static void InitializeRocket(int rocketX, int rocketY, UnityEngine.Vector2 rocketPos, GridManager gridManager)
    {
        bool horizontal = UnityEngine.Random.value > 0.5f;

        GameObject rocketPrefab = horizontal ? gridManager.rocketHPrefab : gridManager.rocketVPrefab; // Deciding if the rocket will be vertical or horizontal

        // Instantiate prefab and get its RectTransform component
        GameObject rocketObj = UnityEngine.Object.Instantiate(rocketPrefab, gridManager.gridBackground);
        RectTransform rt = rocketObj.GetComponent<RectTransform>();
        rt.anchoredPosition = rocketPos;

        // Take its RocketItem script and fill in
        RocketItem rocketItem = rocketObj.GetComponent<RocketItem>();
        rocketItem.isHorizontal = horizontal;
        // Fill its grid item properties
        GridItem gridItem = rocketObj.GetComponent<GridItem>();
        gridItem.gridX = rocketX;
        gridItem.gridY = rocketY;
        gridItem.itemCode = horizontal ? "hro" : "vro";

        rocketItem.InitializeRocketParts(); // Initialize the rocket parts

        gridManager.SetGridItem(gridItem.gridX, gridItem.gridY, gridItem); // Placing it on the grid


    }

    public void InitializeRocketParts()
    {
        if (rocketPart1 != null) Destroy(rocketPart1);
        if (rocketPart2 != null) Destroy(rocketPart2);

        rocketPart1 = Instantiate(rocketPart1Prefab, transform);
        rocketPart2 = Instantiate(rocketPart2Prefab, transform);

        rocketPart1.SetActive(false);
        rocketPart2.SetActive(false);
    }

    public override void OnPointerClick(UnityEngine.EventSystems.PointerEventData eventData)
    {
        base.OnPointerClick(eventData); // Triggering base class' onPointerClick

        ActivateRocketParts(); // Activate its rocket part additionally
    }

    private void ActivateRocketParts()
    {
        float offset = 100f;
        RectTransform rt1 = rocketPart1.GetComponent<RectTransform>();
        RectTransform rt2 = rocketPart2.GetComponent<RectTransform>();
        if (isHorizontal)
        {
            rt1.anchoredPosition = new UnityEngine.Vector2(offset, 0); // Left
            rt2.anchoredPosition = new UnityEngine.Vector2(-offset, 0); // Right
        }
        else
        {
            rt1.anchoredPosition = new UnityEngine.Vector2(0, offset); // Up
            rt2.anchoredPosition = new UnityEngine.Vector2(0, -offset); // Down
        }
    }

    private void SpawnSmoke(UnityEngine.Vector3 pos, GridManager gridManager, ref float lastSmokeTime, float smokeInterval) // Spawn smoke while rocket parts moving
    {
        if (Time.time - lastSmokeTime < smokeInterval) return;

        if (particleSmokePrefab != null)
        {
            var smoke = UnityEngine.Object.Instantiate(
                particleSmokePrefab,
                pos,
                UnityEngine.Quaternion.identity,
                gridManager.gridBackground
            );
            UnityEngine.Object.Destroy(smoke, 0.3f); // Destroy it after 0.3 seconds
        }

        lastSmokeTime = Time.time;
    }

    public event Action<ItemsToExplode> OnRocketExplosionComplete; // Event to inform the explosion is completed

    public ItemsToExplode ExplodeRocket(GridItem[,] itemMatrix, GridManager gridManager)
    {
        ItemsToExplode result = FindTargetsInRange(itemMatrix); // Getting items in rocket's range

        int width = itemMatrix.GetLength(0);
        int height = itemMatrix.GetLength(1);
        float duration = 0.8f;
        float cellSize = gridManager.cellSize;

        var image = GetComponent<UnityEngine.UI.Image>(); // Hide rocket base
        if (image != null)
        {
            image.enabled = false;
            image.raycastTarget = false;
        }

        if (rocketPart1 == null || rocketPart2 == null)
        {
            Debug.LogWarning("Rocket parts missing, skipping animation.");
            OnRocketExplosionComplete?.Invoke(result);
            return result;
        }

        // Activate rocket parts
        rocketPart1.SetActive(true);
        rocketPart2.SetActive(true);

        RectTransform rt1 = rocketPart1.GetComponent<RectTransform>();
        RectTransform rt2 = rocketPart2.GetComponent<RectTransform>();

        if (rt1 == null || rt2 == null)
        {
            Debug.LogWarning("RectTransforms missing, aborting explosion tween.");
            OnRocketExplosionComplete?.Invoke(result);
            return result;
        }

        // Calculate target positions
        Vector2 target1, target2;
        if (isHorizontal)
        {
            target1 = rt1.anchoredPosition + new Vector2(width * cellSize, 0);
            target2 = rt2.anchoredPosition + new Vector2(-width * cellSize, 0);
        }
        else
        {
            target1 = rt1.anchoredPosition + new Vector2(0, height * cellSize);
            target2 = rt2.anchoredPosition + new Vector2(0, -height * cellSize);
        }

        float lastSmoke1 = 0f;
        float lastSmoke2 = 0f;
        float smokeInterval = 0.12f;

        // Initialize tweens
        Tween t1 = rt1.DOAnchorPos(target1, duration)
            .SetEase(Ease.Linear)
            .OnUpdate(() =>
            {
                if (rt1 == null || rocketPart1 == null || !rocketPart1.activeInHierarchy)
                {
                    DOTween.Kill(rt1);
                    return;
                }
                SpawnSmoke(rt1.position, gridManager, ref lastSmoke1, smokeInterval);
            })
            .OnComplete(() =>
            {
                if (rocketPart1 != null) rocketPart1.SetActive(false);
            });

        Tween t2 = rt2.DOAnchorPos(target2, duration)
            .SetEase(Ease.Linear)
            .OnUpdate(() =>
            {
                if (rt2 == null || rocketPart2 == null || !rocketPart2.activeInHierarchy)
                {
                    DOTween.Kill(rt2);
                    return;
                }
                    SpawnSmoke(rt2.position, gridManager, ref lastSmoke2, smokeInterval);
            })
            .OnComplete(() =>
            {
                if (rocketPart2 != null) rocketPart2.SetActive(false);
            });

        // Trigger the event when both parts have reached their targets
        DOTween.Sequence()
        .Append(t1)
        .Join(t2)
        .OnComplete(() =>
        {
            if (this == null || gameObject == null)
                return;

            //Kill tweens on complete
            if (rt1 != null) DOTween.Kill(rt1);
            if (rt2 != null) DOTween.Kill(rt2);
            // Making them inactive
            if (rocketPart1 != null && rocketPart1.activeInHierarchy)
                rocketPart1.SetActive(false);

            if (rocketPart2 != null && rocketPart2.activeInHierarchy)
                rocketPart2.SetActive(false);

            OnRocketExplosionComplete?.Invoke(result); // Event triggered

            StartCoroutine(DisableRocketSafely()); // Disable rocket with some latency
        });

        return result;
    }

    private IEnumerator DisableRocketSafely()
    {
        yield return new WaitForSeconds(0.05f);
        if (this != null && gameObject != null)
            gameObject.SetActive(false);
    }

    public ItemsToExplode FindTargetsInRange(GridItem[,] itemMatrix) // Finding items in rockets range and storing them in ItemsToExplode structure
    {
        ItemsToExplode result = new ItemsToExplode();
        result.connectedCubes.Add(this);

        int width = itemMatrix.GetLength(0);
        int height = itemMatrix.GetLength(1);

        if (isHorizontal)
        {
            for (int x = 0; x < width; x++)
            {
                GridItem g = itemMatrix[x, gridY];
                if (g == null || g == this) continue;

                switch (g.itemCode)
                {
                    case "bo":
                    case "v":
                    case "s":
                        result.nearObstacles.Add(g);
                        break;

                    case "vro":
                    case "hro":
                        result.nearRockets.Add(g);
                        break;

                    default:
                        result.connectedCubes.Add(g);
                        break;
                }
            }
        }
        else
        {
            for (int y = 0; y < height; y++)
            {
                GridItem g = itemMatrix[gridX, y];
                if (g == null || g == this) continue;

                switch (g.itemCode)
                {
                    case "bo":
                    case "v":
                    case "s":
                        result.nearObstacles.Add(g);
                        break;

                    case "vro":
                    case "hro":
                        result.nearRockets.Add(g);
                        break;

                    default:
                        result.connectedCubes.Add(g);
                        break;
                }
            }
        }

        return result;
    }

    public void FindAllNearbyRocketsRecursive(GridItem[,] itemMatrix, HashSet<GridItem> visited, ItemsToExplode accumulated) // Finding rocket chain recursively
    {
        if (visited.Contains(this)) // If the rocket already visited, return
            return;

        visited.Add(this);

        ItemsToExplode localResult = FindTargetsInRange(itemMatrix);

        // Merge the results
        foreach (var c in localResult.connectedCubes) accumulated.connectedCubes.Add(c);
        foreach (var o in localResult.nearObstacles) accumulated.nearObstacles.Add(o);
        foreach (var r in localResult.nearRockets) accumulated.nearRockets.Add(r);

        // Continue scanning by adding the new rockets in range
        foreach (var rocket in localResult.nearRockets)
        {
            if (rocket != null && !visited.Contains(rocket))
            {
                RocketItem ri = rocket as RocketItem;
                if (ri != null)
                    ri.FindAllNearbyRocketsRecursive(itemMatrix, visited, accumulated);
            }
        }
    }
    
    public ItemsToExplode PrepareFullChain(GridItem[,] itemMatrix)
    {
        ItemsToExplode fullChain = new ItemsToExplode();
        HashSet<GridItem> visited = new HashSet<GridItem>();

        FindAllNearbyRocketsRecursive(itemMatrix, visited, fullChain);

        return fullChain;
    }

}